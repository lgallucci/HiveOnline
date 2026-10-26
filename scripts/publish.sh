#!/usr/bin/env bash
# Requires org.freedesktop.Platform and Sdk 24.08 (flatpak install flathub org.freedesktop.Sdk//24.08).
# Builds distributable packages into ./artifacts.
#   scripts/publish.sh windows   -> artifacts/HiveOnline-win-x64.zip (single HiveOnline.exe + Content)
#   scripts/publish.sh flatpak   -> artifacts/HiveOnline.flatpak (needs flatpak-builder)
#   scripts/publish.sh           -> both
#
# Windows code signing (optional; the exe is left unsigned if nothing is configured). Choose one:
#   Azure Artifact Signing (uses the cross-platform `sign` tool and your az login / service principal):
#     SIGN_AZURE_ENDPOINT   e.g. https://eus.codesigning.azure.net
#     SIGN_AZURE_ACCOUNT    Artifact Signing account name
#     SIGN_AZURE_PROFILE    certificate profile name
#   Certificate file (needs osslsigncode on Linux/macOS, or signtool on Windows via Git Bash):
#     SIGN_PFX_FILE         path to the .pfx
#     SIGN_PFX_PASSWORD     its password
#   Optional: SIGN_TIMESTAMP_URL (default http://timestamp.digicert.com), SIGN_DESCRIPTION
set -euo pipefail
cd "$(dirname "$0")/.."
target="${1:-all}"
mkdir -p artifacts

sign_windows_exe() {
  local exe="$1"
  local timestamp="${SIGN_TIMESTAMP_URL:-http://timestamp.digicert.com}"
  local description="${SIGN_DESCRIPTION:-Hive Online}"

  if [[ -n "${SIGN_AZURE_ENDPOINT:-}" ]]; then
    : "${SIGN_AZURE_ACCOUNT:?SIGN_AZURE_ACCOUNT is required}" "${SIGN_AZURE_PROFILE:?SIGN_AZURE_PROFILE is required}"
    dotnet tool update --global sign --prerelease >/dev/null
    export PATH="$PATH:$HOME/.dotnet/tools"
    sign code artifact-signing "$exe" \
      --artifact-signing-endpoint "$SIGN_AZURE_ENDPOINT" \
      --artifact-signing-account "$SIGN_AZURE_ACCOUNT" \
      --artifact-signing-certificate-profile "$SIGN_AZURE_PROFILE" \
      --timestamp-url "$timestamp" --description "$description" \
      --base-directory "$(dirname "$exe")"
  elif [[ -n "${SIGN_PFX_FILE:-}" ]]; then
    : "${SIGN_PFX_PASSWORD:?SIGN_PFX_PASSWORD is required}"
    if command -v osslsigncode >/dev/null; then
      osslsigncode sign -pkcs12 "$SIGN_PFX_FILE" -pass "$SIGN_PFX_PASSWORD" -n "$description" \
        -h sha256 -ts "$timestamp" -in "$exe" -out "$exe.signed"
      mv "$exe.signed" "$exe"
    elif command -v signtool >/dev/null; then
      signtool sign /f "$SIGN_PFX_FILE" /p "$SIGN_PFX_PASSWORD" /fd sha256 /tr "$timestamp" /td sha256 /d "$description" "$exe"
    else
      echo "SIGN_PFX_FILE is set but neither osslsigncode nor signtool was found" >&2
      exit 1
    fi
  else
    echo "Skipping code signing (no SIGN_* settings): Windows will show a SmartScreen warning."
    return
  fi
  echo "Signed $exe"
}

publish_windows() {
  rm -rf artifacts/win-x64
  dotnet publish HiveOnline -c Release -r win-x64 -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=None -o artifacts/win-x64
  sign_windows_exe artifacts/win-x64/HiveOnline.exe
  (cd artifacts/win-x64 && rm -f ../HiveOnline-win-x64.zip && zip -qr ../HiveOnline-win-x64.zip .)
  echo "Created artifacts/HiveOnline-win-x64.zip"
}

publish_flatpak() {
  rm -rf artifacts/linux-x64
  dotnet publish HiveOnline -c Release -r linux-x64 -p:DebugType=None -o artifacts/linux-x64
  # OSTree needs a filesystem with hardlink/permission support, so build outside the repo by default.
  work="${FLATPAK_WORK:-${TMPDIR:-/tmp}/hiveonline-flatpak}"
  flatpak-builder --force-clean --state-dir="$work/state" \
    --repo="$work/repo" "$work/build" flatpak/io.github.lgallucci.HiveOnline.yml
  flatpak build-bundle "$work/repo" artifacts/HiveOnline.flatpak io.github.lgallucci.HiveOnline
  echo "Created artifacts/HiveOnline.flatpak (install with: flatpak install --user artifacts/HiveOnline.flatpak)"
}

case "$target" in
  windows) publish_windows ;;
  flatpak) publish_flatpak ;;
  all) publish_windows; publish_flatpak ;;
  *) echo "usage: $0 [windows|flatpak]"; exit 1 ;;
esac
