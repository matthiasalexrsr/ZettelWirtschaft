#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

extract_zip() {
  local archive="$1"
  local target="$2"

  if [[ -d "$target" ]]; then
    echo "[skip] $target already exists"
    return
  fi

  echo "[info] extracting $archive -> $target"
  mkdir -p "$target"
  unzip -q "$ROOT_DIR/$archive" -d "$target"
}

require_file() {
  local path="$1"
  if [[ ! -f "$path" ]]; then
    echo "[error] missing expected file: $path" >&2
    exit 1
  fi
}

extract_zip "DocuDeskStarter_v6.zip" "$ROOT_DIR/unpacked/DocuDeskStarter"
extract_zip "DocuDesk_V1_Starter_v6.zip" "$ROOT_DIR/unpacked/DocuDeskV1"
extract_zip "DocuDesk_V1_Starter_v6_fresh.zip" "$ROOT_DIR/unpacked/DocuDeskV1_fresh"

require_file "$ROOT_DIR/unpacked/DocuDeskStarter/DocuDeskStarter/DocuDesk.sln"
require_file "$ROOT_DIR/unpacked/DocuDeskV1/DocuDeskV1_v6/DocuDesk.sln"
require_file "$ROOT_DIR/unpacked/DocuDeskV1_fresh/DocuDeskV1_v6/DocuDesk.sln"

echo "[ok] all archives unpacked and validated"
echo "[next] open unpacked/DocuDeskV1/DocuDeskV1_v6/DocuDesk.sln on Windows with .NET 10 + WPF workload"
