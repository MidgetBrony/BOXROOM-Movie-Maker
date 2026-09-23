$ErrorActionPreference = 'Stop'

$assetName = 'ffmpeg-n9.0-latest-win64-gpl-9.0.zip'
$expectedSha256 = 'acbfc07e39f3ab379be41471b4e1c383aedee7d62686dd795240f17f07e09abe'
$releaseUrl = "https://github.com/BtbN/FFmpeg-Builds/releases/download/autobuild-2026-09-23-14-55/$assetName"
$projectRoot = Split-Path -Parent $PSScriptRoot
$destination = Join-Path $projectRoot 'ThirdParty\FFmpeg\win-x64'
$temporary = Join-Path ([IO.Path]::GetTempPath()) "boxroom-ffmpeg-$([Guid]::NewGuid().ToString('N'))"

try {
    New-Item -ItemType Directory -Path $temporary | Out-Null
    New-Item -ItemType Directory -Force -Path $destination | Out-Null
    $archive = Join-Path $temporary $assetName
    Invoke-WebRequest -Uri $releaseUrl -OutFile $archive

    $actualSha256 = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($actualSha256 -ne $expectedSha256) {
        throw "FFmpeg archive checksum mismatch. Expected $expectedSha256 but received $actualSha256."
    }

    Expand-Archive -LiteralPath $archive -DestinationPath $temporary
    $package = Join-Path $temporary 'ffmpeg-n9.0-latest-win64-gpl-9.0'
    Copy-Item -LiteralPath (Join-Path $package 'bin\ffmpeg.exe') -Destination (Join-Path $destination 'ffmpeg.exe') -Force
    Copy-Item -LiteralPath (Join-Path $package 'LICENSE.txt') -Destination (Join-Path $destination 'LICENSE.txt') -Force
    Write-Host 'Verified FFmpeg n9.0.2-3-ga5923073bf-20260923 for Windows.'
}
finally {
    if (Test-Path -LiteralPath $temporary) {
        Remove-Item -LiteralPath $temporary -Recurse -Force
    }
}
