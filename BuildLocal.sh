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
  HttpCode="$(curl -s -o /dev/null -w "%{http_code}" "$LocalServerUrl/" || true)"
  [ "$HttpCode" != "000" ]
}

if ! IsServerUp; then
  echo "Error: Local SpacetimeDB is not running at $LocalServerUrl"
  echo "Start it in another terminal from: $RepoRootDirectory"
  echo "Command: spacetime start"
  exit 1
fi

echo "Step 1: spacetime build"
(
  cd "$SpacetimeDbDirectory"
  spacetime build
)

echo "Step 2: spacetime generate (C# bindings)"
mkdir -p "$BindingsOutputDirectory"
(
  cd "$SpacetimeDbDirectory"
  spacetime generate --lang csharp --out-dir "$BindingsOutputDirectory"
)

PublishDatabase() {
  local ServerNickname="$1"
  local TargetDatabaseName="$2"

  (
    cd "$SpacetimeDbDirectory"
    if [ "$ServerNickname" = "maincloud" ]; then
      spacetime publish --server "$ServerNickname" "$TargetDatabaseName" --delete-data
    else
      printf "y\n" | spacetime publish --server "$ServerNickname" "$TargetDatabaseName" --delete-data
    fi
  )
}

DeleteDatabase() {
  local ServerNickname="$1"
  local TargetDatabaseName="$2"

  (
    cd "$SpacetimeDbDirectory"
    printf "y\n" | spacetime delete --server "$ServerNickname" "$TargetDatabaseName"
  )
}

RunTestsAgainstDatabase() {
  local TargetDatabaseName="$1"
  (
    export SpacetimeDatabaseName="$TargetDatabaseName"
    bash "$UnitTestsScriptPath"
  )
}

if [ "$RunUnitTests" = "true" ]; then
  echo "Step 3: publish to temporary local test DB '$TestDatabaseName'"
  PublishDatabase "local" "$TestDatabaseName"

  echo "Step 4: run unit/integration tests against '$TestDatabaseName'"
  RunTestsAgainstDatabase "$TestDatabaseName"

  echo "Step 5: delete temporary local test DB '$TestDatabaseName'"
  DeleteDatabase "local" "$TestDatabaseName"

  if [ "$UseMainCloud" = "true" ]; then
    echo "Step 6: publish to MAINCLOUD real DB '$DatabaseName' (manual confirmation required)"
    PublishDatabase "maincloud" "$DatabaseName"
  else
    echo "Step 6: publish to LOCAL real DB '$DatabaseName'"
    PublishDatabase "local" "$DatabaseName"
  fi
else
  echo "Step 3: publish to local DB '$DatabaseName'"
  PublishDatabase "local" "$DatabaseName"
fi

echo "Done."
