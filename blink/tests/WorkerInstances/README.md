# Actual worker instance qualification

All four worker modes passed in
`blink/artifacts/worker-instances/attempt-kd2trw_m/receipt.json`
(SHA256 `f17019198ff8f02aa3d522944e04c94d05de9ad10509e6cdb9fa742f56a79483`).
The 13 commands exited normally; 16 real workers completed 40 exact native HTTP
comparisons. An independent final review verified 1,382 identities, each actual
stop reason, all guest joins, memory release, and private IO disposal. No cleanup
signals were used. Execution-time sources, including the earlier README bytes,
remain in the private raw/optimized source snapshots; this result text was added
after verification.
The runner requires a passed public stable threaded delivery receipt and the
passed native static-musl GC-profile witness. The actual run used public delivery
`attempt-i4a5mfa8`, native GC witness `attempt-lp8kf_q3`, and native traffic witness
`attempt-znwlxua8` (SHA256
`4dc8d1122825b03fb75dad720665e212eaad97ccb8447ee882b90b8d2c5b2755`). It does not rebuild the compiler,
guest ELF, native oracle or translated C producers.

For each raw/optimized JIT/rooted NativeAOT worker mode, the same public
`BlinkInstance` controller runs four real worker processes:

1. Start two simultaneous guests at guest port 8080, using separately loaded
   executable paths `/instance-a/service` and `/instance-b/service`. Both use the
   same pinned ELF bytes and exact four GC environment entries. Require distinct
   worker PIDs and distinct actual loopback host ports.
2. Compare both health responses with the native oracle. On the first guest,
   also send a 3,573-byte padded health request, the same request in seven
   consecutive awaited writes, and an ordinary `/missing` request returning 404.
   Stop it through HTTP, then compare another health response from the second.
   Write boundaries do not establish TCP segmentation or guest receive counts.
3. Cooperatively stop the second through the controller, requiring actual final
   cleanup detail and a normal worker process exit. Restart the first in a fresh
   process, then compare health and HTTP stop responses again.
4. Start a fourth worker with a 15-second wall limit, compare health, then leave
   the ordinary accept idle with no pending client. Require its actual `Deadline`
   final result and graceful cleanup before the controller's hard deadline.

That is ten exact native HTTP comparisons and four worker completions per mode:
40 comparisons and 16 real worker runs across four modes. Guest output and stderr
are checked independently of protocol frames and worker diagnostic stderr. All
threaded Machines must release, all guest workers join, shared memory release,
and private IO drain. A parent `stopped` label alone is not sufficient evidence.

The two image namespaces also receive distinct `/instance.txt` contents. Their
hashes are retained, but the service does not read those files, so the test does
not claim guest-visible auxiliary file contents were exercised. Actual different
executable paths are loaded from each private namespace.

No worker crash, ignored stop, malformed frame/image, resource exhaustion or
custom failure injection is executed. Historical mock controller tests are not
part of this runner. The parent retains hard containment, but these cases must
complete with no cleanup signals. This does not qualify a hard-kill path.

Run after coordinator source review and build-slot release:

```sh
python3 blink/tests/WorkerInstances/native-traffic.py \
  --native-gc-receipt blink/artifacts/dotnet-guest-gc-profile/attempt-lp8kf_q3/receipt.json

python3 blink/tests/WorkerInstances/run.py \
  --delivery-receipt blink/artifacts/translation/<passed-attempt>/receipt.json \
  --native-gc-receipt blink/artifacts/dotnet-guest-gc-profile/attempt-lp8kf_q3/receipt.json \
  --native-traffic-receipt blink/artifacts/worker-native-traffic/<passed-attempt>/receipt.json
```

The native traffic witness reuses the unchanged executable and exact four GC
settings, runs port zero with actual readiness parsing, and retains request,
response, write schedule, syscall trace, process and source identities. It adds
no sleeps between writes, premature disconnects, or over-limit input. Its normal
stop response also matches the earlier native witness. The managed matrix accepts
only a passed native traffic receipt for that same binary/configuration.

The runner reconstructs private repository-shaped copies of original protocol,
worker and C# owner sources. Raw uses the immutable archived raw library context;
optimized uses the stable generated sources and exact original authored source
references. No generated C# repairs or postprocessing of the authored worker is
performed. Executable and dependency trees, compiler identities, native bytes,
closed logs and source closures are checked before/after. Build outputs and JIT
execution copies are separated from later AOT publication. Every failure and
available response/report artifact remains in its attempt directory. Shared host
SDK, runtime and NuGet cache mean the build is not fully hermetic.

Observed completed-instruction counts varied from 1,204,953 to 1,245,961 per
worker with ordinary scheduling. These are semantic checks, not a throughput
comparison. The control read drained on explicit stop; normal exit/deadline
reported `receiverDrained=false` while guest execution and private IO completed
cleanup. The remaining control read was owned by the exiting process, not counted
as a drained guest operation. Auxiliary marker files remain unobserved by the
guest, as described above.
