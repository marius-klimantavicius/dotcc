# Non-linear host memory boundary

`src/HostMemory/` supplies bounded private anonymous host allocations for the
interpreter's explicit guest address translation. It does not implement guest
page tables, guest mmap algorithms, file mappings, host page protection, or JIT
memory. Those remain upstream code or separate unsupported host operations.

## Reached calls and source boundary

Unchanged upstream `InitMap` discovers a native page size and probes arbitrary
fixed host addresses using `msync` and `mmap`. Without `HAVE_MAP_ANONYMOUS`, its
portable mmap helper creates and unlinks a temporary host file. Those discovery
and fallback behaviors are unsuitable for this managed profile.

`stage-map.py` verifies the entire pinned upstream map.c SHA256
`f35096d88178f3acf3e6bf475974379d7e7b2ce116325c2a3ea9b355cbc93267`, then replaces
only `GetSystemPageSize` and `GetBitsInAddressSpace` with explicit host capability
getters and adds one adapter header include. It records both function hashes,
exact replacement text, and the staged output hash. `InitMap`, address scaling,
PortableMmap and the public map wrappers remain upstream code. The staged
configuration requires NOLINEAR, HAVE_MAP_ANONYMOUS and the campaign mman header
together. The global core configuration does not independently enable this
capability before the adapted source is staged.

The explicit policy is 4096-byte allocation granularity and 47 usable positive
address bits for the lower half of x86-64's 48-bit canonical guest space. This
produces upstream `FLAG_vaspace == 0x7ffffffff000`; it is not a measurement of
actual host pointer addresses. Host pointers are kept in upstream g_hostpages
and guest page-table entries contain indices in this non-linear configuration.
The test also runs untouched native discovery separately, without treating that
oracle as a managed product implementation.

`AllocateAnonymousPage` requests a 64-page, 256 KiB private read/write anonymous
slab through `AllocateBig` and `Mmap`. `NewSystem` and `NewMachine` additionally
use the existing real `posix_memalign` runtime and generic malloc/free; their
allocation sizes and all other host services are outside this slab budget.
InitFds, process identity, general malloc/realloc resource limits and the rest
of the System lifecycle still need complete instance host integration.

## Allocation ownership and errors

`BlinkHostMemoryBegin(limit)` creates one active owner on the current host worker
thread. The configured limit includes rounded page payloads and 24-byte LP64
ownership records, is at least one page plus metadata, and is at most 256 MiB.
The upper bound keeps the shared runtime's current int-sized memset/malloc
length conversions lossless. The charged total is payload plus ownership
records, not process RSS: allocator alignment padding, runtime ownership-table
entries and native allocator bookkeeping add per-allocation overhead. Each slab uses real posix_memalign/free, which the
managed runtime implements with NativeMemory allocation and a live ownership
table. Payload bytes start at zero and addresses are aligned to 4096.

Only a null-address, private anonymous, read/write request with fd=-1 and
offset=0 is supported. Fixed addresses, file/shared mappings and other
protections return MAP_FAILED with ENOTSUP. Zero/overflowing lengths return
EINVAL; exceeding the owner's budget returns ENOMEM without changing its live
state. mprotect and msync return ENOTSUP, never pretend to protect or flush.

munmap requires the current owner's exact base pointer and matching rounded
length. Partial, foreign and repeated frees return EINVAL and retain live
allocations. An inactive owner returns ENODEV. Successful exact frees remove
the record and reduce the charged total. Records hold unmanaged pointers and
integers only; no CLR reference is stored in C memory. The implementation also
avoids retaining a raw pointer into managed thread-static aggregate storage.

## Worker lifetime restriction

Upstream FreeSystem returns anonymous pages to a process-global cache; it does
not release the slab or clear g_allocator/g_hostpages/g_bus. Consequently,
`BlinkHostMemoryEnd` refuses an owner with live mappings. Calling
`BlinkHostMemoryDisposeWorker` after FreeSystem and then reusing the same
upstream globals would leave dangling slab references and is forbidden.

The required sequence is: stop execution and callbacks, finish upstream
Machine/System cleanup, discard the complete upstream worker state (including
all cache/global references), then release the owner's remaining mappings and
discard the worker. The staged native core oracle disposes only immediately
before its process exits. A persistent managed instance must either discard its
complete owning generated core state or add a separately reviewed upstream
cache cleanup seam before it can reinitialize that state. This adapter does
not claim that instance teardown integration is complete.

## Evidence

Run `python3 blink/tests/HostMemory/run.py`. Each attempt snapshots source,
headers, capabilities and compiler identity. It verifies eleven mman constants
and MAP_FAILED against native headers; audits staged map imports to exclude
native mapping/discovery/filesystem fallback calls; and compares staged native
InitMap plus allocation/error tests with raw/optimized JIT and NativeAOT.
The managed consumer runs two independent worker threads with live mappings
across compacting GC and requires CS8500-free builds. Generated C# is not edited.

The runner also links the unchanged native core execution probe against the
staged map adapter, thread-disabled upstream Bus and actual native Blink
archive. Both repetitions of arithmetic, undefined instruction, instruction
budget and unmapped-address fault cases pass through actual NewSystem,
AllocatePageTable and AllocateAnonymousPage. Two mappings remain cached at the
end, with 270384 charged bytes; they are released only as the worker exits.
This is native execution evidence. Managed instruction execution remains a
separate, unfinished gate.

The final complete receipt is
`artifacts/host-memory/attempt-p2rrmgaz/receipt.json`; successful reruns update
`artifacts/host-memory/latest.json` without overwriting earlier attempts.

The honest managed profile selects bus.h's 32-bit-host helper path because it
does not advertise a native CPU platform. Its emitted inline functions therefore
require unchanged upstream pte32.c, which native archive extraction may omit.
The memory probe adds that real translation unit instead of inventing helpers.
