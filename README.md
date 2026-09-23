# BOXROOM Movie Maker

Got a video that will not play properly in BOXROOM? BOXROOM Movie Maker prepares it for the game with a simple drag, drop, and click.

It works with individual videos or entire folders on Windows and Linux. FFmpeg is already included in the downloads, so there is nothing else to install.

## Download

Get the latest version from the [GitHub Releases page](https://github.com/MidgetBrony/BOXROOM-Movie-Maker/releases/latest).

- Choose **Windows x64 AIO** for a regular 64-bit Windows PC.
- Choose **Linux x64 AIO** for a 64-bit Linux PC.
- The **Source** download is only for developers who want to inspect or build the program.

Extract the complete download before opening it. Keep the application and FFmpeg together in the extracted folder.

## How to use it

1. Open **BOXROOM Movie Maker**.
2. Drop one or more videos into the window, or drop a folder to find every supported video inside it.
3. Select **Make BOXROOM ready**.
4. Wait for each movie to show **Ready for BOXROOM**.

The converted movie is saved beside the original with `_BOXROOM` added to its name:

```text
My Movie.mkv
My Movie_BOXROOM.mp4
```

Your original videos are never changed or deleted. If a converted filename already exists, the program safely adds `_2`, `_3`, and so on.

## Supported videos

BOXROOM Movie Maker accepts:

- MP4, MKV, MOV and AVI
- WebM and M4V
- MPG and MPEG
- WMV and FLV
- TS, MTS and M2TS
- OGV

Folders are searched automatically, including their subfolders.

## Windows

Extract the ZIP and open **BOXROOM Movie Maker.exe**.

The Windows download has a tidy AIO layout containing the application, `ffmpeg.exe`, the README, and a `Licences` folder. The Avalonia and .NET components are contained inside the application rather than appearing as hundreds of loose files.

## Linux

Extract the archive and run:

```shell
./BoxroomMovieMaker
```

Executable permissions are already stored in the Linux archive. If your archive program removes them, restore them with:

```shell
chmod +x BoxroomMovieMaker ffmpeg
```

## If something goes wrong

- Keep `ffmpeg.exe` or `ffmpeg` beside BOXROOM Movie Maker.
- Make sure you extracted the complete download instead of opening the application from inside the archive.
- Check that you can write files to the folder containing the original video.
- Expand **Conversion log** inside the program to see the FFmpeg error for a failed movie.
- Use **Choose FFmpeg** if you deliberately moved FFmpeg somewhere else.

## Privacy

All conversion happens locally on your computer. Videos are not uploaded anywhere by BOXROOM Movie Maker.

## FFmpeg and licences

BOXROOM Movie Maker runs FFmpeg as a separate command-line program. The included FFmpeg build uses GPL-covered components such as `libx264`.

Open **About & licences** inside the application for its exact FFmpeg version, copyright, licence, source revision, and build information. The complete GPLv3 text and third-party notice are also included in the packaged `Licences` folder.

<details>
<summary><strong>Building from source</strong></summary>

Install the .NET 10 SDK. From a source-only checkout, fetch the verified FFmpeg build first:

```powershell
.\scripts\fetch-ffmpeg-windows.ps1
```

or on Linux:

```shell
./scripts/fetch-ffmpeg-linux.sh
```

Restore and build the application:

```shell
dotnet restore
dotnet build -c Release --no-restore
```

Create the clean single-file packages:

```shell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:DebugType=None -p:DebugSymbols=false
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:DebugType=None -p:DebugSymbols=false
```

The exact third-party revisions and checksums are documented in [`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md).

</details>
