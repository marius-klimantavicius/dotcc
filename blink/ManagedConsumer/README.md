# Configurable machine sample (P6 integration pending)

The new ordinary entry point is `ManagedConsumer GUEST_ELF [InProcess|SeparateProcess]`.
It mounts the supplied ELF's parent at guest `/work`, then executes the guest path
directly. The default is genuinely in-process; separate-process workers are built
and deployed automatically by the imported consumer targets. No worker path or
`ImportImage` call is needed.

For an ordinary supported static Linux x64 console program or folder-processing
program, use `ManagedConsumer --run ./work /work/my_app argument`. The guest cwd is
`/work`; stdin/stdout/stderr attach as binary streams and host writes persist in
`./work`. Choose `--run-process` for a separate worker. Network access is denied by
default; this generic entry supplies `LANG=C` explicitly and uses the machine's
bounded default memory/instruction settings. The public `BlinkMachine` API exposes
those settings directly when an application needs different limits.

For the pinned Kestrel fixture the sample starts two independent concurrent
machines, checks real HTTP, performs normal HTTP shutdown, then restarts the first
machine and requests cooperative stop. This new path is source preparation until
its fresh instance-v1 product and actual JIT/NativeAOT runs are recorded. The older
explicit-worker overload below remains a P5 baseline, not a P6 pass.

# Translated Blink .NET service consumer

The sample uses `BlinkInstance` to start a separate managed worker, which owns
`ThreadedGuestExecution` and the translated interpreter. It sends real HTTP
health and stop requests to the .NET NativeAOT guest, verifies normal exit and
resource release, then starts a fresh worker and demonstrates cooperative stop.
CPU instructions execute through translated upstream Blink. The .NET guest ELF
is distinct from the optional NativeAOT build of the emulator worker.

The earlier raw-socket version built and passed with both JIT and NativeAOT
controller/worker pairs in
`artifacts/managed-consumer-delivery/attempt-8k2fus34/receipt.json`.
Those historical executions verified exact HTTP responses, normal exit, a fresh worker restart,
cooperative stop and resource cleanup. See
[the reproducible check](../tests/ManagedConsumerDelivery/README.md).
The current ASP.NET Core/Kestrel sample passes the actual solution build and
both JIT/NativeAOT controller-worker pairs at
`artifacts/managed-consumer-delivery/attempt-ljvj6fxh/receipt.json` (SHA-256
`502992598acf8897e45020c17aeed4c2007e08d980cac0796fe16433f965aaaf`).
Four actual workers and six native HTTP comparisons pass, including normal
exit, fresh-process restart, cooperative stop and complete cleanup. It consumes
public delivery `translation/attempt-kaddtzlg`; the old raw-socket receipt
remains evidence only for its original sources.

## Generate, build and run

From the repository root:

```bash
dotnet build dotcc.sln -c Release -p:UseLocalLalrCc=false
bash blink/scripts/translate.sh --offline
dotnet build blink/ManagedConsumer.slnx -c Release --disable-build-servers

dotnet run --project blink/ManagedConsumer/ManagedConsumer.csproj -c Release --no-build -- \
  blink/artifacts/kestrel-guest-musl/attempt-o5jvvf7t/publish/KestrelService \
  blink/src/Managed.Emulation.Worker/bin/Release/net10.0/Managed.Emulation.Worker.dll
```

The two required arguments are the guest ELF and worker executable. A worker
`.dll` is launched with `dotnet`; a published native worker is launched directly.
`--help` prints usage. The retained guest path above contains the natively qualified static
musl NativeAOT ELF, SHA-256
`ef6f1433794a42fe32b0fed4851bf88dd0631cd6a836550c6effca79d9e9a3ac`.
To reproduce its build, follow [the pinned Kestrel musl build](../tests/KestrelService/README.md),
then pass the resulting attempt's `publish/KestrelService` path. The generated library and guest binary
are local build artifacts, not checked-in substitutes.

The sample passes the six entries from the reviewed native Kestrel profile:
`LANG=C`, `DOTNET_GCHeapHardLimit=1000000`, `DOTNET_GCRegionRange=2000000`,
`DOTNET_GCRegionSize=100000`, `DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false`,
and `DOTNET_EnableDiagnostics=0`. GC sizes are hexadecimal and belong to the
guest, not the host CLR. Each worker has a 128 MiB coupled guest address-space
and backing limit, 128
descriptors, 16 KiB captured output, a 100-million-instruction budget, and a
60-second wall deadline. The whole demonstration has a three-minute parent
deadline. The larger address-space allowance accommodates ordinary runtime reservations;
it does not assert 128 MiB of physical use. Execution qualification remains pending.
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
  blink/artifacts/kestrel-guest-musl/attempt-o5jvvf7t/publish/KestrelService \
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

HTTP checks require the native status, every header value and body. The actual
RFC1123 Date is validated but may vary; header order may vary. The optional
`BLINK_SAMPLE_EVIDENCE_DIRECTORY` environment variable retains actual request
and response bytes, guest output, worker results and cleanup reports for the
qualification runner.
