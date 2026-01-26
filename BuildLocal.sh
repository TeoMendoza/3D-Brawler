#!/usr/bin/env bash
set -euo pipefail

RequirePublishConfirmation="false"
DatabaseName="3dbrawler"
LocalServerUrl="http://127.0.0.1:3000"

for Argument in "$@"; do
  case "$Argument" in
    --RequirePublishConfirmation)
      RequirePublishConfirmation="true"
      ;;
    --DatabaseName=*)
      DatabaseName="${Argument#*=}"
      ;;
    *)
      echo "Unknown argument: $Argument"
      echo "Valid arguments:"
      echo "  --RequirePublishConfirmation"
      echo "  --DatabaseName=3dbrawler"
      exit 1
      ;;
  esac
done

ScriptDirectory="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
RepoRootDirectory="$ScriptDirectory"

SpacetimeDbDirectory="$RepoRootDirectory/server-rust/spacetimedb"
UnityProjectDirectory="$RepoRootDirectory/client-unity"
BindingsOutputDirectory="$UnityProjectDirectory/Assets/autogen"

if [ ! -d "$SpacetimeDbDirectory" ]; then
  echo "Error: Expected directory not found: $SpacetimeDbDirectory"
  exit 1
fi

if [ ! -d "$UnityProjectDirectory" ]; then
  echo "Error: Expected directory not found: $UnityProjectDirectory"
  exit 1
fi

IsServerUp() {
  HttpCode="$(curl -s -o /dev/null -w "%{http_code}" "$LocalServerUrl/" || true)"
  [ "$HttpCode" != "000" ]
}

if ! IsServerUp; then
  echo "Error: Local SpacetimeDB is not running at $LocalServerUrl"
  echo "Start it in another terminal from: $UnityProjectDirectory"
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

echo "Step 3: spacetime publish to local DB '$DatabaseName'"
(
  cd "$SpacetimeDbDirectory"
  if [ "$RequirePublishConfirmation" = "true" ]; then
    spacetime publish --server local "$DatabaseName" --delete-data
  else
    printf "y\n" | spacetime publish --server local "$DatabaseName" --delete-data
  fi
)

echo "Done."
