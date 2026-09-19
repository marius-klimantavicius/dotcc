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
the full-matrix passed flag false. The four-mode matrix remains pending.

The historical native oracle receipts are reused only after their ELF, fixture
and complete request/response hashes match. The first raw gate predates the
runner's internal TMPDIR and interrupted-child cleanup refinement; its outer
launch provided a unique TMPDIR, and no timeout or interruption occurred.
