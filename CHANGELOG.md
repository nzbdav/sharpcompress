# Changelog

## [0.50.1](https://github.com/nzbdav/sharpcompress/compare/v0.50.0...v0.50.1) (2026-07-13)


### Bug Fixes

* **io:** finish closed-issue audit gaps for ReadExact and RAR coverage ([896b617](https://github.com/nzbdav/sharpcompress/commit/896b6176d3c02002c18a1a30e7d9f95e106dabee))
* **io:** finish closed-issue audit gaps for ReadExact and RAR coverage ([556ae0c](https://github.com/nzbdav/sharpcompress/commit/556ae0ce02911765c1cd2109099ccff04be1bd04))
* **sevenzip:** validate signature and surface header CRC errors ([e29b4b8](https://github.com/nzbdav/sharpcompress/commit/e29b4b85f165105c17b660bc38f63993852d9311))
* **sevenzip:** validate signature and surface header CRC errors ([be65add](https://github.com/nzbdav/sharpcompress/commit/be65add21f41514bd232854c54d5fef3569dd85a))
* **stream:** make Rewind report failure for callers to handle ([02611cc](https://github.com/nzbdav/sharpcompress/commit/02611cced311059e037fd6b7bd09f9e6b167e6fc))
* **stream:** make Rewind report failure for callers to handle ([787eec9](https://github.com/nzbdav/sharpcompress/commit/787eec926d3e319937d8e7fcacbe151d551612b0))
* **utility:** validate DOS date components without swallowing exceptions ([402036e](https://github.com/nzbdav/sharpcompress/commit/402036eb13e4c267eb970538492f435bc2385efe))
* **utility:** validate DOS date components without swallowing exceptions ([b2b1a7c](https://github.com/nzbdav/sharpcompress/commit/b2b1a7cf1e182f2487d57dba1d876ed92851abbb))
* **xz:** verify index records, index CRC32, and footer ([786b6b6](https://github.com/nzbdav/sharpcompress/commit/786b6b6ed8b43686a7d6d2d52a0d2056260fed49))
* **xz:** verify index records, index CRC32, and footer against decoded blocks ([9d85648](https://github.com/nzbdav/sharpcompress/commit/9d856483d851a99c2cac3fdc2513473393d34781)), closes [#16](https://github.com/nzbdav/sharpcompress/issues/16)
* **zip:** clear CodeQL ECB alert for WinZip AES-CTR ([eb3be7c](https://github.com/nzbdav/sharpcompress/commit/eb3be7ce530b1ac275a8942cf54d5686b367ce75))
* **zip:** clear CodeQL ECB alert for WinZip AES-CTR ([e144c71](https://github.com/nzbdav/sharpcompress/commit/e144c715418dc38d2d7c21e69c8389b6a1412238))
* **zip:** harden ShrinkStream allocation and Stream contract ([347bd0b](https://github.com/nzbdav/sharpcompress/commit/347bd0b05104a6db144dfc0e77096da36213f4c3))
* **zip:** harden ShrinkStream allocation and Stream contract ([901d9c8](https://github.com/nzbdav/sharpcompress/commit/901d9c8e8beba14b51539eeee9968908e1073cc6))
* **zstandard:** stop JobThreadPool from swallowing job exceptions ([2c566dd](https://github.com/nzbdav/sharpcompress/commit/2c566dd4a3b1addab1b4cbe6a3ec5d1468ba4df1))
* **zstandard:** stop JobThreadPool from swallowing job exceptions ([eaeb713](https://github.com/nzbdav/sharpcompress/commit/eaeb7133a20caaea7ac8ab04cef541d4e71ea5fc)), closes [#47](https://github.com/nzbdav/sharpcompress/issues/47)

## [0.50.0](https://github.com/nzbdav/sharpcompress/compare/v0.49.1...v0.50.0) (2026-07-13)


### ⚠ BREAKING CHANGES

* **net10:** removes public AsyncEnumerableEx, EnumerableExtensions, and AsyncEnumerableExtensions polyfill types from the SharpCompress namespace. Use System.Linq.AsyncEnumerable instead.
* **runtime:** target .NET 10 only and publish as NzbDav.SharpCompress

### Features

* **runtime:** target .NET 10 only and publish as NzbDav.SharpCompress ([35130b6](https://github.com/nzbdav/sharpcompress/commit/35130b64c52c52ff0f8c5f807791f66aa5a6dc2f))


### Bug Fixes

* change default reader options in OpenArchive method for FileInfo ([d99b406](https://github.com/nzbdav/sharpcompress/commit/d99b406e09d097ca82e701b1b39bab4860875d4d))
* Change LeaveStreamOpen default from true to false ([72b81b8](https://github.com/nzbdav/sharpcompress/commit/72b81b86091e2c72f767495cdaf088cb2bbaa0df))
* Change LeaveStreamOpen default from true to false ([37c0f5a](https://github.com/nzbdav/sharpcompress/commit/37c0f5aec575f7dcdc06e76f75e9ca9707bbf0ee))
* **deps:** Bump Microsoft.VisualStudio.Threading.Analyzers from 17.14.15 to 18.7.23 ([d8136c8](https://github.com/nzbdav/sharpcompress/commit/d8136c8cc641cab2864d841827b2632846454663))
* **deps:** Bump Microsoft.VisualStudio.Threading.Analyzers from 17.14.15 to 18.7.23 ([885a8ce](https://github.com/nzbdav/sharpcompress/commit/885a8cea6406cf6d4158b7ddd0fe20a5861b9dfc))
* **deps:** bump NuGet/login from ebc737b6fc418a6ca0073cf116ec8dc156d8b81e to 8d196754b4036150537f80ac539e15c2f1028841 in the github-actions group ([b11ad2f](https://github.com/nzbdav/sharpcompress/commit/b11ad2f0ebdd09afcae5709af67f4b90421cce26))
* **deps:** bump NuGet/login in the github-actions group ([fc7ee4a](https://github.com/nzbdav/sharpcompress/commit/fc7ee4aff243f20a2da17a81d65ed5077aa20351))
* **deps:** Bump the nuget-minor-and-patch group with 1 update ([0b586cc](https://github.com/nzbdav/sharpcompress/commit/0b586cc63e81c3562477cab97dc6d311edd2a147))
* **deps:** Bump the nuget-minor-and-patch group with 1 update ([ec77678](https://github.com/nzbdav/sharpcompress/commit/ec7767839dd98badc7219a97196570f28338b3db))
* **deps:** restore SDK lock versions and fix VSTHRD103 findings ([f5ac992](https://github.com/nzbdav/sharpcompress/commit/f5ac992b708a0a83a17ae88abe1c8a33767a5684))
* **docs:** clarify stream handling rules and ownership in AGENTS.md ([ea95045](https://github.com/nzbdav/sharpcompress/commit/ea950457f51b4ac8f076bf4110f15f0e354f963b))
* increase RewindableBufferSize to 160KB to cover ZStandard worst-case first block ([d56b676](https://github.com/nzbdav/sharpcompress/commit/d56b676ee94e2c177ffd9e89f79b0feed243013f))
* **rar:** harden EOF, sync crypto, and concurrent unpack sharing ([722c1f3](https://github.com/nzbdav/sharpcompress/commit/722c1f36c51a19405b8aabafb20ebf4d78277703))
* **zip,tar:** implement OpenEntryStreamAsync for writable entries ([10178a1](https://github.com/nzbdav/sharpcompress/commit/10178a159450fff73c73eeabf2f9bc9a23f8f478))
* **zip:** stop overriding LeaveStreamOpen in OpenArchive ([23840b7](https://github.com/nzbdav/sharpcompress/commit/23840b7a0ff0f7eb8048249c75fa038ab14490cd))


### Miscellaneous Chores

* **net10:** modernize for .NET 10 and remove dead code ([87018a4](https://github.com/nzbdav/sharpcompress/commit/87018a422481a22cf0fd3811eb1a7080a6c911f5)), closes [#33](https://github.com/nzbdav/sharpcompress/issues/33) [#34](https://github.com/nzbdav/sharpcompress/issues/34) [#35](https://github.com/nzbdav/sharpcompress/issues/35) [#36](https://github.com/nzbdav/sharpcompress/issues/36) [#37](https://github.com/nzbdav/sharpcompress/issues/37) [#38](https://github.com/nzbdav/sharpcompress/issues/38) [#39](https://github.com/nzbdav/sharpcompress/issues/39) [#40](https://github.com/nzbdav/sharpcompress/issues/40) [#9](https://github.com/nzbdav/sharpcompress/issues/9)
* **release:** force first package release as 0.50.0 ([b3eff60](https://github.com/nzbdav/sharpcompress/commit/b3eff607d6cd8cb8db93fb5dac009c352b8ad0f5))

## Changelog

This repository is the [nzbdav](https://github.com/nzbdav) fork of
[adamhathcock/sharpcompress](https://github.com/adamhathcock/sharpcompress),
published as `NzbDav.SharpCompress`. Release Please maintains the version history
below. For changes prior to the fork package identity, see the upstream project.
