# Public service lifecycle control

This consumer imports a frozen image closure and runs the real service through
`BlinkMachine`, including the ordinary matching worker for `SeparateProcess`.
It uses three health requests, two echo requests and normal HTTP stop, with exact
bodies, stdout, disposal, resources released and worker-exit checks.

```sh
python3 Scripts/production_support.py dotnet build \
  blink/tests/PerformanceService/ServiceProbe.csproj -c Release -o /absolute/output
dotnet /absolute/output/PerformanceServiceProbe.dll \
  InProcess closure.json report.json
dotnet /absolute/output/PerformanceServiceProbe.dll \
  SeparateProcess closure.json report.json
```

Select a frozen raw or processed project with `-p:BlinkProject=/absolute/project`.
Publishing with `-r linux-x64 -p:PublishAot=true` uses the normal consumer targets
to publish both this controller and its actual NativeAOT worker. Check their ELF
identities and complete deployment hashes before and after execution. No portable
worker fallback qualifies as a NativeAOT host run.

The closure schema matches `CoreClrRuntime`: Images, Files (GuestPath, File,
Sha256, Executable), and Environment. An optional top-level Executable selects
the NativeAOT guest; otherwise `/dotnet/dotnet` runs the minimal service DLL.
`GuestKestrelMinimal` and `CoreClrReadyToRun` contain the same service source.
Publishing that unchanged source with NativeAOT needs no route adaptation.

The measured NativeAOT guest publication copied `GuestKestrelMinimal`'s
`Program.cs`, `Service.csproj` and the pinned `global.json` to a separate scratch
directory, then used this command under the production locks:

```sh
python3 Scripts/production_support.py dotnet publish /absolute/scratch/Service.csproj \
  -c Release -r linux-x64 --self-contained true \
  -p:UseAppHost=true -p:PublishAot=true -p:PublishReadyToRun=false \
  -p:InvariantGlobalization=true -o /absolute/native-guest
```

Pin the resulting ELF and all loader/shared-library dependencies in the image
closure, including `/lib64/ld-linux-x86-64.so.2`; set its top-level `Executable`
to the guest apphost path. Run the same exact HTTP contract natively first.
The retained publication used SDK 10.0.112/runtime 10.0.12, glibc Linux x64 and
no middleware edits. Invariant globalization matches the CoreCLR guest environment.
This is a guest publication step, separate from publishing this consumer/worker
as a NativeAOT host. The full frozen dependency and source hashes are retained in
`artifacts/performance-implementation/nativeaot-service`.

Defaults are 180 seconds and 150 million instructions. Optional final arguments
select 1–7,200 seconds and at most three billion instructions for finite CoreCLR
comparisons. Backing remains 256 MiB with explicit 4 GiB virtual space; all other
public defaults remain. This is a selected minimal service control, with no
claim of arbitrary ASP.NET compatibility or representative throughput.

Public readiness observes listening-port publication, and includes import/start
in its elapsed value. The lower-level `../Performance` timing harness separates
import and samples the service's stdout readiness marker. Keep these observations
distinct. Neither consumer traces instructions or changes interpreter dispatch.
