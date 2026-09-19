# Actual translated guest service

`run.py --translation-receipt <passing delivery receipt>` builds the separate C#
execution owner and HTTP fixture against byte-identical delivery raw and final
optimized sources, then exercises JIT and NativeAOT. The original referenced
authored source closure is hash-checked and privately copied for the fixture;
the stable delivery keeps its direct source references. `--raw-only` is a diagnostic first gate;
it cannot mark the four-mode matrix passed. `--prepare-only` makes no build or
execution claims.

`--assembly-receipt` without a delivery receipt remains an explicitly labeled
historical canonical-link variant with independent postprocessing; it cannot
serve as a byte-identical final delivery claim.

The pinned static musl service and read-only fixture are mounted in private IO.
The owning thread binds host contexts, calls the actual translated ELF loader,
arms the existing numeric longjmp transport and calls `ExecuteInstruction`.
All lifecycle and dispatch code is in `Managed.Emulation.Execution`, a separate
C# assembly. The product contains only the narrow `TerminateSignal` callback
bridge, with no CoreProbe or authored C execution driver.

A BCL client waits for the exact captured readiness bytes, publishes the actual
guest listening descriptor, and sends the existing six native Linux/native Blink
oracle requests: health, file, fragmented file, 128 KiB large body, missing path,
and normal stop. Every complete HTTP response, including its headers, must match
both saved native oracles byte for byte. The private guest port is 8080; only the
host loopback endpoint is assigned dynamically. This port mapping is recorded,
not normalized into the request or response bytes.

The execution owner has a 100 million completed-dispatch budget and a 120-second
deadline. The loop separately reports guest exit, genuine guest signal/halts and
host stop reason. The exiting syscall may unwind before the completed-dispatch
counter increments. On normal cancellation, syscall cleanup state is checked
before defensive frontend cleanup. Memory accounting is captured before final
owner release; no translated call follows that release. Upstream retained caches
require process discard, so no restart or second execution is claimed.

The runner retains failures, exact producer/profile/compiler identities, copied
host and consumer sources, runtime binary hashes, all wire bytes, direct IL and
NativeAOT publication inventories. Child commands run in their own process group;
an internal isolated `TMPDIR` is recorded for the attempt. Timeout, SIGTERM or
another interrupted wait requests group termination, waits up to two seconds
for the direct child, then kills remaining group members and waits up to five
seconds. This does not control descendants that deliberately detach into a new
session. These checks do not prove isolation of all framework
or indirect calls. The direct IL inventory roots the translated library and host
initializers; it does not automatically cover the separate execution consumer
assembly, whose authored source is reviewed separately. No native emulator is
used as an execution fallback.

The first raw JIT gate passed in
`artifacts/guest-service/attempt-pu9_o0z7/receipt.json` (SHA-256
`9875d127a520eec4d86184fcea03c38960535d4a7ef484ee7393056535cee6e1`),
against delivery `attempt-4yjaed1_`. All six exact wire cases passed; the actual
guest printed `READY 8080` then `STOPPED`, with empty stderr and exit status zero.
It completed 1,725,554 instruction dispatches. Before successful final memory
release, three cached mappings charged 532,682 bytes. The raw-only receipt keeps
the full-matrix passed flag false.

The historical native oracle receipts are reused only after their ELF, fixture
and complete request/response hashes match. The first raw gate predates the
runner's internal TMPDIR and interrupted-child cleanup refinement; its outer
launch provided a unique TMPDIR, and no timeout or interruption occurred.

The first full-matrix attempt, `attempt-bzu3jva5`, passed raw JIT, then raw
NativeAOT aborted while writing the fixture's JSON report: reflection-based
serialization was disabled. The binary, logs and wire artifacts remain intact;
there is no final AOT service result and no AOT pass claim for that attempt.
The fixture now writes its closed report schema with `Utf8JsonWriter`, without
reflection configuration or metadata-rooting workarounds. The runner records
execution binary hashes before launching, including for failing attempts.

The repaired four-mode matrix passed in
`artifacts/guest-service/attempt-18nn8vfz/receipt.json` (SHA-256
`a9283cb6bdc87aa83a64fa1f4c19ea2bb02b6640aef60684b8c88e4502b60c4e`).
Raw JIT, raw NativeAOT, optimized JIT and optimized NativeAOT each passed all six
native wire cases: 24 complete request/response comparisons. Each execution
reported 1,725,554 completed dispatches, the genuine exit-trap halt `-10`, guest
exit status zero, no guest signal or owner stop, empty stderr and successful
memory release after the same three retained mappings/532,682 charged bytes.
These are correctness observations, not a timing benchmark.

Both NativeAOT builds had zero IL warnings. The raw executable SHA-256 is
`ae5e808fa5cc5d7a4701808351605bd0a2c7eff4e02dededf4277070ae179381`;
the optimized executable is
`04fd82889d34866d206f8eef13d8d56a5117648eecb2b6a2c50c94e9270578a3`.
The scoped publication inventory passed in
`artifacts/publication-audit/attempt-xnmou9mj/receipt.json` (SHA-256
`f951f59dcf0c0e705998571c043d64032e679fc72620045fcfbf56573f3b436a`).
The runner rechecked the exact delivery raw/final sources, original authored
source closure, compiler inputs and execution binaries. This qualifies service
startup and normal HTTP completion; the separate stop/deadline fixture has its
own acceptance gate.
