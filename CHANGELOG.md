# Changelog

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
