# Managed lwIP campaign

This campaign translates lwIP 2.2.1 into a per-instance managed networking stack.
It includes bundled protocol/application/link features, including self-contained
PPP crypto, while excluding external crypto/TLS libraries and production TAP/TUN
or physical drivers. See [the complete plan](docs/PLAN.md) for required scope and
open acceptance gates, and [host contracts](docs/host.md) for current boundaries.

Implementation and qualification are in progress. A successful translation or
bootstrap run does not establish the complete plan. Required unimplemented suites
fail explicitly rather than reporting a partial workload as full acceptance.

```bash
bash lwip/scripts/translate.sh --fetch missing --profile threaded
dotnet run --project lwip/samples/ManagedConsumer -c Release
python3 lwip/tests/NativeOracle/run.py --profile threaded
python3 lwip/tests/NativeOracle/run.py --profile nosys
dotnet run --project lwip/tests/SysArch -c Release
```

The managed consumer connects two independent translated owners over copied
Ethernet queues, transfers a deterministic 256 KiB TCP echo and UDP datagram over
IPv4 and IPv6, then closes endpoints and joins its workers. Select a generated
alternate product with `-p:LwipProject=/absolute/path/to/TranslatedLwip.csproj`;
the consumer imports capabilities from that actual project. No profile hint is
needed for the main-loop product; an explicitly mismatching `LwipProfile` fails
the build.
All runtime sockets in this workload are lwIP sockets, not OS sockets.

NativeOracle requires a C compiler and POSIX threads on Linux. Its native code is
a test dependency only. Product builds use the .NET 10 toolchain and shared
DotCC.Libc runtime. Authored host/facade files stay under `src`; generated projects
reference those originals so ordinary builds pick up host changes directly.

When selecting `LwipProject` inside a consumer project, import `src/LwipProduct.props`
and forward the normalized selection to its facade reference:

```xml
<ProjectReference Include="path/to/Managed.Lwip.csproj"
  AdditionalProperties="LwipProject=$(_LwipSelectedProject);LwipProfile=$(LwipProfile)" />
```

The import enables static graph restore, adds an explicit selected-product
reference, and removes only its duplicate SDK-generated transitive edge. Other
application dependencies retain their normal transitive behavior. This preserves
the selection during both restore and build, without duplicate product instances.
This matters for relative paths: project-local properties do not automatically
propagate to referenced projects. `tests/LocalReference` verifies this pattern
with a local relative NO_SYS selection and no command-line product/profile hints.
Older auxiliary publications (including contrib and http-content) need a fresh
translation before using the current facade, because capability metadata is now
required alongside the generated project.
