# lwIP validation evidence

Full acceptance is incomplete. Active implementation is in `/home/marius/p/dotcc`
on branch `sqlite` after migration merge `38c1cfa8`. Earlier receipts were copied
from `/home/marius/p/dotcc-lwip-full`, which remains a read-only historical backup. The [plan](PLAN.md) defines the complete
scope; [coordination](COORDINATION.md) records ownership and next work. All matrix
results below are Linux x64. “Four cells” means raw/processed × JIT/NativeAOT.

The user subsequently restricted verification: **do not run corruption tests
unless those exact scenarios exist as upstream tests**. The receipts currently
indexed below predate that restriction and may include custom cases that are now
excluded by the user. Preserve them as historical evidence; do not rerun or require
those excluded cases. The source audit is complete in
[`verification-policy.json`](../config/verification-policy.json): 13 custom case
groups removed/replaced, 23 entrypoint groups including native prerequisites,
137 authored source hashes, and 11 exact upstream case/file/registration records
verified. No authored corruption exception is retained. Revised fixtures await
new behavioral receipts. Ordinary valid API errors, authentication rejection,
lifecycle and non-corrupt timeout/loss/reordering controls remain eligible.

## Migration and source snapshots

Root migration checks passed 43 framework, 105 focused compiler, 10 functional
(with four optional native-oracle skips), six pipeline checks and layout. New-root
threaded raw/processed JIT consumer passes `20260929-100709-5e0adecd`. Logs and
verified WIP-copy manifest: `artifacts/migration-20260929`. Merged-toolchain raw/processed refresh of all eight profiles with applicable
EAP repair has passed; fresh behavioral qualification is in progress. Earlier full matrices below
qualify their recorded snapshots, not all subsequent source/compiler changes.

## Qualified behavior by recorded snapshot

Fresh merged-toolchain raw/processed publications in the original checkout pass:
threaded `20260929-100938-1e61b771`, nosys `20260929-101126-d7f9a0c3`, locking
`20260929-101258-8d10f70a`, contrib `20260929-101436-e4f2dfc8`, makefsdata
`20260929-101623-bba174b7`, HTTP-content `20260929-101639-f50e3581`, unit
`20260929-101818-531970c1`, unit-nosys `20260929-101941-06379f74`. Applicable
products include the staged bundled EAP failure-path release repair in addition
to IPv4 forwarding and the tool checksum-loop repair. New qualification is in
progress. Receipts live under `artifacts/campaign/`; older rows retain snapshot scope.

