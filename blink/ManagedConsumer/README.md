# Translated Blink .NET service consumer

The sample uses `BlinkInstance` to start a separate managed worker, which owns
`ThreadedGuestExecution` and the translated interpreter. It sends real HTTP
health and stop requests to the .NET NativeAOT guest, verifies normal exit and
resource release, then starts a fresh worker and demonstrates cooperative stop.
CPU instructions execute through translated upstream Blink. The .NET guest ELF
is distinct from the optional NativeAOT build of the emulator worker.

The actual solution builds, and this sample passes with both JIT and NativeAOT
controller/worker pairs in
`artifacts/managed-consumer-delivery/attempt-8k2fus34/receipt.json`.
Both executions verify exact HTTP responses, normal exit, a fresh worker restart,
cooperative stop and resource cleanup. See
[the reproducible check](../tests/ManagedConsumerDelivery/README.md).
This is the existing raw-socket .NET service baseline. The required ASP.NET Core
Kestrel guest remains separate pending work; this result does not complete P5.

## Generate, build and run

From the repository root:

```bash
dotnet build dotcc.sln -c Release -p:UseLocalLalrCc=false
bash blink/scripts/translate.sh --offline
dotnet build blink/ManagedConsumer.slnx -c Release --disable-build-servers

dotnet run --project blink/ManagedConsumer/ManagedConsumer.csproj -c Release --no-build -- \
  blink/artifacts/dotnet-guest-musl/attempt-8za50rji/publish/DotNetService \
  blink/src/Managed.Emulation.Worker/bin/Release/net10.0/Managed.Emulation.Worker.dll
```

The two required arguments are the guest ELF and worker executable. A worker
`.dll` is launched with `dotnet`; a published native worker is launched directly.
`--help` prints usage. The retained guest path above contains the qualified static
musl NativeAOT ELF, SHA-256
`b8fc2c2ba465ded0349c46ecd332dc361ebd0d8c7265b17938adc7e79eac82b3`.
To reproduce its build, follow [the guest instructions](../tests/DotNetService/README.md)
and [the pinned musl build](../tests/DotNetService/MUSL.md), then pass the resulting
attempt's `publish/DotNetService` path. The generated library and guest binary
are local build artifacts, not checked-in substitutes.

The sample passes the qualified four guest environment entries, including the
bounded GC configuration. Those settings belong to the guest, not the host CLR.
Each worker has a 64 MiB guest backing limit, 128 descriptors, 16 KiB captured
output, a 20-million-instruction budget, and a 30-second parent deadline.
The current threaded owner bounds the total created guest workers to 16.
The private guest port is 8080; the host loopback port is allocated independently.

## NativeAOT hosts

Publish the actual worker and controller sample separately:

```bash
dotnet publish blink/src/Managed.Emulation.Worker/Managed.Emulation.Worker.csproj \
  -c Release -r linux-x64 -p:PublishAot=true -o blink/build/worker-aot

dotnet publish blink/ManagedConsumer/ManagedConsumer.csproj \
  -c Release -r linux-x64 -p:PublishAot=true -o blink/build/managed-consumer-aot

blink/build/managed-consumer-aot/ManagedConsumer \
  blink/artifacts/dotnet-guest-musl/attempt-8za50rji/publish/DotNetService \
  blink/build/worker-aot/Managed.Emulation.Worker
```

The solution references original authored projects under `src`. The generated
`TranslatedBlink` project links original adapters and the Host project directly.
Edit authored code there and build normally; generation never overwrites it.
The public translation pipeline selects the threaded NativeAOT guest profile by
default. `--profile single-thread` retains the older library profile but is not
compatible with this threaded worker sample. Raw snapshots are archival test
inputs; only generated code transformations are published after postprocessing.

The worker reserves raw stdin/stdout for bounded control frames and keeps guest
output in private descriptors. A final stopped status alone is insufficient:
the sample requires a clean worker exit and the actual joined/quiescent/IO and
memory-release report. A new worker process owns every restart because upstream
retains static caches. This is controlled local execution, not an OS security
sandbox or a Windows qualification claim.
