# Owning managed APIs

Use `BlinkMachine` for the current persistent namespace and execution API. Its
[machine contract](MACHINE-API.md) covers in-process and separate-process runs.
Defaults are 256 MiB memory, 512 MiB private storage, 8,192 nodes, 1,024 descriptors
and 64 guest thread slots per run, with null instruction budget/deadline and a
`long.MaxValue` lifetime output ceiling. Capture remains 64 KiB combined, and
console queues remain 64 KiB each. Explicit limits remain supported; larger quotas
do not preallocate their full capacity. `ExecutionOptions.AllowElfInterpreter`
explicitly permits ELF interpreter loading from the supplied guest filesystem;
the default remains static-only. See the machine contract for packaging and
the finite loader/libc qualification scope.

`MachineOptions.VirtualMemoryLimit` optionally gives sparse guest reservations a
separate virtual-address budget, up to 4 GiB. Null preserves the existing policy
of using `MemoryLimit` for both budgets. An explicit value must be page-aligned
and at least `MemoryLimit`; it does not increase the backing-memory cap. The
CoreCLR fixture uses this option because its native runtime reserves gigabytes
of address space while using much less backing memory.

## Earlier explicit worker controller

`BlinkInstance` and `InstanceOptions` retain the earlier explicit worker protocol
and its independent bounded defaults. Their original disposable subprocess
fixtures do not execute an ELF; actual translated service qualification is tracked
in the [validation ledger](../../docs/VALIDATION.md). The current `BlinkMachine`
separate-process implementation uses `MachineOptions` and `MachineWorkerHost`,
not this older profile.

`BlinkInstance.StartAsync(WorkerLaunch, InstanceOptions)` starts one explicit
managed worker command with shell expansion disabled. Inputs include image files,
executable/argv/environment/cwd, memory/descriptor/output/instruction/deadline
limits and explicit guest ports. Arguments exclude argv[0]; the worker prepends
the executable. Image input is bounded to 16 MiB. Options
are copied, validated and serialized before spawning so caller mutation cannot
change the accepted worker configuration. The 24 MiB frame writer is bounded during
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
