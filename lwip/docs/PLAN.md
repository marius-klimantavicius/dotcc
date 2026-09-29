# Translate lwIP with DotCC

Status: implementation active; initial stack/consumer boundaries qualified, full acceptance incomplete.
Campaign: `lwip/`. Updated: 2026-09-29.

This plan adopts [translation baseline v2](../../docs/plans/translation-baseline.md)
and the [shared framework](../../docs/campaigns.md), using the
[project template](../../docs/plans/translation-template.md). Common execution,
provenance, repair and publication rules stay in those documents. Current user
instructions govern implementation, commits, branch work and delegation. The user has now authorized coordinator-led implementation through completion.
Implementation agents work in the isolated `campaign/lwip-full` worktree;
qualification gates remain open until current evidence passes them.

## Outcome and scope

Translate lwIP's bundled implementation into `TranslatedLwip`, preserving its
protocol algorithms, buffers, timers, application protocols and public C APIs.
Include every implemented feature that can run using the supplied source,
`DotCC.Libc` and authored .NET host code, without adding an external product
library. The managed host supplies execution services and packet/byte transport;
lwIP continues to implement TCP/IP, socket semantics and application protocols.

The first useful workload creates two independent stacks in one process, connects
their Ethernet interfaces through managed frame queues, and transfers a
deterministic 256 KiB TCP payload plus UDP datagrams over IPv4 and IPv6. Run it
through raw APIs in the main-loop profile and through netconn/socket APIs in the
threaded profiles. Verify bytes, closure and resource cleanup. This is an early
milestone; it does not close the broader translation. Final delivery also exposes
link, routing, configuration and application features with runnable examples.

| Scope | Features/APIs/workloads |
| --- | --- |
| Required core and APIs | `NO_SYS=1` and `NO_SYS=0`; raw, plain altcp, netconn, BSD sockets, netif API, resolver APIs and actual `tcpip_thread`; IPv4/IPv6, ICMP/ICMPv6, UDP/UDP-Lite, TCP, fragmentation/reassembly, Ethernet/ARP, DHCPv4/ACD/AutoIP, bundled stateless DHCPv6, DNS, SLAAC/ND/DAD, IGMP/MLD, loopback and IPv4/IPv6 forwarding. |
| Required links | Ethernet bridging, SLIP, PPPoS, PPPoE, PPPoL2TP, supported PPP negotiation/authentication/compression options subject to the dependency policy below, IEEE 802.15.4/BLE 6LoWPAN and ZEP. Supply managed byte/frame transports and fixtures; hardware is not required. |
| Required applications | HTTP client/server and proxy CONNECT, content/SSI/CGI/POST support, MQTT, SMTP, SNTP, mDNS, NetBIOS responder, TFTP client/server, lwiperf, SNMPv1/v2c and dependency-free SNMPv3 `noAuthNoPriv`. Include their bundled callbacks and configuration variants through explicit profiles/cases. |
| Required bundled utilities/contrib | Dependency-free contrib applications, examples and addons, including echo/chargen/netio/ping/RTP, netconn HTTP, socket examples, static IPv6 routing, DHCP option hooks and bundled-MD5 TCP ISN/signature addons. Classify the entire contrib tree at P0. Translate uncompressed `makefsdata` as a separate tool; keep demos/generators outside the stack library. |
| Excluded dependencies | TLS/DTLS and mbedTLS integration, SNMPv3 authentication/privacy requiring an external crypto provider, external PolarSSL/mbedTLS backends, external DNS-SD/Bonjour resolver, and makefsdata compression requiring separately acquired miniz or zlib. No new product library or NuGet dependency to restore these features. |
| Excluded platform delivery | Production TAP/TUN, physical NIC/radio/serial device drivers and upstream OS-specific ports; replace the execution/link boundary with managed host interfaces. Protocol code above those interfaces stays in scope. |
| Upstream limitations | Preserve the selected release's actual limitations. Do not promise stateful DHCPv6 or implement PPP modes explicitly marked unsupported upstream (for example CBCP, ECP and demand mode). Classify such entries as upstream-unimplemented, not translation successes. |
| Later extensions | External-library features and additional execution targets (Windows/ARM64). No bundled working feature is deferred merely to shorten this campaign. |
| Baseline departures | None in execution methodology. Initial runtime qualification is Linux x64; portable Windows orchestration remains required. Random fuzzing, extended stress and performance thresholds are not acceptance requirements. |

Dependency policy applies to additional libraries, not automatically to bundled
code: PPP ships its own hash/cipher primitives. The user explicitly selected
those self-contained modes with external crypto backends disabled; TLS and
external-provider crypto remain excluded. Record this distinction in the feature
manifest and do not acquire a crypto dependency through autodetection. BCL entropy
for protocol randomness is a host service, not TLS implementation.

Required features may use different compile-time profiles; mutually exclusive
options need not coexist. Every supported feature family must map to active
translated code and a behavior test in at least one required profile. Merely
listing a file whose body preprocesses away is not coverage. Do not demote a
feature because it needs more host code or exposes a compiler defect.

## Decisions and feasibility work

`Decided` records a planning choice, not a successful experiment. `Provisional`
choices require the stated evidence before dependent functionality is accepted.

