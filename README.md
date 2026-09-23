# BOXROOM Movie Maker 1.1.3

A small Avalonia desktop tool for preparing videos for BOXROOM and Unity's video player on Windows or Linux.

## What it does

- Accepts individual video files or an entire dropped/selected folder.
- Searches selected folders recursively for supported video files.
- Keeps every source file untouched.
- Saves each result beside the source as `name_BOXROOM.mp4`.
- Never overwrites an existing result; it adds `_2`, `_3`, and so on.
- Shows each movie's status, supports cancellation, and keeps a useful conversion log.

The conversion uses the developer-provided settings:

```text
ffmpeg -i "input.mp4" -c:v libx264 -pix_fmt yuv420p -c:a aac -movflags +faststart "output.mp4"
```

## FFmpeg

FFmpeg is included in the packaged Windows and Linux builds. The app finds it in any of these places:

1. The bundled `ffmpeg.exe` (Windows) or `ffmpeg` (Linux) beside the app.
2. A normal system `PATH` installation.
3. A file selected with **Choose FFmpeg**.

## Build

Install the .NET 10 SDK, then run:

```shell
dotnet restore
dotnet build -c Release --no-restore
```

Create clean single-file/AIO packages with:

```shell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:DebugType=None -p:DebugSymbols=false
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:DebugType=None -p:DebugSymbols=false
```

Platform publishing automatically places the matching FFmpeg binary and its GPL licence beside the app. The repository's `ThirdParty/FFmpeg` folder must be populated with the pinned binaries before publishing.

From a source-only checkout, fetch the exact verified build first:

```powershell
.\scripts\fetch-ffmpeg-windows.ps1
```

or on Linux:

```shell
./scripts/fetch-ffmpeg-linux.sh
```

## About and FFmpeg licensing

The packaged application includes FFmpeg `n9.0.2-3-ga5923073bf-20260923`, an unmodified BtbN static build configured with `--enable-gpl`, `--enable-version3`, and `--enable-libx264`.

The Windows AIO package contains only the single-file application, `ffmpeg.exe`, this README, and the `Licences` folder. Avalonia, .NET, and their native libraries are bundled inside the application executable.

Open **About & licences** inside the application for the exact version, copyright, licence, source revision, and build-script revision. The full GPLv3 text is included in each binary package. See `THIRD-PARTY-NOTICES.md` for source links and checksums.

## Notes

- Output video is H.264 (`libx264`) with `yuv420p` pixel format.
- Output audio is AAC.
- `+faststart` moves MP4 metadata to the beginning for more compatible playback.
- The app does not modify or delete the original video.
