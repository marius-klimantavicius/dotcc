# Owning managed controller

This library implements the process/controller part of the proposed Blink API.
Its disposable subprocess fixtures pass JIT and NativeAOT. The translated service
worker is not yet qualified; this library alone does not implement P5 or execute
an ELF. Partial Embedding/worker files are excluded from committed delivery after
an automated review stopped their implementation.

`BlinkInstance.StartAsync(WorkerLaunch, InstanceOptions)` starts one explicit
managed worker command with shell expansion disabled. Inputs include image files,
executable/argv/environment/cwd, memory/descriptor/output/instruction/deadline
limits and explicit guest ports. Arguments exclude argv[0]; the worker prepends
the executable. Image input is bounded to1MiB for the initial protocol. Options
are copied, validated and serialized before spawning so caller mutation cannot
change the accepted worker configuration. The3MiB frame writer is bounded during
serialization; incoming lengths are checked before allocation. JSON metadata is
source-generated for NativeAOT.

`Ready` yields guest-port/host-loopback-port mappings, `Completion` distinguishes
guest results from worker failure and deadline, `StopAsync(grace)` sends stop and
kills the worker if the grace period expires, and `DisposeAsync` stops/releases
its process. The grace period covers sending as well as waiting. The worker must
reserve raw stdin/stdout for framed control data, capture guest descriptors
separately and emit at most one ready and one final event. The controller checks
publication membership, instruction/output bounds and event ordering. Unexpected
worker stderr is retained through64Ki characters; overflow terminates the worker.

The independent deadline can kill blocked or spinning workers. Redirected-channel
drains are cancelled on forced stop and250ms after root exit, so inherited pipe
handles cannot keep controller completion waiting indefinitely. Tree kill applies
while the root is alive; cleanup of detached descendants is not guaranteed. The
qualified translated worker profile must not launch descendants. This API does
not provide an OS sandbox, resource enforcement outside the worker contracts,
or a hardened boundary for hostile programs.

Run `python3 blink/tests/InstanceLifecycle/run.py` for actual subprocess tests.
These deliberately use protocol fixture processes, including ignored stop,
crash, incoming length overflow, diagnostic overflow and inherited handles.
They do not stand in for the separate actual translated HTTP service gate.
