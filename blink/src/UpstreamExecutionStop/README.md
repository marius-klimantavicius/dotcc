# Cooperative private execution stop

Source preparation only; runtime qualification is pending. This adaptation is
part of the private nonlinear interpreter profile, not an upstream bug fix.
The immutable reference and historical profiles remain unchanged.

The pinned `syscall.c` is from Blink revision
`f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580`, SHA256
`4eb3f54173ba37341b300e7668cc4ba3650578cc3d23d713573ffa486ae0e2c3`.
`stage.py` verifies the complete source and the two function blocks, reproduces
the exact checked-in two-hunk patch, retains the upstream license, and writes
only fresh output paths. The receipt identifies the original/staged source,
original/replacement blocks, script, patch, and required header by SHA256.
The canonical profile must retain this receipt and freeze the matching header
and managed owner/bridge sources. This does not permit patching generated C#.

The two safe points are:

- `CheckInterrupt`, including its signal-restart label: a latched owner stop
  sets guest AX to `-EINTR`, sets `m->interrupted`, and returns normally. No guest
  signal is queued, delivered, or used as a substitute for owner cancellation.
- `Poll` outer loop: consult the same persistent owner state before scanning
  descriptors. This covers `nfds == 0`, which never reaches the existing
  per-descriptor checks. Its ordinary failure return leaves syscall temporary
  buffers for the unchanged `OpSyscall` cleanup.

The adaptation includes `host-execution-stop.h` locally. It requires
`DISABLE_JIT` and `NOLINEAR`; it does not change other translation units' global
header preambles. A staged native adapter may implement the same explicit host
query contract. Linux signal delivery is not equivalent to this private stop.

## Owner contract

`HostExecutionStop(TimeSpan? timeout = null, CancellationToken cancellation =
default)` exposes `Token`, `Reason`, `RequestStop()` and `RequestBudgetStop()`.
Reasons have fixed values: None=0, Requested=1, Deadline=2, Budget=3. The first
latched reason wins. The timer and every reason query check elapsed
`Stopwatch` time; long deadlines use bounded timer slices. Timer scheduling and
cooperative safe points do not promise an exact wall-time stop bound.

Bind the owner with `BindHostExecutionStop`, and pass its same token to
`BindHostIo(io, stop.Token)` and `BindHostSleep(sleep, stop.Token)` on the execution
thread. The original one-argument I/O and sleep binding methods remain intact.
`blink_host_execution_stop_reason()` returns the owner reason, or zero for an
unbound historical consumer. The authored C# owner calls `RequestBudgetStop()`
directly; no C budget callback or C execution loop is needed.

The owning instruction loop must check the reason before and after each
`ExecuteInstruction`. A racing completed syscall retains its real register and
memory effects; the owner reports cancellation separately from guest exit or
signal status. Existing private blocked I/O uses the same token and returns its
real canceled result. Host sleep wakes on persistent token cancellation with
`EINTR` and remaining duration. `SysNanosleep` then sees the stop at
`CheckInterrupt` rather than repeatedly restarting the wait. This also prevents
empty Poll from spinning after its ignored nanosleep error.

The controller touches only the owner, never Machine pointers, translated TLS,
or syscall scratch state. The normal return through `OpSyscall` retains page
lock collection, temporary allocation collection, and syscall nesting cleanup.
The owner is cooperative: an unrelated unsupported blocking callback without a
token/safe point is not made cancellable by this adapter.

After the execution thread has returned, inspect `NotificationFailure` (a
throwing token callback is retained as an owner failure), destroy guest state,
unbind all bridges and dispose the owner. Never dispose it from one of its token
callbacks, or treat disposal as a substitute for requesting and joining a stop.
Disposal drains timer/external-token notification callbacks before releasing
the token source. No translated call may use a disposed owner.

## Proposed bounded qualification

Pending review/release: the shared C# owning consumer running actual guest
instruction-loop cancellation and budget,
zero-descriptor infinite poll cancellation/deadline, valid nanosleep
cancellation/deadline, and ordinary blocked pipe read or TCP accept with the
same owner token. Record actual return/register state, stop reason, clean
syscall depth/page locks/freelist and ordinary owner teardown. Include normal
completion controls and a real deadline elapsed-time observation; do not assert
Linux signal equivalence or exact native/managed elapsed-time equality. No
fault injection, invalid pointers, malformed ELF or controller protocol is
part of this scope. Source preparation alone establishes none of these runtime
claims.
