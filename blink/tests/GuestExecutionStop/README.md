# Cooperative stop through the C# execution owner

Source preparation only. No fixture build or runtime qualification has run.
The separate `Managed.Emulation.Execution.GuestExecution` consumer owns all
translated calls, including loading, instruction dispatch and teardown. These
tests reference that same owner; there is no duplicate C or C# instruction loop.

Four small, valid, static x86-64 ELF guests are assembled from `fixture.S` with
explicit CPU, poll, sleep and pipe selections and the shared linker script.
With only argv[0], each completes its ordinary operation, writes one exact
`ok:<fixture>\n` marker and exits zero. Linux hardware and the pinned native
Blink CLI execute these four completion controls. An extra argument selects
ordinary continuing work or a pending operation; no corrupted inputs or forced
host failures are used.

The exact 11 managed cases are pinned in `cases.json`:

| Cases | Observation and outcome |
|---|---|
| Four completion controls | Native marker and guest exit zero match; no stop or signal. |
| Requested CPU stop | Wait for at least 1,024 observed completed instructions, then request stop. |
| Requested empty-poll stop | Wait for real `HostSleep.IsWaiting` inside `poll(NULL,0,-1)`, then request stop. |
| Requested sleep stop | Wait for real sleep inside a valid 30-second nanosleep, then request stop. |
| Requested pipe-read stop | Wait for `PendingPipeOperations == 1` with an open writer and empty pipe, then request stop. |
| Poll and sleep deadlines | Five-second monotonic deadline covering Run; a real pending wait must be observed before expiry. |
| CPU instruction budget | Exactly 128 completed instructions; Budget reason distinct from guest exit. |

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
Canceled guest pipe descriptors may remain until the caller disposes InstanceIo:
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
