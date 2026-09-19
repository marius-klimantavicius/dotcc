# Private resource limits and priority

`HostResourcesBridge.cs` implements typed `getrlimit`, `setrlimit`,
`getpriority` and `setpriority` callbacks for the private instance owners.
It never queries or changes OS process limits, OS identities or BCL thread
priorities. The existing authored `sys/resource.h` provides the aliases and
native-measured LP64 record; `host-resources.h` is the explicit module include.

## Supported policy

| Selector | Reported soft and hard limits | Source |
| --- | --- | --- |
| `RLIMIT_NOFILE` | Actual descriptor-table capacity, including standard streams | Locked `InstanceIo.DescriptorCapacity()` |
| `RLIMIT_AS`, `RLIMIT_DATA` | Actual active mapping-owner budget | Read-only C `BlinkHostMemoryLimit()` |

The AS/DATA policy is a **private backing-budget policy**. The budget counts
rounded mapped payload and ownership records, as the existing HostMemory owner
does. It is not total OS virtual address space, total resident memory, or a
limit on generic malloc, CLR allocations, allocator alignment bookkeeping or
all guest virtual reservations. Queries return the configured ceiling, not the
remaining bytes. Allocation and release do not change that ceiling.

Exact idempotent limit setters succeed. All requested changes, including
lowering a limit or requesting infinity, fail with EPERM. A requested soft
limit above the hard limit fails with EINVAL. Recognized but unsupported
selectors fail with EOPNOTSUPP; unknown/negative selectors fail with EINVAL.
No guessed infinity or successful accounting stub fills the unsupported cases.
Null record pointers for a supported query fail with EFAULT. Failed getters
leave caller output untouched, and successful operations preserve errno.

Priority is immutable private nice value zero. PRIO_PROCESS and PRIO_PGRP
accept zero or the bound private process ID, matching the existing singleton
process/group namespace. PRIO_USER accepts zero/the bound UID. Other targets
fail with ESRCH, and unknown selectors fail with EINVAL. Setting zero is
idempotent; every other value, including integer extremes, fails with EPERM.
The value does not describe the actual scheduler priority of a .NET thread.

## Lifecycle and integration

Bind the existing `InstanceIo` owner before any resource call. Bind
`HostIdentity` before priority calls. Begin HostMemory on the same C worker
before AS/DATA queries. No separate mutable resource owner or duplicated budget
configuration is introduced. An unbound dependency returns ENODEV. A disposed
I/O owner returns EBADF. Ending/discarding the C memory owner makes AS/DATA
unavailable; the read-only getter returns zero when no memory owner is active.
The getter does not retain a pointer into TLS storage.

Add `src/Host/HostResourcesBridge.cs` to the managed binding sources;
include `host-resources.h` where explicit module declarations are wanted. The
core already includes HostMemory.c, which now exports its additive getter.
Frozen objects produced before that getter must be regenerated to use this
bridge. The new `InstanceIo.Resources.cs` partial belongs to the copied Host
project. Full-core builds use the existing BLINK_FULL_CORE containing-class
selection. Existing callbacks and allocation algorithms are unchanged.

## Actual upstream initialization gap

Pinned `memorymalloc.c:257` initializes every `System.rlim` slot to infinity.
`NewSystem` makes no host resource query. Excluded CLI `blink.c:198` defines
ProgramLimit and its Exec path calls it only for NOFILE after initial loading.
The retained `SysGetrlimit` and `SysSetrlimit` paths in `syscall.c:3644–3700`
handle AS, DATA and NOFILE directly in `System.rlim`, bypassing host callbacks.
Other resource selectors pass through the host boundary. Retained priority
syscalls pass through these callbacks.

Therefore these callbacks **do not initialize or enforce guest System.rlim**.
The embedding driver must separately seed reviewed guest limits and qualify
that integration. Guest setters can modify their existing upstream records
within their own rules; this module's immutable host policy does not rewrite
those algorithms. Current host mapping and descriptor allocation quotas remain
enforced independently. Other accounting (`getrusage`, `times`), interval
timers, CPU-time limits and scheduler controls remain separate tasks.

## Evidence

Run `python3 blink/tests/HostResources/run.py` from this worktree. Receipt
`blink/artifacts/host-resources/attempt-de3l8ktb/receipt.json` records native
resource ABI/common invariants, the actual native HostMemory getter lifecycle,
and raw/optimized JIT/NativeAOT runs over copied source inputs. Native common
checks set only identical existing process values; they do not mutate limits
or priority. Native process capacities are not expected to equal private owner
capacities.

Translated C checks real emitted record size/alignment, member addresses and
byte layout; exact/invalid/unsupported resource selectors; idempotent/refused
setters; null/output-atomicity behavior; infinity and integer extremes; and
owner disappearance. Two workers use different descriptor limits, identities
and memory budgets, with barriers and compacting GC while mappings remain
live. Descriptor duplication reaches the actual table ceiling; a mapping
request exceeding the remaining budget fails with ENOMEM. Subsequent queries
retain the configured ceilings. No generated C# is hand-edited; optimization
runs on a copy, raw source hashes are checked, and CS8500 is forbidden.

The process namespace cannot create children, so RUSAGE_CHILDREN returns an
actual all-zero usage record. Invalid selectors fail EINVAL. Self CPU usage and
times are explicitly unsupported and fail EOPNOTSUPP without changing outputs;
wall time or the controller's process CPU consumption are never substituted.
These refusals remain until per-guest accounting is implemented. Native probes
qualify the LP64 rusage/tms storage and no-child accounting invariant.
