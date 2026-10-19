`Managed.Zstd` owns contexts from the translated standard single-thread zstd
core. Its only backend is the selected `TranslatedZstd` project. Set the
`ZstdProject` MSBuild property to select raw or processed output; the separate
sample and tests pass that property through their ordinary project references.

```csharp
using Managed.Zstd;

byte[] frame = ZstdCodec.Compress(payload, level: 9);
byte[] restored = ZstdCodec.Decompress(frame, maxOutputBytes: 16 * 1024 * 1024);

using var inputStream = new MemoryStream(payload);
using var compressedStream = new MemoryStream();
using var outputStream = new MemoryStream();
using var compressor = new ZstdCompressor();
compressor.Compress(inputStream, compressedStream);
compressedStream.Position = 0; // Rewind the seekable frame before decoding.
using var decoder = new ZstdDecompressor();
decoder.Decompress(compressedStream, outputStream, maxOutputBytes: 1L << 30);
```

Contexts implement `IDisposable` and use SafeHandle finalization as fallback.
All calls and disposal serialize per context. Buffers are pinned only during a
synchronous call. A dictionary copies its input bytes; loading a dictionary
makes another upstream-owned copy, so neither spans nor dictionary objects must
stay pinned or alive after loading. `LoadDictionary(null)` clears a dictionary.
`ZstdDictionary.Train` accepts independent records and returns portable trained
dictionary bytes. `ZstdDictionary` also accepts raw-content dictionaries.

`Process` exposes consumed/written byte counts and the original remaining hint.
Call it repeatedly while retaining unconsumed input. `Flush` and `End` must be
repeated until `Completed` is true. Interpret compression `Completed` only for
these two directives: `Continue` may return zero without ending a frame.
For decompression `Completed` marks a frame
boundary; feed further input to read concatenated or skippable frames. Empty
output is valid when input was consumed. The stream helpers handle these loops
and reject truncated frames. They leave input and output streams open. A failed
stream copy may have written partial output; discard that output before retrying.

An upstream error raises `ZstdException` with its original error code. After an
upstream context error, call `Reset` before using that context again. Reset keeps
parameters and the copied dictionary. One-shot convenience decoding uses bounded
incremental output, supports unknown content size, and never allocates an output
array directly from an advertised frame size. The span overload accepts at most
`int.MaxValue` output bytes; use stream operations for larger content.

`SetParameter` accepts only declared facade enum values. Experimental stable
input/output buffer modes require addresses to survive across calls, so the
facade rejects those modes before translated entry. `Workers=0` selects this
single-thread profile; positive worker requests receive the original upstream
unsupported-parameter error. The complete generated API remains available to
callers that implement its lower-level ownership contracts themselves.

Every translated call enters one private process-lifetime libc runtime and
restores the caller's binding afterward. This preserves static table roots and
allocator ownership, including finalizer frees. Independent compression contexts
can run concurrently; this API does not expose separately disposable translated
program instances or promise isolation between such instances.
