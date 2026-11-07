# Full-JIT qualification fixtures

Guests are Linux x86-64 ELFs on Linux and Windows hosts. Restore the exact
[checkpoint producer input](../../src/Managed.Emulation.FullCore/README.md)
before building current threaded consumers. A changed producer requires separate
qualification; generated C# is never edited by hand. Use matching controller and
worker binaries from the same closure. Current resume commands and finite
qualification limits are in the [checkpoint](../../artifacts/jit-implementation/full-jit-20261027-r1/CHECKPOINT.md).

`prepare.py` builds the retained ordinary GuestJit fixtures, positive native
chaining/revocation and precise memory recovery witnesses, the native HostMemory
read-pin control, and the existing 514 normal CPU cases. `cpu-fixtures.py` emits
one small ELF per CPU case using the original capture assembly and records an
independent Linux native capture. It excludes the original custom-fault cases.
Those captures and CPU fixtures are portable input assets for a Windows host;
the native oracle itself runs only on Linux. All generated assets go to the task
storage tree.

Build `FullJit.csproj` with `BlinkProject` selecting the freshly produced generated
project. The consumer target builds/publishes a matching worker. Invoke:

```
FullJit public ordinary-fixtures output.json
FullJit state ordinary-fixtures output.json
FullJit cpu cpu-fixtures output.json InProcess
FullJit cpu cpu-fixtures output.json SeparateProcess
FullJit tripwire ordinary-fixtures output.json
```

For a CoreCLR host prefix these commands with `dotnet FullJit.dll`. NativeAOT
hosts invoke the published executable. `public` includes both public execution
modes, deterministic result/output/counter parity, strict zero-interpreter
accounting, actual distinct-block native tail jumps, simulated BaselineOnly,
stop/deadline/cache pressure and simultaneous owners with moving collections.
`state` checks 455 exact architectural/instruction boundaries. `cpu` compares
514 normal cases against the interpreter and, when supplied, their native defined
field/RAM captures. `tripwire` intentionally calls three old dispatch entry points
and checks rejection/counters; it does not execute a guest instruction.

Run `FullJitSignals` separately for caught/nested/uncaught #UD and Linux signal
context parity, and `GuestJit/ContractTests` for exact native caps, table ways,
misses, revocation exclusions and machine state. These are finite witnesses;
producer `coverage.py` is the separate complete selected-source lowering gate.

`bounded-static-workers.py` creates a separate functional profile of the retained
worker fixture: after one work chunk the child waits on a futex and the parent
wakes it. The original busy-loop fixture remains unchanged. Its shared-budget
exhaustion on the faster native child is retained as a failed observation; the
bounded profile uses the same instruction budget and is identified separately.
`mapping-signal.c`/`.S` exercise an interrupted pinned syscall buffer and a
separate SA_RESTART control. Native Linux controls and managed runs are distinct
evidence; these timing-based witnesses do not force every internal race schedule.

`qualify-windows.ps1` requires an actual Windows x64 host, PowerShell7, .NET10 and
transferred Linux fixture trees. It builds raw/processed CoreCLR/NativeAOT hosts
with matching Windows workers, preserves mitigation checks and records real
process evidence. Linux execution, AST checks and cross-builds do not qualify
Windows. Current Windows runtime status is **UNVERIFIED**.

Durable commands, source hashes, failures, reports and reproduction orchestration:
`blink/artifacts/jit-implementation/full-jit-20261027-r1/`. Read `report.md` for
actual matrix/performance status and residual qualification gaps.

`atomic-family-r156.c` is the current registered nested-signal/crossing-page guest.
Compile with `gcc -O2 -static -fno-pie -no-pie`; pass its ELF to ContractTests
`atomic-family` or `native-family-batch`. It is a Linux guest fixture, never a
host optional-ISA probe. Historical c179 broad receipts used the original guest;
the current expanded source has separate passing CoreCLR/NativeAOT atomic gates.
