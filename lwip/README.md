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


`LwipTftpService` owns one original TFTP client or server per stack, with copied
in-memory files and explicit limits for file bytes, total stored bytes and file
count. It supports octet transfers with printable ASCII names within the selected
original filename limit. The following uses two stacks already connected by the
application's packet transport:

```csharp
using var server = LwipTftpService.StartServer(serverStack,
    maximumFileBytes: 65536, maximumStoredBytes: 262144, maximumFiles: 16);
using var client = LwipTftpService.StartClient(clientStack, maximumTransferBytes: 65536);
server.SetFile("sample.bin", "hello"u8);
var endpoint = new LwipEndpoint(IPAddress.Parse("10.0.0.2"), 69);
byte[] downloaded = await client.DownloadAsync(endpoint, "sample.bin");
await client.UploadAsync(endpoint, "copy.bin", downloaded);
```

Files are stored in memory; filenames do not access the host filesystem.
`CancelPending()` closes the active original transfer and restarts the service;
partial writes are discarded. Original client requests have no retry timer, so
applications must explicitly cancel an unanswered request. Disposal cancels
pending client tasks, stops original timers/PCBs, drains callbacks and releases
storage handles. A borrowed original TFTP instance prevents owning construction
without being stopped. Stop that borrowed instance before starting an owner.

`LwipSntpClient` owns one original SNTP client per stack and exposes a bounded queue
of copied time values. User continuations run outside the original core:

```csharp
using var timeClient = LwipSntpClient.Start(clientStack,
    IPAddress.Parse("10.0.0.3"), notificationCapacity: 16);
LwipNtpTime sample = await timeClient.ReadTimeAsync(cancellationToken);
```

`Seconds` is the unsigned NTP era-second word and `Fraction` is in units of
2^-32 seconds. Consumers resolve the era and decide how to use the sample; the
service does not change the OS clock. A full queue drops its oldest sample.
Canceling a read cancels only that wait. Disposing the service stops original
poll/retry activity, drains callbacks and completes pending readers. See the
[UDP application cases and limits](tests/Applications.Udp/README.md).

The additional `threaded-enhanced` profile enables SACK output, TCP timestamps,
listen backlog, window scaling, linger, receive metadata, UDP broadcast reuse,
Linux-style `FIONREAD`, posted core timeouts and netif removal callbacks. It uses
a 131072-byte receive window with scale 2 and retains the ordinary 32768-byte
send budget. Select it explicitly with `--profile threaded-enhanced`; the three
baseline profiles retain their existing settings. The [option inventory](config/feature-options.json)
links compiled contracts separately from the completed native/JIT/AOT evidence.

`LwipMqttClient` owns one original MQTT client. Multiple clients can share a stack,
and different stacks keep separate callback and request registries. This example
uses an initialized stack with an application-provided packet transport and a
reachable broker; it is a usage snippet, not a standalone executed sample:

```csharp
using var mqtt = LwipMqttClient.Create(clientStack,
    new LwipMqttOptions { MaximumPendingRequests = 4, PublicationQueueCapacity = 16 });
var broker = new LwipEndpoint(IPAddress.Parse("10.0.0.2"), 1883);
var status = await mqtt.ConnectAsync(broker,
    new LwipMqttConnectOptions("example-client") { KeepAliveSeconds = 0 });
if (status != LwipMqttConnectionStatus.Accepted)
    throw new IOException($"Broker refused the connection: {status}");
await mqtt.SubscribeAsync("example/in", maximumQualityOfService: 1);
await mqtt.PublishAsync("example/out", "hello"u8.ToArray(), qualityOfService: 1);
LwipMqttPublication publication = await mqtt.ReadPublicationAsync(cancellationToken);
```

Topic/payload notifications and outgoing payloads are copied. Notification queues
are bounded and discard new items when full; counters expose discarded items.
Consumer limits can be lower than original MQTT limits. `CancelPending()` and
`Disconnect()` stop the whole original connection and settle its pending tasks;
there is no per-request original cancellation or public graceful DISCONNECT-packet
entry. Canceling a publication read only cancels that wait. Disposal stops original
work, drains callbacks, releases request/client roots and unregisters the completed
service. Broker FIN faults pending requests; ordinary CONNACK refusals return their
status. The [TCP application cases and remaining modes](tests/Applications.Tcp/README.md)
distinguish tested QoS1/lifetime behavior from broader unqualified modes.

Completed built-in services leave active stack tracking only after successful
cleanup; failed disposal remains registered for retry. Resolver callback retention
is separate: disposed resolver wrappers, including empty ones, remain in the
post-core-stop list until their owner stops. The MQTT tests prove collection of a
disposed wrapper with queued copied data while its stack remains alive.
