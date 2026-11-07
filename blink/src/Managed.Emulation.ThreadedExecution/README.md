# Threaded execution owner

This authored C# API consumes the portable selected producer in
`../Managed.Emulation.FullCore/Managed.Emulation.FullCore.csproj`. Its source
archive, ordered compilation units and legal provenance are hash-pinned; the
default build needs the separately retained archive installed as described in
[the producer input instructions](../Managed.Emulation.FullCore/README.md), but
needs no producer scratch tree. The archive is intentionally not in Git. Explicit `BlinkProject`
overrides still select a different producer, which must satisfy the required
source and runtime protocols. See the durable campaign ledger for exact
qualification boundaries.

The constructor's optional `memoryLimitBytes` accepts page-aligned limits from
8 KiB through 256 MiB; its low-level default remains 64 MiB. The public
`BlinkMachine` passes its own 256 MiB default explicitly. The owner passes that value to shared backing creation;
existing resource initialization sets guest RLIMIT_AS/DATA to the same limit.
Thus reserved guest addresses count toward admission even before physical pages
are touched. This is not a measurement of process RSS.

`Run(..., allowElfInterpreter: true)` enables the translated upstream ELF
interpreter handoff; the default is false. Public callers set
`ExecutionOptions.AllowElfInterpreter` instead. The interpreter and its libraries
must exist in the supplied guest filesystem. The preflight checks PT_INTERP
policy and interpreter availability; upstream loading and guest `ld.so` perform
the actual loading and relocations. See [the public contract](../Managed.Emulation/MACHINE-API.md)
and [ELF qualification](../../tests/ElfRuntime/README.md). This policy is not a
security boundary: explicitly executing a loader ELF is still possible.

The staged upstream clone path retains flag validation, Machine allocation,
register/TLS/stack setup and TID writes. Its narrow callback transfers an existing
Machine only after a dedicated C# thread starts. Each worker attaches the same
HostMemory context and private IO, acquires its own stop/sleep and virtual signal
mask, and arms its own jump-buffer identity. Interpreter mode retains upstream
CPU dispatch; Full selects the compiled paths described below.

Thread/group exit records the real syscall request and unwinds on that worker.
Workers release syscall/page locks, clear the actual child-TID word and free their
own Machine. The main Machine stays alive until every child Thread has actually
joined, preventing last-Machine system teardown under an active child. Final
exit callbacks, host contexts and backing disposal occur afterward. Errors in
cancellation notification or binding cleanup prevent successful completion.

The owner limits concurrently owned workers, including main, to
`maximumWorkers` (16 by default; the public API supplies 64). A child slot is
reused only after Machine/binding cleanup, host-thread join and synchronization
resource disposal. Completed results remain in history. The instruction budget
is shared, and stopped syscall state is checked before cleanup. Each owner runs
once and creates a private translated program context. A later public run uses
a fresh owner/context while retaining the machine's guest filesystem.

The caller borrows IO and external stop until `IsQuiescent` is true. If a worker
cannot join or upstream ownership fails to release, shared backing is retained
and the caller must discard the containing worker process before releasing those
borrowed resources. No concurrent forced free or native host signal is used.
Linux guest signals use the translated signal machinery and managed worker
notifications on both Linux and Windows hosts; they never raise a native host
signal. Guest SIGILL, sigaction/ucontext and rt_sigreturn retain the Linux guest
ABI on either host. Ordinary group shutdown follows the managed callback path.
Pinned robust-list cleanup remains upstream's existing no-op and is not newly
qualified.

## Guest JIT execution contract

`GuestJitOptions.Mode` accepts `Interpreter` (default), `Hybrid` and `Full`.
Legacy `Enabled = true` selects Hybrid when Mode remains Interpreter. The
public machine and per-run options use the same contract. Full remains opt-in;
correctness gates do not establish that it is faster than Interpreter.

Full uses retained multi-instruction translation blocks, normally up to 512
instructions with earlier control, state, page and emission-size boundaries.
Generated native code computes admitted ordinary scalar, memory, atomic,
vector, MMX, floating-point, canonical x87 and string/REP semantics. It does
not enter managed `Op*` functions to compute those admitted operations. The
remaining form/context omissions are coverage gaps. A typed ordinary semantic
fallback is still a native-coverage gap even if it avoids interpreter dispatch;
predecoding or a native wrapper does not make that computation native.

