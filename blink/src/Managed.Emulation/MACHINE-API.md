# Machine API source contract

This API passes the selected Linux x64 P6 qualification under JIT and NativeAOT
in both execution modes. The public fixture covers eleven normal cases per mode;
the actual Kestrel and console/folder samples pass separately. See
[the validation ledger](../../docs/VALIDATION.md) for exact receipts and limits.

`MachineOptions.Metadata` enables [private IMDSv2 simulation](IMDSV2.md),
including explicit instance identity and role credentials, in both execution modes.

```csharp
await using var machine = new BlinkMachine(new MachineOptions
{
    ExecutionMode = ExecutionMode.InProcess, // default; no worker is created
    Environment = new Dictionary<string, string> { ["LANG"] = "C" }
});
machine.MountDirectory("/work", "./work"); // live read/write by default
MachineRunResult result = await machine.ExecuteAsync(new ExecutionOptions
{
    Executable = "/work/my_app",
    Arguments = ["first", "second"], // excludes argv[0]
    WorkingDirectory = "/work"
});
```

The executable resolves directly from the mounted guest namespace; no image
import or separate registration is required. `ImportImage` remains optional for
private in-memory content.

Service defaults apply to `new BlinkMachine()`, `new MachineOptions()` and both
execution modes. The resource quotas are ceilings, not startup reservations.

| Option | Previous default | Service default |
| --- | --- | --- |
| `MemoryLimit` | 64 MiB | 256 MiB (`268435456` bytes) |
| `WritableStorageLimit` | 16 MiB | 512 MiB (`536870912` bytes) |
| `FileNodeLimit` | 1,024 | 8,192 |
| `DescriptorLimit` | 128 | 1,024, including stdin/stdout/stderr |
| `ThreadLimit` | 16 | 64 concurrently owned guest worker slots, including the main thread |
| `InstructionLimit` | 100,000,000 | `null` (signed 64-bit counter ceiling) |
| `ExecutionDeadline` | `null` | `null` |
| `ConsoleOptions.OutputLimit` | 1 MiB | `long.MaxValue`, combined stdout/stderr lifetime bytes per run |
| `ConsoleOptions.CaptureBytes` | 64 KiB | 64 KiB, combined retained stdout/stderr |
| `ConsoleOptions.BufferBytes` | 64 KiB | 64 KiB per console queue |

Capture can truncate while the service continues writing to attached or redirected
streams. A larger lifetime output ceiling does not retain more logs in memory.
Pipes retain their separate 64 KiB per-pipe capacity, 1 MiB aggregate storage and
128 pending-operation defaults. Filesystem stores retain their 256 KiB aggregate
UTF-8 path-name budget and 4,096-byte path bound; long names can exhaust that
budget before the node ceiling. `ThreadLimit` bounds concurrently owned guest
workers. A child slot becomes reusable after its Machine and bindings are released,
its host thread is joined, and its synchronization resources are disposed. Clear-TID
alone does not release the slot; a clone during remaining cleanup can still return
`EAGAIN`. The main slot stays occupied through run teardown. Completed thread
results remain in history without retaining the retired workers' host resources.

Memory stays within the supported 256 MiB cap, shared by the execution owner and
its C backing allocator. It limits guest address-space/backing accounting, including
owner metadata, rather than reserving 256 MiB at startup. Storage, nodes,
descriptors and thread slots are also allocated as used. Existing validation and
explicit smaller values remain effective; for example:

```csharp
var bounded = new MachineOptions
{
    MemoryLimit = 64L << 20,
    WritableStorageLimit = 16L << 20,
    FileNodeLimit = 1024,
    DescriptorLimit = 128,
    ThreadLimit = 16,
    InstructionLimit = 100_000_000,
    ExecutionDeadline = TimeSpan.FromMinutes(1)
};
var boundedConsole = new ConsoleOptions { OutputLimit = 1L << 20 };
```

`ProcessMachineRun` serializes all resource values in `start-v1`, including the
nullable instruction budget and the 64-bit output ceiling. `MachineWorkerHost`
validates those values again, builds the private host filesystem with the same
storage/node/descriptor quotas, and uses the same `InProcessMachineRun` owner.
That owner passes memory, thread and descriptor values explicitly, without using
the older low-level constructor defaults. The earlier `BlinkInstance`/
`InstanceOptions` demonstration protocol retains its separate bounded profile;
it is not the protocol used by `BlinkMachine` separate-process execution.

