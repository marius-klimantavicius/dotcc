# Shared private memory-barrier registration

Run `python3 blink/tests/HostProcessMembarrier/run.py` from the repository root.
This normal fixture registers private expedited barriers on the creator, then
executes actual translated C barrier calls on two real C# threads attached to
one `HostProcessMemoryBarrier`. Native Linux uses two pthreads and the real
membarrier syscall. Neither path starts a guest interpreter or substitutes for
its lifecycle qualification.

A native condition variable / managed event orders the two data steps. Both
workers attach before the first step; the first writes42 and fences, the second
observes42, writes52 and fences. The workers do not register independently.
The owner verifies one capability probe, two expedited fences, successful join
and zero attachments before final disposal. Successful callbacks preserve errno.
The event ordering is explicit: this is shared registration and actual fence
invocation qualification, not an independent hardware memory-ordering proof.

`artifacts/host-process-membarrier/attempt-04hsv8u9/receipt.json` passes native
and raw/optimized JIT/NativeAOT with exact output:

```
process registration=24 workers=2 fences=2 value=52
```

Receipt SHA-256:
`14996bc97e7800a89a4f1aa97881f7d7575be551d1009ada83ccf099057bfb94`.
All command logs and execution binary hashes were independently rechecked.
The runner retains immutable raw sources, exact compiler/header/Host/bridge
snapshots and before/after executable dependency identities. It restores
original authored sources after semantic postprocessing. No fault injection,
unsupported-command cases or guest-thread completion claim is included.
