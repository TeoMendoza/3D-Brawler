#!/usr/bin/env bash
set -euo pipefail

: "${SpacetimeDatabaseName:?Missing SpacetimeDatabaseName}"

SupportsColor=0
if [ -t 1 ]; then SupportsColor=1; fi

Bold() { if [ "$SupportsColor" -eq 1 ]; then printf '\033[1m%s\033[0m' "$1"; else printf '%s' "$1"; fi; }
Dim() { if [ "$SupportsColor" -eq 1 ]; then printf '\033[2m%s\033[0m' "$1"; else printf '%s' "$1"; fi; }
Green() { if [ "$SupportsColor" -eq 1 ]; then printf '\033[32m%s\033[0m' "$1"; else printf '%s' "$1"; fi; }
Red() { if [ "$SupportsColor" -eq 1 ]; then printf '\033[31m%s\033[0m' "$1"; else printf '%s' "$1"; fi; }
Yellow() { if [ "$SupportsColor" -eq 1 ]; then printf '\033[33m%s\033[0m' "$1"; else printf '%s' "$1"; fi; }

PrintLine() { printf "%s\n" "$1"; }
PrintHeader() { PrintLine "$(Bold "$1")"; }
PrintInfo() { PrintLine "  $(Dim "•") $1"; }
PrintOk() { PrintLine "  $(Green "✓") $1"; }
PrintWarn() { PrintLine "  $(Yellow "!") $1"; }
PrintFail() { PrintLine "  $(Red "✗") $1"; }

CurrentTestName=""
TestCountTotal=0
TestCountPassed=0
SuiteStartSeconds=0

BeginSuiteTimer() {
  SuiteStartSeconds="$(date +%s)"
}

GetElapsedSeconds() {
  local EndSeconds
  EndSeconds="$(date +%s)"
  echo $((EndSeconds - SuiteStartSeconds))
}

CurrentTestStartSeconds=0

BeginTest() {
  CurrentTestName="$1"
  CurrentTestStartSeconds="$(date +%s)"
  TestCountTotal=$((TestCountTotal + 1))
  PrintHeader "▶ Running test: $CurrentTestName"
}

EndTest() {
  local EndSeconds
  EndSeconds="$(date +%s)"
  local DurationSeconds
  DurationSeconds=$((EndSeconds - CurrentTestStartSeconds))

  TestCountPassed=$((TestCountPassed + 1))
  PrintOk "Passed: $CurrentTestName (${DurationSeconds}s)"
  PrintLine ""
  CurrentTestName=""
  CurrentTestStartSeconds=0
}

FailNow() {
  local Message="$1"
  PrintFail "Failed: ${CurrentTestName:-<no test>}"
  PrintFail "$Message"
  PrintLine ""
  PrintFail "Summary: ${TestCountPassed}/${TestCountTotal} tests passed"
  exit 1
}

trap 'FailNow "Unexpected error on line $LINENO."' ERR

SpacetimeSql() {
  local Query="$1"
  spacetime sql --server local "$SpacetimeDatabaseName" "$Query" 2>&1 | sed '/^WARNING: This command is UNSTABLE/d'
}

SpacetimeCall() {
  local FunctionName="$1"
  local Arguments="${2:-}"
  if [ -n "$Arguments" ]; then
    spacetime call --server local "$SpacetimeDatabaseName" "$FunctionName" "$Arguments" 2>&1 | sed '/^WARNING: This command is UNSTABLE/d'
  else
    spacetime call --server local "$SpacetimeDatabaseName" "$FunctionName" 2>&1 | sed '/^WARNING: This command is UNSTABLE/d'
  fi
}

ExtractLastInteger() {
  local RawOutput="$1"
  echo "$RawOutput" | grep -Eo '[0-9]+' | tail -n 1
}

AssertEquals() {
  local Actual="$1"
  local Expected="$2"
  local Message="$3"

  if [ "$Actual" != "$Expected" ]; then
    PrintFail "$Message"
    PrintInfo "Expected: $Expected"
    PrintInfo "Actual:   $Actual"
    exit 1
  fi
}

AssertSqlCountEquals() {
  local TableOrQuery="$1"
  local ExpectedCount="$2"
  local Message="$3"

  local Query
  if [[ "$TableOrQuery" == SELECT* ]]; then
    Query="$TableOrQuery"
  else
    Query="SELECT COUNT(*) AS CountValue FROM $TableOrQuery;"
  fi

  local RawOutput
  RawOutput="$(SpacetimeSql "$Query")"

  local ActualCount
  ActualCount="$(ExtractLastInteger "$RawOutput")"

  if [ -z "$ActualCount" ]; then
    PrintFail "$Message"
    PrintInfo "Could not parse count from SQL output:"
    PrintLine "$RawOutput"
    exit 1
  fi

  AssertEquals "$ActualCount" "$ExpectedCount" "$Message"
}