| Evidence | Passed scope | Receipt/report |
| --- | --- | --- |
| Threaded repaired matrix | Host 32 contracts, natural resource lifecycle, IPv4 routing, independent kernel TCP/UDP; all four cells | `20260929-090503-a94d59d9/receipt.json` |
| NO_SYS repaired matrix | Independent kernel TCP/UDP through raw callbacks, natural resource lifecycle, IPv4 routing, DNS, ABI and consumer; all four cells | `20260929-091423-b5f02ec9/receipt.json` |
| Locking repaired matrix | ARP queueing/datagrams, host 32 contracts, natural resource lifecycle, socket semantics, DNS, IPv4 routing, independent kernel TCP/UDP, ABI and consumer; all four cells | `20260929-091556-6c1b6415/receipt.json` |
| Threaded independent cases | Updated netif ABI, consumer, finite owner lifecycle, socket semantics and DNS; all four cells, before product-only forwarding repair | `20260929-085113-333ba145/receipt.json` |
| DHCP/AutoIP/pool and service recovery | DHCP acquisition/ACD/renewal/NAK/expiry/conflict, AutoIP, actual pool exhaustion/recovery and other-owner independence, registered blocked UDP cleanup, disposal retries, ARP policy, lifecycle and consumer; three profiles × four cells | `20260929-094413-fe06cf88`, `20260929-094658-5dd39ead`, `20260929-095020-fab83601` (before new HTTP quiescence/recovery refinement) |
| HTTP cleanup/consumer refinement | Active HTTP context refusal before core stop, explicit cleanup-only fs_close and retry; consumer; three profiles × four cells | `20260929-095609-5894d6ee`, `20260929-095703-fadcd930`, `20260929-095758-3e4181b3` |
| Final finite service/pool guards | Actual timer/core-context reentry rejection, blocked UDP shutdown, pool/owner independence, cleanup-only recovery and retries; three profiles × four cells | `20260929-100126-d50e6381`, `20260929-100152-2c79deb8`, `20260929-100221-287fcea6` |
| IPv6 addressing and multicast | DAD/RA/SLAAC lifetime/duplicate controls, stateless DHCPv6 retry and DNS replacement, IGMP/MLD group admission/report/leave; three profiles × four cells | `20260929-103455-c49324c6`, `20260929-103633-09b9abb5`, `20260929-103831-2b08f372`; checkpoint `e229fc42` |
| L2TP, finite LOWPAN, IPv6 routing and bridge | L2TP tunnel/authentication/control/data; ten independently encoded radio/BLE inputs repeated across two owners and after A disposal; guarded IPv6 hop-limit boundaries; bridge forwarding/teardown and managed owner independence; three profiles × four cells | `20260929-110105-df4730c6`, `20260929-110524-6852b698`, `20260929-111004-d11b3587` |
| Native forwarding controls | Original failure and adapted pass for all three product profiles; IP/UDP/TCP/ICMP checksums, fragments, TTL and same-interface cases | `artifacts/native-routing/<profile>/report.json` |
| Managed upstream Check | Fresh merged-toolchain original 137 cases and negative/fixture controls, exact native registration; all four normal unit cells | `20260929-102529-f99f8fc4/receipt.json` |
| Managed NO_SYS upstream Check | Fresh merged-toolchain 132 applicable cases and native registration/negative-fixture parity; all four cells; five threaded socket cases inapplicable | `20260929-102607-122fea01/receipt.json` |
| Native upstream Check | 137 original cases with zero failures/errors; test-only Check 0.15.2 | `artifacts/upstream-native/lwip_unittests.xml` |
| Native makefsdata | Original loop failure bounded by 8 MiB; staged progress repair passes empty/odd/multiple-MSS checksum/data/framing controls | `artifacts/native-tools/report.json` |
| Managed makefsdata generator | Native-exact 27,529-byte image, checksum/header-offset/temp cleanup; all four cells | `20260929-083157-23870a49/receipt.json` |
| Generated-content HTTP | Actual translated processed makefsdata image consumed by original fs.c; empty/odd/4097-byte content served correctly; all four HTTP-content cells | Publication `20260929-093414-95dafdc6`, behavior `20260929-093554-eba3af0c/http-transactions-report.json` |
| Actual PPP serial and independent PPPoE | Auth success/wrong credentials, UDP/TCP, VJ/MPPE wire controls, bad FCS, LQR negotiation in locking; independent PPPoE discovery/session/data/reject/close; all three profiles × four cells | `20260929-102218-436a6eb5`, `20260929-102323-8af6d3ee`, `20260929-102423-3e0a37ed` (`ppp-serial-report.json`, `ppp-ethernet-report.json`) |
| Bundled PPP controls | Primitive/auth/MPPE vectors, including 600 stateful/stateless packets; all three profiles × four cells; actual serial/PPPoE negotiations separately qualified above | `20260929-092814-c3aef247`, `20260929-092953-8a3a53fc`, `20260929-093126-d3ef1043` (`bundled-ppp-report.json`) |
| Contrib addon controls | ABI/owner callbacks/static routes/ISN; all four cells; contrib application workflows pending | `20260929-092742-40fc80aa/contrib-addons-report.json` |
| SLIP framing | Original decoder with real UDP IPv4/IPv6 ingress, framing/escapes, joined worker and partial-pbuf cleanup; all three profiles × four cells | `20260929-092814-c3aef247`, `20260929-092953-8a3a53fc`, `20260929-093126-d3ef1043` (`slip-framing-report.json`) |
| HTTP baseline | Five actual transactions: custom GET, IPv6 SSI, CGI parameters, 404 and POST/split headers; all three profiles × four cells | Same three receipts (`http-transactions-report.json`) |
| Shared compiler/runtime repairs | 14 focused compiler and 6 affected functional checks; full lwIP retries and isolated QuickJS translation/JIT consumer regressions | `docs/blockers.md` L005/L007; QuickJS `080236-2f0ecda6`, `080433-b0e54d3a` |

