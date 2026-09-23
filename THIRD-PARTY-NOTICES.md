# Third-party software notices

## FFmpeg

BOXROOM Movie Maker distributions include an unmodified FFmpeg command-line executable as a separate program.

- Version: `n9.0.2-3-ga5923073bf-20260923`
- FFmpeg source commit: `a5923073bfd8f25b7300d93af3f8e690174ebd30`
- Build provider: BtbN/FFmpeg-Builds
- Build scripts commit: `ccbffa4f85d0e8de5c135c69ebb10e4c14911fa9`
- Build configuration includes: `--enable-gpl --enable-version3 --enable-libx264`
- Copyright: © 2000–2026 the FFmpeg developers
- Licence for the bundled build: GNU General Public License version 3 or later

The complete GNU GPL version 3 text is distributed at `Licences/FFmpeg-GPLv3.txt` in each binary package. FFmpeg is not owned by BOXROOM Movie Maker or BOXROOM.

### Corresponding source and build information

- Exact FFmpeg source: https://github.com/FFmpeg/FFmpeg/tree/a5923073bfd8f25b7300d93af3f8e690174ebd30
- Source archive: https://github.com/FFmpeg/FFmpeg/archive/a5923073bfd8f25b7300d93af3f8e690174ebd30.tar.gz
- Exact build scripts: https://github.com/BtbN/FFmpeg-Builds/tree/ccbffa4f85d0e8de5c135c69ebb10e4c14911fa9
- Build-script archive: https://github.com/BtbN/FFmpeg-Builds/archive/ccbffa4f85d0e8de5c135c69ebb10e4c14911fa9.tar.gz
- FFmpeg licensing information: https://ffmpeg.org/legal.html

The BtbN build scripts identify and retrieve the source revisions for FFmpeg's statically linked dependencies and document how the binaries are reproduced.

### Original binary packages

The bundled executables were extracted without modification from the BtbN GPL static packages published on 23 September 2026.

- Windows archive: `ffmpeg-n9.0-latest-win64-gpl-9.0.zip`
  - SHA-256: `acbfc07e39f3ab379be41471b4e1c383aedee7d62686dd795240f17f07e09abe`
  - Extracted `ffmpeg.exe` SHA-256: `288ef71027b17e4d83d5d95777f14495fc59cc53e7b4c35637d0d68269c6d151`
- Linux archive: `ffmpeg-n9.0-latest-linux64-gpl-9.0.tar.xz`
  - SHA-256: `545b37cbaa22a9a83edcc9615993044b8a59a6612fa39387348da12b395cf857`
  - Extracted `ffmpeg` SHA-256: `c80a67e538a8b555a3ba12ce3b071866499498d6c3f20d0cde0f54799e3729f6`

Immutable binary release: https://github.com/BtbN/FFmpeg-Builds/releases/tag/autobuild-2026-09-23-14-55
