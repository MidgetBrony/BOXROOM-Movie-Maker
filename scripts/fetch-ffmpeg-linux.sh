#!/usr/bin/env bash
set -euo pipefail

asset_name='ffmpeg-n9.0-latest-linux64-gpl-9.0.tar.xz'
expected_sha256='545b37cbaa22a9a83edcc9615993044b8a59a6612fa39387348da12b395cf857'
release_url="https://github.com/BtbN/FFmpeg-Builds/releases/download/autobuild-2026-09-23-14-55/${asset_name}"
script_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
project_root="$(dirname -- "${script_dir}")"
destination="${project_root}/ThirdParty/FFmpeg/linux-x64"
temporary="$(mktemp -d)"
trap 'rm -rf -- "${temporary}"' EXIT

mkdir -p -- "${destination}"
curl --fail --location --retry 3 --output "${temporary}/${asset_name}" "${release_url}"
echo "${expected_sha256}  ${temporary}/${asset_name}" | sha256sum --check --status

tar -xJf "${temporary}/${asset_name}" -C "${temporary}"
package="${temporary}/ffmpeg-n9.0-latest-linux64-gpl-9.0"
install -m 0755 "${package}/bin/ffmpeg" "${destination}/ffmpeg"
install -m 0644 "${package}/LICENSE.txt" "${destination}/LICENSE.txt"
echo 'Verified FFmpeg n9.0.2-3-ga5923073bf-20260923 for Linux.'