GetSingleGameId() {
  local RawOutput
  RawOutput="$(SpacetimeSql "SELECT id AS GameId FROM game;")"
  local GameId
  GameId="$(ExtractLastInteger "$RawOutput")"
  if [ -z "$GameId" ]; then
    PrintFail "Could not parse game id"
    PrintLine "$RawOutput"
    exit 1
  fi
  echo "$GameId"
}

EnableUnitTestMode() {
  BeginTest "Enable Unit Test Mode"
  PrintInfo "Enabling unit test mode"
  SpacetimeCall "enable_unit_test_mode" >/dev/null
  AssertSqlCountEquals "logged_in_players" "1" "Unit test mode should have 1 logged-in player"
  AssertSqlCountEquals "logged_out_players" "0" "Unit test mode should have 0 logged-out players"
  EndTest
}

DisableUnitTestMode() {
  BeginTest "Disable Unit Test Mode"
  PrintInfo "Disabling unit test mode"
  SpacetimeCall "disable_unit_test_mode" >/dev/null
  AssertSqlCountEquals "logged_in_players" "0" "Unit tests are finished, there should be 0 logged-in players"
  AssertSqlCountEquals "logged_out_players" "1" "Unit tests are finished, there should be 1 logged-out players"
  EndTest
}

AssertInitWorked() {
  BeginTest "Init inserts expected rows"
  AssertSqlCountEquals "map" "4" "Init should insert 4 maps"
  EndTest
}

TestJoinAndStartSinglePlayerMatch() {
  BeginTest "Join starts a single-player match"
  PrintInfo "Calling reducer: test_join_and_start_game_single_player"
  SpacetimeCall "test_join_and_start_game_single_player" >/dev/null

  local GameId
  GameId="$(GetSingleGameId)"

  AssertSqlCountEquals "game" "1" "Test Join should insert 1 game"
  AssertSqlCountEquals "move_all_magicians" "1" "Test Join should insert move_all_magicians timer"
  AssertSqlCountEquals "handle_magician_timers_timer" "1" "Test Join should insert handle_magician_timers_timer timer"
  AssertSqlCountEquals "handle_magician_stateless_timers_timer" "1" "Test Join should insert handle_magician_stateless_timers_timer timer"
  AssertSqlCountEquals "gravity_magician" "1" "Test Join should insert gravity_magician timer"
  AssertSqlCountEquals "player_effects_table_timer" "1" "Test Join should insert player_effects_table_timer timer"
  AssertSqlCountEquals "magician" "1" "Test Join should create 1 magician"
  AssertSqlCountEquals "SELECT COUNT(*) AS CountValue FROM game WHERE id = ${GameId} AND in_progress = true;" "1" "Game should be in progress after join"
  AssertSqlCountEquals "SELECT COUNT(*) AS CountValue FROM game_timers WHERE game_id = ${GameId};" "1" "Game timer should exist after match starts"
  EndTest
}

TestLeaveAndCleanupMatch() {
  BeginTest "Leave cleans up match when empty"
  PrintInfo "Calling reducer: test_leave_game_and_cleanup_match_if_empty"
  SpacetimeCall "test_leave_game_and_cleanup_match_if_empty" >/dev/null

  AssertSqlCountEquals "magician" "0" "Leave should remove magician"
  AssertSqlCountEquals "game" "0" "Match should be deleted when last player leaves"
  AssertSqlCountEquals "game_timers" "0" "Game timers should be deleted on match end"
  AssertSqlCountEquals "move_all_magicians" "0" "move_all_magicians timer should be deleted on match end"
  AssertSqlCountEquals "handle_magician_timers_timer" "0" "handle_magician_timers_timer timer should be deleted on match end"
  AssertSqlCountEquals "handle_magician_stateless_timers_timer" "0" "handle_magician_stateless_timers_timer timer should be deleted on match end"
  AssertSqlCountEquals "gravity_magician" "0" "gravity_magician timer should be deleted on match end"
  AssertSqlCountEquals "player_effects_table_timer" "0" "player_effects_table_timer timer should be deleted on match end"
  EndTest
}

RunAllTests() {
  BeginSuiteTimer

  PrintHeader "Running tests against database: $(Bold "$SpacetimeDatabaseName")"
  PrintLine ""

  EnableUnitTestMode
  AssertInitWorked
  TestJoinAndStartSinglePlayerMatch
  TestLeaveAndCleanupMatch
  DisableUnitTestMode

  local ElapsedSeconds
  ElapsedSeconds="$(GetElapsedSeconds)"

  PrintHeader "$(Green "All tests passed.")"
  PrintLine "Summary: ${TestCountPassed}/${TestCountTotal} tests passed in ${ElapsedSeconds}s"
  PrintLine ""
}

RunAllTests
