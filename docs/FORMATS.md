# Formats

## Accessing Archives

* Archive classes allow random access to a seekable stream.
* Reader classes allow forward-only reading on a stream.
* Writer classes allow forward-only Writing on a stream.

## Supported Format Table

| Archive Format     | Compression Format(s)                                               | Compress/Decompress | Archive API     | Reader API | Writer API      |
| ------------------ | ------------------------------------------------------------------- | ------------------- | --------------- | ---------- | --------------- |
| Ace                | None                                                                | Decompress          | N/A             | AceReader  | N/A             |
| Arc                | None, Packed, Squeezed, Crunched                                    | Decompress          | N/A             | ArcReader  | N/A             |
| Arj                | None                                                                | Decompress          | N/A             | ArjReader  | N/A             |
| Rar                | Rar                                                                 | Decompress          | RarArchive      | RarReader  | N/A             |
| Zip (2)            | None, Shrink, Reduce, Implode, DEFLATE, Deflate64, BZip2, LZMA, PPMd, ZStandard, XZ | Both                | ZipArchive      | ZipReader  | ZipWriter       |
| Tar                | None                                                                | Both                | TarArchive      | TarReader  | TarWriter (3)   |
| Tar.GZip           | DEFLATE                                                             | Both                | N/A             | TarReader  | TarWriter (3)   |
| Tar.BZip2          | BZip2                                                               | Both                | N/A             | TarReader  | TarWriter (3)   |
| Tar.Zstandard      | ZStandard                                                           | Decompress          | N/A             | TarReader  | N/A             |
| Tar.LZip           | LZMA                                                                | Both                | N/A             | TarReader  | TarWriter (3)   |
| Tar.XZ             | LZMA2                                                               | Decompress          | N/A             | TarReader  | N/A             |
| Tar.LZW            | LZW                                                                 | Decompress          | N/A             | TarReader  | N/A             |
| GZip (single file) | DEFLATE                                                             | Both                | GZipArchive     | GZipReader | GZipWriter      |
| 7Zip (4)           | LZMA, LZMA2, BZip2, PPMd, BCJ, BCJ2, Deflate                        | Both                | SevenZipArchive | N/A        | SevenZipWriter  |

1. SOLID Rars are only supported in the RarReader API.
2. Zip format supports pkware and WinzipAES encryption. However, encrypted LZMA is not supported. Zip64 reading/writing is supported but only with seekable streams as the Zip spec doesn't support Zip64 data in post data descriptors. Deflate64, Shrink, Reduce, Implode, and XZ are only supported for reading. ZStandard is supported for reading and writing. See [Zip Format Notes](#zip-format-notes) for details on multi-volume archives and streaming behavior.
3. The Tar format requires a file size in the header. If no size is specified to the TarWriter and the stream is not seekable, then an exception will be thrown.
4. The 7Zip format doesn't allow for reading as a forward-only stream, so 7Zip read support is only through the Archive API. Writing is supported through SevenZipWriter for non-solid archives with LZMA/LZMA2 and requires a seekable output stream. See [7Zip Format Notes](#7zip-format-notes) for solid-folder reuse and async extraction details.
5. LZip has no support for extra data like the file name or timestamp. There is a default filename used when looking at the entry Key on the archive.

`ArchiveFactory.GetArchiveInformation(...).SupportsRandomAccess` is `true` when the detected format has an Archive API in this table. It is `false` for reader-only formats such as Ace, Arc, Arj, and standalone LZW. Compressed tar wrappers are supported by `ReaderFactory`/`TarReader`, not by `ArchiveFactory`/`TarArchive`; ArchiveFactory detection blocks them instead of opening the outer compression wrapper as a standalone archive.

### Zip Format Notes

- Multi-volume/split ZIP archives require ZipArchive (seekable streams) as ZipReader cannot seek across volume files.
- ZipReader processes entries from LocalEntry headers (which include directory entries ending with `/`) and intentionally skips DirectoryEntry headers from the central directory, as they are redundant in streaming mode - all entry data comes from LocalEntry headers which ZipReader has already processed.
- ZIP supports ZStandard for reading and writing. Tar.Zstandard support is read/decompress only.

### 7Zip Format Notes

- **Solid folder reuse (Archive API)**: Sequential `OpenEntryStream()` / `OpenEntryStreamAsync()` calls within the same solid folder reuse the folder decoder and skip only the remaining bytes to the next entry. Opening an earlier entry in the same folder (or otherwise seeking backward) requires a full folder re-decode — inherent to solid 7z. Concurrent entry opens on the same archive bypass the cache and each build a fresh decoder (prefer one active stream, or use `ExtractAllEntries()` for sequential extraction).
- **Async extraction**: `OpenEntryStreamAsync` and `ExtractAllEntriesAsync` use real async decoder I/O (`GetFolderStreamAsync` / folder-stream caching). `ExtractAllEntriesAsync` reuses one folder stream across sequential entries in the same solid folder, matching sync `ExtractAllEntries` behavior. Prefer `ExtractAllEntries()` / `ExtractAllEntriesAsync()` for solid sequential extraction; prefer sync APIs when you do not need to free the calling thread.

### XZ Format Notes

- XZ is a container format around LZMA2-compressed blocks, not just raw LZMA/LZMA2 data.
- XZ streams can include per-block integrity checks selected by the stream header: CRC32, CRC64/XZ, SHA-256, or none. SharpCompress validates these checks while reading XZ blocks.
- Raw LZMA/LZMA2 decoding does not provide the same container-level CRC validation; it only validates what the decoder format itself can detect, such as malformed compressed data or invalid end markers.

## Compression Streams

For those who want to directly compress/decompress bits. The single file formats are represented here as well. However, BZip2, LZip and XZ have no metadata (GZip has a little) so using them without something like a Tar file makes little sense.

| Compressor      | Compress/Decompress |
| --------------- | ------------------- |
| BZip2Stream     | Both                |
| GZipStream      | Both                |
| DeflateStream   | Both                |
| Deflate64Stream | Decompress          |
| LZMAStream      | Both                |
| PPMdStream      | Both                |
| LzwStream       | Decompress          |
| ADCStream       | Decompress          |
| LZipStream      | Both                |
| XZStream        | Decompress          |
| ZStandard CompressionStream/DecompressionStream | Both                |

## Archive Formats vs Compression

Sometimes the terminology gets mixed.

### Compression

DEFLATE, LZMA are pure compression algorithms

### Formats

Formats like Zip, 7Zip, Rar are archive formats only. They use other compression methods (e.g. DEFLATE, LZMA, etc.) or propriatory (e.g RAR)

### Overlap

GZip, BZip2, LZip, XZ, LZW, and ZStandard are single file or compression wrapper formats. The overlap in the API happens because Tar uses these formats as "compression" methods and the API tries to hide this a bit.

`ArchiveType` represents archive containers exposed by the high-level APIs (`Rar`, `Zip`, `Tar`, `SevenZip`, `GZip`, `Arc`, `Arj`, `Ace`, and `Lzw`). `XZ` and `ZStandard` are represented as `CompressionType` values rather than `ArchiveType` values.
