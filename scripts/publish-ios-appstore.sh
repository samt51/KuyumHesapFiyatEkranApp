#!/usr/bin/env bash
set -euo pipefail

PROJECT="KuyumHesapFiyatEkran.UI/KuyumHesapFiyatEkran.UI.csproj"
FRAMEWORK="net9.0-ios"
CONFIGURATION="Release"
RUNTIME="ios-arm64"
BRAND="${BRAND:-SalihCetinkaya}"

if [[ -z "${CODESIGN_KEY:-}" ]]; then
  echo "CODESIGN_KEY is required. Example: Apple Distribution: Company Name (TEAMID)"
  exit 1
fi

if [[ -z "${CODESIGN_PROVISION:-}" ]]; then
  echo "CODESIGN_PROVISION is required. Use the App Store provisioning profile name."
  exit 1
fi

if [[ "${RUN_WORKLOAD_RESTORE:-0}" == "1" ]]; then
  dotnet workload restore "$PROJECT"
fi

dotnet publish "$PROJECT" \
  -f "$FRAMEWORK" \
  -c "$CONFIGURATION" \
  -p:Brand="$BRAND" \
  -p:RuntimeIdentifier="$RUNTIME" \
  -p:ArchiveOnBuild=true \
  -p:CodesignKey="$CODESIGN_KEY" \
  -p:CodesignProvision="$CODESIGN_PROVISION"
