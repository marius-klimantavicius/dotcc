# Cooperative stop through the C# execution owner

The corrected inherited-pipe fixture passed all eight Linux/native-Blink
completion controls and all 44 managed cases (11 each in raw/optimized JIT and
NativeAOT). The earlier failed native attempt is retained below.
The separate `Managed.Emulation.Execution.GuestExecution` consumer owns all
translated calls, including loading, instruction dispatch and teardown. These
tests reference that same owner; there is no duplicate C or C# instruction loop.

Four small, valid, static x86-64 ELF guests are assembled from `fixture.S` with
explicit CPU, poll, sleep and pipe selections and the shared linker script.
With only argv[0], each completes its ordinary operation, writes one exact
`ok:<fixture>\n` marker and exits zero. Linux hardware and the pinned native
Blink CLI execute these four completion controls. An extra argument selects
continuing CPU work or a pending poll/sleep operation. The inherited-pipe case
waits when its host-supplied pipe is empty. No corrupted inputs or forced host
failures are used.

The exact 11 managed cases are pinned in `cases.json`:

| Cases | Observation and outcome |
|---|---|
| Four completion controls | Native marker and guest exit zero match; no stop or signal. |
| Requested CPU stop | Wait for at least 1,024 observed completed instructions, then request stop. |
| Requested empty-poll stop | Wait for real `HostSleep.IsWaiting` inside `poll(NULL,0,-1)`, then request stop. |
| Requested sleep stop | Wait for real sleep inside a valid 30-second nanosleep, then request stop. |
| Requested inherited pipe-read stop | Wait for `PendingPipeOperations == 1` on stdin with an open writer and empty pipe, then request stop. |
| Poll and sleep deadlines | Five-second monotonic deadline covering Run; a real pending wait must be observed before expiry. |
| CPU instruction budget | Exactly 128 completed instructions; Budget reason distinct from guest exit. |

The two `pipe-*` IDs mean **inherited stdin reads**, never guest pipe creation.
For native controls the runner supplies byte `0x5a` through a real subprocess
stdin pipe. The managed harness creates a private pipe, installs its reader at
fd0 with `DuplicateTo`, closes the redundant reader descriptor and leaves the
writer open until I/O disposal. Completion preloads the same byte; cancellation
leaves the pipe empty. The guest executes supported `read(0,buffer,1)` and checks
the byte only if the read completes. The report retains supplied-byte count and
the actual writer-open observation after Run.

Every case runs in a fresh process. The controller reads only the owner's
thread-safe progress/sleep observations and existing private-I/O counters;
it never reads or writes a Machine pointer. The owner checks stop before and
after dispatch and asserts ordinary-return syscall cleanup before its general
cleanup. These tests do not normalize guest return values or manufacture guest
signals. They do not inspect guest nanosleep remaining bytes; the existing host
sleep contract covers remaining-duration output separately.

Reports retain actual elapsed time, barrier time, progress/pending counters,
the complete owner result, captures, retained host-memory accounting captured
before release, and successful memory release status. No zero retention after
translated memory disposal is inferred. A first latched reason must survive
later ordinary stop requests. Any owner `NotificationFailure` fails the case.
Reports use explicit `Utf8JsonWriter` fields, including numeric enum values and
nullable observations; they require no reflection-based serialization in AOT.
Inherited pipe descriptors may remain until the caller disposes InstanceIo:
guest metadata cleanup alone does not close host descriptors. After joining the
execution thread and disposing I/O, its public descriptor, pipe-byte and pending
operation counters must all be zero. No translated call follows final disposal.

Native controls do not establish Linux/private cancellation equivalence. The
requested/deadline/budget cases qualify the private cooperative contract only.
The five-second deadline is observed with a small launch-accounting allowance;
there is no exact scheduling or cross-runtime elapsed-time equality guarantee.
The runner has a separate safety timeout; any timeout is a failed attempt,
never evidence that cooperative stop succeeded.

After review and serialized execution release:

```sh
python3 blink/tests/GuestExecutionStop/run.py \
  --delivery-receipt blink/artifacts/translation/<qualified-attempt>/receipt.json
```

`--native-only` runs only the eight Linux/native-Blink completion witnesses and
leaves full `passed` false. The complete runner requires a passed 108-producer,
literal-pool product delivery with no C execution or test frontend, then builds
the same authored owner/consumer against raw and normally postprocessed product
projections, each under JIT and NativeAOT. It never rebuilds the compiler or edits
generated C#. Test-only snapshots retain input hashes; production delivery uses
its original source references. The exact 11×4 selected matrix, closed logs,
fixtures, native tools, binaries before/after, source/producer receipts and
final input identities are recorded in fresh `artifacts/guest-execution-stop`
attempts. Previous attempts are never resumed or overwritten.

## Observed qualification

Passed receipt: `artifacts/guest-execution-stop/attempt-kl7r74np/receipt.json`,
SHA256 `aacfeae66a3d1eb23cee6194147d2451667e06f3bfd0dc74cee223bd5579443f`.
It consumes product delivery `artifacts/translation/attempt-4yjaed1_/receipt.json`
and the same authored C# execution owner, with all 108 canonical producers
identified and no C execution/test frontend.

All four modes passed requested CPU stop, requested zero-descriptor poll with
indefinite timeout, requested 30-second sleep interruption, requested inherited
pipe-read cancellation, the two observed-wait deadline cases, and the exact
128-instruction budget. Zero-descriptor polling with an indefinite timeout is
distinct from each native/managed finite five-millisecond completion control.
All eight deadline observations were between 5.0004648 and 5.0061477 seconds;
these are measured results, not a scheduling guarantee. Every requested pipe
stop observed one pending operation and a still-open writer, and all private
descriptor/pipe-byte/pending-operation counters drained to zero on disposal.
No case reported an injected guest signal or a notification failure.

Final independent verification matched 1,384 hashes, including all 1,038 frozen
inputs and the logs/executed binaries for 68 commands. It also rechecked all 44
report schemas, selected case coverage, exact native completion markers,
first-latched stop reasons, wait observations, budget counts and cleanup state.
The explicit JSON writer completed under both NativeAOT modes.

The first attempt, `artifacts/guest-execution-stop/attempt-geaqvgvp/receipt.json`
(SHA256 `3812ae97af000f8492061eb41e260e2cdcaefcc7974f7edc5f432372b7c59579`),
is retained as failed evidence: all four Linux controls and three native Blink
controls passed, but the original pipe fixture assumed guest `pipe` creation.
The pinned profile disables threads and fork, which excludes the upstream
pipe/pipe2 dispatcher entries. No managed build ran in that attempt. The
inherited-descriptor correction changes the fixture, not the profile or Host.
