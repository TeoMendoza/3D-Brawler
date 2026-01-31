#!/usr/bin/env bash
set -euo pipefail

DatabaseName="bash"
LocalServerUrl="http://127.0.0.1:3000"

RunUnitTests="false"
DeployToSelfHost="false"

for Argument in "$@"; do
  case "$Argument" in
    --unit) RunUnitTests="true" ;;
    --deploy) DeployToSelfHost="true" ;;
    *)
      echo "Unknown argument: $Argument"
      echo "Valid arguments:"
      echo "  --unit"
      echo "  --deploy"
      exit 1
      ;;
  esac
done

if [ "$DeployToSelfHost" = "true" ]; then
  RunUnitTests="true"
fi

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

PromptYesNo() {
  local PromptText="$1"
  local Response
  printf "%s [y/N]: " "$PromptText"
  read -r Response || true
  case "$Response" in
    y|Y|yes|YES|Yes) return 0 ;;
    *) return 1 ;;
  esac
}

ConfirmSelfHostDeployOrExit() {
  PrintBlankLine
  echo "Self-host deployment confirmation"
  echo "  Target server: self-host"
  echo "  Target database: $DatabaseName"
  echo "  Action: publish module and DELETE ALL existing data"
  PrintBlankLine
  if ! PromptYesNo "Proceed with self-host deploy?"; then
    echo "Aborted."
    exit 1
  fi
  PrintBlankLine
}

PublishDatabaseLocal() {
  local TargetDatabaseName="$1"
  RunQuietInDir "Publishing to LOCAL DB '$TargetDatabaseName'..." "$SpacetimeDbDirectory" \
    bash -lc "printf 'y\n' | spacetime publish --server 'local' '$TargetDatabaseName' --delete-data"
  echo "  Published: local/$TargetDatabaseName"
}

PublishDatabaseSelfHost() {
  local TargetDatabaseName="$1"
  RunQuietInDir "Publishing to SELF-HOST DB '$TargetDatabaseName'..." "$SpacetimeDbDirectory" \
    bash -lc "printf 'y\n' | spacetime publish --server 'self-host' '$TargetDatabaseName' --delete-data"
  echo "  Published: self-host/$TargetDatabaseName"
}

DeleteDatabaseLocal() {
  local TargetDatabaseName="$1"
  RunQuietInDir "Deleting DB '$TargetDatabaseName' on 'local'..." "$SpacetimeDbDirectory" \
    bash -lc "printf 'y\n' | spacetime delete --server 'local' '$TargetDatabaseName'"
  echo "  Deleted: local/$TargetDatabaseName"
}

RunTestsAgainstDatabase() {
  local TargetDatabaseName="$1"
  echo "Step 4: run unit/integration tests against '$TargetDatabaseName'"
  (
    export SpacetimeDatabaseName="$TargetDatabaseName"
    bash "$UnitTestsScriptPath"
  )
}

StartSelfHostDocker() {
  local DockerDirectory
  DockerDirectory="$(cd "$RepoRootDirectory/.." && cd "Docker" && pwd)"
  RunQuietInDir "Starting self-host Docker Compose..." "$DockerDirectory" \
    docker compose up -d
  echo "  Self-host Docker Compose started"
}

PrintBlankLine
echo "Configuration"
echo "  Repo: $RepoRootDirectory"
echo "  Local server: $LocalServerUrl"
echo "  Run unit tests: $RunUnitTests"
echo "  Deploy to self-host: $DeployToSelfHost"
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
  PublishDatabaseLocal "$TestDatabaseName"
  PrintBlankLine

  RunTestsAgainstDatabase "$TestDatabaseName"
  PrintBlankLine

  echo "Step 5: delete temporary local test DB '$TestDatabaseName'"
  DeleteDatabaseLocal "$TestDatabaseName"
  PrintBlankLine

  if [ "$DeployToSelfHost" = "true" ]; then
    echo "Step 6: start self-host Docker Compose"
    StartSelfHostDocker
    PrintBlankLine

    echo "Step 7: confirm self-host deploy"
    ConfirmSelfHostDeployOrExit

    echo "Step 8: publish to SELF-HOST real DB '$DatabaseName'"
    PublishDatabaseSelfHost "$DatabaseName"
    PrintBlankLine
  else
    echo "Step 6: publish to LOCAL real DB '$DatabaseName'"
    PublishDatabaseLocal "$DatabaseName"
    PrintBlankLine
  fi
else
  echo "Step 3: publish to local DB '$DatabaseName'"
  PublishDatabaseLocal "$DatabaseName"
  PrintBlankLine
fi

echo "Done."
PrintBlankLine