Focused [default contract tests](../../tests/MachineDefaults/README.md) exercise
options, source-generated framing, host quotas and console construction without
executing a translated guest. Raising quotas does not extend guest compatibility:
the selected profile remains static Linux x86-64 ELF, including the pinned musl
NativeAOT Kestrel service, with explicitly granted numeric IPv4 TCP. Dynamic ELF,
CoreCLR, general Linux userspace, ambient DNS, UDP and IPv6 remain outside this
profile. Fresh full-product qualification is separate from these host tests.

Create a fresh run on the same machine to reuse its private filesystem. Machine
configuration and mount changes are allowed only while idle. Independent machines
may execute concurrently; one machine admits one run at a time. Guest environment
does not inherit host variables. Per-run null values remove machine environment
entries. Guest cwd, descriptors and environment never modify their host equivalents.

`ExecutionMode.SeparateProcess` is explicit and immutable. Both modes use the same
options and run API. Worker discovery checks `blink-worker` beside the application
for a published native worker or its managed DLL/runtimeconfig. An explicit
`WorkerLaunch` overrides discovery. Ordinary consumers import
`Managed.Emulation.Consumer.targets`; it builds the worker and copies its complete
output into that directory. Publishing packages the worker closure, and an AOT
consumer publishes an AOT worker with the same explicit RID in isolated artifacts.
Discovery neither downloads a worker nor switches execution mode.

`StartAsync` returns a `MachineRun`. Its `Ready` task means the static guest image
was loaded by default. `ListeningPorts` waits for all explicitly granted listeners
and returns their host ports; `OutputMarker` recognizes a bounded binary stdout
marker independently of captured output. Readiness timeout requests cooperative
stop. No application protocol or fixed success text is assumed.

`ExecuteAsync` drains attached output streams or uses bounded capture. Capture
truncation is separate from the total output limit. For manual binary streaming,
use `StartAsync` with `RedirectInput` and/or `RedirectOutput`; close `StandardInput`
to deliver EOF and drain stdout and stderr concurrently. Full output queues apply
backpressure. Nonblocking guest descriptors return EAGAIN. Optional attached streams
are left open by default. Pump drainage has an explicit timeout; a caller-owned
stream that ignores cancellation can retain its own pending operation after guest
resources have quiesced. `AttachCurrent` uses raw console streams; its Ctrl+C policy
is scoped to the run and removed on completion.

Canceling `WaitAsync` or the wait inside `ExecuteAsync` does not stop a guest.
`CurrentRun` retains access after a canceled convenience wait. `StopAsync` is
cooperative. A stop timeout retains all borrowed resources and prevents another
run from using that machine. `KillAsync` is available only in separate-process mode;
in-process calls throw `NotSupportedException`. Disposal requests cooperative stop
and likewise refuses to release resources beneath live guest threads.

Private root bytes and eager COW copies share the configured private writable
quota. Imports consume that quota. Live mounts are external grants and do not
consume private storage quota. `ResetPrivateStorage` clears root and COW contents;
it never changes live grants or silently refreshes a COW import. Exports select one
private store and never overwrite existing destination files. Import/export errors
may leave a partial destination and report failure. Separate-process private stores
use machine-owned directories so acknowledged writes survive worker termination.
An unlinked or replaced open inode retains its bytes and node quota once across
independent opens. Duplicated descriptors share one open description; the last
description's final close releases the retained storage. Open-file metadata and
identity remain available after unlink/replacement, with zero links, and descriptor
chmod/time/write/truncate operations continue to update that inode.

Host directory grants use cross-platform System.IO, with conservative path and
reparse-point checks. They are not atomic containment against hostile concurrent
host mutation, hard-link aliases or special files. Use trusted host roots. Windows
does not provide persistent Unix modes through this backend: unsupported chmod and
executable imports fail explicitly. BCL-only implementation is not a claim of
Windows guest execution qualification or an OS security sandbox.

Network access is denied by default. Explicit numeric IPv4 publication and outbound
grants are enforced before host bind/connect; DNS is not an ambient capability.
Memory, thread, descriptor, pipe, storage, output, instruction and deadline limits
are independent. A null instruction limit uses the owner's signed 64-bit counter
ceiling, terminating there rather than allowing overflow. A null deadline means no
deadline. In-process limits govern guest resources, not an OS process-wide hard
memory or CPU sandbox.
