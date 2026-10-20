# Zstandard translated to C#

This campaign translates the upstream portable Zstandard library with DotCC and
delivers an owning .NET API and a separate file-archive application. The selected
portable single-thread core is qualified on Linux x64 in raw/processed JIT and
NativeAOT forms, including clean regeneration and the file consumer. See
[the plan](docs/PLAN.md), [validation](docs/validation.md) and
[the resolved blocker ledger](docs/blockers.md) for evidence.

The default `portable` profile uses 42 typed .NET primitive replacements built
with `Unsafe`, `BinaryPrimitives` and `BitOperations`. The complete correctness
matrix passes; the final local benchmark improves every measured workload, with
128 KiB level 3 compression 3.77–4.29× faster and decompression 1.40–1.49× faster.
See [contracts, preserved baseline and measurements](docs/optimizations.md) for
workloads, dispersion, limits and separate historical acceptance evidence.

The product selects the complete portable standard library core: all compression
strategies, long-distance matching, decompression, streaming, checksums,
dictionary use and dictionary training. Worker-thread compression, legacy frames
before v0.8, architecture-specific assembly/intrinsics and optional native
weak-symbol tracing hooks are excluded. Source
pins and configuration live in [config](config/); upstream references remain in
`ref/` and generated output is reproducible through the shared campaign runner.
The delivered codec has no native libzstd backend.

Use .NET 10 and Python 3.11 or newer. Native/AOT qualification additionally needs
a C toolchain, CMake and the .NET Linux x64 NativeAOT prerequisites.

```bash
bash zstd/scripts/fetch.sh
DOTCC_CAMPAIGN_COMMAND_LOCKS=production bash zstd/scripts/translate.sh --jobs 2
dotnet build zstd/ManagedConsumer.slnx -c Release -m:1
bash zstd/scripts/test.sh --suite consumer --form all --mode all --rid linux-x64
DOTCC_CAMPAIGN_COMMAND_LOCKS=production bash zstd/scripts/verify.sh --fetch never --jobs 2 --rid linux-x64
```

`build.sh` builds existing products. `translate.sh` emits every selected C unit,
builds raw and processed forms and publishes transactionally. `verify.sh` also
runs the declared native, primitive, ABI, consumer, behavior, lifecycle, interoperability,
upstream-case and dependency audits. Use `--hashes off` to disable provenance
hashing and `--fetch never` to use existing inputs; NuGet restore is controlled
separately with `--restore`. Scripts resolve paths from their own locations.

Reference [Managed.Zstd.csproj](src/Managed.Zstd/Managed.Zstd.csproj) normally.
Its `ZstdProject` property selects the generated product and defaults to the
processed form. The raw form is
`generated/TranslatedZstd.Raw/TranslatedZstd.csproj`.

The explicit `--profile original` opt-out emits unchanged C primitive bodies to
`generated/profiles/original/TranslatedZstd/TranslatedZstd.csproj` (and the `.Raw`
sibling). Pass that project as `-p:ZstdProject=ABSOLUTE_PATH` when building the API
or consumer. It shares the codec scope and API, without the managed adapter.
Its current profile-specific smoke scope is recorded in the optimization report.

```csharp
using Managed.Zstd;

byte[] frame = ZstdCodec.Compress(payload, level: 9);
byte[] restored = ZstdCodec.Decompress(frame, maxOutputBytes: 64 * 1024 * 1024);

using var inputStream = new MemoryStream(payload);
using var compressedStream = new MemoryStream();
using var restoredStream = new MemoryStream();
using var compressor = new ZstdCompressor();
compressor.Compress(inputStream, compressedStream);
compressedStream.Position = 0; // Rewind the seekable frame before decoding.
using var decoder = new ZstdDecompressor();
decoder.Decompress(compressedStream, restoredStream, maxOutputBytes: 1L << 30);

ZstdDictionary dictionary = ZstdDictionary.Train(sampleRecords);
byte[] dictionaryFrame = ZstdCodec.Compress(payload, dictionary: dictionary);
byte[] dictionaryResult = ZstdCodec.Decompress(dictionaryFrame, dictionary);
```

The streams must be positioned at the intended input before each operation.
Convenience decompression bounds decoded output and never allocates from an
untrusted advertised frame size. Byte-array decoding defaults to 256 MiB; stream
decoding defaults to 1 GiB. Pass a suitable explicit bound for larger data.
Unknown-size, concatenated and skippable frames are decoded incrementally.

Codec objects own upstream contexts and serialize calls and disposal per object.
Independent codec objects can run concurrently. Dictionaries copy their input;
upstream contexts also copy loaded dictionary data. Call `Reset` after an upstream
error before reusing a context. `Dispose` is idempotent and later calls reject
disposed objects. Span addresses remain pinned for each synchronous call only.
A private process-lifetime DotCC runtime preserves translated static table roots
and allocator identity and restores the caller's ambient runtime binding.

The separate sample supports:

```bash
dotnet run --project zstd/samples/ManagedConsumer -c Release -- pack INPUT_DIRECTORY output.dzar.zst
dotnet run --project zstd/samples/ManagedConsumer -c Release -- unpack output.dzar.zst NEW_DIRECTORY
dotnet run --project zstd/samples/ManagedConsumer -c Release -- train SAMPLE_DIRECTORY output.dict
```

Append a dictionary path to `pack`/`unpack` to use that dictionary. The archive is
a standard zstd frame containing the sample's `DZAR1` multi-file envelope with
UTF-8 paths and binary file content. A native zstd decoder can decode the frame;
the sample interprets the inner archive. The consumer self-test trains a
dictionary, packs/restores four files with and without it, and compares file
inventory and content digests.

Generated build/publish output preserves upstream LICENSE, COPYING and selected
third-party notices, including libdivsufsort's permission notice. Linux x64
raw/processed JIT/NativeAOT is the initial qualification matrix. Other targets
remain unqualified until their own execution gates pass.
