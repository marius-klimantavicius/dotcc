# Machine API source contract

This API is being implemented for P6. Runtime qualification is pending the new
generated instance ABI and fresh product; these examples describe the intended
public contract, not a completed platform qualification.

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
