#!/usr/bin/env bash
set -euo pipefail

DatabaseName="bash"
LocalServerUrl="http://127.0.0.1:3000"

RunUnitTests="false"
UseMainCloud="false"

for Argument in "$@"; do
  case "$Argument" in
    --unit) RunUnitTests="true" ;;
    --main) UseMainCloud="true" ;;
    *)
      echo "Unknown argument: $Argument"
      echo "Valid arguments:"
      echo "  --unit"
      echo "  --main"
      exit 1
      ;;
  esac
done

ScriptDirectory="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
RepoRootDirectory="$ScriptDirectory"

SpacetimeDbDirectory="$RepoRootDirectory/server-rust/spacetimedb"
BindingsOutputDirectory="$RepoRootDirectory/client-unity/Assets/autogen"
UnitTestsScriptPath="$RepoRootDirectory/scripts/UnitTests.sh"

TestDatabaseName="${DatabaseName}-test"

if [ ! -d "$SpacetimeDbDirectory" ]; then
  echo "Error: Expected directory not found: $SpacetimeDbDirectory"
  exit 1
fi

if [ ! -f "$UnitTestsScriptPath" ]; then
  echo "Error: Expected unit test file not found: $UnitTestsScriptPath"
  exit 1
fi

IsServerUp() {
  local HttpCode
  HttpCode="$(curl -s -o /dev/null -w "%{http_code}" "$LocalServerUrl/" || true)"
  [ "$HttpCode" != "000" ]
}

if ! IsServerUp; then
  echo "Error: Local SpacetimeDB is not running at $LocalServerUrl"
  echo "Start it in another terminal from: $RepoRootDirectory"
  echo "Command: spacetime start"
  exit 1
fi

TempLogFile="$(mktemp)"
cleanup() { rm -f "$TempLogFile"; }
trap cleanup EXIT

PrintBlankLine() { echo ""; }

RunQuietInDir() {
  local StepLabel="$1"
  local WorkingDirectory="$2"
  shift 2

  echo "$StepLabel"
  : > "$TempLogFile"
  (
    cd "$WorkingDirectory"
    "$@" >"$TempLogFile" 2>&1
  ) || {
    PrintBlankLine
    echo "Error: Step failed. Full output:"
    echo "------------------------------------------------------------"
    cat "$TempLogFile" || true
    echo "------------------------------------------------------------"
    exit 1
  }
}

PublishDatabase() {
  local ServerNickname="$1"
  local TargetDatabaseName="$2"

  if [ "$ServerNickname" = "maincloud" ]; then
    RunQuietInDir "Publishing to MAINCLOUD DB '$TargetDatabaseName'..." "$SpacetimeDbDirectory" \
      spacetime publish --server "$ServerNickname" "$TargetDatabaseName" --delete-data
  else
    RunQuietInDir "Publishing to LOCAL DB '$TargetDatabaseName'..." "$SpacetimeDbDirectory" \
      bash -lc "printf 'y\n' | spacetime publish --server '$ServerNickname' '$TargetDatabaseName' --delete-data"
  fi

  echo "  Published: $ServerNickname/$TargetDatabaseName"
}

DeleteDatabase() {
  local ServerNickname="$1"
  local TargetDatabaseName="$2"

  RunQuietInDir "Deleting DB '$TargetDatabaseName' on '$ServerNickname'..." "$SpacetimeDbDirectory" \
    bash -lc "printf 'y\n' | spacetime delete --server '$ServerNickname' '$TargetDatabaseName'"

  echo "  Deleted: $ServerNickname/$TargetDatabaseName"
}

RunTestsAgainstDatabase() {
  local TargetDatabaseName="$1"
  echo "Step 4: run unit/integration tests against '$TargetDatabaseName'"
  (
    export SpacetimeDatabaseName="$TargetDatabaseName"
    bash "$UnitTestsScriptPath"
  )
}

PrintBlankLine
echo "Configuration"
echo "  Repo: $RepoRootDirectory"
echo "  Local server: $LocalServerUrl"
echo "  Run unit tests: $RunUnitTests"
echo "  Publish target: $(if [ "$UseMainCloud" = "true" ]; then echo "maincloud"; else echo "local"; fi)"
echo "  Real DB name: $DatabaseName"
if [ "$RunUnitTests" = "true" ]; then
  echo "  Test DB name: $TestDatabaseName"
fi
PrintBlankLine

echo "Step 1: spacetime build"
RunQuietInDir "  Running..." "$SpacetimeDbDirectory" spacetime build
echo "  Built"
PrintBlankLine

echo "Step 2: spacetime generate (C# bindings)"
mkdir -p "$BindingsOutputDirectory"
RunQuietInDir "  Running..." "$SpacetimeDbDirectory" \
  spacetime generate --lang csharp --out-dir "$BindingsOutputDirectory"
echo "  Generated bindings -> $BindingsOutputDirectory"
PrintBlankLine

if [ "$RunUnitTests" = "true" ]; then
  echo "Step 3: publish to temporary local test DB '$TestDatabaseName'"
  PublishDatabase "local" "$TestDatabaseName"
  PrintBlankLine

  echo "Step 4: run unit/integration tests against '$TestDatabaseName'"
  (
    export SpacetimeDatabaseName="$TestDatabaseName"
    bash "$UnitTestsScriptPath"
  )
  PrintBlankLine

  echo "Step 5: delete temporary local test DB '$TestDatabaseName'"
  DeleteDatabase "local" "$TestDatabaseName"
  PrintBlankLine

  if [ "$UseMainCloud" = "true" ]; then
    echo "Step 6: publish to MAINCLOUD real DB '$DatabaseName' (manual confirmation required)"
    echo "  WARNING: This will destroy ALL data in MAINCLOUD DB '$DatabaseName'."
    PrintBlankLine
    PublishDatabase "maincloud" "$DatabaseName"
  else
    echo "Step 6: publish to LOCAL real DB '$DatabaseName'"
    PublishDatabase "local" "$DatabaseName"
  fi
  PrintBlankLine
else
  echo "Step 3: publish to local DB '$DatabaseName'"
  PublishDatabase "local" "$DatabaseName"
  PrintBlankLine
fi

echo "Done."
PrintBlankLine
