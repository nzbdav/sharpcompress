# Changelog

## [0.54.0](https://github.com/nzbdav/sharpcompress/compare/v0.53.1...v0.54.0) (2026-07-21)


### Features

* **rar:** expose IsUncompressedSizeUnknown on file headers ([253dc9e](https://github.com/nzbdav/sharpcompress/commit/253dc9ea03aa083dd3ca7ceedacdd3598c6bf86d))
* **rar:** expose IsUncompressedSizeUnknown on file headers ([110578b](https://github.com/nzbdav/sharpcompress/commit/110578b4cee4606ca6425986d16f83b4424aaeda))

## [0.53.1](https://github.com/nzbdav/sharpcompress/compare/v0.53.0...v0.53.1) (2026-07-20)


### Bug Fixes

* **7z:** bound decoder-stream coder-chain recursion ([be98ec3](https://github.com/nzbdav/sharpcompress/commit/be98ec3c47fb269deb4a2604bc7133eeeb94cc8d))
* **7z:** bound decoder-stream recursion to prevent stack overflow ([8c61264](https://github.com/nzbdav/sharpcompress/commit/8c61264f8cb72154675f03f535e82d2109e5a1b4))
* **deps:** bump the github-actions group with 2 updates ([d5fe559](https://github.com/nzbdav/sharpcompress/commit/d5fe55983ba3b8effc5f30e3d1992845300c805e))
* **deps:** bump the github-actions group with 2 updates ([dfebdd2](https://github.com/nzbdav/sharpcompress/commit/dfebdd2485565c257d242f0781ac96ba6dba1e47))
* **deps:** Bump the nuget-minor-and-patch group with 1 update ([f380ec6](https://github.com/nzbdav/sharpcompress/commit/f380ec6ea5f196cf609debf9a532ea90dcd107fe))
* **deps:** Bump the nuget-minor-and-patch group with 1 update ([7147d62](https://github.com/nzbdav/sharpcompress/commit/7147d62ae77832ca0cbed05ceb71ff4ea3a27f02))
* **deps:** refresh transitive package locks for System.IO.Hashing 10.0.9 ([ec6625c](https://github.com/nzbdav/sharpcompress/commit/ec6625cdf90bc67575c4bcf44781e9b236bfd7bb))

## [0.53.0](https://github.com/nzbdav/sharpcompress/compare/v0.52.0...v0.53.0) (2026-07-18)


### Features

* **rar:** add RarHeaderReadException for truncation vs corruption ([344b465](https://github.com/nzbdav/sharpcompress/commit/344b465c67e8738eff31f4735b9990026c0170ad)), closes [#119](https://github.com/nzbdav/sharpcompress/issues/119)
* **rar:** typed RarHeaderReadException for header-parse failures ([591b06c](https://github.com/nzbdav/sharpcompress/commit/591b06ce117324f4c00365a9201e8e20d9786181))

## [0.52.0](https://github.com/nzbdav/sharpcompress/compare/v0.51.1...v0.52.0) (2026-07-18)


### Features

* public metadata APIs for streaming consumers ([95be1ad](https://github.com/nzbdav/sharpcompress/commit/95be1adc786b1ad6c6bdb93098b5cb2b187646f9))
* **rar:** add public RarKeyDerivation.DeriveKey API ([a9f4b95](https://github.com/nzbdav/sharpcompress/commit/a9f4b95bcf6d14475c36d757f0d80e15db07023f))
* **rar:** expose typed public header interfaces for streaming consumers ([29abac6](https://github.com/nzbdav/sharpcompress/commit/29abac661cb9d8f4c7eafe0f7d0b9fc7415d7a46))
* **sevenzip:** public packed byte ranges and AES-first CompressionType ([6796c3e](https://github.com/nzbdav/sharpcompress/commit/6796c3e7cb4a921b482c5ed834bde75e7abe89e5))


### Bug Fixes

* **rar:** avoid required-member metadata on RarDerivedKey ([76c028d](https://github.com/nzbdav/sharpcompress/commit/76c028d1c04eb018cee87452d51ce96088d00803))
* **rar:** defer seekable packed-data skip until next header advance ([de7ddc6](https://github.com/nzbdav/sharpcompress/commit/de7ddc6555629480d002c931abee76728288c457))

## [0.51.1](https://github.com/nzbdav/sharpcompress/compare/v0.51.0...v0.51.1) (2026-07-16)


### Bug Fixes

* **core:** async disposal gaps leak RAR unpack windows and volumes ([50a14bf](https://github.com/nzbdav/sharpcompress/commit/50a14bf252498cbabed53d63050c736e5fcd1f31))
* **core:** EntryStream falsely marks entry completed on zero-length reads ([84f49f8](https://github.com/nzbdav/sharpcompress/commit/84f49f84d9203cfcfbf33b51b87626dd549f5e70))
* **lzma:** CrcCheckStream pool/dispose/dead-work bugs ([9f922fb](https://github.com/nzbdav/sharpcompress/commit/9f922fb6e8eac9b02c03d72f8bdb68ad7b62c213))
* **sevenzip:** SevenZipArchive property bugs and per-access LINQ ([7106460](https://github.com/nzbdav/sharpcompress/commit/7106460bcce4792ffbdd36bdf4d63d09bf19e3e7))


### Performance Improvements

* **core:** route Crc32Stream/checksum CRC through SIMD Crc32Helper ([842332a](https://github.com/nzbdav/sharpcompress/commit/842332a418a70bc4f270ec2c22647332aeac0cc8))
* **io:** SourceStream O(parts) rescans, LINQ Length, dispose modernization ([fc89ae0](https://github.com/nzbdav/sharpcompress/commit/fc89ae0abae64d2526e051ec4dbff6426a9a7231))
* **lzma:** reuse LZMA2 Decoder across chunk state resets ([a8d8092](https://github.com/nzbdav/sharpcompress/commit/a8d809270b5b8ff953517c356c912a264121db98))
* pass3 performance and quality audit ([#103](https://github.com/nzbdav/sharpcompress/issues/103)–[#116](https://github.com/nzbdav/sharpcompress/issues/116)) ([0a9c58f](https://github.com/nzbdav/sharpcompress/commit/0a9c58f90bbe1651ad99371e8a91b4450646e365))
* **sevenzip:** cache 7z AES key derivation ([0ea554a](https://github.com/nzbdav/sharpcompress/commit/0ea554ad7f9df7b6b58e73fffae66f60f83cc9ee))
* **stream:** eliminate per-call allocations in SharpCompressStream async ring-buffer reads ([6c3a951](https://github.com/nzbdav/sharpcompress/commit/6c3a951beb8022d313abf8c0fc3efc9ea560197e))
* **stream:** implement Read(Span)/ReadAsync(Memory) on hot-path stream wrappers ([d1fca5f](https://github.com/nzbdav/sharpcompress/commit/d1fca5fdb76dd43f18602bf9e2939bb931de1a90))

## [0.51.0](https://github.com/nzbdav/sharpcompress/compare/v0.50.1...v0.51.0) (2026-07-13)


### ⚠ BREAKING CHANGES

* **stream:** Constants.BufferSize no longer has a public setter. Configure buffer sizes per operation via ReaderOptions.BufferSize, ExtractionOptions.BufferSize, or WriterOptions.BufferSize.

### Features

* **core:** use System.IO.Hashing for CRC32 ([3571898](https://github.com/nzbdav/sharpcompress/commit/3571898a791cd6ec3467a8dc5ac11953f2cf7007)), closes [#78](https://github.com/nzbdav/sharpcompress/issues/78)
* **lzip:** Length/Position and multimember reads ([913616a](https://github.com/nzbdav/sharpcompress/commit/913616a545c396ab155ed154ca5f5da92239909b)), closes [#61](https://github.com/nzbdav/sharpcompress/issues/61)
* **rar:** positional reads for encrypted stored entries ([4a9294a](https://github.com/nzbdav/sharpcompress/commit/4a9294a9097f117a4b7d26d34fd288f4ba7da155)), closes [#86](https://github.com/nzbdav/sharpcompress/issues/86)
* **reader:** add ReaderOptions.CancelOnEntryStreamDispose ([51f5f9d](https://github.com/nzbdav/sharpcompress/commit/51f5f9d1aca97b3491dc7a2dd02021a577c22cbe))
* **sevenzip:** IWritableArchive support ([2e05fb9](https://github.com/nzbdav/sharpcompress/commit/2e05fb987d84c6250d9542af2b7c9ecd59a4bfb7)), closes [#63](https://github.com/nzbdav/sharpcompress/issues/63)
* **sevenzip:** solid archive writing ([1ff2019](https://github.com/nzbdav/sharpcompress/commit/1ff201977a5572e49abf94e49f313a0f53d41d64))
* **stream:** make Constants.BufferSize read-only ([858998f](https://github.com/nzbdav/sharpcompress/commit/858998f1eb903945c3882e0006737e2c25b3bc0c))


### Bug Fixes

* **ci:** place release-please markers on their own lines ([9df046a](https://github.com/nzbdav/sharpcompress/commit/9df046ae8ecb7526d0ea6899e5dc8f7dc33ea0aa))
* **ci:** use generic updater for csproj Version bumps ([989b410](https://github.com/nzbdav/sharpcompress/commit/989b410659ed85af8eb00d9ef79c992958687dba))
* **ci:** use generic updater for csproj Version bumps ([984af57](https://github.com/nzbdav/sharpcompress/commit/984af570d1a957ea5a43de94de6e7a6a2af16b6c))
* **core:** ArrayPool usage audit ([6cd1c59](https://github.com/nzbdav/sharpcompress/commit/6cd1c59db069714974d84d0ef8957451017e3ce4)), closes [#96](https://github.com/nzbdav/sharpcompress/issues/96)
* **core:** lazy collection thread safety ([6680dc6](https://github.com/nzbdav/sharpcompress/commit/6680dc6bc06920252233d8206095f329dffd7998)), closes [#100](https://github.com/nzbdav/sharpcompress/issues/100)
* **core:** synchronize LazyReadOnlyCollection enumeration ([bd12baa](https://github.com/nzbdav/sharpcompress/commit/bd12baa24f136231326f87eeeea101a6d7ac5ce9)), closes [#64](https://github.com/nzbdav/sharpcompress/issues/64)
* **deflate64:** tighten InflaterManaged literal decode ([d03b9a9](https://github.com/nzbdav/sharpcompress/commit/d03b9a95d2c42fab91b5ba2ebfbe43eaa53e2ca7)), closes [#62](https://github.com/nzbdav/sharpcompress/issues/62)
* eliminate hot-path allocations ([#28](https://github.com/nzbdav/sharpcompress/issues/28)–[#32](https://github.com/nzbdav/sharpcompress/issues/32)) ([ffe9766](https://github.com/nzbdav/sharpcompress/commit/ffe9766374b23de368178bed4b4f7e0a81fa7522))
* **lzma:** buffer async range coder input ([585a339](https://github.com/nzbdav/sharpcompress/commit/585a339c61d846d6daa416b633469dea37d0ba5b)), closes [#82](https://github.com/nzbdav/sharpcompress/issues/82)
* **rar:** add seekable fast path for stored (m0) entries ([f4d8aa5](https://github.com/nzbdav/sharpcompress/commit/f4d8aa5556bcd62ba84543409f3020711bd6c2fc)), closes [#20](https://github.com/nzbdav/sharpcompress/issues/20)
* **rar:** batch AES decryption and remove per-byte queues ([0b1e181](https://github.com/nzbdav/sharpcompress/commit/0b1e181f6ece3ac344ed2f09723b290c833be96b))
* **rar:** cache entry/archive metadata instead of re-running LINQ per access ([b6e96b3](https://github.com/nzbdav/sharpcompress/commit/b6e96b3b14d9ddd6a81cc5492a2a0a523d759ded))
* **rar:** cache RAR5 KDF results across header blocks and file parts ([6c2aa34](https://github.com/nzbdav/sharpcompress/commit/6c2aa34c1d665b9466e06eb67cd023da08f760a4)), closes [#77](https://github.com/nzbdav/sharpcompress/issues/77)
* **rar:** clarify UnpackV2017 method-29 routing ([91f9360](https://github.com/nzbdav/sharpcompress/commit/91f9360daa888c3c7d677ab45fed8efe467a64eb)), closes [#67](https://github.com/nzbdav/sharpcompress/issues/67)
* **rar:** hoist per-call allocations out of async unpack ([980ae94](https://github.com/nzbdav/sharpcompress/commit/980ae94438f6a4f1b9336f9c44f761e6ef88c5df)), closes [#83](https://github.com/nzbdav/sharpcompress/issues/83)
* **rar:** optimize BLAKE2sp hashing ([ed4618c](https://github.com/nzbdav/sharpcompress/commit/ed4618cd4e3d498fd7c915b2eed65a49c196f113)), closes [#79](https://github.com/nzbdav/sharpcompress/issues/79)
* **rar:** reduce multi-volume stream overhead ([bedefb3](https://github.com/nzbdav/sharpcompress/commit/bedefb36cf1edffb133daf3e6630dc9e153a3a75)), closes [#88](https://github.com/nzbdav/sharpcompress/issues/88)
* **rar:** replace 64KB stackalloc with pooled buffers and harden ArrayPool rent/return guarantees ([d34dcb0](https://github.com/nzbdav/sharpcompress/commit/d34dcb0f587c78382350145b0242959b68ac5171))
* **rar:** seekable fast path for stored (m0) entries ([8ba2a5a](https://github.com/nzbdav/sharpcompress/commit/8ba2a5a84e6c3d29ad18c9a01f01e844347e0605))
* **rar:** single-pass RAR3 key derivation with copyable SHA-1 ([76d2fc8](https://github.com/nzbdav/sharpcompress/commit/76d2fc8294f0ba948fd9d7d34fff844b1b3f3c37))
* **rar:** speed up UnpackV2017 CopyString fast path ([9c3aa26](https://github.com/nzbdav/sharpcompress/commit/9c3aa265afb512d35c20cbe82ae4ffed1e31d9ca)), closes [#68](https://github.com/nzbdav/sharpcompress/issues/68)
* **rar:** use UTF-16LE password bytes in RAR3 KDF ([8dea618](https://github.com/nzbdav/sharpcompress/commit/8dea618a4b75a7e1bd99bd2953a7eaf3a4bff42a))
* resolve all open issues ([#10](https://github.com/nzbdav/sharpcompress/issues/10)–[#68](https://github.com/nzbdav/sharpcompress/issues/68)) ([c871ff9](https://github.com/nzbdav/sharpcompress/commit/c871ff917e905b5a2b779efe4a04b603e3c07c5e))
* **sevenzip:** cache folder stream for Archive API entry opens ([cc99d8b](https://github.com/nzbdav/sharpcompress/commit/cc99d8bfd8beb6c3613dcc2771826f1f74f8cd23))
* **sevenzip:** use real async folder streams for ExtractAllEntries ([a6662b6](https://github.com/nzbdav/sharpcompress/commit/a6662b6f36c735013ef1ff0a72011918c4a87675))
* **stream:** cache single-byte buffers in async decoders instead of per-call allocations ([c200bd2](https://github.com/nzbdav/sharpcompress/commit/c200bd24d9d83f6a2067c51d71c88ddd5e992269))
* **stream:** EntryStream over-read via IOverreadingStream ([247b1fa](https://github.com/nzbdav/sharpcompress/commit/247b1fa5b2f1bb1efebda7521b5045aafee95953)), closes [#42](https://github.com/nzbdav/sharpcompress/issues/42)
* **streaming:** improve RAR, 7z, and reader performance ([74bcc4d](https://github.com/nzbdav/sharpcompress/commit/74bcc4ddb875a74b0d4ce803350c149ee262d225))
* **stream:** never wrap SharpCompressStream as natively seekable ([dd30c94](https://github.com/nzbdav/sharpcompress/commit/dd30c94814a88e9fd127025cceb56a1f9091e7dc))
* **stream:** reduce ring-buffer copies ([ca49f6f](https://github.com/nzbdav/sharpcompress/commit/ca49f6fae7a134764bd159e855aed1439762e121)), closes [#91](https://github.com/nzbdav/sharpcompress/issues/91)
* **stream:** release ring buffer after format detection ([a20c2ab](https://github.com/nzbdav/sharpcompress/commit/a20c2aba25502436bbd3401016d566e56094f3cc))
* **stream:** seek past skipped bytes on seekable sources ([42f70fd](https://github.com/nzbdav/sharpcompress/commit/42f70fd53bbea5eaeb3f32fc1a8d85820f558a25))
* **tar:** preserve detection buffering for async-only sources ([48d1b4e](https://github.com/nzbdav/sharpcompress/commit/48d1b4e12f90034d52de252cd1c2e2d0a7770818))
* **zip:** eliminate per-read allocations in PKWare crypto and PPMd async streams ([7f21d32](https://github.com/nzbdav/sharpcompress/commit/7f21d32879e259c29098632f8ba84678408b5084))
* **zip:** track ZipWriter entry counts as ulong ([f85e928](https://github.com/nzbdav/sharpcompress/commit/f85e9283ae87acf0f9a572f9738953e9a0dc77e1)), closes [#65](https://github.com/nzbdav/sharpcompress/issues/65)
* **zlib:** cross-platform SIMD Adler32 ([aa2da9f](https://github.com/nzbdav/sharpcompress/commit/aa2da9fab53595da7f0561761954701110ea3e2a)), closes [#81](https://github.com/nzbdav/sharpcompress/issues/81)


### Performance Improvements

* pass2 performance and quality audit ([#77](https://github.com/nzbdav/sharpcompress/issues/77)–[#100](https://github.com/nzbdav/sharpcompress/issues/100)) ([c664ae5](https://github.com/nzbdav/sharpcompress/commit/c664ae5b66cf119e3d79c060474a76a53ab589a1))

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
