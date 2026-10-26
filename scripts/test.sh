#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd -- "$(dirname -- "$0")/.." && pwd)"
cd "$ROOT"

LAYER="${1:-all}"
case "$LAYER" in
  all) CATEGORY="" ;;
  api) CATEGORY=Api ;;
  functional) CATEGORY=Functional ;;
  visual) CATEGORY=Visual ;;
  e2e) CATEGORY=E2E ;;
  *) echo "Usage: bash scripts/test.sh [all|api|functional|visual|e2e]" >&2; exit 2 ;;
esac

command -v dotnet >/dev/null || { echo "Install the .NET SDK specified in global.json first." >&2; exit 1; }
command -v pwsh >/dev/null || { echo "Install PowerShell 7 (pwsh) first." >&2; exit 1; }

dotnet restore --locked-mode
dotnet build --no-restore

BROWSER_NAME="${BROWSER:-chromium}"
case "$BROWSER_NAME" in
  chromium|firefox|webkit) ;;
  *) echo "BROWSER must be chromium, firefox or webkit." >&2; exit 2 ;;
esac

pwsh SnipeIt.Automation/bin/Debug/net10.0/playwright.ps1 install "$BROWSER_NAME"

ARGS=(
  --no-build
  --settings runsettings.runsettings
  --logger "trx;LogFileName=${LAYER}.trx"
)

if [[ -n "$CATEGORY" ]]; then
  ARGS+=(--filter "TestCategory=$CATEGORY")
fi

dotnet test "${ARGS[@]}" -- \
  Playwright.BrowserName="$BROWSER_NAME" \
  Playwright.LaunchOptions.Headless="${HEADLESS:-true}"
