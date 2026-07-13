# Implementation Plan — Issues #20–#27 and #43 (nzbdav/sharpcompress)

Plan for resolving the RAR/7z streaming-performance audit issues [#20](https://github.com/nzbdav/sharpcompress/issues/20)
through [#27](https://github.com/nzbdav/sharpcompress/issues/27), plus the related maintainability issue
[#43](https://github.com/nzbdav/sharpcompress/issues/43). Each section is self-contained: an
implementing agent should be able to execute one issue end-to-end from its section plus the Ground Rules.

Issue bodies and all comments were reviewed on 2026-07-13. Decision comments on the issues override the
original proposal comments where they conflict; those corrections are already folded in below.

---

## Status snapshot

| Issue | Title | Priority / Effort | State | Work needed |
|---|---|---|---|---|
| [#20](https://github.com/nzbdav/sharpcompress/issues/20) | [2.1] Direct-stream fast path for stored (m0) RAR entries | P0 / M | **Closed** (landed via PR #71, commit `f4d8aa55`) | Verify only |
| [#21](https://github.com/nzbdav/sharpcompress/issues/21) | [2.2] Shared `IRarUnpack` breaks concurrent entry streams | P0 / M | **Closed** (landed, see issue comment) | Verify only |
| [#22](https://github.com/nzbdav/sharpcompress/issues/22) | [2.3] RAR decryption: per-byte `Queue` + per-block allocations | P1 / M | Open | Full implementation |
| [#23](https://github.com/nzbdav/sharpcompress/issues/23) | [2.4] `CryptKey3` RAR3 KDF: O(n²) hashing + LOH allocation | P1 / S | Open | Full implementation |
| [#24](https://github.com/nzbdav/sharpcompress/issues/24) | [2.5] 7z async API is sync under the hood | P1 / L | Open | Characterize, then fix or document |
| [#25](https://github.com/nzbdav/sharpcompress/issues/25) | [2.6] 7z Archive API re-decodes solid folder per entry open | P1 / M | Open | Full implementation |
| [#26](https://github.com/nzbdav/sharpcompress/issues/26) | [2.7] `EntryStream.Dispose` silently downloads rest of entry | P1 / S | Open | Full implementation |
| [#27](https://github.com/nzbdav/sharpcompress/issues/27) | [2.8] Ring buffer records all reads forever on non-seekable streams | P1 / M | Open | Full implementation |
| [#43](https://github.com/nzbdav/sharpcompress/issues/43) | [5.3] `SharpCompressStream` mode complexity + test hooks in production | P2 / M | Open | Full implementation (coordinated with #27) |

## Ground rules (apply to every issue)

1. **Branch & PR per issue.** Branch names like `fix/rar-crypto-pipeline-22`. One issue per PR. Reference the
   issue number in the PR body (`Closes #22`).
2. **Conventional Commits** with scope, per AGENTS.md: `fix(rar): …`, `feat(reader): …`, `chore(…): …`.
   Performance work with no API change = `fix(scope):`. New public API = `feat(scope):`.
   Do **not** commit or push unless the user explicitly asks.
3. **Workflow commands** (run from repo root):
   ```bash
   dotnet tool restore
   dotnet restore SharpCompress.slnx --locked-mode
   dotnet build SharpCompress.slnx --configuration Release --no-restore
   dotnet csharpier format .        # after every edit batch
   dotnet csharpier check .         # must pass before handing off
   dotnet test tests/SharpCompress.Test/SharpCompress.Test.csproj --configuration Release
   ```
   Targeted test runs use `--filter "FullyQualifiedName~Rar"` (or `~SevenZip`, `~Streams`, etc.).
4. **Benchmarks** live in `tests/SharpCompress.Performance/Benchmarks/`. Run with
   `dotnet run --project tests/SharpCompress.Performance -c Release -- --filter "*<Name>*"`.
   Compare against `tests/SharpCompress.Performance/baseline-results.md` locally only — CI compares against a
   static baseline and its wall-clock deltas are advisory noise; the **Allocated** column is the trustworthy metric.
5. **Nullability:** `is null` / `is not null`; declare non-nullable and validate at boundaries.
6. **Public API changes** require doc updates in the same PR: `docs/API.md`, `docs/USAGE.md`,
   `docs/FORMATS.md`, `README.md` as applicable, plus XML docs. No `init` setters / `required` members on
   public surface.
7. **Test style:** xUnit + AwesomeAssertions, base class `TestBase`, fixtures from `tests/TestArchives/`.
   No "Arrange/Act/Assert" comments. Match naming in neighboring test files.
8. Internal-only types/members should stay `internal`. This library ships `InternalsVisibleTo` for the test
   assembly (check `src/SharpCompress/SharpCompress.csproj` before assuming — if absent, test through public API).

---

## Issue #20 — [2.1] Stored (m0) RAR fast path — CLOSED, verify only

**Resolution already landed** in PR #71 / commit `f4d8aa55` (`fix(rar): add seekable fast path for stored (m0) entries`).

What exists now (confirm, do not re-implement):

- `src/SharpCompress/Compressors/Rar/StoredRarEntryStream.cs` + `.Async.cs` — seekable stream over the
  ordered `RarFilePart` list with `TryCreate(parts, out var storedStream)` gate.
- Gated in `src/SharpCompress/Archives/Rar/RarArchiveEntry.cs` (line ~79) and
  `RarArchiveEntry.Async.cs` (line ~29): stored + not encrypted + not solid + seekable parts.
- Per the decision comment: incremental CRC32 validated at EOF for pure sequential reads; any `Seek`
  disables validation for that stream instance. Reader API (`RarReader`) unchanged.
- Tests: `tests/SharpCompress.Test/Rar/RarStoredEntryStreamTests.cs`.

**Verification steps for the agent:**

1. `dotnet test … --filter "FullyQualifiedName~RarStoredEntryStream"` — all green.
2. Confirm the gate conditions in both `OpenEntryStream` twins match the decision comment
   (`IsStored && !IsEncrypted && !IsSolid` + seekable volumes).
3. If any gap is found (e.g. async twin missing a condition), open a follow-up PR; otherwise no action.

---

## Issue #21 — [2.2] Shared `IRarUnpack` concurrency — CLOSED, verify only

**Resolution already landed** (issue comment: "non-solid archives get a private unpacker per
OpenEntryStream; solid archives keep a shared unpacker with an Interlocked concurrency guard").

What exists now (confirm): `src/SharpCompress/Archives/Rar/RarArchive.cs` —
`_activeSolidEntryStreams` guarded with `Interlocked.CompareExchange` (line ~78), fresh
`UnpackV1.Unpack` / `UnpackV2017.Unpack` per stream for non-solid (line ~74), shared `Lazy` instances only
for solid. Documented in `docs/API.md` / `docs/USAGE.md`.

**Verification steps for the agent:**

1. `dotnet test … --filter "FullyQualifiedName~Rar"` — green.
2. Confirm a test exists that (a) reads two interleaved entry streams from a non-solid archive and
   (b) asserts the solid-archive concurrent-open exception. The comment says coverage came from issue #49's
   concurrency tests — search `tests/SharpCompress.Test/Rar/` for `Interleaved|Concurrent`. If genuinely
   missing, add both tests per the issue's Verification section; otherwise no action.

---

## Issue #22 — [2.3] RAR decryption pipeline: kill per-byte `Queue` + per-block allocations

**Branch:** `fix/rar-crypto-pipeline-22` · **Commit:** `fix(rar): batch AES decryption and remove per-byte queues`

### Problem recap

Three consumers decrypt 16 bytes at a time through `BlockTransformer.ProcessBlock`, which allocates **two**
arrays per block (`cipherText.ToArray()` + result array), then push every decrypted byte through a
`Queue<byte>` one at a time. 1 GB encrypted entry ≈ 67M blocks → ~134M transient arrays and ~2B
enqueue/dequeue ops, and AES-NI batching is forfeited.

### Current code (verified 2026-07-13)

| File | What's there now |
|---|---|
| `src/SharpCompress/Crypto/BlockTransformer.cs` | `ProcessBlock(ReadOnlySpan<byte>)` → allocates 2 arrays per call; wraps `ICryptoTransform` |
| `src/SharpCompress/Common/Rar/RarCryptoWrapper.cs` | `Queue<byte> _data`; sync `ReadAndDecrypt` reads 16-byte blocks in a loop (`stackalloc byte[16]`), enqueues bytes; async twin `ReadAndDecryptAsync` allocates `new byte[16]` per call and does the same |
| `src/SharpCompress/Common/Rar/RarCryptoBinaryReader.cs` | Same queue pattern in `ReadAndDecryptBytes`; also `ClearQueue()` / `SkipQueue()` (repositions `BaseStream` forward by `_data.Count`); per-byte `UpdateCrc(b)` |
| `src/SharpCompress/Common/Rar/AsyncRarCryptoBinaryReader.cs` | Async twin of the above, same pattern |
| `src/SharpCompress/Common/Rar/ICryptKey.cs` | `ICryptoTransform Transformer(byte[] salt)` — key/IV not exposed |

Note: RAR uses **AES-CBC, PaddingMode.None** (see `CryptKey3.Transformer`). CBC decryption is chainable:
the IV for block N+1 is the ciphertext of block N. `ICryptoTransform.TransformBlock` maintains that chain
internally across calls — any replacement must preserve it explicitly.

### Implementation steps

1. **Rewrite `BlockTransformer`** (keep the class name/file to limit churn):
   - Replace `ProcessBlock` with `void Process(ReadOnlySpan<byte> input, Span<byte> output)` handling any
     multiple of 16 bytes in one call.
   - Easiest correct implementation that preserves chaining without touching `ICryptKey`: keep the
     `ICryptoTransform` but call `TransformBlock` **once per run** on a pooled buffer instead of per
     16-byte block:
     ```csharp
     public void Process(byte[] input, int offset, int count, byte[] output, int outputOffset) =>
         transformer.TransformBlock(input, offset, count, output, outputOffset);
     ```
     `AesCng/AesImplementation` transforms honor multi-block input and keep CBC state across calls — verify
     with a round-trip unit test against the old per-block loop output (byte-identical requirement).
   - Optional stretch (only if the above shows insufficient gains): extend `ICryptKey` (internal) with
     key/IV accessors and use `Aes.DecryptCbc(run, iv, output, PaddingMode.None)` per run, manually carrying
     the last ciphertext block of run N as the IV of run N+1. This must stash the pre-decryption ciphertext
     tail because decryption overwrites the buffer.
   - Implement `Dispose` to dispose the transform (currently empty — fine to fix while here).
2. **Replace `Queue<byte>` with a 16-byte carry-over** in all three consumers. Shared shape (put the shared
   logic in one internal helper if it stays clean; the two binary readers are near-identical):
   - Fields: `byte[] _carry = new byte[16]; int _carryLength; int _carryOffset;`
   - Read path for `count` bytes: serve from carry first; then compute the aligned ciphertext size for the
     remainder, read it **in one `ReadExactly`/`ReadExactlyAsync` call** into a pooled buffer
     (`ArrayPool<byte>.Shared`), decrypt the whole run with one `Process` call **directly into the caller's
     buffer** for all whole blocks that fit, and stash the ≤15-byte tail (plus any decrypted overshoot) in
     the carry buffer.
   - `RarCryptoWrapper`: apply to `ReadAndDecrypt` (sync), `ReadAndDecryptAsync`, and the `ReadAsync(Memory…)`
     override if present — check `RarCryptoWrapper` for all read overloads before starting.
   - `RarCryptoBinaryReader` / `AsyncRarCryptoBinaryReader`: same replacement inside
     `ReadAndDecryptBytes(Async)`. Keep the per-byte `UpdateCrc(b)` loop over the decrypted output
     (CRC order must not change), or use the span-based CRC update if `RarCrcBinaryReader` exposes one.
3. **Preserve `SkipQueue()`/`ClearQueue()` semantics exactly.** Current behavior:
   `BaseStream.Position += _data.Count; queue.Clear()`. Port as
   `BaseStream.Position += (_carryLength - _carryOffset); _carryLength = _carryOffset = 0;`.
   These are called during header/data transitions — get this wrong and encrypted RAR5 header parsing breaks.
4. **`_readCount` bookkeeping** in the binary readers must keep counting decrypted bytes returned (not
   ciphertext consumed) — mirror the existing `_readCount += count`.
5. Dispose pooled buffers deterministically (`try/finally` around rent/return, or a small disposable holder
   owned by the wrapper).

### Tests

- Existing coverage is the safety net: `dotnet test … --filter "FullyQualifiedName~Rar"` must pass —
  this exercises RAR3 + RAR5, header-encrypted and file-encrypted fixtures under
  `tests/TestArchives/Original/Rar/` (e.g. `Rar.Encrypted_filesAndHeader.rar`, RAR5 equivalents).
- Add a unit test asserting the new `Process` run-based decryption output is byte-identical to a reference
  per-16-byte-block loop for a few sizes (16, 32, 4096, 4096+16) with a fixed key/IV.
- Add/extend an interleaved test: open an encrypted entry, read with odd sizes {1, 7, 15, 16, 17, 8192}
  alternating, compare against full extraction (guards the carry-over edge cases).

### Benchmarks

`tests/SharpCompress.Performance/Benchmarks/RarEncryptedReadBenchmarks.cs` already exists ("Rar5 encrypted:
full entry read" + non-seekable variant). Run before/after; the win must show in the **Allocated** column
(expect O(bytes) → O(1) per read; the baseline shows ~5.8 MB allocated per read today).

### Acceptance criteria

- No `Queue<byte>` remains in `src/SharpCompress/Common/Rar/`.
- One `TransformBlock`/decrypt call per contiguous run, not per 16-byte block.
- All existing encrypted-RAR tests pass; allocated bytes in `RarEncryptedReadBenchmarks` drop dramatically.
- `docs/` untouched (no public API change).

---

## Issue #23 — [2.4] `CryptKey3` RAR3 KDF: single-pass SHA-1, no LOH allocation

**Branch:** `fix/rar3-kdf-23` · **Commits:** two, per the decision comment:
1. `fix(rar): single-pass RAR3 key derivation with copyable SHA-1`
2. `fix(rar): use UTF-16LE password bytes in RAR3 KDF` (separate commit, same PR)

### Problem recap

`src/SharpCompress/Common/Rar/CryptKey3.cs` builds a `(rawPassword.Length + 3) * 262144`-byte array
(≈5.8 MB → LOH) and calls `SHA1.HashData` over growing prefixes at 16 checkpoints plus once over the full
array — ~50+ MB hashed (O(n²)) where unrar does one incremental ~6 MB pass. Runs once per encrypted file
part (per-file salt), so many-file archives multiply the cost. The code is marked
`// See #23: O(n²) hashing + large LOH allocation in RAR3 KDF.`

### Decision (from issue comment — this is binding)

Add a small internal **copyable SHA-1** implementation (struct with copyable state) under
`src/SharpCompress/Crypto/`, mirroring unrar's `sha1_context` copy semantics in
`reference/unrar/crypt3.cpp` (in-repo reference): one ~6 MB incremental pass; at each of the 16
checkpoints, copy the running state, finalize the **copy** to produce the digest, take `digest[19]` as the
IV byte. No mega-array. Byte-exact output against existing encrypted RAR3 fixtures is mandatory.

### Implementation steps

1. **Read `reference/unrar/crypt3.cpp` first.** Confirm the exact checkpoint rule: unrar sets
   `Iv[i / (HashRounds/16)]` from a finalized *copy* of the sha1 context at `i % (HashRounds/16) == 0`.
   Note carefully whether the checkpoint digest is taken *before or after* feeding round `i`'s data —
   the current C# takes it **after** appending round `i`'s block (`(i + 1) * blockLen` prefix). Match the
   current C# observable output, which the fixtures lock in.
2. **Create `src/SharpCompress/Crypto/CopyableSha1.cs`** — internal struct:
   - Fields: `uint _h0.._h4`, `ulong _length`, `byte[64] _block` (fixed-size inline array or plain array),
     `int _blockLength`.
   - Methods: `void Append(ReadOnlySpan<byte> data)`, `void FinalizeTo(Span<byte> digest20)` (non-mutating
     on a copy: because it's a struct, `var copy = running; copy.FinalizeTo(dst);` gives checkpoint
     semantics for free), standard SHA-1 compression function.
   - Validate against `System.Security.Cryptography.SHA1` with randomized inputs in a unit test
     (lengths 0, 1, 55, 56, 64, 65, 1MB).
   - Keep the `CA5350` suppression pattern used in `CryptKey3.cs` (SHA-1 is format-mandated).
3. **Rewrite `CryptKey3.Transformer`:**
   - Build `rawPassword` (password-derived bytes + salt) once, as today (≤ ~64+8 bytes typical).
   - Single loop `i in 0..262143`: `sha.Append(rawPassword); sha.Append(counter3)` where `counter3` is the
     3 little-endian counter bytes written into a reusable `Span<byte>` scratch (stackalloc, size 3).
   - At each checkpoint (`i % 16384 == 0`, after appending — match step 1's finding): copy struct, finalize
     copy into a stackalloc 20-byte span, `aesIV[i / 16384] = digest[19]`.
   - After the loop, finalize the running state into `digest` and derive `aesKey` with the existing
     byte-order swizzle loop (keep it verbatim — it encodes RAR3's big-endian word layout).
   - Delete the `data` mega-array and the `//slow code ends` comment.
4. **Separate commit — non-ASCII password bug:** today `rawPassword` is sized `2 * _password.Length` but
   filled from `Encoding.UTF8.GetBytes(_password)` indexed per `char` — non-ASCII passwords mis-derive
   (and can throw for multi-byte chars). Fix to UTF-16LE, matching unrar (`WideToRaw`):
   ```csharp
   var passwordBytes = Encoding.Unicode.GetBytes(_password); // UTF-16LE, 2 bytes per char
   // copy passwordBytes directly, then append salt
   ```
   ASCII passwords produce identical bytes → existing fixtures still pass. Try to produce a RAR3 archive
   encrypted with a non-ASCII password (e.g. `WinRAR`/`rar a -hp пароль`) and add it under
   `tests/TestArchives/Original/Rar/`; if tooling is unavailable, document the coverage gap in the PR body
   as the decision comment allows.

### Tests

- New: `tests/SharpCompress.Test/Crypto/CopyableSha1Tests.cs` (digest parity vs `SHA1.HashData`).
- Existing encrypted-RAR3 tests must pass unchanged (byte-exact ground truth):
  `dotnet test … --filter "FullyQualifiedName~Rar"`.
- Optional micro-benchmark: key derivation in `RarEncryptedReadBenchmarks` style; expect >10× faster,
  near-zero alloc (no LOH). At minimum note before/after `Allocated` from the existing encrypted benchmarks
  (KDF cost is embedded there).

### Acceptance criteria

- No allocation proportional to `noOfRounds` remains in `CryptKey3`.
- Total bytes hashed ≈ 6 MB per derivation (one pass), not ~50 MB.
- Encrypted RAR3 fixtures decrypt byte-identically; UTF-16LE fix is its own commit.

---

## Issue #24 — [2.5] 7z async is sync under the hood — characterize, then fix or flip

**Branch:** `fix/sevenzip-async-24` · **Commit(s):** `fix(sevenzip): …` (+ `chore(docs): …` if doc-only outcome)

### Problem recap

`SevenZipArchive.SevenZipReader.GetEntryStreamAsync` (in
`src/SharpCompress/Archives/SevenZip/SevenZipArchive.cs`, ~line 196) is
`=> new(GetEntryStream());` with this comment:

> Sync fallback: LZMA decoder async paths have historically corrupted decoder state
> (IndexOutOfRangeException, DataErrorException). Prefer sync GetEntryStream until
> LzmaStream.ReadAsync / Decoder.CodeAsync / OutWindow async are fixed.

So "async" 7z extraction blocks thread-pool threads. **Decision comment (binding): characterize first.**
Also per the decision comment: `SyncOnlyStream` is already deleted — do not look for it; only the sync
fallback remains.

### Current async LZMA surface (files to study)

- `src/SharpCompress/Compressors/LZMA/LzmaStream.cs` + `LzmaStream.Async.cs`
- `src/SharpCompress/Compressors/LZMA/LzmaDecoder.cs` + `LzmaDecoder.Async.cs`
- `src/SharpCompress/Compressors/LZMA/LZ/LzOutWindow.cs` + `LzOutWindow.Async.cs`
- `src/SharpCompress/Common/SevenZip/ArchiveDatabase.Async.cs` — `GetFolderStreamAsync` already exists
- Test helper: `tests/SharpCompress.Test/Mocks/AsyncOnlyStream.cs` (forces async-only I/O)

### Step 1 — characterization stress test (do this before touching any decoder code)

Create `tests/SharpCompress.Test/SevenZip/SevenZipAsyncStressTests.cs`:

1. **Raw LZMA round-trip:** generate ≥64 MB of mixed compressible/random data in memory
   (deterministic seed), compress with the LZMA encoder (`LzmaStream` write path — LZMA encode is
   supported), then decode via `LzmaStream.ReadAsync` wrapped in `AsyncOnlyStream`, using read buffer
   sizes `{1, 7, 4096, 81920}`. Assert output equals input. Mark the 64 MB case
   `[Trait("format", "stress")]` or similar if runtime is a concern; a 4–8 MB variant should run in the
   default suite.
2. **7z end-to-end:** existing LZMA/LZMA2 fixtures from `tests/TestArchives/` read through
   `ArchiveFactory.OpenAsyncArchive` / `OpenEntryStreamAsync` and through the async Reader path, again with
   odd buffer sizes + `AsyncOnlyStream` wrapping the source where the API accepts a stream.
3. Run repeatedly (`for i in {1..20}`) — the historical failures were nondeterministic state corruption.

### Step 2A — if corruption reproduces

1. Diff the sync/async twin files **line by line** (`LzmaDecoder.cs` vs `LzmaDecoder.Async.cs`,
   `LzOutWindow.cs` vs `LzOutWindow.Async.cs`, `LzmaStream.cs` vs `LzmaStream.Async.cs`). The audit's
   hypothesis is drift between twins around pending-byte and window-flush handling. Fix the async twin to
   mirror sync semantics exactly; shared mutable fields used by both paths are the prime suspects.
2. Re-run the stress tests until stable across ≥20 iterations, then proceed to Step 2B.

### Step 2B — flip the async path on (either after fixing, or directly if no repro)

1. Replace the `SevenZipReader.GetEntryStreamAsync` sync fallback with a true async implementation
   mirroring the sync `GetEntryStream` folder-caching logic, using
   `_archive._database.GetFolderStreamAsync(…)` and async skip. Keep `_currentFolderStream` reuse
   semantics identical (dispose on folder change, create once per folder).
2. Audit `SevenZipArchiveEntry.OpenEntryStreamAsync` → `SevenZipFilePart.GetCompressedStreamAsync` — it
   already calls `GetFolderStreamAsync`; make sure nothing else routes async callers through sync decode.
3. Delete the sync-fallback comment. Keep the stress test as the permanent regression guard.
4. Update docs: `docs/API.md` (7z async notes), `docs/FORMATS.md` if it mentions the limitation.

### Step 2C — if a decoder fix is judged too risky in one PR

Keep the sync fallback, but: document loudly on `SevenZipArchive`/`SevenZipReader` XML docs and in
`docs/API.md` + `docs/FORMATS.md` that 7z async APIs currently execute synchronously (consumers should use
`Task.Run` isolation), and land the stress test with the failing case quarantined
(`Skip = "tracked by #24"`) so the repro is preserved. File the decoder fix as its own follow-up.

### Validation

```bash
dotnet test tests/SharpCompress.Test/SharpCompress.Test.csproj --filter "FullyQualifiedName~SevenZip"
dotnet run --project tests/SharpCompress.Performance -c Release -- --filter "*7Zip*"
```
The baseline shows async 7z at ~3× the sync cost (16.8 ms vs 5.7 ms) — a true async path should narrow or
at least not worsen this while unblocking thread-pool threads.

### Acceptance criteria

- Stress test exists and passes deterministically (or is explicitly quarantined with the issue link).
- Either the async path is real end-to-end, or docs state the sync-execution caveat — no silent lying API.

---

## Issue #25 — [2.6] 7z Archive API: stop re-decoding the solid folder per entry open

**Branch:** `fix/sevenzip-folder-cache-25` · **Commit:** `fix(sevenzip): cache folder stream and offsets for Archive API entry opens`

### Problem recap

`src/SharpCompress/Common/SevenZip/SevenZipFilePart.cs` `GetCompressedStream()` (sync, ~line 41; async twin
~line 63) builds a **fresh decoder chain** via `_database.GetFolderStream(...)` and decompress-discards all
preceding bytes in the folder on **every** entry open → reading N entries of a solid folder is
O(N · folder size). Also per open: `_database._folders.IndexOf(Folder!)` is an O(folders) reference scan,
and `skipSize` is recomputed with a loop. The forward-only `SevenZipReader` already caches
`_currentFolderStream` (see `SevenZipArchive.cs` ~line 174) — the Archive API does not.

### Implementation steps

1. **Cache the folder index on `SevenZipFilePart`** (cheap, zero risk):
   - The ctor already resolves `database._fileIndexToFolderIndexMap[index]` to assign `Folder`. Keep the
     `int` in a new `internal int FolderIndex { get; }` (set `-1` when `!Header.HasStream`).
   - Replace both `_database._folders.IndexOf(Folder!)` call sites with `FolderIndex`.
2. **Precompute cumulative in-folder offsets** (removes the per-open skip loop):
   - In `ArchiveDatabase` (`src/SharpCompress/Common/SevenZip/ArchiveDatabase.cs`), where
     `_folderStartFileIndex` / `_fileIndexToFolderIndexMap` are filled (`Fill`/`FillFolderStartFileIndex`,
     ~lines 60–105), also build `internal List<long> _fileInFolderOffset` — for file `i` with a stream,
     the sum of sizes of preceding in-folder files. Then `skipSize = _database._fileInFolderOffset[Index]`.
   - Both sync and async `GetCompressedStream*` use it.
3. **Add an MRU folder-stream cache for the Archive API** on `SevenZipArchive`:
   - State: `(int folderIndex, long positionInFolder, Stream stream)` for the most recently used folder
     stream. `positionInFolder` = decompressed bytes consumed so far.
   - In `SevenZipFilePart.GetCompressedStream` the part has no reference to the archive — so put the cache
     on `ArchiveDatabase` instead (it is per-archive and already flows into the file part). Add
     `internal Stream GetFolderStreamCached(Stream baseStream, int folderIndex, long targetOffset, IPasswordProvider pw)`:
     - If cached folder == requested folder and `targetOffset >= positionInFolder`: skip forward
       `targetOffset - positionInFolder` and reuse.
     - Else: dispose cached stream, build fresh via existing `GetFolderStream`, skip `targetOffset`.
     - Update `positionInFolder` as entries read — simplest correct accounting: after handing out an entry
       substream of length L starting at `targetOffset`, set `positionInFolder = targetOffset + L` and wrap
       the returned `ReadOnlySubStream` so early dispose drains the remainder of L from the folder stream
       (or invalidate the cache if the substream wasn't fully consumed — invalidation is the simpler,
       always-correct choice: track `bool fullyConsumed` via the substream's position on dispose).
   - `ReadOnlySubStream` over the cached folder stream must use `leaveOpen: true` (currently
     `leaveOpen: false` — the entry stream disposal must not kill the cached folder stream).
   - Dispose the cached stream in `SevenZipArchive.Dispose` and in the async twin.
   - **Concurrency:** same rule as #21 — one active entry stream per archive. Guard with an `Interlocked`
     flag on the cache (concurrent opens bypass the cache and take the old build-fresh path, or throw
     matching the RAR solid message — prefer bypass to avoid a behavioral break; document).
   - Mirror in `GetCompressedStreamAsync` using `GetFolderStreamAsync`. Coordinate with #24: if #24 lands
     first, both async paths exist; if not, the async twin still routes through the cache but decodes
     synchronously underneath — fine.
3. **Docs note:** in `docs/FORMATS.md` / `docs/API.md`, state that solid 7z sequential entry opens now
   reuse the decoder, and that *backward* seeks within a solid folder still require a full re-decode
   (inherent to the format).

### Tests

- New: open all entries of a solid 7z **sequentially via the Archive API** (`entry.OpenEntryStream()` in
  order) and byte-compare each against `ExtractAllEntries` reference output. Fixture: any solid archive in
  `tests/TestArchives/` (`7Zip.solid.7z` exists — verify name via `ls tests/TestArchives/Original/7Zip/`).
- New: out-of-order open (entry 3, then entry 1) still correct (cache invalidation path).
- New: open an entry stream, dispose it half-read, open the next entry — correct output (the
  fully-consumed=false invalidation path).
- Run: `dotnet test … --filter "FullyQualifiedName~SevenZip"`.

### Benchmarks

`tests/SharpCompress.Performance/Benchmarks/SevenZipSolidRandomAccessBenchmarks.cs` exists
("7z solid: open each entry via Archive API"). Baseline mean ~5.6 ms; expect roughly O(N²)→O(N) — a large
drop for multi-entry solid folders. Cite before/after in the PR.

### Acceptance criteria

- Sequential Archive-API reads of a solid folder decode the folder ~once total, not once per entry.
- No `IndexOf(Folder)` or per-open `skipSize` loops remain.
- Out-of-order and early-dispose paths still produce correct bytes (tests above).

---

## Issue #26 — [2.7] `EntryStream.Dispose` must not silently download the rest of the entry

**Branch:** `feat/reader-cancel-on-dispose-26` · **Commits:**
1. `feat(reader): add ReaderOptions.CancelOnEntryStreamDispose`
2. `fix(stream): seek past remaining entry bytes on skip when length is known` (independent, same PR or separate)

### Problem recap

Disposing an `EntryStream` before EOF calls `SkipEntry()` → `this.Skip()` → `CopyTo(Stream.Null)` — on
network-backed streams that downloads the entire remainder synchronously with no cancellation. The escape
hatch (`IReader.Cancel()` before dispose) is undocumented.

**Decision (binding):** add `bool ReaderOptions.CancelOnEntryStreamDispose` (default `false` — current
drain behavior preserved). When `true`, `EntryStream.Dispose` calls `_reader.Cancel()` instead of draining.
Independently, add the seek-past optimization where remaining length is known, including auditing the
`SharpCompressStream` exclusion in the skip fast path.

### Current code (verified)

- `src/SharpCompress/Common/EntryStream.cs` — `Dispose(bool)`: `if (!(_completed || _reader.Cancelled)) SkipEntry();`
- `src/SharpCompress/Common/EntryStream.Async.cs` — `DisposeAsync()` mirrors it via `SkipEntryAsync()`.
- `src/SharpCompress/IO/StreamExtensions.cs` (namespace `SharpCompress`, C# `extension(Stream)` members):
  - `Skip(long advanceAmount)` — fast-path `Position +=` when `CanSeek && stream is not SharpCompressStream`.
  - `Skip()` / `SkipAsync()` — unconditional `CopyTo(Stream.Null)`.
- `src/SharpCompress/Readers/IReader.cs` — `void Cancel();` exists; `AbstractReader` sets `Cancelled`.
- `src/SharpCompress/Readers/ReaderOptions.cs` — `sealed record` with `get; set;` properties and fluent
  `WithX` helpers + presets (`ForExternalStream`, …).

### Implementation steps — part 1 (the option)

1. Add to `ReaderOptions`:
   ```csharp
   /// <summary>
   /// When true, disposing an entry stream before it is fully read cancels the reader
   /// (IReader.Cancel()) instead of draining the remaining entry bytes. After cancellation,
   /// MoveToNextEntry() returns false. Default false: dispose drains to the end of the entry
   /// so the reader can continue — on remote/network sources that download the remainder.
   /// </summary>
   public bool CancelOnEntryStreamDispose { get; set; }
   ```
   Follow the existing XML-doc + fluent style; add a `WithCancelOnEntryStreamDispose(bool)` helper only if
   the neighboring options all have one (match file conventions).
2. `EntryStream` needs access to the option. It already holds `_reader` (an `IReader`); `IReader` exposes
   options via `AbstractReader.Options` — check whether `IReader` itself surfaces `ReaderOptions` (grep
   `interface IReader`). If not, pass a bool into the `EntryStream` ctor from `AbstractReader.CreateEntryStream`
   (internal ctor — safe).
3. In `Dispose(bool)` and `DisposeAsync()`:
   ```csharp
   if (!(_completed || _reader.Cancelled))
   {
       if (_cancelOnDispose) { _reader.Cancel(); }
       else { SkipEntry(); /* or await SkipEntryAsync() */ }
   }
   ```
   Everything else (deflate/LZMA knock-back flushes, `_stream.Dispose()`) stays unchanged.
4. **Archive API note:** `EntryStream` is also used by archive entry streams where `_reader` may be a
   dummy/internal reader. Verify which `IReader` instance archive paths pass; the option must be a no-op
   there if `Cancel()` would corrupt archive state (check `AbstractArchive`'s entry-stream creation).
   If archives don't flow `ReaderOptions` into `EntryStream`, scope the feature to Reader API and say so
   in the XML docs.
5. Docs: `docs/USAGE.md` — new subsection "Abandoning an entry early" documenting default drain cost, the
   new option, and the `IReader.Cancel()` interplay; XML docs on `IReader.OpenEntryStream` implementations
   if they carry remarks today. `docs/API.md` — option table entry.

### Implementation steps — part 2 (seek-past optimization, independent)

1. Audit why `Skip(long)` excludes `SharpCompressStream` (`stream is not SharpCompressStream`): the wrapper
   tracks `_logicalPosition`/ring buffer, and a raw `Position +=` bypasses its accounting **but**
   `SharpCompressStream.Position` setter routes through `SeekToPosition` which handles buffered ranges —
   for the seekable `SeekableSharpCompressStream` subclass a position bump is valid. Extend the fast path:
   `stream.CanSeek && (stream is not SharpCompressStream scs || scs is SeekableSharpCompressStream)` —
   validate with the `Streams` test suites (`SharpCompressStreamSeekTest`, `StreamStackRewindTests`).
2. `EntryStream.SkipEntry` currently drains via parameterless `Skip()`. Where the wrapped `_stream` exposes
   a known remaining length over a seekable source (`ReadOnlySubStream` and `TarReadOnlySubStream` — check
   for a `BytesLeftToRead`/`Remaining` internal), replace drain with a bounded `Skip(remaining)` so the
   seekable fast path can kick in. Keep the unconditional drain for everything else. **Do not** skip bytes
   that feed CRC/format validation on paths where tests prove it matters — the full suite is the arbiter.

### Tests

New file `tests/SharpCompress.Test/Readers/EntryStreamDisposeTests.cs` (match neighboring naming):

1. Wrap a fixture (e.g. a multi-entry zip/tar from `tests/TestArchives`) in a **counting stream** (model on
   `tests/SharpCompress.Test/Mocks/` — add a simple `CountingReadStream` mock if none exists; note
   `src/SharpCompress/IO/CountingStream.cs` is internal to the lib).
2. Default options: dispose entry stream at ~10% read → underlying read count increases (drain happened),
   `MoveToNextEntry()` still works. (Locks in back-compat.)
3. `CancelOnEntryStreamDispose = true`: dispose at 10% → **no further reads** on the counting stream,
   subsequent `MoveToNextEntry()` returns `false`, `reader.Cancelled == true`.
4. Async twins via `OpenAsyncReader` + `DisposeAsync`.
5. Seek-past part: full suite `dotnet test tests/SharpCompress.Test/SharpCompress.Test.csproj` (touches all
   formats' skip paths).

### Acceptance criteria

- Default behavior byte-for-byte unchanged (test 2).
- Opt-in cancel: zero post-dispose reads (test 3).
- Docs updated (`docs/API.md`, `docs/USAGE.md`, XML).
- Seek-past change lands only with a fully green suite.

---

## Issue #27 — [2.8] Stop ring-buffer recording after format detection

**Branch:** `fix/stream-ring-buffer-27` · **Commit:** `fix(stream): release ring buffer after format detection on non-seekable streams`

> Coupled with #43: issue #43's comment says this fix should land as part of the `SharpCompressStream`
> restructuring. Sequence per the execution order below — #43 Phase 1 (invariants doc) first, then this
> issue, then #43 Phase 3 (mode split).

### Problem recap

`SharpCompressStream.Create(nonSeekable)` always allocates the ring buffer
(`src/SharpCompress/IO/SharpCompressStream.Create.cs`, final branch), and `ReadWithRingBuffer`
(`SharpCompressStream.cs` ~line 380) copies **every byte read** into the ring buffer for the stream's
lifetime — an extra full-stream memcpy plus ~80 KB pooled memory held per stream, long after detection
finished.

### Current state (important — partially improved since the issue was filed)

- `StopRecording()` exists but only stops the *anchor* semantics and rewinds `_logicalPosition`; reads
  still write through the ring buffer afterwards ("frozen recording mode" keeps `_recordingStartPosition`).
- `GZipFactory` (~line 152), `LzwFactory` (~line 70), `TarFactory` (~line 318) already call
  `StopRecording()` after probing. `ReaderFactory.OpenReader` drives `StartRecording()`/`Rewind()` during
  detection — read `src/SharpCompress/Factories/*` and `Readers/ReaderFactory.cs` before editing.
- Over-read protection is a second, *legitimate* consumer of the ring buffer: deflate/LZMA over-read
  knock-back (`EntryStream.Dispose` flush + `IStreamStack`), Zip's `DataDescriptorStream` scanning, and
  BZip2/ZStandard Tar wrappers sizing via `TarWrapper.MinimumRewindBufferSize` /
  `MaximumRewindBufferSize` (see `src/SharpCompress/Common/Constants.cs` remarks). **Do not** break these.

### Implementation steps

1. **Add a release mechanism** to `SharpCompressStream`:
   ```csharp
   /// <summary>
   /// Requests release of the ring buffer once all replayed bytes have been consumed.
   /// After release, reads pass straight through and Rewind/StartRecording throw.
   /// </summary>
   public virtual void FreezeAndReleaseBuffer()   // pick a name consistent with StopRecording
   ```
   - Sets `_bufferReleaseRequested = true`, clears `_recordingStartPosition`, `_isRecording = false`.
   - Cannot free immediately in the general case: `_logicalPosition` may be behind `streamPosition`
     (rewound-but-unread bytes must still be replayed). In `ReadWithRingBuffer`, after the catch-up loop,
     when `_bufferReleaseRequested && _logicalPosition == streamPosition`: dispose + null `_ringBuffer`.
     Subsequent `Read` calls then take the existing direct-read branch (`_ringBuffer is null`).
   - Guard `StartRecording`/`Rewind`/`StopRecording` after release with the existing
     `ArchiveOperationException` pattern.
   - `SeekableSharpCompressStream` (no ring buffer): make it a no-op, mirroring its `StopRecording_IsNoOp`
     test.
2. **DEBUG diagnostics:** under `#if DEBUG`, assert (throw) if `Rewind`/`TrySetBufferedPosition`
   backwards-seek is attempted after release — this is the tripwire for formats that still rely on late
   rewinds.
3. **Wire the call sites conservatively (allowlist, not default):**
   - In `ReaderFactory.OpenReader`/`OpenAsyncReader`: after a factory is selected and the reader
     constructed, call `FreezeAndReleaseBuffer()` **only** for formats known not to need post-detection
     rewind/over-read protection. Start with: Tar (uncompressed), GZip single-stream, LZW — i.e. exactly
     the factories that already call `StopRecording()` today (change those calls to the new method).
   - Explicitly **exclude** for now: Zip Reader (non-seekable data-descriptor scanning via
     `src/SharpCompress/IO/DataDescriptorStream.cs` — inspect whether it rewinds through the wrapper before
     deciding), anything routed through deflate/LZMA knock-back, and Tar-over-BZip2/ZStandard (the
     `MinimumRewindBufferSize` consumers).
   - For each candidate format, flip it on, run the **full** suite, keep it only if green with the DEBUG
     tripwire active. Document the resulting allowlist in a comment at the call site.
4. **Do not** change `SharpCompressStream.Create`'s allocation behavior in this PR (lazy allocation is a
   possible follow-up; the win here is releasing after detection, which also stops the per-read memcpy).

### Tests

- New tests in `tests/SharpCompress.Test/Streams/` (model on `SharpCompressStreamPropertyTest` /
  `…ErrorTest` naming):
  - Release while logical position is behind: replayed bytes still correct, buffer freed after catch-up,
    subsequent reads correct.
  - `Rewind` after release throws `ArchiveOperationException`.
  - Passthrough + seekable variants: no-op / throw per contract.
- Full suite — this touches every format's read path:
  `dotnet test tests/SharpCompress.Test/SharpCompress.Test.csproj -c Release` (run **Debug** config too, so
  the `#if DEBUG` tripwire is exercised: `dotnet test tests/SharpCompress.Test/SharpCompress.Test.csproj`).

### Benchmarks

Reader-API streaming decode over non-seekable sources: `TarBenchmarks` ("Tar: Extract all entries
(Reader API)"), `GZipBenchmarks`, `RarStoredStreamingBenchmarks` (non-seekable variant). Expect measurable
throughput gain and ~80 KB less retained memory per open stream; cite before/after `Allocated`.

### Acceptance criteria

- After detection, allowlisted formats read with zero ring-buffer memcpy and the 80 KB pooled buffer is
  returned to the pool.
- No format regresses (full suite green in Debug + Release).
- The DEBUG tripwire exists so future formats can't silently depend on late rewinds.

---

## Issue #43 — [5.3] `SharpCompressStream`: document invariants, split modes, remove test hooks

**Branch:** `chore/stream-modes-43` · **Commits (one per phase):**
1. `chore(stream): document SharpCompressStream mode invariants`
2. `chore(stream): move ThrowOnDispose assertion into a test utility`
3. `chore(stream): split passthrough mode into its own type`

### Problem recap

One class serves four behavioral modes — passthrough / ring-buffered / recording / frozen-recording — plus
the `SeekableSharpCompressStream` subclass, and carries a test-only `ThrowOnDispose` flag in production.
`Create` has non-obvious unwrap rules (returns inner streams as-is; `bufferSize` is ignored for seekable
and already-wrapped inputs). Every format's correctness depends on this class, and its invariants live only
in scattered comments. The issue comment (binding) says the #27 fix (freeze/detach buffer after detection)
should land as part of this restructuring — hence the coordination below.

### Current code (verified 2026-07-13)

| Item | Location |
|---|---|
| `internal bool ThrowOnDispose { get; set; }` | `src/SharpCompress/IO/SharpCompressStream.cs` (~line 53) |
| Dispose-time checks of the flag (4 sites) | `SharpCompressStream.cs` `Dispose` (~line 93), `SharpCompressStream.Async.cs` `DisposeAsync` (~line 252), `SeekableSharpCompressStream.cs` (~line 101), `SeekableSharpCompressStream.Async.cs` (~line 34) |
| Test consumers that **set** it | `tests/SharpCompress.Test/ArchiveTests.cs` (multiple sites — arms the flag, later sets it `false` to "disarm") |
| Tests that **assert** the throw | `Streams/SharpCompressStreamEdgeTest.cs`, `Streams/SharpCompressStreamErrorAsyncTest.cs`, `Streams/SharpCompressStreamPassthroughAsyncTest.cs` |
| Mode selection / unwrap rules | `src/SharpCompress/IO/SharpCompressStream.Create.cs` (`Create`, `CreateNonDisposing`) |

### Phase 1 — write down the invariants (docs only, zero behavior change)

Expand the class-level `<remarks>` on `SharpCompressStream` (and `Create`/`CreateNonDisposing` XML docs
where the detail belongs there) to state, per mode:

1. **Ownership:** who disposes the underlying stream (`LeaveStreamOpen` per mode; `CreateNonDisposing`
   never disposes).
2. **`Position` semantics:** passthrough delegates to the underlying stream; buffered modes report
   `_logicalPosition` (which can lag `streamPosition` after a rewind).
3. **Recording rules:** when reads are copied into the ring buffer, what `StartRecording(minBufferSize)`
   guarantees, what "frozen recording" means after `StopRecording()`/`Rewind(stopRecording: true)`.
4. **Legal call sequences:** `StartRecording → (reads) → Rewind [→ Rewind…] → StopRecording`; which calls
   throw in passthrough mode; what is legal after buffer release (once #27 lands).
5. **`Create` guarantees for already-wrapped inputs:** existing `SharpCompressStream` returned as-is
   (non-passthrough), passthrough unwrapped and rewrapped, `IStreamStack` inner `SharpCompressStream`
   returned as-is, and the cases where `bufferSize` is silently ignored (seekable / already-wrapped).

Source the facts from the code, not memory — read `SharpCompressStream.cs`, `.Async.cs`, `.Create.cs`, and
`SeekableSharpCompressStream.cs` in full first. This phase is the prerequisite for #27's call-site
allowlist work and for Phase 3's split.

### Phase 2 — replace `ThrowOnDispose` with a test-side utility

1. Add `tests/SharpCompress.Test/Mocks/DisposalGuardStream.cs` (name to match `Mocks/` conventions): a
   wrapper `Stream` that throws from `Dispose`/`DisposeAsync` while "armed", with an `Allow()`/`Disarm()`
   method replicating today's `stream.ThrowOnDispose = false` disarm pattern used by `ArchiveTests.cs`.
2. Rework the `ArchiveTests.cs` sites: wrap the fixture stream in the guard **before** handing it to the
   archive/reader instead of reaching into the wrapper afterwards. Note the guard now sits **under**
   `SharpCompressStream` rather than being it — the assertion still catches premature disposal of the
   caller's stream, which is the actual intent of those tests.
3. Rewrite the three `Streams/*Test.cs` tests that assert the flag's throw behavior: they become tests of
   the guard utility itself, or are deleted if redundant (the flag they tested no longer exists —
   deleting production-flag tests alongside the flag is correct, not a coverage loss).
4. Delete `ThrowOnDispose` and all four production check sites.
5. Full suite green. This phase is independent of #27 and can land any time.

### Phase 3 — split the modes (structural, behavior-preserving)

Sequencing: land **after** #27's freeze/release change is merged (the issue comment couples them; doing
the split first would force #27 to be written twice).

1. Make passthrough its own internal type (e.g. `PassthroughSharpCompressStream : SharpCompressStream`),
   or equivalently make `SeekableSharpCompressStream` the only seekable path — target state: **each
   concrete class implements exactly one buffering strategy**, and `_isPassthrough` branching disappears
   from `Read`/`Position`/`Flush`/`CanSeek`/etc.
2. `Create`/`CreateNonDisposing` keep their signatures and documented guarantees (Phase 1 text is the
   contract); only the concrete runtime types change. `SharpCompressStream` itself stays `public` with its
   public members preserved — the issue is explicitly **Breaking: no**.
3. Audit `is SharpCompressStream` / `is SeekableSharpCompressStream` type checks across the codebase
   (`grep -r "SharpCompressStream" src/`) — e.g. `StreamExtensions.Skip`'s exclusion (see #26) and
   `IStreamStack` walkers — and make sure the new subtype(s) keep those checks truthful.
4. Move mode-specific members down: recording/ring-buffer members out of the passthrough type (they throw
   there today — after the split they simply don't exist on it; keep `virtual` members on the base only
   where a subclass genuinely overrides).

### Tests & validation

- No new behavior: the full suite in Debug and Release is the arbiter for Phases 2–3:
  `dotnet test tests/SharpCompress.Test/SharpCompress.Test.csproj` and `… -c Release`.
- `Streams/` suites (`SharpCompressStreamPassthroughTest`, `…SeekTest`, `…ErrorTest`,
  `SeekableSharpCompressStreamTest`, `StreamStackRewindTests`) are the mode-contract tests — they must
  pass unmodified in Phase 3 (only Phase 2's flag tests get rewritten).
- `dotnet csharpier check .`; no public API diff (verify with a before/after `git diff` of public members
  or the AotSmoke build: `dotnet build tests/SharpCompress.AotSmoke`).

### Acceptance criteria

- Class-level invariants documented where the code lives (Phase 1).
- Zero test-only members in `src/` (`ThrowOnDispose` gone; no replacement flag added).
- One buffering strategy per concrete class; `Create` behavior and public API unchanged.
- Full suite green in both configurations.

---

## Suggested execution order

1. **#23** (S, isolated file, byte-exact fixtures as ground truth) — good warm-up, no interactions.
2. **#22** (M, crypto path; independent of #23 apart from both being "crypto" — different files).
3. **#26** (S, Reader options + EntryStream; small surface).
4. **#43 Phases 1–2** (docs + `ThrowOnDispose` removal; low risk, and Phase 1's invariants doc is the
   groundwork #27 builds on).
5. **#27** (M, stream infra; run after #26 since both touch skip/drain behavior — #26's tests help catch
   #27 regressions).
6. **#43 Phase 3** (mode split; explicitly after #27 per the issue comment coupling them).
7. **#24** (L, characterize first; outcome gates scope).
8. **#25** (M, SevenZip; do after #24 to avoid rebasing the `SevenZipArchive.cs` / `SevenZipFilePart.cs`
   async plumbing twice — or before it if #24 stalls in characterization, accepting a small rebase).

Conflict surface to watch: #24 and #25 both edit `SevenZipArchive.cs` + `SevenZipFilePart.cs`; #26 and #27
both touch skip/stream-extension behavior; #27 and #43 both restructure `SharpCompressStream` (respect the
phase ordering above); #22 touches files that #21's landed fix already synchronized — rebase onto current
`main` (`8ba2a5a8` or later) before starting each issue.

## Definition of done (per issue)

- [ ] Code + tests + docs updated in one PR referencing the issue (`Closes #NN`).
- [ ] `dotnet csharpier check .` clean; `dotnet restore --locked-mode` still succeeds (no unlocked dep changes).
- [ ] Full test suite green: `dotnet test tests/SharpCompress.Test/SharpCompress.Test.csproj -c Release`.
- [ ] Relevant benchmark run before/after; **Allocated** deltas quoted in the PR description (wall-clock
      deltas vs the checked-in baseline are advisory only — see CI note in Ground Rules).
- [ ] No public-API break; additive options default to current behavior.
