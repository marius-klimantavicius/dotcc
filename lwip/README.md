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

`LwipLowpanLink` owns a radio, BLE, or ZEP interface and its shutdown. Radio and
ZEP links in one stack share the original PAN, short address, compression contexts,
reassembly list and timer; BLE has separate owner-wide peer/context state. Keep
their configuration consistent within each stack. Different stacks have separate
state. See [the radio/BLE/ZEP example and verification scope](tests/Zep/README.md).

```csharp
using System.Collections.Concurrent;
using System.Net;
using Managed.Networking;

using var stack = new LwipStack();
stack.Start();
var outgoing = new ConcurrentQueue<byte[]>();
LwipLowpanLink.ConfigureRadio(stack, 0x1234);
using var radio = LwipLowpanLink.StartRadio(stack, IPAddress.Parse("fd00::1"),
    new byte[] { 2, 0, 0, 0, 0, 1 }, outgoing.Enqueue);
radio.SetDefaultRoute();
// Drain outgoing on the transport side; each callback supplies an owned copy.
// Supply complete received frames with their radio FCS already removed:
// radio.Receive(frameWithoutFcs);
```

The output callback runs on the stack core and should enqueue its copied bytes.
Deliver queued input outside callbacks. Radio output includes the original FCS;
`Receive` takes a complete frame with that trailer already removed. BLE uses a
complete L2CAP payload. Input is copied before entering the original algorithm.
Disposing a link stops ingress, drains queued callbacks and then frees interface
backing. Disposing the stack also closes its registered links. A surviving radio
or ZEP link keeps the shared timer and pending reassembly until the last lease
closes. Application-owned raw PCBs still require explicit cleanup.
