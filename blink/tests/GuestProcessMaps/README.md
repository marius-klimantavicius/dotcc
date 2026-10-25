`python3 blink/tests/GuestProcessMaps/run.py` runs only serial private builds,
using the repository shared/native production locks. It performs no translation
or shared delivery build and leaves all inputs, outputs, failed attempts, hashes,
commands, timeouts, and process-group cleanup in `artifacts/guest-process-maps`.

`fixture.c` is an ordinary dynamic ELF suitable for the parent's generated guest
matrix. The Linux oracle uses the pinned libc/loader from the existing
CoreClrRuntime native closure (override with `--native-closure`). It checks
successful real glibc `pthread_getattr_np` on main and worker threads with local
variables inside the reported stacks. It reads and parses `/proc/self/maps` to
check sorted real mappings, private RW/PROT_NONE/read-only changes, executable
text, an unmapped hole, and an untouched shared PROT_NONE memfd followed by RW.
`elf.path`, `elf.sha256`, `expected_stdout_hex`, `fixture_ready`, and the Linux
`executions` row expose the oracle to parent runners. This runner does not claim
a generated guest execution pass; `managed_passed` remains false.

`memory-native.c` compiles the actual authored HostMemory implementation and
checks private, mutable shared, and immutable shared metadata, including
PROT_NONE, unchanged errno/accounting, and complete release. `Program.cs`
compiles unchanged authored HostIo/Resources/InstanceIo and the exact map walker
against a focused ABI fixture containing real four-level software page tables.
It checks reserved leaves, untouched shared backed leaves, coalescing, holes,
canonical address ordering, rwx, per-open snapshots, reads/seeks/fstat/dup,
close-on-exec, write/pwrite/truncate/shared-lease denial, permanent read-only
access flags, both local and combined memory-budget rollback, cancellation,
unbinding, fragmented output bounded to 1 MiB, and cleanup. Actual generated
constant/mutex declarations are checked independently; generated execution and
its locks remain the parent's integration responsibility.

Product maps intentionally use anonymous-style identities: offset zero,
`00:00`, inode zero, and no pathname. Backing file names, offsets, device/inode
identities, and stack labels are omitted because the guest page tables do not
retain reliable file identities. Virtual intervals and rwx come from the guest
page tables under the generated mmap mutex. Shared/private comes from the
mapping owner's metadata, including backed file leaves still marked reserved.
No host `/proc` contents or physical host addresses enter product maps. Each
open takes its own immutable snapshot, capped at 1 MiB and charged through the
existing combined memory-file owner. The guest memory cap remains 256 MiB.
The fixed stack limit is bound from generated `kStackSize`, matching the loader;
requests to change it are refused by the existing resource policy.

The earlier coordinator's automated content-filter stop citing possible cyber
risk remains recorded in every receipt. No safeguards are disabled or bypassed.
The tests use normal libc/mapping lifecycles, without malformed ELF images or
fault injection. Failed attempts remain available; in particular the first
focused stub used an incorrect generated API shape and did not validate public
integration. Corrected product references and stubs use direct BlinkCore
constants and nested BlinkCore.Libc pthread mutex functions.
