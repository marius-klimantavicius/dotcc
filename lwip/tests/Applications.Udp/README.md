# Finite UDP application and owning-service qualification

The seven registered `udp-app-*` suites pass on threaded, NO_SYS and locking
profiles, in raw/processed JIT and NativeAOT. Canonical consumers pass alongside
those matrices. Receipts: `152417-bf654250` (threaded), `153013-22a4e10d`
(NO_SYS), `153409-2521e1f0` (locking), all dated 2026-09-29. The deeper borrowed
TFTP factory rollback additionally passes threaded receipt `152929-8bc38f5f`;
NO_SYS/locking matrices include it. These finite cases do not close the broader
application plan described below. Historical draft/native and failed attempts
remain preserved; they are not replacement evidence for this final source.

## Preserved original-source observations

Native NO_SYS attempts are under `artifacts/udp-native-20260929/`, with separate
`<case>-attempt-1` directories and diagnostic-only `tftp-put-attempt-2`. Each
preserves input/config/header/staged hashes, commands, assertion control, process
logs, and the observed exit status. No protocol corruption or malformed input
was used. These runs preceded the new owning services and PUT adapter.

| Native case | Observation |
| --- | --- |
| `tftp-server` | Six exact RRQ/WRQ transfers of 0, 512 and 1025 bytes; six opens/closes; zero live handles; UDP PCB count 0 → 0 |
| `tftp-get` | Original client receives 1025 bytes; one open/close; zero live handles; UDP count 0 → 0 |
| `sntp` | One native probe notification: NTP seconds 3999999123, fraction 1073741824; original stop; UDP count 0 → 0 |
| `netbios` | One 62-byte address response; original stop; UDP count 0 → 0 |
| `mdns` | Correct PTR/SRV/TXT, three TXT callbacks, service/interface removed; original internal UDP PCB remains, count 0 → 1 |
| `tftp-put` | Original client returns ERROR 5, `Wrong block number`, after ordinary WRQ/ACK-0; both failures retained |

Original `tftp_put` initializes block 1, while the unchanged ACK handler expects
its current block number and a server correctly acknowledges WRQ with block 0.
The second attempt added only output diagnostics and confirmed the returned
opcode/error/message. It did not change transmitted bytes. PUT remains required; the historical failure is not an exclusion,
expected pass, or reason to use a nonstandard negotiation.

## Original algorithms and product boundaries

`src/port/udp_app_services.c` includes the selected original
`apps/tftp/tftp.c` and replaces its standalone compilation unit. Its helper
`lwip_host_tftp_put` retains the exact public-entry validation and original
`send_request`/`mode_to_string` calls, initializing block **0 before sending**.
A typed override redirects managed `tftp_put` to that helper. The protocol parser,
serialization, block advancement, retry timer and original cleanup remain
translated original code. There is no source text patch, after-send mutation,
or replacement host protocol engine.

Preparation must guard the unique original PUT body, state fields, initialization
and callback registration, `close_handle`, receive DATA/ACK/ERROR close branches,
`send_data` error close, timer close, and cleanup. Whole-file hashes record
provenance; the local contract guards must preserve the campaign's hash warn/off
semantics for unrelated original edits.

The owning wrapper installs a receive observer which copies only the first two
opcode bytes, calls original `tftp_recv` unchanged, then restores its context.
`lwip_owned_tftp_close_context` exposes original closure context: DATA=3, ACK=4,
ERROR=5, explicit stop=256, timer/other=0. Storage errors take precedence over a
last-block marker. Final DATA or ACK is required for successful completion;
explicit stop cancels, and original server timeout is unsuccessful. The observer
does not modify packets or replace parser decisions.

`LwipTftpService` registers its wrapper before acquiring original state and owns
one client or server per stack. It provides copied memory-backed server files,
client `DownloadAsync`/`UploadAsync`, `CancelPending`, and deterministic disposal.
Callbacks retain explicit owner, stable callback table and rooted storage handles
until original cleanup and the core barrier complete. Errors are contained in
callbacks. Original ERROR callbacks pass a network-order code without `ntohs`;
the managed diagnostic normalizes the copied code without changing parsing.