Independent kernel interoperability is implemented and passes all three product
profiles in all four cells: active/passive TCP 256 KiB echo and bidirectional UDP
123 bytes over IPv4 and IPv6. A test-only TAP runs in an unprivileged user/network
namespace, with per-cell packet captures. No product TAP/TUN adapter is delivered.
Selected independent application/link peer workflows remain required.

Natural lifecycle cases observe generated pool/heap counters returning to the
initialized baseline **before** owner disposal or host registry clearing. Remaining
cyclic timers and the TCP/IP worker are identified initialized infrastructure.
These finite cases do not prove arbitrary pending-protocol teardown. The low-level
borrowed core API requires callers to settle their resources. Owned blocking UDP service stop and failed-disposal retries now have the finite
recovery matrices above. Pending HTTP-context refusal, explicit cleanup-only recovery and actual timer
reentry guards now pass their separate finite matrices above. Auxiliary protocol matrices
used explicit fixture teardown and do not qualify all protocol-specific owned
services (SLIP/PPP/app registrations still need their own pending-stop cases).
Shared facade/host sources are now frozen during dependent worker builds.

The latest link/routing/bridge rows use refreshed product publications
`20260929-105719-74e8e2eb` (threaded), `20260929-110311-4b343b75` (NO_SYS), and
`20260929-110757-36b627d9` (locking). They add the scoped IPv6 hop-limit macro
override, authored bridge lifecycle include-wrapper, and fourth guarded source
repair for LOWPAN short-source address recognition. They do not qualify broader
LOWPAN fragmentation/ZEP or all pending owning-service lifetimes. Auxiliary
publications above retain their earlier source snapshots pending the next refresh.

## Historical snapshot evidence

The following receipts establish earlier behavior, not acceptance of subsequent
configuration or host edits. Required affected cases must be refreshed before
final acceptance.

| Earlier evidence | Scope | Receipt |
| --- | --- | --- |
| Initial stack translation | 119 upstream stack C units plus two host units in each product profile, raw/processed | Historical publication receipts; current product IDs above |
| Initial threaded/NO_SYS consumers | Two owners, both IP families, TCP/UDP, B continues after A disposal; four cells | `20260929-075129-14a2b48d`, `20260929-075656-c2e46bad` |
| Early ABI/sys_arch | 19 layouts/checksums and 24 original host contracts; threaded four cells | `20260929-075904-16948989`, `20260929-075553-7edc1797` |
| Whole-library rooted NativeAOT | Threaded raw and processed, separately rooted from trimmed consumer | `20260929-075750-7fe1bb64` |
| Initial locking boundaries | Consumer, ABI/wire images, sys_arch; four cells | `20260929-081643-1a17a7f2` |
| Direct APIs/datagrams | Plain altcp/applicable netconn, fragmentation and UDP-Lite; three profiles × four cells | `20260929-082120-9e746efd`, `20260929-082457-04b31050`, `20260929-082706-a7b10f5a` |

## Remaining acceptance

Full socket APIs, memory/timer/core recovery, addressing, IPv6 routing/bridging,
SLIP/PPP/6LoWPAN/ZEP, remaining HTTP variants and application/contrib workflows,
pending/blocked service lifecycle, selected independent application/link peers,
final import/active-feature/adaptation audits, all-profile rooted AOT and owning
examples, clean/local-input reproduction and Windows orchestration remain open.
Custom corruption cases excluded by the user's later instruction are no longer
acceptance requirements; their exclusion is neither a pass nor a blocker.
Finite suites do not close their broader aggregate gates. No missing gate is
counted as a pass; a historical receipt does not qualify changed code.
