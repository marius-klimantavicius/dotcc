# Genuine ASP.NET Core Kestrel NativeAOT guest

This fixture uses `Microsoft.NET.Sdk.Web`, `WebApplication.CreateSlimBuilder`,
Minimal API routing and Kestrel's normal socket transport. It is separate from
the preserved raw TCP `../DotNetService` fixture. No custom listener, HTTP parser
or transport replaces Kestrel. Native and translated Blink execution pass the
selected Linux x64 qualification; see [current validation](../../docs/VALIDATION.md).

## Build an executable with Docker or Podman

From the repository root, export a static musl NativeAOT executable:

```bash
docker build --platform linux/amd64 \
  --output type=local,dest=blink/build/kestrel-nativeaot \
  blink/tests/KestrelService
```

The Dockerfile uses the pinned Alpine SDK below; no host .NET SDK or native
compiler is needed to build the guest. Package restore requires network access.
The build context is the service directory, not the repository root.
Docker's [local artifact exporter](https://docs.docker.com/build/building/export/)
copies the final publish directory to the host, including `KestrelService` and
its debugging symbols. No running container is needed afterward.

With Podman (including versions without `build --output`), build the export image
and copy from a temporary container without starting it:

```bash
podman build --platform linux/amd64 -t blink-kestrel-nativeaot blink/tests/KestrelService
podman create --name blink-kestrel-export blink-kestrel-nativeaot /unused
mkdir -p blink/build/kestrel-nativeaot
podman cp blink-kestrel-export:/. blink/build/kestrel-nativeaot/
podman rm blink-kestrel-export
```

Run that executable through Blink and leave it serving browser requests:

```bash
dotnet build blink/samples/ManagedConsumer/ManagedConsumer.csproj -c Release --disable-build-servers
dotnet blink/samples/ManagedConsumer/bin/Release/net10.0/ManagedConsumer.dll --serve \
  blink/build/kestrel-nativeaot/KestrelService
```

Visit **http://127.0.0.1:8080/health** and press **Ctrl+C** in the terminal to stop.
Append `8081 SeparateProcess` to use another host port and a separate worker.
The sample supplies the guest's required runtime settings; the Dockerfile
controls compilation, not the VM's environment or network grants.

Edit `Program.cs` to customize this example and rebuild. To reuse the Dockerfile
for another .NET 10 NativeAOT-compatible project, copy it and `.dockerignore` to
that project's build context and pass `--build-arg PROJECT=YourApp.csproj` (or a
relative project path). Include referenced projects in that context and keep any
`global.json` compatible with SDK 10.0.401. The exported executable name follows
the project's assembly name. Static musl packaging alone does not guarantee that
every application uses only guest facilities implemented by Blink. `--serve` is
the Kestrel example launcher and passes a port argument; use `--run` for a general
console guest as described in [ManagedConsumer](../../ManagedConsumer/README.md).

## Service and qualification recipe

The deliberate service configuration is HTTP/1 on IPv4 loopback, a port supplied
as the sole argument (zero selects an actual ephemeral port), and cleared logging
providers to reserve stdout for `READY <actual-port>` and `STOPPED`. Transport
queue/thread defaults remain unchanged. `/health` returns `ok\n`; `/stop` returns
`stopped\n` and its response completion requests ordinary application shutdown;
the fallback returns HTTP 404 and `not found\n`. Responses specify text/plain and
Content-Length. READY follows actual `StartAsync`; STOPPED follows host shutdown.

Microsoft documents the [NativeAOT CreateSlimBuilder template](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/native-aot?view=aspnetcore-10.0)
and [Kestrel endpoint protocols](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-10.0).
The build uses the previously successful standard Alpine NativeAOT recipe:

```sh
python3 blink/tests/KestrelService/build-native.py
```

The runner pins official image
`mcr.microsoft.com/dotnet/sdk:10.0.401-alpine3.23-aot-amd64@sha256:240a20b94625153877c8ec4ed0394d3396f243a5663fe74b8f601c31f1dac3fc`,
SDK 10.0.401 and runtime/ASP.NET Core/ILCompiler 10.0.12. It publishes one ordinary
`linux-musl-x64` NativeAOT build with `StaticExecutable=true`,
`PositionIndependentExecutable=false` and invariant globalization, verifies
static x86-64 ET_EXEC ELF with no interpreter/shared-library dependencies, and
stops on a failed build without alternate toolchains or automatic retries.
Receipts retain source snapshots, resolved package identities, tool and container
identity, command logs, ELF headers, complete native syscall trace and raw HTTP.

