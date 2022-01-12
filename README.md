# IronCompress

<img src="managed/IronCompress/icon.png" width=80 height=80 align="left"/> .NET buffer compression library supporting all the most popular compression algorithms like gzip, snappy, brotli, zstd,  etc. See the [latest documentation](https://www.aloneguid.uk/projects/ironcompress/) for up to date specifications. This page is only hosting source code.

You will need more or less recent C++ compiler, CMake and .NET 6 to build the code.


## Building

See [workflow file](.github/workflows/ci.yml) for building instructions. To develop locally, you might want to download the latest artifact from Actions output and put it into `native/bin` so you have binaries for all platforms.

