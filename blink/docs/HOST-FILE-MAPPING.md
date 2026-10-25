# Private file snapshots for host mmap

The host memory owner can explicitly enable file-backed `MAP_PRIVATE` requests
over a bound instance's private file data. It allocates real aligned owned
memory and copies bytes through synchronous positional reads. It never calls
OS mmap, opens a host path, or substitutes guest page-table/loader algorithms.

The host snapshot boundary supports loader reads and nonlinear guest private
file mappings. Maintained staging also adapts the guest reservation transaction,
EOF lookup and signal translation; those changes are described below. Native
boundary and metadata checks alone do not qualify dynamic ELF, CoreCLR, or general
.NET compatibility. Actual loader/libc qualification belongs to the ELF runner.

## Binding and lifetime

Include `src/Host/HostFileMapping.c` and `HostFileMapping.h` in the managed
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
a tracked NONE/READ/WRITE/RW mode, a readable regular private file descriptor, a
positive length, and a nonnegative offset aligned to 4096. Protection modes and
validated per-page changes use the [software-only memory contract](HOST-MEMORY.md):
raw backing stays RW and guest PTEs enforce guest permissions. Shared, fixed,
executable and sync requests remain explicit ENOTSUP errors; unknown protection
bits return EINVAL.

The requested span may include whole pages beyond EOF, start beyond EOF, or
map an empty file. The last partial file page contains its file bytes followed
by zeroes. Whole pages at/beyond EOF carry separate unavailable metadata and must
never be exposed as readable zero pages by guest translation. This availability
bit shares the existing protection-byte array; `mprotect` preserves it.
`BlinkHostMemoryFilePageAvailable(pointer)` reports availability without touching
payload or changing errno; unowned pointers return zero. `Contains` continues to
report ownership, and `Protection` continues to report only the protection mode.
The raw allocation is RW and nonexecutable, including unavailable pages; only the
guest translation path enforces absence. Direct host pointer reads are not a
qualification of guest access semantics. Later source truncation does not update
an existing snapshot's availability.

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

The allocation charge is rounded payload plus the 24-byte record and one byte
per rounded page,
under the unchanged maximum 256 MiB owner limit. Budget checks happen before
allocation. The record joins the live ownership list only after a complete
copy; read failures free both unregistered allocations and leave mapping counts
and byte charges unchanged. Exact-owner munmap and diagnostic range checks use
the same registry as anonymous allocations. The snapshot owner has no separate mapped-file cache.

The owning VFS now has separate [node/name quotas](HOST-FILE-CONTROL.md#allocation-limits-and-remaining-gap),
qualified after this mapping matrix. Payload, namespace and mapping limits do
not claim complete CLR or instance allocation bounds.

## Guest staging and replacement

`src/Host/guest-private-mapping.inc`, applied by
`scripts/memory_bindings.py`, replaces the reviewed nonlinear `ReserveVirtual`
body in fresh staged inputs. Its complete predecessor function identity is
pinned in `config/private-mapping-adaptations.json`; other edits require unique
exact anchors. Immutable reference sources and generated C# remain untouched.
The threaded producer must reapply these corrections after its thread/mremap
source chain, and receipts retain the actual adapter and fragment hashes.

Reservation prepares bounded transaction storage, all needed intermediate page
tables, and each private file backing page before replacing guest leaves. A
normal allocation or file-read failure frees prepared backing and transaction
storage, detaches newly created empty tables, restores accounting, and returns
errno. Existing guest leaves and their bytes survive failed preparation. Empty
slab pages may remain in the existing upstream owner cache until final teardown.
The commit uses existing guest page-lock waits and retirement helpers, preserves
permissions and file-page accounting, then invalidates translations. Anonymous
BSS remains demand-zero and can replace unavailable file pages. Guest fixed and
no-replace placement is decided under the existing mmap lock; no-replace checks
run before replacement, including when both fixed flags are supplied. Nonfixed
hints round down and sizes round up to guest pages before quota calculations.

The staged memory lookup prevents unavailable pages from becoming committed or
cached translations. Permission checks take precedence over unavailable-file
faults. Permitted guest accesses to unavailable file pages follow the synchronous
guest SIGBUS/BUS_ADRERR path; syscall buffer validation returns EFAULT. No host
signal handler, hardware protection, raw absent-page read, or process exit is
used for this contract. Source/invariant review covers signal routing and
precedence; custom guest fault probes remain excluded.

Host page identities remain an inherited per-instance high-water registry.
Publishing file identities after successful preparation avoids stale file
identities on backing-allocation rollback. Successful map/unmap churn can still
grow the registry until the owning core is discarded. Mapping payload/record
accounting is bounded; it is not a claim that every CLR array, generic allocator,
or process byte is included in that quota.

## Current validation and historical evidence

`python3 blink/tests/HostFileMapping/run.py --native-only` runs common ordinary
Linux and native adapter mappings. Without that option the runner also builds
its separate raw/processed JIT/AOT boundary consumers; coordinate compiler and
build-slot use first. Current checks cover positional reads, rounded-page data,
partial-page zero tails, private writes, close-after-map lifetime, mappings of
empty files and beyond-EOF spans, availability surviving protection changes,
exact owner cleanup, and a normal configured-budget ENOMEM followed by a
successful smaller allocation. Absent whole pages are inspected through metadata
and never accessed. The runner does not execute historical custom fault
injection, invalid-access branches, or the old native SIGBUS crash child.
`GuestFileMapping/run.py` adds ordinary guest workload and staged PTE invariants;
its native result remains partial evidence until a fresh product executes the
same static ELF workload.

Historical full boundary receipts include
`artifacts/host-file-mapping/attempt-u9v_4xeg/receipt.json` and software protection
refresh `artifacts/host-file-mapping/attempt-mmr6w46m/receipt.json`; historical ASan
receipts include `asan-vidxymdj` and `asan-o2y4be1f` in that artifact directory.
Those runs predate the new EOF contract and include cases now excluded by the
campaign's test scope. Their results are retained as history and are not new
passes of excluded tests or qualification of this changed guest mapping path.
