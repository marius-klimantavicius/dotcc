# Private file snapshots for host mmap

The host memory owner can explicitly enable file-backed `MAP_PRIVATE` requests
over a bound instance's private file data. It allocates real aligned owned
memory and copies bytes through synchronous positional reads. It never calls
OS mmap, opens a host path, or substitutes guest page-table/loader algorithms.

This supports the exact read/write private mapping shape at upstream
`loader.c:449` and `:726`. The existing `Mmap` / `PortableMmap` wrappers forward
that request without algorithm changes. The loader's `pread` header check at
`:685` also has an authored positional-read definition. Its separate
`MAP_SHARED` inspection at `:849` remains unsupported; this work does not claim
that the full managed loader or guest startup now executes.

## Binding and lifetime

Include `src/HostMemory/HostFileMapping.c` and `HostFileMapping.h` in the managed
C source/header snapshot, with the updated `HostMemory.c/h` and `HostIoBridge.cs`.
The C translation unit defines `pread` and a read-length wrapper, forwarding to
the fixed callbacks `blink_io_pread` and `blink_io_read_at_length`. It also exports
`BlinkHostMemoryEnablePrivateFiles()`.

The worker must bind its `InstanceIo`, successfully call `BlinkHostMemoryBegin`,
then enable private files before issuing file mappings. Enable registers two
ordinary C function pointers in that worker's memory owner; no CLR reference is
stored in C storage. The generic `BlinkHostMemorySetFileReader` contract is
synchronous: callbacks must borrow buffers only for the call, preserve fd
position, and report errors through errno. Anonymous-only users need no I/O
source or callback binding.

Successful End and worker disposal clear both callbacks. A later owner cannot
inherit the previous file binding. Mapped payload has independent ownership, so
closing the source fd or disposing the entire `InstanceIo` does not invalidate
an existing snapshot. Existing upstream slab-cache teardown restrictions still
apply to the memory owner; file mapping does not make FreeSystem sufficient for
worker reuse.

## Supported mappings and boundaries

Accepted requests have a null preferred address, exactly `MAP_PRIVATE`, exactly
`PROT_READ | PROT_WRITE`, a readable regular private file descriptor, a positive
length, and a nonnegative offset aligned to 4096. Read-only protection is
rejected because this allocation model cannot enforce it. Shared, fixed, exec,
protection-change, sync, and other flag/protection combinations remain explicit
`ENOTSUP` errors.

The entire rounded page range must lie within existing file pages. A final
partial file page is copied through EOF and its remaining bytes are zero.
Requests beginning at/beyond EOF, empty files, and requests that include a whole
page beyond EOF return `ENOTSUP`. Native Linux allows those latter mappings but
raises SIGBUS when their absent pages are accessed; this adapter deliberately
rejects the mapping instead of making those pages silently readable. It does
not emulate SIGBUS or faults after later source-file truncation.

The copy includes accessible file bytes throughout the rounded page, even when
the requested length ends partway through that page. Writes to the snapshot
never modify the source file. Later file changes do not update the copied
payload; this is an explicit snapshot contract, not a shared page cache. A
concurrent source mutation is not promised an atomic filesystem snapshot. If
the source becomes shorter while copying, an early zero read fails with `EIO`
and rolls the allocation back.

Positional reads use the private descriptor table and VFS under their existing
locks. They do not change the description's shared cursor. Each callback copies
at most 65536 bytes; normal short reads continue until the selected payload is
complete. Negative offsets, unreadable/closed descriptors, non-file sources,
unbound I/O, and callback errors return explicit errors. Access timestamps are
updated by real read requests according to the existing metadata policy.

The allocation charge remains rounded payload plus the existing 24-byte record,
under the unchanged maximum 256 MiB owner limit. Budget checks happen before
allocation. The record joins the live ownership list only after a complete
copy; read failures free both unregistered allocations and leave mapping counts
and byte charges unchanged. Exact-owner munmap and diagnostic range checks use
the same registry as anonymous allocations. There is no new guest memory
algorithm or separate unbounded mapped-file cache.

The owning VFS now has separate [node/name quotas](HOST-FILE-CONTROL.md#allocation-limits-and-remaining-gap),
qualified after this mapping matrix. Payload, namespace and mapping limits do
not claim complete CLR or instance allocation bounds.

## Validation

Run `python3 blink/tests/HostFileMapping/run.py`. The completed native and
raw/optimized JIT/AOT matrix is
`artifacts/host-file-mapping/attempt-u9v_4xeg/receipt.json`.
It snapshots complete Host sources, headers, C wrappers, bridges, compiler, and
postprocessor. Raw generated C# is retained; `CS8500` is an error.

The same C checks run against real native mmap, the native compiled adapter,
and all four managed forms. They compare literal file bytes, aligned offsets,
full rounded-page contents for short mappings, zero EOF tails, positional-read
cursor preservation, private writes, and close-after-map lifetime. A separate
native subprocess proves SIGBUS on a whole page beyond EOF; the adapter checks
the corresponding pre-allocation rejection.

Adapter-specific checks cover empty/invalid/unreadable files, unsupported
flags/protections, negative/unaligned offsets, budget exhaustion, injected second
read failure, unexpected early EOF, successful short-read loops, unchanged
ownership on failure, exact range membership, and clearing callbacks between
owners. Two managed workers retain distinct file snapshots after disposing their
file instances and across compacting GC. Existing HostFiles and InstanceIo
regressions pass against the copied source project.

`python3 blink/tests/HostFileMapping/run-asan.py` reuses the frozen successful
native-adapter inputs with AddressSanitizer and leak detection. The ownership
and failure rollback check passed at
`artifacts/host-file-mapping/asan-vidxymdj/receipt.json`.

The existing anonymous allocation/actual native core matrix passed again at
`artifacts/host-memory/attempt-ywdxk6r8/receipt.json`, and the diagnostic
native/raw/optimized JIT/AOT matrix passed again at
`artifacts/host-diagnostic/attempt-2dfnxpxb/receipt.json`.
