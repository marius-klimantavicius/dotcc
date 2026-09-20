# Managed Blink worker

This executable consumes the existing `InstanceProtocol` over raw standard input
and output. It runs one guest through the authored C# `ThreadedGuestExecution`
owner on a dedicated thread. It has no C execution loop. Worker/controller integration passed raw and optimized JIT and NativeAOT in
`artifacts/worker-instances/attempt-kd2trw_m/receipt.json`: 16 actual worker
processes and 40 exact native HTTP comparisons. Normal exit, cooperative stop,
idle deadline, and fresh-process restart all completed with actual guest joins,
memory release, private IO disposal, and no process cleanup signals.

The worker captures the raw output stream before redirecting `Console.Out` to
`Console.Error`. Protocol frames therefore stay separate from generic translated
diagnostics. Guest file descriptors use `InstanceIo` captures. One main loop writes
at most readiness and final frames. A stop request or control EOF requests ordinary
cooperative stop. The controller owns the independent hard process deadline and
tree termination. Canceling an outstanding control read is best effort; the worker
observes it for at most 250 ms, records `receiverDrained`, and observes later
faults without delaying guest join or process exit. An undrained control read
remains process-owned; guest IO drainage is reported separately.

The supported option bounds are explicit:

- The selected guest memory ceiling is 64 or 128 MiB and is forwarded to the
  execution owner. Existing resource initialization couples this backing limit
  to guest RLIMIT_AS/DATA, including reserved virtual addresses. The owner caps
  16 cumulative guest workers. These are not process RSS or the .NET GC heap limit.
- Working directory is `/`; there is one requested published guest port and at
  most 128 private descriptors. The declared output limit is enforced by InstanceIo.
- The image totals at most 16 MiB. Private writable files total at most 1 MiB;
  pipe buffers total at most 1 MiB, with 64 KiB capacity each and 128 pending pipe
  operations. Standard input starts empty. Image files and executable permissions
  come from the caller; there are no implicit host mounts.
- The exact caller instruction budget is passed to the owner. Wall time must be
  at least two seconds. Cooperative stop starts one second before the configured
  worker wall deadline, reserving a best effort cleanup window. The controller
  starts its independent hard deadline earlier, so graceful cleanup is not guaranteed.

Image paths, argv and environment pass through after validation; the worker does
not hardcode a service binary or substitute configuration. Compatibility evidence
is limited to the selected frozen static-musl .NET service and its four GC environment
entries. Arbitrary guest compatibility is not implied by accepting a valid image.

Readiness uses a documented fixture convention: the first complete captured line
must be `READY <published-port>\n`, and an actual unique private listening socket
must publish to host loopback. Merely receiving a start frame cannot produce
readiness. A final reason is `exited`, `stopped`, `deadline`, `budget`, `fault`, or
`worker-failure`. Any nonzero child halt/signal is selected for the top-level outcome.
Guest status, halt, signal, completed instructions and captured
bytes are retained.

`WorkerEvent.Detail` is bounded JSON written with `Utf8JsonWriter`, suitable for
NativeAOT. It records actual join/quiescence, memory release, IO disposal, stop
reason, notification failure and retained pool metrics before release. Thread rows
are `[tid,instructions,termination,status,halt,signal,machineReleased]`; syscall
summary rows are `[tid,observationCount,lastNumber,lastReturnOrNull]`. This is a
bounded summary, not a full syscall trace. Collections are inspected only after
join and quiescence. Unknown pool metrics are null. No post-disposal memory getter
or zero-pool claim is made. Unjoined resources remain alive for process discard.

The controller may label hard containment `stopped` without receiving a final
frame. Consumers must check the real final detail and worker exit code before
claiming cleanup. The normal qualification in `tests/WorkerInstances` does so.

Build the public translated library first using `blink/scripts/translate.sh
--offline`, then build this project. It references the original C# owner and
protocol projects. The owner references `blink/generated/TranslatedBlink`; the
worker does not copy active bridges or Host sources.


Kestrel integration preparation raises the bounded image admission to16MiB and
control frame to24MiB: the measured genuine Kestrel ELF is9,371,272 bytes, which
cannot fit the old2MiB profile. Encoding remains bounded and occurs before worker
launch. These admission changes await the Kestrel worker matrix; the preceding
raw-socket receipt used its original2MiB/3MiB bounds.

The 128 MiB profile is prepared for Kestrel after its ordinary Gate-thread
reservation exceeded the earlier 64 MiB virtual-address ceiling. It changes
both coupled ceilings; it does not assert 128 MiB of physical use. Actual
Kestrel worker qualification remains pending. The historical raw-socket result
above used 64 MiB, a 2 MiB image bound and a 3 MiB control-frame bound.