| ID | Decision | Choice and reason | Status; resolving evidence/gate |
| --- | --- | --- | --- |
| D1 | Source version and dependencies | lwIP 2.2.1, tag `STABLE-2_2_1_RELEASE`, peeled commit `77dcd25a72509eb83f72b033d219b1d40cd8eb95`. Use this identified release instead of a moving development branch. Product and bundled tests share this source pin. No additional translated product dependency. | Decided; inspected release on 2026-09-29; acquisition metadata and native build due P0. |
| D2 | Source/configuration closure | Explicit manifests covering core, APIs, netifs, PPP, 6LoWPAN, all applications without external dependencies, and classified contrib/tool inputs. Default `threaded`, alternate `nosys` and `threaded-locking`, diagnostic `unit` and `unit-nosys`. C17 with authored architecture/OS ports and fixed includes. | Decided scope; exact per-profile closure and feature-to-suite inventory P0. |
| D3 | ABI and wire formats | DotCC's little-endian LP64 model on Linux x64: 8-bit bytes, 16-bit short, 32-bit int, 64-bit long/pointers/size_t; fixed-width lwIP types and `MEM_ALIGNMENT=8`. Preserve packed network layouts, network byte order, socket structures, descriptor/errno conventions and PPP/6LoWPAN byte encodings. | Provisional; actual native/emitted storage and callback probes P1. |
| D4 | Host/dependency policy | Reuse `DotCC.Libc` for supported libc/pthread/clock/file services; add typed BCL host implementations for `sys_arch`, links, storage and application callbacks. Keep lwIP's heap/pools and networking algorithms translated. No native lwIP, OS socket substitution, external protocol/crypto/compression library or project-specific platform import in the product. | Decided; per-symbol capability/import audit P1/P2. |
| D5 | State/concurrency/lifecycle | One generated `Lwip` owner per stack with `--instance-methods --runtime=c` at emission and link. Main-loop profile serializes entry; threaded profiles allow upstream-defined concurrent API entry and use the real TCP/IP thread/core lock. Thread/callback leases retain their explicit owner. | Provisional; isolated globals, per-thread semaphores, safe blocking/concurrency and worker shutdown P1/P3. No singleton/subprocess fallback. |
| D6 | Translation/delivery | Object emission/managed-library linking; class `Lwip`, namespace `Managed.Networking`, assembly `TranslatedLwip`. Shared `link_flags`: `--split=size --split-size=262144`, `--literal-pool`, `--deduplicate-inline`; no exception. Host linked from `src/Host/`, facade under `src/Managed.Lwip/`; tools/examples separately delivered. | Decided; all selected profiles raw/processed P2, consumer P5. |
| D7 | Native and independent controls | Native harness with matching profile, inputs and logical host contracts. Independent kernel TCP/IP peer through a test-only pre-provisioned link, plus application/link peers or published fixtures selected per feature. Native lwIP and another managed owner are not independent implementations. | Decided; native validity P0, independent TCP/UDP/application checks P4. Test TAP bridge remains isolated from product delivery. |
| D8 | Required matrix | Linux x64 × each required product profile × raw/processed × JIT/NativeAOT; corresponding diagnostic profiles for upstream tests. Feature cases run in declared applicable profiles, not every impossible combination. Rooted library AOT and normally trimmed consumers are separate gates. | Decided; P4–P6. |
| D9 | Tests | All applicable upstream cases, core/API/link/application scenarios, ordinary errors, bounded recovery, thread/owner isolation and independent peers. Exclude random fuzzing, exhaustive allocation-fault injection and extended stress. Test tools/peers are separate from product dependencies. | Decided; feature and case inventory P0, required rows pass P4. |
| D10 | Adaptation precedence | Upstream options/port interfaces first; guarded macro and typed function overrides next; a minimal staged text patch only after documenting why these cannot express the needed change. Apply to product, tests and native controls. | Decided user requirement; override reports and patch-necessity audit P1–P6. |

Create `config/sources.json` with repository/archive URLs, tag, peeled commit,
archive prefix `lwip-77dcd25a72509eb83f72b033d219b1d40cd8eb95`, required files,
license locations and optional archive checksum. Acquisition URL:
`https://github.com/lwip-tcpip/lwip/archive/77dcd25a72509eb83f72b033d219b1d40cd8eb95.tar.gz`.
Keep revision/checksum literals in metadata, not Python/Bash. Record native
compiler, kernel peer and Check framework versions as test/tool inputs separately.
Preserve `COPYING` and source-specific notices in source and binary delivery.

Additional planned metadata: source-group manifests, `config/features.json`,
`config/profiles.json`,
`config/dotcc-overrides.json`, `config/adaptations.json`, and
`config/upstream-tests.json`. Default hashes are `warn`, `off` is supported and
strict reproduction is optional. Existing usable edited source trees are accepted
with their observed provenance. Exact adaptations retain unique local guards in
every hash mode. Do not mutate acquired references during preparation.

## Source, host and ownership contracts

### Source and configuration closure

