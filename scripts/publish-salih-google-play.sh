#!/usr/bin/env bash
set -euo pipefail
repo_dir="$(cd "$(dirname "$0")/.." && pwd)"
cd "$repo_dir"
dotnet_bin="${SALIH_DOTNET:-$repo_dir/.play-release/dotnet/dotnet}"
android_sdk="${SALIH_ANDROID_SDK:-$repo_dir/.play-release/android-sdk}"
java_sdk="${SALIH_JAVA_SDK:-$repo_dir/.play-release/jdk}"
export DOTNET_ROOT="$(dirname "$dotnet_bin")"
export PATH="$DOTNET_ROOT:$PATH"
"$dotnet_bin" publish KuyumHesapFiyatEkran.UI/KuyumHesapFiyatEkran.UI.csproj \
  -f net10.0-android36.0 -c Release \
  -p:Brand=SalihCetinkaya -p:GooglePlayRelease=true \
  -p:AndroidSdkDirectory="$android_sdk" -p:JavaSdkDirectory="$java_sdk" \
  -p:RuntimeIdentifier=android-arm64 -p:AndroidPackageFormats=aab
printf '\nBundle output: %s\n' "$repo_dir/KuyumHesapFiyatEkran.UI/bin/SalihCetinkaya/Release/net10.0-android36.0/android-arm64/publish"
printf 'The bundle must be signed with the verified upload key before Play Console upload.\n'
