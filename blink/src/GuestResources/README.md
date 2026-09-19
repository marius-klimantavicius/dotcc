# Initial guest resource limits

`BlinkHostInitializeBoundResourceLimits(struct System *)` initializes the actual
upstream guest resource records from the current private owners. Call it after
successful `NewSystem(XED_MACHINE_MODE_LONG)` and before `NewMachine`, page-table
allocation, ELF loading, descriptor installation or execution. Host I/O must
already be bound and `BlinkHostMemoryBegin` must have succeeded. A failed call
leaves the entire System unchanged; discard the new System or resolve the owner
error before proceeding. The coordinator's core driver owns that integration.

The managed bridge queries the existing `InstanceIo.DescriptorCapacity()` API;
the C adapter reads `BlinkHostMemoryLimit()`. It stores both current and maximum
limits using unchanged upstream `Write64`:

| Guest record | Initial value | Actual selected upstream use |
| --- | --- | --- |
| `RLIMIT_AS_LINUX` | Mapping-owner byte budget | `GetMaxVss`/`GetMaxRss` clamp to upstream ceilings and divide by4096; selected mmap/brk paths consult them |
| `RLIMIT_DATA_LINUX` | Same byte budget | Queryable/settable guest record; no separate DATA enforcement consumer in the pinned source |
| `RLIMIT_NOFILE_LINUX` | Descriptor-table capacity including standard streams | `GetFileDescriptorLimit`, used by open, pipe, duplication and socket paths |

The capacity is a limit, not the number of currently unused descriptors. AS is
not remaining memory, and records preserve exact byte units, including a
non-page-aligned budget. Rounded mappings and ownership bookkeeping can exhaust
the actual HostMemory budget before this guest AS threshold. Upstream resident
accounting also includes its own page-table/slab behavior. Generic malloc,
System/Machine allocations, managed buffers and CLR memory are not all charged
to HostMemory. Loader/internal allocation paths need their existing owner budget
checks; seeding these records is not proof that every allocation consults rlim.

This operation is restricted to a fresh, single-worker long-mode System. It
refuses nonempty machines/descriptors/filemaps, page tables, real-mode storage,
resident/virtual counts, loaded state, or any resource record changed from the
initial infinity. This is not a thread-safe runtime limit reset. Unrelated
resource records remain untouched. Null returns EFAULT, no active mapping owner
ENODEV, zero or greater-than-INT_MAX descriptor capacity EINVAL, and nonfresh
state EBUSY. The bound entry first reports unbound I/O ENODEV or disposed I/O
EBADF. Success preserves errno.

The low-level C entry `BlinkHostInitializeResourceLimits(s, descriptorCapacity)`
is for an embedding that has already obtained its real owner capacity; the
managed entry supplies that value automatically. Neither path reads or modifies
OS resource limits.

## Existing upstream semantics and remaining gate

`memorymalloc.c:NewSystem` sets all guest records to infinity. Retained
`syscall.c:SysGetrlimit` and `SysSetrlimit` handle AS/DATA/NOFILE directly through
`System.rlim`, bypassing the private host `getrlimit/setrlimit` callbacks. Thus
host callback qualification alone did not configure guest limits.

Pinned upstream `SetResourceLimit` checks each requested current/maximum value
against the old maximum, but does **not** check `current <= new maximum`. It can
therefore accept an internally inconsistent pair. This adapter leaves that
upstream behavior unchanged; it does not claim immutable guest limits or repair
guest setrlimit. The private host setter's immutable policy is a different
boundary. Actual guest syscall execution and enforcement remain behind the full
core execution gate.

## Validation

Run `python3 blink/tests/GuestResources/run.py`. Native validation links the
unchanged pinned archive, calls actual `NewSystem`, seeds its actual System and
calls the upstream descriptor/virtual/resident limit consumers. A second native
profile oracle and raw/optimized JIT/NativeAOT probes allocate the actual System
type and call the same adapter. They check byte storage, failure atomicity,
nonfresh/repeated initialization, invalid capacities, owner disposal and two
simultaneous workers with different budgets/capacities across compacting GC.
The managed probe initializes fresh storage itself; it does **not** execute
managed NewSystem, guest syscalls, or emulate instruction execution. Receipts
preserve native archive/source, complete compiler, header, host-source and
unmodified raw generated-source identities.
