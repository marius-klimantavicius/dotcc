# Read-only host clock boundary

`src/Host/HostClockBridge.cs` supplies the actual C-facing
`gettimeofday` and `clock_getres` callbacks through the instance-bound
`HostEnvironment`. It shares the existing environment binding used by
`clock_gettime`; it does not introduce a process clock or timer handler.
The binding manifest must include both bridge partials, and the managed
profile redirects `clock_getres` through `host-clock.h` and `gettimeofday`
through `sys/time.h`.

`gettimeofday` returns the injected provider's UTC time, normalized into
seconds and nonnegative microseconds. Submicrosecond values are truncated
after normalization, including times before the Unix epoch. A null result
is `EFAULT`; the obsolete nonnull timezone argument is `ENOTSUP`. Neither
failure modifies the output record. These are explicit private-boundary
policies, not claims about every native platform's null/timezone extension.

`clock_getres` accepts realtime and monotonic clocks, including a null
output pointer. An unknown clock is `EINVAL`. Realtime reports the
software representation's 100 ns output quantum. Monotonic reports the
larger of 100 ns and one provider timestamp tick rounded upward to whole
nanoseconds. This is a declared software output resolution, not measured
physical clock precision or accuracy; a provider may update less often.
The result is normalized even when the tick is one second. An invalid
provider frequency or provider exception is `EIO`. Both callbacks return
`ENODEV` before binding, preserve output on failure, and preserve `errno`
on success.

Run `python3 blink/tests/HostClocks/run.py`. Receipt
`artifacts/host-clocks/attempt-z_i3waun/receipt.json` records a passing
native C oracle and raw/optimized JIT/AOT consumers of translated C.
The runner snapshots the full Host project, both bridge partials, headers,
compiler identities, and raw output before postprocessing a copy. The
native oracle checks valid normalized output, optional resolution output,
invalid clock IDs, output preservation, and success preserving `errno`.
The managed tests additionally exercise injected UTC values before and
after the epoch, timestamp frequencies 1, 2, 3, ten million and `long.MaxValue`,
provider failures, private null/timezone policies, unbound calls, and two
simultaneous separately bound workers across forced compacting GC.
`CS8500` is an error in every generated consumer build. The receipt's
original scope label mentions entropy because it reused the environment
runner; this fixture qualifies only the two read-only clock additions.

## Selected-core dependency audit

Run `python3 blink/src/Host/scripts/audit-host-calls.py`. The report at
`artifacts/host-clocks/host-call-audit.json` joins the recorded native
archive's undefined object symbols to the actual selected core source
closure, then supplies lexical macro and authored-definition hints from
the current host-binding manifest. It currently covers 83 selected native
objects and 180 imported symbol names. It hashes its manifests, inspected
sources, and headers. Status counts belong to that report's input snapshot.

Native object imports establish compiled dependencies; they do not prove
that a particular guest executes every path. Macro selection and staged
source adaptations can change managed dependencies. A lexical definition
candidate does not establish a successful managed link or qualified API
behavior. The final full-core object/link receipt remains the authority
for linking gaps.

The native `syscall.o` imports `gettimeofday`, `clock_getres`, `nanosleep`,
and `clock_nanosleep`. `log.o` and `syscall.o` import `clock_gettime`; the
managed `time.c` also selects its clock fallback without native CPU macros.
The authored profile does not define `TIMER_ABSTIME`, so the actual
`SysClockNanosleep` selects its `nanosleep` fallback. The translated fixture
checks this same selector. No sleep callback is qualified here.

The remaining sleep contract is substantive: `SysNanosleep` in
`blink/syscall.c:3925` and `SleepTime` in `blink/timespec.h:65` expect valid
sleep requests to finish or fail with `EINTR`, and retry or report remaining
time toward the requested deadline. A bounded duration cap that returns
success early would violate that contract; an unsupported-error stub would
violate the existing retry assertions. Worker cancellation, virtual signal
interruption, and remaining-time behavior need an explicit implementation
before these paths can execute safely. The isolated timer/alarm operations
also remain unresolved. `SysTime` at `blink/syscall.c:4207` calls `time()`, which the combined binding profile isolates as
`blink_host_time`; it still needs an instance-clock implementation.