Native execution uses exactly `LANG=C`, `DOTNET_GCHeapHardLimit=1000000`,
`DOTNET_GCRegionRange=2000000`, and `DOTNET_GCRegionSize=100000` (GC values are
hexadecimal, matching the earlier controlled native guest profile). This does
not establish a whole-process memory limit. The first actual binary size,
runtime syscall needs and observed thread count determine whether existing
managed worker limits suffice; no compatibility is presumed.

Five normal requests cover health, a health header padded with 3,500 bytes, the
same request sent in seven immediate writes, a missing path, and stop. Writes do
not imply TCP packet boundaries. Full status/reason, every header except the
validated actual RFC1123 Date value, and response body form the semantic oracle;
raw Date bytes are retained. The runner requires ordinary exit zero, exact
readiness/shutdown output, empty stderr and process-group cleanup without signals.
Native passing results do not count as translated managed execution results.

## Observed first native qualification

`../../artifacts/kestrel-guest-musl/attempt-o5jvvf7t/receipt.json`
has SHA-256 `7bc07c1e8d01dd3d326fdbb436473ff0b2b8dcaf2910aea6fffebdaa7b119865`.
The unchanged standard publish completed in 23.91 seconds with no warnings or
errors. All 14 recorded commands exited zero, the container was removed, and an
independent read-only audit verified 90 recorded source/tool/package/log/binary
identities. This was the first build attempt; no musl workaround was needed.

The actual `publish/KestrelService` executable is **9,371,272 bytes**, SHA-256
`ef6f1433794a42fe32b0fed4851bf88dd0631cd6a836550c6effca79d9e9a3ac`.
It is static x86-64 ET_EXEC with no PT_INTERP or DT_NEEDED, and TLS file size 24,
memory size 296, alignment 8. Its size exceeds the previous worker's 2 MiB image
limit. ASP.NET Core and .NET runtime download dependencies and NativeAOT compiler
resolve to the pinned 10.0.12 versions.

Native health, the 3,592-byte padded request, the same request in seven writes,
404 and stop all passed. Each returned the expected status/body, Kestrel server
header, content type/length and connection close, plus a real valid Date header.
Actual readiness and STOPPED output, exit zero, empty stderr and process-group
drain without signals passed. The full syscall trace has SHA-256
`0ef6c3ddbbdd70a83dda93fa3353318a16708430343288f24da5f32ece1a40e4`.

The trace observes 14 TIDs (13 clone entries). It includes one epoll creation,
six registrations and 17 epoll wait entries; the final wait is interrupted by
normal process exit. Registrations request EPOLLIN, EPOLLOUT and EPOLLET for the
listener and accepted sockets, and actual waits return IN/OUT/HUP events.
Sockets use nonblocking fcntl, accept4 and TCP_NODELAY. Listener shutdown requests
SO_LINGER with on=1 and linger=0. These require more than the previous empty-epoll
raw TCP workload.

Other observed normal startup/shutdown calls include inotify (92 watch additions
and removals against the native content-root tree), pipe2/poll, memfd_create,
fchmod, filesystem metadata/enumeration and a diagnostics Unix socket under
`/tmp`. These are observations of this default configuration and native content
root, not proof that every call is indispensable for HTTP. The four GC variables
do not prove that the previous 64 MiB/16-worker managed limits suffice.


## Fixed-configuration native profile

The unchanged ELF also passes all five native cases with exactly two additional
supported settings: `DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false` and
`DOTNET_EnableDiagnostics=0`. The fixed private image needs no configuration hot
reload or debugger/diagnostic IPC; genuine Kestrel transport remains unchanged.
Microsoft documents [reload configuration](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/?view=aspnetcore-10.0)
and [diagnostic settings](https://learn.microsoft.com/en-us/dotnet/core/runtime-config/debugging-profiling).

```sh
python3 blink/tests/KestrelService/native-profile.py \
  --guest-receipt blink/artifacts/kestrel-guest-musl/attempt-o5jvvf7t/receipt.json
```

Receipt `kestrel-native-profile/attempt-zphi57zq/receipt.json` has SHA-256
`e1e3c2ecf4c929f6f13d0f4937757cdc0dc82ee2b55d2c76d1fd88c4ec7db01a`.
All request bytes/write schedules and semantic response fields match the earlier
native witness except independently validated actual Date values. Normal exit,
stdout/stderr, process drainage and source/tool/ELF identities pass. The trace
observes12 TIDs, actual edge-triggered registrations/nonblocking/MSG_PEEK, and
listener SO_LINGER. Configuration watches and diagnostics memfd/stream IPC are
absent; the ordinary Unix datagram capability probe remains. These are measured
profile observations, not a new executable or a managed pass.
