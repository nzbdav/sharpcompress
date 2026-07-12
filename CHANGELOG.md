# Changelog

## [1.0.0](https://github.com/nzbdav/sharpcompress/compare/v0.50.0...v1.0.0) (2026-07-12)


### ⚠ BREAKING CHANGES

* **runtime:** target .NET 10 only and publish as NzbDav.SharpCompress

### Features

* **runtime:** target .NET 10 only and publish as NzbDav.SharpCompress ([35130b6](https://github.com/nzbdav/sharpcompress/commit/35130b64c52c52ff0f8c5f807791f66aa5a6dc2f))


### Bug Fixes

* change default reader options in OpenArchive method for FileInfo ([d99b406](https://github.com/nzbdav/sharpcompress/commit/d99b406e09d097ca82e701b1b39bab4860875d4d))
* Change LeaveStreamOpen default from true to false ([72b81b8](https://github.com/nzbdav/sharpcompress/commit/72b81b86091e2c72f767495cdaf088cb2bbaa0df))
* Change LeaveStreamOpen default from true to false ([37c0f5a](https://github.com/nzbdav/sharpcompress/commit/37c0f5aec575f7dcdc06e76f75e9ca9707bbf0ee))
* **docs:** clarify stream handling rules and ownership in AGENTS.md ([ea95045](https://github.com/nzbdav/sharpcompress/commit/ea950457f51b4ac8f076bf4110f15f0e354f963b))
* increase RewindableBufferSize to 160KB to cover ZStandard worst-case first block ([d56b676](https://github.com/nzbdav/sharpcompress/commit/d56b676ee94e2c177ffd9e89f79b0feed243013f))
* **zip:** stop overriding LeaveStreamOpen in OpenArchive ([23840b7](https://github.com/nzbdav/sharpcompress/commit/23840b7a0ff0f7eb8048249c75fa038ab14490cd))

## Changelog

This repository is the [nzbdav](https://github.com/nzbdav) fork of
[adamhathcock/sharpcompress](https://github.com/adamhathcock/sharpcompress),
published as `NzbDav.SharpCompress`. Release Please maintains the version history
below. For changes prior to the fork package identity, see the upstream project.