Server storage has separate maximum file size, retained payload bytes and file
count. Distinct nonempty names with zero-byte payloads consume file-count budget.
One active transfer, copied outputs and retained read snapshots need additional
bounded transient storage; aggregate payload accounting is not a promise of an
exact process-memory ceiling. Partial failed/canceled writes preserve the prior
file. Client submission failures run original cleanup before releasing remaining
unsubmitted handles. Callback failure is observable through `ThrowCallbackErrors`.

Original client `tftp_get`/`tftp_put`/`send_request` do not schedule a request timer.
An unanswered client request therefore remains pending until explicit
`CancelPending` or disposal in these cases. Only the original **server**
DATA/retry/timeout path is qualified by the authored loss scenario. Automatic
client request retransmission/timeout is not supplied by these original entries
and is not claimed by the facade. It must not be inferred from server evidence
or silently added as host protocol code.

## Actual SNTP notification service

The managed `sntp_service.c` wrapper includes `src/port/udp_app_services.h`
before unchanged original `sntp.c`. Native comparison supplies the same header
with its compiler-supported forced include:

```
SNTP_SET_SYSTEM_TIME_NTP(seconds, fraction)
    -> void lwip_host_sntp_time(unsigned int seconds, unsigned int fraction)
    -> LwipUdpApplications.SntpTime(Lwip owner, uint seconds, uint fraction)
```

The typed binding delivers to `LwipSntpClient` registered on that same owner.
`LwipNtpTime` copies host-order unsigned NTP words: seconds retain their NTP era,
and the fraction unit is 2^-32 seconds. Consumers resolve the era and wall-clock
policy. Neither this service nor the native comparison sets the OS clock.

`LwipSntpClient.Start` configures a numeric server and original poll mode. It
registers before acquisition, rejects an already active borrowed client, and uses
a bounded BCL channel with asynchronous continuations. At capacity it discards
the oldest notification. `TryReadTime`/`ReadTimeAsync` expose copied samples;
`ObservedNotifications` counts offered notifications. Disposal uses original
`sntp_stop`, drains the core, removes the owner callback and completes pending
readers. Buffered copied samples may still be read after disposal.

The old `sntp-clock.c/.h` counter is now **native fixture support only**. It
implements the same C notification ABI for original-source comparison; its
getters are never selected as a product delivery API. The managed fixture reads
the actual owning service. No `UdpApplicationsClockProbe` build property remains.

The exact-response case assumes `SNTP_COMP_ROUNDTRIP=0`, echoes the transmit
stamp into Originate Timestamp and supports implemented response-check levels
0–2. It is numeric-address coverage, not DNS qualification. Startup waits are
bounded at ten seconds, without changing original scheduling algorithms.
A later DNS/roundtrip variant must retain DNS name storage and provide
`SNTP_GET_SYSTEM_TIME_NTP` with controlled local NTP timestamps and an expectation
for the compensated result. The same owner-bound notification ABI applies.
Response-check levels 3 and 4 are upstream TODOs.

## Authored finite cases

`Program.cs` requires a matching successful native report before executing its
selected managed case, and emits success only after assertions and `VirtualPair`
disposal. Two translated owners exchange copied Ethernet frames. `UdpPeer` uses
original translated UDP/IP; its bounds checks inspect output rather than create
corrupt inputs.

| Case | Authored scope |
| --- | --- |
| `tftp-server` | Original RRQ/WRQ for 0, 512 and 1025 bytes, terminating blocks and exact storage closes |
| `tftp-get` / `tftp-put` | Original client/server 1025-byte transfer, ordinary negotiation, cleanup; PUT uses the reviewed entry adapter in product comparisons |
| `owned-tftp` | Actual owning client/server, copied GET/PUT, duplicate admission and borrowed-original ERR_USE construction rollback, ordinary missing file, explicit cancellation/restart, independent owner disposal/continuation, partial-write cancellation, original server timeout, bounded metadata/payload rejection and recovery |
| `sntp` | Actual owning notification with exact NTP words, duplicate rollback, independent owner disposal/continuation, pending read completion and original stop |
| `netbios` | Original ordinary NB address query, encoded name/transaction/address and stop |
| `mdns` | Original probing, legacy unicast PTR/SRV/TXT reply, service/interface/result-callback removal |

