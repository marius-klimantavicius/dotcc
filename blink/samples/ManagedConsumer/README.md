# Configurable Blink machine sample

The ordinary entry point is `ManagedConsumer GUEST_ELF [InProcess|SeparateProcess]`.
It mounts the ELF's parent at guest `/work` and executes `/work/<filename>`.
The default runs inside this application's process. The optional separate-process
mode discovers its automatically deployed worker; no worker path or `ImportImage`
call is needed.

The public `BlinkMachine` API also runs other supported static Linux x64 programs:

```csharp
await using var machine = new BlinkMachine(new MachineOptions
{
    Environment = new Dictionary<string, string> { ["LANG"] = "C" }
});
machine.MountDirectory("/work", "./work"); // Live read/write by default.
var result = await machine.ExecuteAsync(new ExecutionOptions
{
    Executable = "/work/my_app",
    Arguments = ["argument"],
    WorkingDirectory = "/work",
    Console = ConsoleOptions.AttachCurrent()
});
```

See [the public API contract](../src/Managed.Emulation/MACHINE-API.md) for
configuration, private storage, RO/COW mounts, streams, ownership and limits.

## Build and run

To build your own guest ELF first, use the
[Kestrel Dockerfile instructions](../tests/KestrelService/README.md#build-an-executable-with-docker-or-podman).

To leave the existing Kestrel guest running and visit it in a browser, run from
the repository root:

```bash
dotnet build blink/samples/ManagedConsumer/ManagedConsumer.csproj -c Release --disable-build-servers
dotnet blink/samples/ManagedConsumer/bin/Release/net10.0/ManagedConsumer.dll --serve \
  blink/artifacts/kestrel-guest-musl/attempt-o5jvvf7t/publish/KestrelService
```

Open **http://127.0.0.1:8080/health**; the guest responds with `ok`.
Press **Ctrl+C** in the terminal to stop execution and dispose the machine.
This mode runs in-process by default, has no execution deadline or configured
instruction budget, and does not perform automatic HTTP checks/shutdown/restart.
The existing guest serves `/health`; `/` returns `404`, and `POST /stop` requests
normal application shutdown. Its executable directory is mounted read-only.

Supply a different host port (or `0` for an automatically assigned port), followed
optionally by `SeparateProcess`; always use the printed browser URL:

```bash
dotnet blink/samples/ManagedConsumer/bin/Release/net10.0/ManagedConsumer.dll --serve \
  blink/artifacts/kestrel-guest-musl/attempt-o5jvvf7t/publish/KestrelService \
  8081 SeparateProcess
```

For the automated two-machine demonstration instead:

From the repository root:

```bash
dotnet build dotcc.sln -c Release -p:UseLocalLalrCc=false
bash blink/scripts/translate.sh --offline
dotnet build blink/ManagedConsumer.slnx -c Release --disable-build-servers

dotnet run --project blink/samples/ManagedConsumer/ManagedConsumer.csproj -c Release --no-build -- \
  blink/artifacts/kestrel-guest-musl/attempt-o5jvvf7t/publish/KestrelService
```

Append `SeparateProcess` to select that mode explicitly. For a normal console or
folder-processing guest, use either command below. Guest cwd is `/work`, binary
stdin/stdout/stderr attach to the caller's console, and writes persist directly
in `./work`. This general entry supplies `LANG=C`, denies network access by default,
and uses configurable machine defaults rather than Kestrel-specific settings.

```bash
dotnet run --project blink/samples/ManagedConsumer/ManagedConsumer.csproj -c Release --no-build -- \
  --run ./work /work/my_app argument
# Use --run-process instead of --run for a separate worker.
```

Publish one actual NativeAOT consumer; its matching worker is published and
packaged automatically:

```bash
dotnet publish blink/samples/ManagedConsumer/ManagedConsumer.csproj \
  -c Release -r linux-x64 -p:PublishAot=true -o blink/build/managed-consumer-aot
blink/build/managed-consumer-aot/ManagedConsumer \
  blink/artifacts/kestrel-guest-musl/attempt-o5jvvf7t/publish/KestrelService SeparateProcess
```

Ordinary applications reference the authored `Managed.Emulation` project and
import `src/Managed.Emulation/Managed.Emulation.Consumer.targets` when packaging
both modes. The showcase solution references original projects and the generated
project links original `src` adapters. Edit authored code there; regeneration
never replaces it. Public translation defaults to the instance-enabled threaded
profile. The legacy `--profile single-thread` output is not compatible with this
machine API.

## Actual service and evidence

The Kestrel demonstration starts two independent overlapping machines, checks
real HTTP and distinct published ports, performs normal HTTP shutdown, then
restarts the first machine and requests cooperative stop. In-process runs have
no worker PID. Separate-process runs create a fresh worker per execution while
machine-owned storage persists.

The guest is a real static-musl ASP.NET Core NativeAOT ELF, SHA-256
`ef6f1433794a42fe32b0fed4851bf88dd0631cd6a836550c6effca79d9e9a3ac`.
Its build is documented in [KestrelService](../tests/KestrelService/README.md).
The guest and optional NativeAOT emulator are separate binaries. The service
example explicitly selects the six native-profile environment values and
128MiB guest address-space/backing limit, 100M instructions, 60-second deadline
and 16-worker bound. Those settings are not defaults imposed by the public API.
They do not assert physical memory use or an OS process-wide resource sandbox.

The final P6 fixture and actual sample pass JIT/NativeAOT in both modes:
`machine-api/attempt-b074atpe` and `machine-api-kestrel/attempt-cqqbx2eu`.
The sample gate records 12 Kestrel executions, 20 native HTTP comparisons and
12 console/folder executions. Exact requests, status, headers and bodies match;
only validated RFC1123 Date values and header order may vary. Set
`BLINK_SAMPLE_EVIDENCE_DIRECTORY` to preserve actual HTTP and execution evidence.
See [validation](../docs/VALIDATION.md) for current receipts and follow-up status.

This is finite Linux x64 qualification, not arbitrary Linux software support,
Windows qualification or hostile-code containment. In-process stop is cooperative;
separate-process force termination is explicit. BCL mount checks require trusted
host roots and cannot make path operations atomic against hostile concurrent
host mutation, hard-link aliases or special files.

The advanced legacy `GUEST_ELF WORKER_PATH` overload remains a P5 baseline.
Its earlier raw-socket receipt is `managed-consumer-delivery/attempt-8k2fus34`;
its Kestrel receipt is `managed-consumer-delivery/attempt-ljvj6fxh` (SHA-256
`502992598acf8897e45020c17aeed4c2007e08d980cac0796fe16433f965aaaf`).
These historical subprocess results are not relabeled as the new machine API.
