# SharpCompress

> **nzbdav fork.** This repository is maintained by the
> [nzbdav](https://github.com/nzbdav) organization as
> [nzbdav/sharpcompress](https://github.com/nzbdav/sharpcompress), based on
> [adamhathcock/sharpcompress](https://github.com/adamhathcock/sharpcompress).
> The NuGet package is **`NzbDav.SharpCompress`** (namespaces remain
> `SharpCompress`). For the upstream package, continue using `SharpCompress` on
> NuGet.org.

SharpCompress is a compression library in pure C# for **.NET 10** that can unrar,
un7zip, unzip, untar, unbzip2, ungzip, unlzip, unxz, unzstd, unarc, unarj, unace,
and unlzw with forward-only reading and file random access APIs. Write support
for zip, tar, bzip2, gzip, lzip, zstandard compression streams, and 7zip
archives is implemented.

The major feature is support for non-seekable streams so large files can be
processed on the fly (i.e. download stream).

**NEW:** All I/O operations now support async/await for improved performance and
scalability. See the [USAGE.md](docs/USAGE.md#async-examples) for examples.

GitHub Actions Build -
[![CI](https://github.com/nzbdav/sharpcompress/actions/workflows/ci.yml/badge.svg)](https://github.com/nzbdav/sharpcompress/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/NzbDav.SharpCompress.svg)](https://www.nuget.org/packages/NzbDav.SharpCompress)

## Install

```bash
dotnet add package NzbDav.SharpCompress
```

## Need Help?

Post Issues on Github!

Check the [Supported Formats](docs/FORMATS.md), [API Reference](docs/API.md), and
[Basic Usage](docs/USAGE.md).

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md). Use scoped Conventional Commits so
Release Please can prepare versions and changelogs.

## Custom Compression Providers

If you need to swap out SharpCompress’s built-in codecs, the `Providers` property
(and `WithProviders(...)` extensions) on `ReaderOptions` and `WriterOptions` lets
you supply a `CompressionProviderRegistry`. The selected registry is used by
Reader/Writer APIs, Archive APIs, and async extraction paths, so the same
provider choice is applied consistently across open/read/write flows. The default
registry is already wired up, so customization is only necessary when you want to
plug in alternatives such as `SystemGZipCompressionProvider` or a third-party
`CompressionProvider`. See
[docs/USAGE.md#custom-compression-providers](docs/USAGE.md#custom-compression-providers)
for guided examples.

## Recommended Formats

In general, GZip (Deflate)/BZip2 (BZip)/LZip (LZMA) are recommended as the
simplicity of the formats lend to better long term archival as well as the
streamability. Tar is often used in conjunction for multiple files in a single
archive (e.g. `.tar.gz`)

Zip is okay, but it's a very hap-hazard format and the variation in headers and
implementations makes it hard to get correct. Uses Deflate by default but
supports a lot of compression methods.

RAR is not recommended as it's a proprietary format and the compression is closed
source. Use Tar/LZip for LZMA.

7Zip and XZ both are overly complicated. 7Zip does not support streamable
formats. XZ has known holes explained here:
(http://www.nongnu.org/lzip/xz_inadequate.html) Use Tar/LZip for LZMA compression
instead.

ZStandard is an efficient format that works well for streaming with a flexible
compression level to tweak the speed/performance trade off you are looking for.

## Notes

XZ implementation based on: https://github.com/sambott/XZ.NET by @sambott

XZ BCJ filters support contributed by Louis-Michel Bergeron, on behalf of aDolus
Technology Inc. - 2022

7Zip implementation based on: https://code.google.com/p/managed-lzma/

Zstandard implementation from: https://github.com/oleg-st/ZstdSharp

LICENSE
Copyright (c) 2000 - 2011 The Legion Of The Bouncy Castle
(http://www.bouncycastle.org)

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in the
Software without restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the
Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN
AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
