#!/usr/bin/env bash
set -euo pipefail

: "${SpacetimeDatabaseName:?Missing SpacetimeDatabaseName}"

SpacetimeSql() {
  local Query="$1"
  spacetime sql --anonymous --server local "$SpacetimeDatabaseName" "$Query"
}

SpacetimeCall() {
  local FunctionName="$1"
  local Arguments="${2:-}"
  spacetime call --anonymous --server local "$SpacetimeDatabaseName" "$FunctionName" "$Arguments" 
}

TestSmokeReducer() {
  SpacetimeCall "test_smoke"
}

RunAllTests() {
  echo "Running tests against database: $SpacetimeDatabaseName"

  TestSmokeReducer

  echo "All tests passed."
}

RunAllTests
