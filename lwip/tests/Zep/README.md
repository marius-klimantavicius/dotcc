# Owned radio, BLE and ZEP links

The facade preserves lwIP's original LOWPAN compression, fragmentation,
reassembly and UDP ZEP algorithms. It owns the netif and callback lifetime. Use
`StartRadio` for complete decoded IEEE802.15.4 frames, `StartBle` for complete
L2CAP payloads, and `StartZep` for the original UDP transport. All constructors
require a started `LwipStack`, a zone-free IPv6 interface address and the selected
product's six-byte interface hardware address.

For BLE, configure the owner-wide local and peer EUI64 pair before traffic:

```csharp
byte[] local = { 2, 0, 0, 0, 0, 0, 0, 1 };
byte[] peer  = { 2, 0, 0, 0, 0, 0, 0, 2 };
LwipLowpanLink.ConfigureBlePeer(stack, local, peer);
using var ble = LwipLowpanLink.StartBle(stack, IPAddress.Parse("fd00::1"),
    new byte[] { 2, 0, 0, 0, 0, 1 }, outgoing.Enqueue);
ble.SetDefaultRoute();
// ble.Receive(completeL2capPayload);
```

Here `stack` is started and `outgoing` is a `ConcurrentQueue<byte[]>`, as in the
root README example. Configure compression prefixes using
`LwipLowpanLink.SetContext(stack, ble: true, index: 0, IPAddress.Parse("fd00::"))`.
Radio/ZEP contexts use `ble: false`. These settings belong to the entire stack,
not an individual link; BLE settings are separate from radio/ZEP settings.

ZEP carries its own radio frames inside ordinary UDP. Its optional transport
handle must come from `stack.AddInterface` on the same owner. The stack keeps
that Ethernet backing alive until registered services close:

```csharp
nint transport = stack.AddInterface("10.1.0.1", "",
    new byte[] { 2, 0, 0, 0, 0, 1 }, ethernetOutgoing.Enqueue);
using var zep = LwipLowpanLink.StartZep(stack, IPAddress.Parse("fd00::1"),
    new byte[] { 2, 0, 0, 0, 0, 1 },
    new LwipEndpoint(IPAddress.Any, 17754),
    new LwipEndpoint(IPAddress.Parse("10.1.0.2"), 17754), transport);
```

`ethernetOutgoing` is another copied-frame queue. The caller supplies its normal
Ethernet input transport. ZEP receives through its owned UDP PCB; calling
`zep.Receive` is an API error. Port zero selects lwIP's ZEP default. Independent
ZEP listeners need distinct local ports. Foreign-owner transport handles are
rejected before native interface construction.

Radio output includes its original two-byte FCS. Verify/remove that trailer at
the transport boundary before `radio.Receive`; BLE payloads have no radio FCS.
The callback byte array is a retained copy and remains valid after the original
pbuf is freed or reused. `Receive` copies caller bytes synchronously. Output
callbacks execute on the core: queue work and perform facade input calls later,
outside that callback. `Index` is owner-relative and can be used as the IPv6 zone
only for endpoints in that owner. `Mtu` reports the original IPv6 interface MTU,
not the radio frame size.

Disposal stops input, drains the core FIFO, and frees backing in separate phases.
A failed phase remains owned for retry. Construction registers an empty wrapper
before acquiring native state so failed rollback remains reachable by stack
disposal. Closing one radio/ZEP link preserves other links' shared timer and
pending reassembly. Closing the last lease releases them. Advanced raw users can
retain a `RetainBorrowedRadioTimer` lease, but must close their raw interfaces and
callbacks before releasing it. Raw application PCBs are caller-owned.

The finite suite uses coherent positive ZEP/radio/BLE inputs, original emitted
radio/BLE output at payload sizes 17, 900, 1232 and 1233, copied-frame retention,
unchanged duplicate/reordered frames, loss with normal expiry, shared timer
leases, rollback, queued-input shutdown, and natural heap/pbuf/timer baselines.
It includes no authored corruption case. Native output controls compare the
same valid frame ordering before/after the narrow staged fragment-link repair.
Reports state exactly which profile/form/runtime cells have passed; this suite
does not establish the complete campaign plan.

```sh
bash lwip/scripts/test.sh --fetch never --profile threaded \
  --suite lowpan-services --form all --mode all
```

Repeat with `nosys` and `threaded-locking` after translating each selected
profile. Native fixtures are Linux test dependencies, not product libraries.
