# EDG msquic BCL host

`../provider.py` prepares isolated projects that link the generated msquic and
picotls C# unchanged. The existing authored `msquic/src/BclHost` supplies socket
I/O, scheduling, synchronization, packet cryptography and the picotls TLS adapter;
`msquic/src/ManagedApi` supplies the transport facade. Picotls uses the same BCL
provider preparation as `picotls/edg/provider.py`.

The preparation manifest records hashes of generated and authored inputs,
adapted copies, and every source spelling replacement. Original dotcc projects
and sources are unchanged. Only authored sources are adapted for the EDG ABI.

`HostLibrary.cs` is compiled into the generated core's assembly and exposes the
source-named `MsQuicLib` global by reference. This supports the existing host's
cleanup of the drained registration rundown event without hard-coded offsets.

`EdgPlatformMemory.cs` replaces the authored host's pinned-array allocation
methods in the copied source. Transport buffers and objects use
`NativeMemory.AlignedAlloc` / `AlignedFree`, preserve 16-byte alignment, zeroing,
size limits and tag/domain ownership checks. The host refuses disposal while
allocations or resources remain live. Pinned arrays remain appropriate for
translated global storage; TLS/provider buffers that transfer ownership to
picotls use its allocator bridge.

Preparation:

```sh
python3 msquic/edg/provider.py \
  --core /path/to/msquic/managed \
  --picotls-core /path/to/picotls/managed \
  --output /path/to/isolated-provider
```

Preparation alone does not establish runtime correctness. The verification
runner in `../tests` records builds, native ABI comparison and transport tests.

The adapter resolves anonymous aggregate field paths and enum collision suffixes
from the emitted record declarations, and records typedef spelling, signed
`char*`, and inline-array address changes. The original source defines the same
ABI; these changes only accommodate its two C# representations. The generated
picotls provider project also retains the `MsQuic.BclHost` friend assembly used
by the original provider's application certificate validation hook.

The combined Release project graph passed a warnings-as-errors build using
`msquic-edg-011/managed` and `msquic-picotls-edg-002/managed`, prepared under
the final test campaign provider directory in the persistent campaign directory. Execution
results are recorded separately by the verification runner.

`audit.py --prepared /path/to/isolated-provider --output /path/to/audit.json`
walks that project graph and rechecks source hashes. The initial integration has
five projects and 248 compile inputs, no package references, and no original
dotcc generated sources or native msquic/picotls/OpenSSL dependencies. Existing
shared libc/POSIX helpers retain runtime-selected OS imports; the audit records
those declarations separately.

The existing managed facade still pins asynchronous managed send buffers,
descriptors and protocol-name storage where those buffers are lent to the
translated engine. The native allocator change applies to the PAL allocation
callbacks and their transport objects/buffers, with callback/drain ownership
unchanged.