A native process has one original global stack. Its TFTP counterpart is an
authored independent coherent UDP peer, not a second pretend native TFTP owner.
Native `owned-tftp` exercises borrowed-original admission/continuation, the same
C wrapper and success close contexts,
explicit client cancellation, ordinary storage callback failure, and original
server timeout. Native and managed addresses intentionally differ, but each
response must contain its own interface address. Managed cases additionally
exercise consumer wrappers and independent generated-owner lifetimes.

For timeout cases only, the native adapter renames the existing host `sys_now`
implementation and adds a controlled clock offset. The managed case supplies its
existing `LwipSysArch` host clock. Both run original `sys_check_timeouts`; no TFTP
timer or retransmission algorithm is replaced. The peer withholds a normal ACK.

`LWIP_NETBIOS_RESPOND_NAME_QUERY` gates **NBSTAT**, not the ordinary NB address
reply covered here. NBSTAT remains a separate valid-query variant. Original mDNS
has no whole-service deinitializer: its retained internal PCB is recorded
explicitly; service/interface removal is not proof of final owning mDNS teardown.

## Source and runner contracts

1. Verification policy records the reviewed files and finite cases. Freeze
   shared config, staged inputs and source hashes before execution; changed
   fixture source requires affected qualification to be refreshed.
2. Select `udp_app_services.c` instead of standalone original `tftp.c`, add the
   selected upstream `src` include root, and select `sntp_service.c` for unchanged original `sntp.c`, supplying
   `udp_app_services.h` first (dotcc does not implement GCC `-include`). Compile the new Host file and managed facade files.
3. Configure two typed overrides: `lwip_host_sntp_time` →
   `LwipUdpApplications.SntpTime`, and original `tftp_put` →
   `LwipUdpApplications.TftpPut`. Preserve pointer/enum ABI. Export the authored
   `lwip_host_tftp_put`, `lwip_owned_tftp_start`, `lwip_owned_tftp_stop` and
   `lwip_owned_tftp_close_context` for the facade.
4. `native.py --profile <profile> --case <case> --output <unique-attempt-path>
   --product-boundaries` selects the same C wrapper and notification ABI.
   Force-include `native-product-adapter.h` only on native fixture call sites
   (`native-tftp.c`), never the wrapper or original TFTP source. It maps public
   `tftp_put` calls to the exact helper. Without this flag the runner preserves
   original-source baseline behavior; `owned-tftp` requires the flag.
5. The native runner uses existing `NativeOracle/sys_arch.c` and `Host/bridge.c`
   as prerequisites but invokes no existing oracle workload. Its assertion
   control deliberately fails before initialization and sends no packet. It
   retains commands, logs, source/config/header manifests, final hash comparison
   and nonzero failures. Unique output directories preserve each attempt.
6. The reviewed campaign imports `campaign-runner.py`. It dispatches explicit
   `udp-app-{tftp-server,tftp-get,tftp-put,owned-tftp,sntp,netbios,mdns}` suites,
   invokes product-boundary native comparison, then raw/processed JIT/AOT.
   The project imports canonical `src/LwipProduct.props`. Profile applicability
   and receipt acceptance remain primary campaign responsibilities.

Remaining scope beyond these finite cases includes client request-loss behavior,
SNTP DNS/roundtrip/DHCP server workflows, NBSTAT, mDNS multicast/search/conflict/
restart and consumer ownership, IPv6 application cases, broader owning services
and protocol-specific consumer examples. None is closed by source selection or
an unexecuted fixture. No custom corruption, malformed-peer or tampering scenario
is present or proposed.