Linux syscalls, mapping/permission checks, guest fault delivery and narrowly
specified environment data services are separate runtime boundaries. Native
code returns before invoking managed services. Clock/entropy instructions use
an authenticated same-issued-operation data protocol: sampling occurs only
after admission, and native code computes the register/flag/state effects.
The RNG path holds the real generated runtime mutex through its native commit.
A sampled request cannot be replayed. Mapping assists do not compute guest
arithmetic or replace native string/atomic operations with selected C helpers.

Each native invocation retains its root translation and the actual store epoch
through RET. Native admission uses the current authority handshake; immutable
entry identity, mutable code versions, runtime polls, exact issue/budget,
attention/cancellation and fault completion remain distinct checks. Direct
links and the dynamic cache authenticate target identity and code stamps before
jumping to another body. An unchanged retained entry can avoid the global
compiler gate, but never its required native enrollment. Helpers execute after
native frames return and their native capabilities have been released;
recursive signal execution retains its one-instruction issue contract.

An owner-wide retained index backs worker hints. Plans, IR and staged code use
reusable charged compiler scratch; payload/descriptor arenas and packed W^X
code regions avoid per-instruction allocation. Full's default code-cache limit
is 1 GiB of demand-committed capacity, while explicit limits retain priority.
Worker data and jump caches each have 256 entries with matching native/managed
indexing. A cache flush requires all affected managed leases, native epochs
and drafts to drain. This independently implemented strategy follows retained
translation-block principles; it contains no QEMU code reuse. See the
[design mapping](../../artifacts/jit-implementation/full-jit-20261027-r1/qemu-tcg-blink-design-mapping.md).

Bounded guest copies revoke affected physical code aliases before stores.
Code snapshots bind code, mapping and content versions plus pending-writer
checks, independently of pure read rendezvous. Actual mapping, backing, raw
pointer and code writes retain their mutation/exclusion protocols. Cached data
mappings may survive only the checked authority transitions. Code lifetime is
not permission to reuse a stale pointer, instruction snapshot or ticket.

The finite canonical x87 software profile is enabled by the default
`BlinkX87StateOwner` build property; an explicit `false` retains the inactive
compatibility configuration. The portable production catalog uses integer-native
binary80 computation, not the historical private-host-x87 experiment. Its
explicit software policies include reserved PC01 arithmetic using PC64 while
preserving raw CW01, and omitted protected16 FOP restoring as zero. FX state
requires the full 512-byte permission proof while copying the selected 416-byte
data image. These policies and the supported TF/RF, AC/AM context are finite
profile choices, not universal Intel/AMD CPU identity claims. AM1/AC1 and broader
unqualified contexts remain outside the admitted profile.

`InlineNativeInstructions + SemanticHelperInstructions` accounts completed
Full instructions; `InterpreterAttempts` and `InterpreterInstructions` must
remain zero. This accounting is not a completeness proof. The helper opcode
breakdown identifies actual retained exceptions. `NativeEntries`, tail jumps,
iterations, assists, compilations, allocations and cache charges measure
separate work; none alone establishes execution speed or coverage.

The guest is Linux on both supported host designs. Windows x64 CoreCLR and
NativeAOT execution remain **UNVERIFIED**; no mitigation is disabled to admit
native code. Linux guest faults/signals use the managed Linux guest machinery,
without leaking host SIGILL/SEH as guest recovery. Generated-code asynchronous
stackwalking/unwind remains **UNQUALIFIED**. Generation-exhaustion terminal
cleanup finding R101-04 remains **OPEN**. All original findings and their scoped
responses remain in the durable ledger.

The c179 default-project family gate and broader combined gate pass under
Linux CoreCLR and NativeAOT. The completed four-cell matched service pilot
still fails the performance goal: startup/compilation-inclusive CPU ratios range
from 0.969 to 2.637, with only one matched pair per cell. Warm IL requests improve
in this pilot, while warm AOT requests remain slower. These are finite results,
not repeatable performance acceptance. Compilation, allocations, cold and warm
CPU/wall evidence remain separate; no cost is subtracted to manufacture a win.

Development is paused at the user's requested checkpoint. Full native form
completeness remains **MISSING**. See the [checkpoint and resume commands](../../artifacts/jit-implementation/full-jit-20261027-r1/CHECKPOINT.md),
[matched results](../../artifacts/jit-implementation/full-jit-20261027-r1/checkpoint-service-results.json)
and [coverage limits](../../artifacts/jit-implementation/full-jit-20261027-r1/checkpoint-coverage.json).