Use the pinned [source list](https://github.com/lwip-tcpip/lwip/blob/77dcd25a72509eb83f72b033d219b1d40cd8eb95/src/Filelists.cmake)
as the starting manifest, then inventory source files it does not compile directly:

| Source group | Treatment |
| --- | --- |
| `lwipcore_SRCS`, `lwipcore4_SRCS`, `lwipcore6_SRCS` | Include all 38 core units with every dependency-free implemented feature represented in the profile matrix. |
| `lwipapi_SRCS` | Include all nine units; actual netconn/socket/netif/tcpip execution in threaded profiles. NO_SYS guards legitimately disable incompatible APIs in `nosys`. |
| `lwipnetif_SRCS`, `lwipsixlowpan_SRCS`, `lwipppp_SRCS` | Include Ethernet, bridge, SLIP, all four 6LoWPAN units, PPP and bundled primitives. Account for unsupported modes separately; do not import an OS port. |
| `lwipallapps_SRCS` | Include SNMP, HTTP, lwiperf, SMTP, SNTP, mDNS, NetBIOS, TFTP and MQTT. Crypto-disabled SNMPv3 remains included; alternative raw/netconn implementations get separate configurations. |
| `lwipmbedtls_SRCS` | Exclude the TLS and SNMPv3 mbedTLS adapters; record exact filenames and dependency reasons. |
| `lwipmakefsdata_SRCS` and included data | Separate managed tool with bundled tinydir, compression disabled. HTTP fsdata is included data, not an independently linked implementation. Record generated-content inputs. |
| `contrib/` | Inventory every file against `contrib/Filelists.cmake` and directory contents. Include dependency-free addons and applications as translated extension/example projects. Classify OS/device ports as replaced host boundaries, TLS examples/Bonjour resolver as external-dependency exclusions, and documentation/build scripts as non-product tooling. No blanket contrib exclusion. |

Create `config/features.json` mapping each module/API/implemented option family to
its source group, required profile, host services, test and disposition. Additional
profiles needed for a bundled feature are required campaign work. Avoid an
unbounded Cartesian product of unrelated options: document representative valid
combinations and compatibility constraints. At P0 prove no feature disappeared
because a source-list aggregate, compile guard or build autodetection hid it.

Stage authored `lwipopts.h`, `arch/cc.h`, `arch/sys_arch.h`, packing and hook
headers in private work. Include order: profile headers, authored port headers,
staged upstream `src/include`, then shared runtime headers; test headers add only
explicit test paths. Use fixed-width types and a verified packing mechanism.
Do not impersonate an OS/compiler to select unrelated branches. Use checked-in
`lwip/init.h`, validating it against the release version; generate from its
CMake template only in staging if necessary. Record required application content
and generator inputs without making optional documentation tools dependencies.

Initial required profile design (freeze all effective macros and pool settings at P0):

| Profile | Execution and distinguishing features |
| --- | --- |
| `threaded` (default) | `NO_SYS=0`, `SYS_LIGHTWEIGHT_PROT=1`, real `sys_arch` and `tcpip_thread`; enable netconn/socket/netif APIs, per-thread netconn semaphores and qualified full-duplex behavior. `LWIP_TCPIP_CORE_LOCKING=0`; work enters the core through upstream message passing. Broad dependency-free core, links and applications enabled. SNMP raw variant. |
| `nosys` | `NO_SYS=1`, `SYS_LIGHTWEIGHT_PROT=0`; raw/altcp API and explicit timeout pumping. Netconn/socket/tcpip API disabled by upstream design. Core/link/raw-application behavior remains required, including forwarding, bridges, DHCPv6 and UDP-Lite. |
| `threaded-locking` | `NO_SYS=0` with core locking enabled and enforced owner/thread assertions; exercise the locking/input path, SNMP netconn/thread-sync variant and concurrent API use. Other incompatible application options receive explicit profile deltas. |
| `unit` | Derive from upstream `test/unit/lwipopts.h` and retain its `NO_SYS=0`, sockets, PPP and mDNS tests. Use upstream test scheduler/sys_arch behavior where tests require it, bind through test-only host overrides, and preserve actual assertions. This scheduler is not the production sys_arch implementation. |
| `unit-nosys` | Main-loop-compatible upstream core/link/application cases with documented test visibility, deterministic clock and pool sizes. Do not force threaded/socket cases into NO_SYS or patch their delivery model unnecessarily. |

All product profiles enable applicable dual-stack TCP/UDP/UDP-Lite/raw/altcp,
ARP/ND/ICMP, fragmentation/reassembly, DNS, DHCPv4/AutoIP/ACD, stateless DHCPv6,
SLAAC/DAD and multicast. Enable IPv4/IPv6 forwarding with endpoint/router behavior
selected per interface/workload; test loopback, bridge ports and routing across
multiple interfaces. DHCPv6 includes the information-request behavior implemented
by this release, not invented stateful address leasing.

Use `LWIP_TIMERS=1`, `LWIP_TIMERS_CUSTOM=0`, software checksums,
`MEM_LIBC_MALLOC=0`, `MEMP_MEM_MALLOC=0`, `MEM_ALIGNMENT=8` and
`IPV6_FRAG_COPYHEADER=1`. Initial Ethernet MTU is 1500. Start with
`MEM_SIZE=262144`, `PBUF_POOL_SIZE=256`, `TCP_MSS=1440`,
`TCP_WND=TCP_SND_BUF=32768`, 16 TCP/UDP PCBs, 8 listeners and 128 TCP segments;
size mailboxes, application/PPP pools and timeout slots for the enabled closure.
Reconcile dependent constraints against native builds before freezing defaults.
Additional valid upstream memory/socket/HTTP options get feature-specific
configuration cases rather than silently disappearing from scope.

Enable PPPoS/PPPoE/PPPoL2TP, supported server/client modes, PAP, bundled
CHAP/MSCHAP/EAP/MPPE and VJ support where upstream implements them, with
`LWIP_USE_EXTERNAL_POLARSSL=0` and `LWIP_USE_EXTERNAL_MBEDTLS=0`. Keep unsupported
CBCP/ECP/demand modes disabled. P0 must inventory supported EAP submethods and
compression modes instead of treating their option names as implementation proof.

Set `LWIP_ALTCP_TLS=0`; omit TLS adapters. Use `LWIP_SNMP_V3=1`,
`LWIP_SNMP_V3_MBEDTLS=0`, `LWIP_SNMP_V3_CRYPTO=0` for `noAuthNoPriv` and reject
unsupported authentication/privacy requests honestly. HTTP/MQTT/SMTP run over
plain translated TCP. `makefsdata` uses `MAKEFS_SUPPORT_DEFLATE=0`; miniz is not
bundled and may not be downloaded implicitly. Keep stats and assertions active
in qualification and record all source-specific notices, including bundled PPP
primitives and tinydir.

### Host strategy and campaign precedents

Follow the established division between translated algorithms and authored host
services. Reuse working shared primitives; do not copy whole older host layers or
assume their successful tests prove the lwIP contract:

- [SQLite overrides](../../sqlite/config/dotcc-overrides.json) and
  [mutex host](../../sqlite/src/HostVfs.Mutex.cs): typed platform registration and
  BCL-backed synchronization. Adapt lifetime/ownership to per-stack instances.
- [MsQuic platform host](../../msquic/src/BclHost/MsQuicHost.Platform.cs) and
  [queue host](../../msquic/src/BclHost/MsQuicHost.Queue.cs): explicit allocation,
  scheduling, callback drain and resource accounting. Do not reuse its native
  socket datapath as a replacement for lwIP's stack.
- [Valkey overrides](../../valkey/config/dotcc-overrides.json) and
  [worker host](../../valkey/src/Host/ValkeyHost.Bio.cs): `passInstance`, worker
  ownership and shared pthread lifecycle.
- [QuickJS overrides](../../quickjs/config/dotcc-overrides.json): guarded macro
  selection, typed host functions and preserved upstream algorithm bodies.
- [Shared pthread runtime](../../DotCC.Libc/PthreadLib.cs) and
  [instance calling convention](../../docs/instance-translation.md): reuse only
  the primitives whose owner/callback/wait semantics are verified for lwIP.

Map each required `sys_*` symbol to an existing libc implementation or a typed
`LwipSysArch` host method. An authored C `sys_arch` adapter may compose builtin
libc primitives; where semantics or owner propagation differ, use a direct
managed override. Declare opaque types in authored `arch/sys_arch.h` and store
host objects in a per-owner registry. No native pthread or OS library dependency
is required beyond the shared runtime/BCL. Keep application socket calls bound to
translated `lwip_*` functions: compatible names must never accidentally resolve
to `DotCC.Libc` OS-socket implementations.

| Boundary | Translated responsibility | Host/reused dependency | Ownership, errors and validation |
| --- | --- | --- | --- |
| Memory/pbuf | lwIP heap/pools, chains/refcounts and protocol queues | Stable owner backing; shared libc for remaining services | Copy ingress to owned pbufs and follow each input function's transfer rules. `linkoutput` copies before returning; no borrowed buffer escapes. Test GC stability, low memory and exact release. |
| Ethernet/radio/serial | Packet parsing, framing, PPP/SLIP, 6LoWPAN compression/reassembly, bridges and routing | Managed frame/byte link interfaces with bounded queues; `sio_*` host contract for serial consumers | Supply Ethernet, 802.15.4/BLE frames, serial bytes and link metadata. ZEP/PPPoL2TP UDP travels through lwIP. Test partial serial reads/writes, escapes, FCS, MTUs, link-down and queued-buffer ownership. No physical device requirement. |
| `sys_sem_*`, `sys_mutex_*` | Upstream synchronization use | Shared pthread primitives or BCL semaphore/monitor adapters | Match validity/invalidation/free rules; timeout zero means indefinite wait. Return `SYS_ARCH_TIMEOUT` only for real timeout and correct elapsed-time semantics on success. No lost wakeups, cross-owner handles or premature disposal. |
| `sys_mbox_*` | TCP/IP and API message dispatch | Bounded per-owner pointer queues and synchronous waits | NULL is a valid message. Blocking post waits for capacity; try-post returns `ERR_MEM` when full; fetch/try-fetch distinguish timeout/empty from a NULL payload. Define nonblocking ISR-style entry without claiming actual ISR support. Preserve FIFO and transfer ownership only when enqueue succeeds. |
| Threads/protection/thread-local state | Real `tcpip_thread`, API workers, core locking and per-thread netconn semaphores | `sys_thread_new`, shared pthread/BCL workers, `sys_arch_protect/unprotect`, thread identity and cleanup hooks | Retain originating owner before launch, bind inside worker, join/drain before disposal. Implement nesting and restoration tokens, core-lock assertions and per-thread semaphore allocation/free. Document stack/priority mapping and failures; no success stubs. |
| Time/randomness | Timers, retransmission, leases, cache expiry and bundled PPP algorithms | `sys_now`, `sys_jiffies` where used, sleep, monotonic clock and BCL entropy; deterministic test clock/seed | Preserve uint32 wraparound. Main-loop host pumps timers; threaded mode uses translated timer/message logic. No duplicate host retransmission engine. |
| Raw/altcp callbacks | Upstream callback dispatch and protocol state | AOT-safe explicit-owner thunks, rooted context and deferred leases | Callback signatures include the generated owner; native oracle uses native C ABI in a separate process. Catch expected host errors and map status; never allow ordinary managed exceptions to escape through callback frames. Defer user notifications where reentry is unsafe. |
| Netconn/socket APIs | `api_lib`, `api_msg`, `sockets`, `netbuf`, `netdb`, select/poll/ioctl/options/error semantics | Managed owning handles and blocking-call scheduling; runtime thread-local errno binding | Caller threads may block while the core progresses. Never hold a facade-wide lock across a blocking operation. Check fd-set/layout limits, partial reads/writes, nonblocking connect, timeout, shutdown/close and supported concurrent read/write/close contracts. |
| TCP/UDP ownership | Ordering, ACKs, receive credit and datagrams | Copying facade buffers by default | Use `TCP_WRITE_FLAG_COPY`, split at C length limits, call `tcp_recved` on consumption, and release receive pbufs once. An error callback may reference an already-freed PCB. Retry `ERR_MEM` close or explicitly abort on cancellation. |
| Address/resolver | DHCPv4/v6, AutoIP, DNS, SLAAC/DAD and multicast | Configuration DTOs, interface identity and completion handles | Preserve IPv6 zone IDs and link/address-ready distinctions. Validate ASCII/IDNA names, reject embedded NUL, copy results and retain canceled-query contexts until C cannot call them. |
| Application services | Actual HTTP/MQTT/SMTP/SNMP/SNTP/mDNS/TFTP/NetBIOS/lwiperf logic | Content/file callbacks, clock-setting notification, MIB/engine/user configuration, credentials and application event sinks | HTTP fsdata/custom content and TFTP storage are bounded consumer-owned resources. Provide deterministic storage/time/MIB fixtures. Do not replace translated clients with HttpClient or other host protocol implementations. |
| Isolation/lifecycle | Per-owner globals, descriptors, PCBs, pools, application state and queues | Entry binding, owner registry and callback/worker leases | `__DotCcEnter()` is synchronous/thread-affine, never across await. Main-loop entry is serialized; threaded APIs retain their real concurrency contract. Prove A/B independence and that disposing A leaves B running. |

lwIP has no symmetric whole-stack deinitializer and `tcpip_thread` contains an
infinite loop. P1 must prove termination at the actual wait/execution boundary:
stop admission, unregister application services, cancel/settle user operations,
perform cleanup on the core, wake workers with valid messages, exit/join workers,
drain leases and only then release handles/backing. Probe upstream hooks such as
`LWIP_TCPIP_THREAD_ALIVE` with a guarded macro override for cooperative exit at a
quiescent point; preserve lock release and message/timer semantics. A stop flag
alone, repeated fake timeouts or a detached live worker is not shutdown. If hooks
cannot express safe exit, document that result before a narrowly guarded patch;
never substitute a host TCP/IP loop for the translated dispatch/state machines.

Inventory DNS callbacks, TCP TIME_WAIT, fragment/ARP/ND queues, application
workers, PPP sessions and pending per-thread API waits. Internal nodes in the
selected owner-backed pools can be reclaimed with backing only after all external
references are gone. Shared-runtime allocations still need explicit release.
Exercise repeated create/use/dispose with pending and blocked work in every
execution profile; do not invent `lwip_deinit` or claim automatic safe cleanup.

### Adaptation precedence

Use this order for product code, tests, tools and native controls:

1. Existing upstream configuration, hook macros, callback tables, authored
   architecture/OS port headers and normal source selection.
2. [Guarded macro overrides](../../docs/macro-overrides.md) and
   [typed function overrides](../../docs/function-overrides.md), including
   prototype-only host bindings, `passInstance`, stable function pointers,
   exported constants and named field types as needed. Scope rules to relevant
   units; use signature/body guards and `requireMatch` where appropriate. Preserve
   override-selection reports and verify the selected effective configuration.
3. Only when those mechanisms are insufficient, a minimal staged text patch with
   unique local anchors. `config/adaptations.json` must record the failing case,
   alternatives tried, why an override/hook is insufficient, affected behavior,
   native/managed validation and removal condition. Preserve original references.

Function overrides still require parsing the source; do not assume they can hide
syntax errors. Repair a general parser/lowering/runtime defect in the shared
compiler rather than rewriting lwIP to avoid it. Native controls should use the
same port/configuration or a native adapter for the same contract; a native-only
patch is also a last resort, not an automatic allowance. Keep test instrumentation
in harnesses or guarded overrides and preserve upstream assertion bodies. No
emitted C# edits, successful no-op host functions, weakened checksums or rewritten
protocol algorithms. Record upstream bug repairs separately from compiler fixes.

### Highest risks

| Risk | Smallest useful experiment | Observable success/failure | Dependent gate |
| --- | --- | --- | --- |
| Packed headers and alignment | Translate checksum/pbuf plus real `eth_hdr`, `ip_hdr`, `ip6_hdr`, `tcp_hdr`, `udp_hdr`, `pbuf`, `netif`, `tcp_pcb` and address unions; compare native layout and byte images, including odd-offset/odd-length packet chains. | Exact sizes/offsets/storage and checksums; correct actual reads/writes. Metadata or mirror structs alone do not pass. | P1/P2 |
| Instance callback convention | Translate a netif callback, TCP receive/error callbacks and timer callback with the actual generated owner; invoke across two stacks under GC and delayed delivery. | Correct owner, stable pointer identity, rooted context, exception containment and no cross-owner allocation/state changes in JIT/AOT. | P1/P3 |
| Full state separation | Initialize two owners with overlapping addresses/ports, descriptors, PPP/application state; interleave raw calls and concurrent netconn/socket callers. | Independent PCBs, caches, pools, descriptors, thread-local state, workers and host handles; disposing A leaves B working without a global lock. | P1/P3 |
| Lifetime and shutdown | Hold queued TCP/PPP data, DNS/fragments/timers and blocked socket/mailbox calls while stopping the owner. | Workers exit/join, waiters resolve, no callback after release, no host allocations/leases, fresh next owner. | P1/P3 |
| Threaded host semantics | Run real tcpip initialization, mailbox traffic and timer waits; force queue full, semaphore timeout, nested protection and two-owner concurrent calls. | Correct statuses, no lost wakeups/deadlocks; initialization callback establishes readiness; no libc OS-socket resolution. | P1/P3 |
| Link/application closure | Preprocess every manifest group/profile and run one real packet transaction per newly enabled family. | Active functions and host imports match feature manifest; external TLS/crypto/compression/DNS-SD symbols are absent. | P0/P2/P4 |
| Test harness fidelity | Run a selected upstream test natively through real Check and the planned lightweight translated runner, including a deliberate failing assertion. | Matching case inventory, fixture execution, failure status and no silently ignored assertions. | P0/P4 |
| Timer-driven progress | Real TCP retransmission and DHCP timeout with an injected clock across uint32 wrap. | Native-equivalent transition/packet sequence and bounded pump completion; no wall-clock sleeps required. | P1/P4 |

No compiler defect has been observed during planning. Packing, callbacks and
owner teardown are hypotheses requiring real emitted/native experiments.

## Framework integration and deliverables

- Recipe: `scripts/campaign.py`, exporting `Recipe("lwip", ...)`, profiles
  `("threaded", "nosys", "threaded-locking", "unit", "unit-nosys")`; add required
  feature-specific variants from the manifest. No additional product libraries.
- Final library: `generated/TranslatedLwip/TranslatedLwip.csproj`.
- Raw library: `generated/TranslatedLwip.Raw/TranslatedLwip.csproj`.
- Nondefault libraries: `generated/profiles/<profile>/TranslatedLwip[.Raw]/`.
- Auxiliary translated tools/contrib: separate projects and examples, with
  generated artifacts under `generated/tools/` and `generated/contrib/`; each
  declares its own source closure and profile/library references. Integrate via
  recipe hooks/shared pipeline; do not build a duplicate campaign runner.
- Root solution: `ManagedConsumer.slnx`, with the processed default product,
  authored facade and sample.
- Sample: `samples/ManagedConsumer/ManagedConsumer.csproj`; ordinary project
  references select the product via `LwipProject`. Propagate that property through
  the facade so raw/processed tests exercise the selected library.
- Authored files: `src/Host/` linked into generated projects from their original
  paths; `src/Managed.Lwip/Managed.Lwip.csproj` for owning APIs;
  `src/port/` for architecture, sys_arch and link shims. Host sources must not create a
  circular project reference to the generated owner.
- Tests: `tests/NativeOracle/`, `tests/Abi/`, `tests/Upstream/`,
  `tests/PacketScenarios/`, `tests/Lifecycle/`, `tests/IndependentPeer/`,
  `tests/SysArch/`, `tests/SocketApi/`, `tests/Links/`, `tests/Applications/`,
  `tests/Tools/`, `tests/RootedAot/`; keep workloads outside the library.
- Hooks: staged port/profile preparation, effective macro/source inventory,
  typed bindings, oracle/suite prerequisite preparation and dependency audit.
  Use shared fetching, tools, raw/processed build and transactional publication.

Prerequisites are Python 3.11+, Bash and the repository's .NET 10/C# 14 toolchain.
Linux native/AOT qualification additionally needs a C compiler/linker and SDK AOT
prerequisites. Native upstream comparison needs Check development headers/library;
keep it test-only and record its installed version. The independent peer requires
a runner-provisioned isolated TAP link and kernel socket helper, with offloads
controlled/recorded. Tests may consume an already available link; campaign scripts
must not run `sudo` or assume they can alter host routes. An unavailable peer
prerequisite leaves that required gate blocked rather than passed. Ordinary
translation, builds and the in-memory consumer have none of those network needs.
Peer executables, Check and capture tools are test-only prerequisites, not product
dependencies; declare versions/acquisition under the test input group. The TAP
bridge is only an interoperability fixture, not a delivered TAP/TUN adapter.
All link/application suites must also have deterministic in-memory controls.

After supplying metadata, recipe and authored consumer, run shared `layout --write`
to create wrappers and canonical scaffolding. Use shared Bash/Python resolution
and paths relative to scripts; no second runner. Windows orchestration must work
under an ordinary account without Developer Mode, using copies rather than host
symlinks. Check command dispatch and staging on Windows separately from the
Linux-only native/TAP tests. Ignore `ref/`, `generated/`, `build/` and `artifacts/`
in the new campaign's local ignore file when implementing it.

## Milestones and evidence

Implementation is active in the isolated worktree. Current receipts and remaining
gates are tracked in [COORDINATION.md](COORDINATION.md). Milestone commits follow
the baseline after their
required checks pass, subject to current session instructions. Implementation is
authorized; no push is requested. Only stage this campaign's changes.

| Phase | Concrete exit gate | Status | Evidence and remaining work |
| --- | --- | --- | --- |
| P0 — Scope/native baseline | Inventory all core/API/link/app/contrib/tool sources and implemented features; freeze profiles, exclusions and suite mapping; create recipe/layout; capture repository/tool baseline; run native raw and threaded TCP/UDP controls and a failing harness control; attempt full-source translation. | In progress | Full source inventory and three product profile builds established; native raw/threaded controls and original Check 137 cases pass. Effective option/auxiliary/diagnostic closure remains open. See `docs/source.md` and coordination receipts. |
| P1 — Boundary feasibility | Native/emitted ABI, packing, callbacks, pbufs, sys_arch, owner isolation, socket ABI and real threaded shutdown pass in raw JIT/AOT. Resolve D3/D5; record host bindings and prove override/hook strategy. | Partial | Actual emitted/native 19 layout observations and unaligned/split checksums, actual-owner sys_arch checks and useful worker lifecycle pass required cells in recorded profiles. Pending/blocked operation teardown and complete boundary cases remain open. |
| P2 — Complete translation | Every required profile and feature source group builds raw then processed; tools/contrib build separately. Audit imports, external dependencies and active code coverage; publish canonical products transactionally. | Partial | Threaded, nosys, threaded-locking, normal unit and makefsdata raw/processed builds published. Unit-nosys raw/processed products now also published; contrib and full active-feature/import audit remain open. |
| P3a — First useful execution | Dual-stack 256 KiB TCP and UDP exchange through real raw/altcp, netconn and socket paths; correct buffer ownership and two-owner lifetime. | Partial | Threaded sockets, nosys raw and locking two-owner Ethernet consumers pass all four cells; direct netconn/altcp matrices pass every applicable product/form/runtime cell; complete lifetime/error qualification remains. |
| P3b — Core/link completion | Addressing including DHCPv6, UDP-Lite, forwarding/bridging, SLIP/PPP/6LoWPAN/ZEP, bundled PPP modes, timers and clean concurrent shutdown. | Planned | `addressing`, `routing`, `links`, `ppp`, `lifecycle`. |
| P3c — Applications/tools/contrib | Real HTTP/MQTT/SMTP/SNTP/mDNS/NetBIOS/TFTP/SNMP/lwiperf workflows, dependency-free contrib cases, and translated makefsdata feeding HTTP content. No required family merely compiles. | In progress | App host callbacks implemented; bounded native makefsdata controls and necessary staged upstream repair pass; managed tools/contrib and application behavior remain pending. |
| P4 — Behavioral qualification | Applicable upstream cases including sockets, PPP, mDNS and MQTT; errors/recovery, concurrent API use and independent peer checks all pass. | Partial | Original upstream Check suite passes 137/137 natively and in all four normal unit cells with harness controls. Unit-nosys132 also passes all four cells; feature/error/recovery/independent-peer matrix remains open. |
| P5 — Consumer delivery | Default threaded root solution and separately selected profile consumers build via ordinary references; owning API and link/application examples run on JIT and trimmed NativeAOT; rooted AOT passes separately. | Partial | Ordinary managed consumer references and threaded/nosys JIT/AOT work; threaded rooted AOT passes both forms. Full owning API, examples and all profiles remain incomplete. |
| P6 — Reproduction/acceptance | Final-tool regeneration of all profiles, complete verification, dependency/patch audit and affected shared regressions; local-input modes and portable orchestration reproduced. | Planned | `docs/validation.md`, framework receipts and specialist attachments; no reduced subset closes acceptance. |

Dependencies: P0 → P1 → P2 → P3a, then P3b/P3c → P4 → P5 → P6.
Application/link design can start early; runtime gates depend on their real core
and host prerequisites. P2 includes diagnostic profiles and auxiliary products.
Milestones may publish partial progress without changing the complete scope.

Create `docs/blockers.md` at the first implementation attempt, recording ID,
compiler/runtime/host/harness/upstream/scope classification, reproducer,
native expected versus observed behavior, regression, repair and full-source
retry. Use shared structural repairs and appropriate `DotCC.Tests` and
`DotCC.FunctionalTests/Fixtures` coverage; rebuild parser tables after grammar
changes. Re-test the full selected lwIP inputs after each substantive repair.
Keep unrelated repository baseline failures separate. Choose downstream campaigns
according to shared behavior changed, and coordinate builds sharing output paths.

## Validation selection

`Product matrix` means each applicable required product profile × raw/processed ×
JIT/NativeAOT × Linux x64. A feature must run in all four form/runtime cells of
its designated profile; shared core/host/lifecycle behavior also runs in every
product profile. `Unit matrix` means the declared applicable diagnostic profiles
with the same four cells. Record profile-specific applicability explicitly;
threaded sockets are not a failed/missing NO_SYS feature. Other feature-specific
profiles discovered at P0 join the required selection, not an optional backlog.

| Behavior/gate | Recipe suite and assertions | Required matrix | Prerequisite |
| --- | --- | --- | --- |
| Native validity | `oracle`: matching source/macros, raw/threaded work, intentional assertion failure, fixtures and case counts | Native Linux x64 for each tested profile | C compiler; Check for upstream cases |
| ABI/callbacks | `abi`: actual sizes/offsets/byte images, packet/socket/fd structures, unaligned checksums, explicit-owner callbacks and identity under GC | Product matrix; profile-sensitive unit layouts | Native observations |
| sys_arch | `sysarch`: NULL mailbox payload, capacity/FIFO/try-post errors, real wait/timeout, semaphore races, nested protection, core locks, thread-local state, init readiness and worker drain | Threaded profiles, four cells | Actual translated tcpip/API code and host counters |
| Useful workflow | `consumer`: two owners, 256 KiB TCP echo plus UDP, IPv4/IPv6, closure and no outstanding host resources | All product profiles, four cells | In-memory links; correct raw versus threaded API selection |
| Core transport | `packets`: raw/altcp, TCP state/half-close/reset, UDP/UDP-Lite checksum coverage, loopback, pbuf chains and v4/v6 fragments/expiry | Product matrix | Deterministic fixtures/native control |
| Sequential/socket APIs | `socket-api`: netconn/netbuf/netdb/netif APIs; bind/listen/accept/connect, partial send/recv, options, select/poll, ioctl/nonblocking, errno, timeout, shutdown and supported concurrent read/write/close | Threaded profiles, four cells | Concurrent callers and real core worker; forbid accidental host OS sockets |
| Addressing | `addressing`: DHCPv4 renewal/NAK/conflict/expiry, AutoIP, DHCPv6 information exchange, DNS A/AAAA/cache/errors, SLAAC/DAD, IGMP/MLD and interface-scoped results | Product matrix | Native and deterministic packet peers |
| Routing/bridge | `routing`: IPv4/IPv6 multi-interface forwarding, TTL/hop-limit/ICMP behavior, routes, bridge learning/aging/flooding and per-port ownership | Product matrix | At least three virtual interfaces; static-routing addon cases |
| Other links | `links`: SLIP partial/escaped serial frames; 802.15.4 and BLE 6LoWPAN compression, fragmentation/reassembly/errors; ZEP encode/decode and transport; MTU/link state | Applicable product profiles, four cells | Virtual byte/radio links, published fixtures and native comparisons |
| PPP including bundled crypto | `ppp`: PPPoS/PPPoE/PPPoL2TP discovery/negotiation/teardown, IPCP/IPv6CP, PAP, bundled CHAP/MSCHAP/EAP/MPPE cases, VJ and supported compression; wrong credentials/truncated frames and cryptographic known-answer fixtures | Applicable product profiles, four cells | Same-source controls plus independent vectors/peers; no external crypto linked into product |
| HTTP | `applications`: HTTP client/server, proxy CONNECT, static/custom content, SSI/CGI/POST, fragmented input, missing paths and close; generated fsdata round trip | Designated raw/threaded profiles, four cells | Native control and independent HTTP client/server |
| Other applications | `applications`: MQTT connect/pub/sub/keepalive and supported QoS; SMTP transaction/error; SNTP response/clock callback; mDNS query/probe/conflict; NetBIOS name reply; TFTP read/write/retry; SNMPv1/v2c get/set/walk/trap plus v3 noAuthNoPriv and rejection of crypto requests; lwiperf data transfer/result | Designated profiles/variants, four cells | Deterministic peers/storage/MIB/time fixtures plus declared independent application peers |
| Contrib/tool delivery | `contrib`: every included addon/app/example gets its declared behavior case, including TCP ISN/signature known-answer and packet verification cases; `tools`: translated uncompressed makefsdata produces content served correctly by translated HTTP | Declared auxiliary profiles, raw/processed JIT/AOT | Native tool/content comparison and ordinary project references |
| Upstream assertions | `upstream`: all applicable bundled core, DHCP, ARP, IP, TCP/UDP, sockets, PPPoS, mDNS and MQTT cases, with fixtures/setup/teardown intact | Unit matrix and matching native cases | Validated Check-compatible test runner |
| Errors/lifecycle | `lifecycle`: exhausted pools/mailboxes, duplicate binds, callback errors, pending DNS/TCP/PPP/apps, blocking calls during cancellation/close, worker join and A/B owner isolation | Product matrix | Bounded cases and resource/lease counters |
| Recovery | `recovery`: bad headers/checksums/lengths/fragments, lost/duplicate/reordered packets, link down/up, backpressure and clock wrap | Applicable product matrix | Bounded scripted disturbances |
| Independent protocol checks | `interop`: kernel TCP active/passive and UDP both families; selected independent application/link peer exchanges supplement same-source controls | Applicable product matrix | Pre-provisioned isolated test link; declared peer inputs and offload settings |
| AOT/dependency/adaptation audit | `aot-rooted`: root and execute library boundaries; `audit`: all manifests/profiles/imports, override reports, patch necessity, authored references and forbidden external dependencies | All product forms/profiles; AOT for rooting; auxiliary products audited too | AOT toolchain and current artifacts |
| Shared regressions | Compiler/functional/postprocessor and affected existing campaigns | Impact-selected forms/targets | Required whenever shared code changes |
| Extended campaigns | Random fuzzing, exhaustive fault injection and long stress | None | Excluded; finite upstream or scripted cases above remain required |

Default suites: `consumer`.
Verification suites: `oracle`, `abi`, `sysarch`, `consumer`, `packets`,
`socket-api`, `addressing`, `routing`, `links`, `ppp`, `applications`, `contrib`,
`tools`, `upstream`, `lifecycle`, `recovery`, `interop`, `aot-rooted`, `audit`.

Declare applicability, source groups and `required_profiles`/dependencies in the
recipe. The full verification selection prepares every required product,
diagnostic and auxiliary profile, then dispatches cases to matching artifacts.
Honor `--form`, `--mode` and `--rid`, or reject unsupported selections explicitly.
Add generic framework support only if the concrete multi-profile requirement
cannot be expressed by existing hooks. Do not recursively build another runner.
A smoke run or a passing default profile alone cannot satisfy full acceptance.

Build `config/upstream-tests.json` from effective preprocessing and actual
registrations, not guessed counts. Record case, source, profile, prerequisites,
fixture, expected assertions and any exclusion reason. Start from the
[test source list](https://github.com/lwip-tcpip/lwip/blob/77dcd25a72509eb83f72b033d219b1d40cd8eb95/test/unit/Filelists.cmake)
and [suite registrations](https://github.com/lwip-tcpip/lwip/blob/77dcd25a72509eb83f72b033d219b1d40cd8eb95/test/unit/lwip_unittests.c).
Sockets, PPP, mDNS and MQTT are required now; the old feature-based exclusions no
longer apply. Keep real threaded production-host tests separate from upstream's
simplified test scheduler. A lightweight Check-compatible translated runner may
provide test-only registration/assertion services through headers and overrides;
compare it to native Check, including deliberate failure and fixture execution.
Unsupported assertion machinery must fail preparation, not silently pass. New
text changes require the same last-resort justification as product adaptations.

For deterministic tests, compare packet bytes, checksums, statuses, payloads,
event order and resource deltas exactly using the same clock/seed. Do not compare
native and managed pointer values; compare aliasing/identity relationships and
layout. For concurrent tests, compare legal partial order, statuses, payloads and
resource ownership rather than requiring identical thread scheduling. Independent
peers have different ISNs, timestamps, ports and packet
segmentation: validate protocol fields, checksum correctness, legal state/order,
payload stream and bounded progress with explicit masks only for those variable
fields. Preserve original PCAP/transcripts alongside normalized results.
Native crashes, missing cases and required skips are failures to classify.

Performance is informational only: optionally report equal-workload native versus
managed throughput and allocations with fixed MTU/payload/pool settings after
correctness passes. No timing threshold or claim is required for acceptance.

## Reproduction commands and acceptance

These commands use the shared framework. Initial product translations and selected
consumer/boundary suites now run; full verification still reports unimplemented or
unqualified gates as failures. Run from the isolated repository root:

```bash
bash Scripts/campaign.sh layout lwip --write
bash Scripts/campaign.sh layout lwip --check
bash lwip/scripts/fetch.sh --fetch missing
bash lwip/scripts/translate.sh --fetch never --profile threaded
bash lwip/scripts/translate.sh --fetch never --profile nosys
bash lwip/scripts/translate.sh --fetch never --profile threaded-locking
dotnet build lwip/ManagedConsumer.slnx -c Release
bash lwip/scripts/test.sh --suite consumer --form all --mode all --rid linux-x64
bash lwip/scripts/verify.sh --fetch never --form all --mode all --rid linux-x64
```

The full `verify` command must prepare and qualify all declared required profiles
as described above, including diagnostic and auxiliary outputs. Preserve its
profile-to-suite selection in the receipt. Run `verify` with `--with repository`
when shared changes require the repository test projects; add specific downstream suites justified by those changes. Record
specialist native/Check/TAP prerequisites and diagnostics before suite execution.
`--fetch never` controls upstream acquisition only; document NuGet separately
through `--restore normal|locked|none`. Pre-restored inputs are required for a
fully disconnected reproduction. No specialist suite may silently download data.

Demonstrate `--hashes warn` and `off` with usable local inputs and unrelated source
edits; content-dependent caches must rebuild/bypass unchecked inputs. Optional
strict reproduction is distinct from acceptance. Repeat with clean campaign-owned
outputs while preserving other files; invoke absolute script paths from an
unrelated directory. Verify translation failure preserves the last published
product, regeneration preserves authored files, and an authored host/facade edit
is picked up by `build.sh` without regeneration.

Final evidence belongs in `docs/validation.md`, linked to
`artifacts/campaign/<run-id>/receipt.json`, the current-product/latest-attempt
receipts and specialist case/PCAP/ABI/dependency reports. Record commands, tool
identity, input provenance, effective profile, case counts, exclusions and all
unrun/failed/blocked work. A published build is not automatically qualified.

Current remaining required work: the open P0–P6 gates above, including diagnostic
and auxiliary translation, complete feature/API/application/upstream matrices,
pending-operation lifecycle, independent peer, portable orchestration and final
reproduction. Passing initial consumer and boundary cells do not close these
gates. Current commands and receipts are recorded in `docs/COORDINATION.md`.
Optional work remaining is limited to the later
extensions and extended campaigns listed above. Completion requires all selected
features, the useful consumer, ownership contracts, required matrix and
reproduction gates with current evidence. A smaller working subset must be
reported as partial without closing this plan.

## Planning references

Source inspection used the exact release commit above; current `master` was not
selected as a product input. Additional primary references informing the plan:

- [lwIP architecture, raw API and main-loop threading rules](https://github.com/lwip-tcpip/lwip/blob/77dcd25a72509eb83f72b033d219b1d40cd8eb95/doc/doxygen/main_page.h).
- [Feature configuration definitions](https://github.com/lwip-tcpip/lwip/blob/77dcd25a72509eb83f72b033d219b1d40cd8eb95/src/include/lwip/opt.h),
  [architecture and packing contract](https://github.com/lwip-tcpip/lwip/blob/77dcd25a72509eb83f72b033d219b1d40cd8eb95/src/include/lwip/arch.h).
- [Timeout implementation](https://github.com/lwip-tcpip/lwip/blob/77dcd25a72509eb83f72b033d219b1d40cd8eb95/src/core/timeouts.c),
  [upstream timer tests](https://github.com/lwip-tcpip/lwip/blob/77dcd25a72509eb83f72b033d219b1d40cd8eb95/test/unit/core/test_timers.c).
- [System abstraction contract](https://github.com/lwip-tcpip/lwip/blob/77dcd25a72509eb83f72b033d219b1d40cd8eb95/src/include/lwip/sys.h),
  [TCP/IP worker](https://github.com/lwip-tcpip/lwip/blob/77dcd25a72509eb83f72b033d219b1d40cd8eb95/src/api/tcpip.c).
- [PPP options and bundled primitives](https://github.com/lwip-tcpip/lwip/blob/77dcd25a72509eb83f72b033d219b1d40cd8eb95/src/include/netif/ppp/ppp_opts.h),
  [contrib source groups](https://github.com/lwip-tcpip/lwip/blob/77dcd25a72509eb83f72b033d219b1d40cd8eb95/contrib/Filelists.cmake),
  [makefsdata dependencies](https://github.com/lwip-tcpip/lwip/blob/77dcd25a72509eb83f72b033d219b1d40cd8eb95/src/apps/http/makefsdata/makefsdata.c).
- [Upstream redistribution notice](https://github.com/lwip-tcpip/lwip/blob/77dcd25a72509eb83f72b033d219b1d40cd8eb95/COPYING).

Paths describe the complete delivery contract. Consult current receipts and the
coordination ledger for actual existing artifacts and qualified scope.
