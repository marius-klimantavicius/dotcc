# lwIP validation evidence

Full acceptance is incomplete. Active implementation is in `/home/marius/p/dotcc`
on branch `sqlite` after migration merge `38c1cfa8`. Earlier receipts were copied
from `/home/marius/p/dotcc-lwip-full`, which remains a read-only historical backup. The [plan](PLAN.md) defines the complete
scope; [coordination](COORDINATION.md) records ownership and next work. All matrix
results below are Linux x64. “Four cells” means raw/processed × JIT/NativeAOT.

Fresh coordination and implementation workers are active after the preserved
worker handoff. Owning LOWPAN radio/BLE/ZEP finite output/lifetime qualification
now passes all three product profiles and four cells, with current common owning
API/lifecycle/consumer regressions. Required application, option/contrib and final
acceptance work remains; the complete plan is not finished. The earlier generic
service errors do not authorize retrying any identified prohibited action.

The user subsequently restricted verification: **do not run corruption tests
unless those exact scenarios exist as upstream tests**. Except for the explicitly
identified permitted receipts below, indexed receipts predate that restriction and may include custom cases that are now
excluded by the user. Preserve them as historical evidence; do not rerun or require
those excluded cases. The source audit is complete in
[`verification-policy.json`](../config/verification-policy.json): 13 custom case
groups removed/replaced, 23 entrypoint groups including native prerequisites,
137 authored source hashes, and 11 exact upstream case/file/registration records
verified. The owning checkpoint expands this to 24 entrypoint groups, 145 authored
source/build hashes and 11 implementation hashes, independently verified again.
No authored corruption exception is retained. Ordinary valid API errors, authentication rejection,
lifecycle and non-corrupt timeout/loss/reordering controls remain eligible.

## Permitted verification after the user restriction

Receipt `20260929-113734-bf3d5d86` passes 30 commands covering ten revised suites
on **threaded raw JIT plus matching native prerequisites**: bundled PPP, PPP serial,
PPPoE, L2TP, SLIP, finite LOWPAN, datagrams, IPv6 addressing, IPv6 routing and
bridge. Removed custom corruption cases were not executed. This receipt qualifies
the source snapshot before the current owning API implementation. Revised
processed/AOT and alternate-profile matrices remain pending; it is not a full
matrix pass. Source-policy checkpoint is `82eb1a4a`.

The owning API, consumer, owner-lifecycle and pool-recovery suites subsequently
passed **three profiles × four cells**: threaded `20260929-115825-fc8e63a0`,
NO_SYS `20260929-120036-26c6b530`, locking `20260929-120312-d52e329b`.
Product publications are respectively `20260929-114549-0edfd8d9`,
`20260929-115149-f29bbb4b`, `20260929-115452-acdef463`. Socket/netconn cases
apply only to threaded profiles; NO_SYS qualifies the owning resolver and common
lifetime paths. Actual product metadata controls the facade's compiled API surface,
and a mismatching explicit profile hint is rejected.

These cases exercise copied socket/netconn buffers and endpoints, supported
concurrent read/write/close, blocked-call cancellation, accepted children racing
service shutdown, descriptor references before original syscall entry, and
natural pool recovery. DNS cases cover IDNA, A/AAAA/cache/NXDOMAIN, copied results,
late callbacks after cancellation and GC, pending-query disposal retries and
owner independence. Ordinary interface backing survives queued callbacks through
stop, a real FIFO barrier and free. No new upstream text repair was needed.

The added interface-construction rollback fixture also passes the owning-only
four-cell matrix on all three profiles: `20260929-120630-457be2fa`,
`20260929-120955-dfcf28c1`, `20260929-121107-56ed1e8c`. It uses a real duplicate
output registration from NETIF_ADDED, verifies restoration of the original
interface/default route and heap/pbuf baseline, then unregisters the callback.
Product implementation was unchanged for these additional fixture runs.

Checkpoint **`aa2e23f1`** qualifies the subsequent final owning implementation and
ordinary project-reference integration. The import uses static graph restore and
normalizes only the selected product's duplicate reference; an unrelated two-hop
project dependency remains available. The local relative NO_SYS consumer builds
without command-line selection hints. Final receipts, all four cells:

| Profile | Owning API | Consumer with final project graph |
| --- | --- | --- |
| Threaded | `20260929-122521-090f75f3` | `20260929-122007-dac94da8` |
| NO_SYS | `20260929-121848-8f47f868` | `20260929-121848-8f47f868` |
| Locking | `20260929-122622-a0980575` | `20260929-122125-d03caecf` |

The final threaded owning runs additionally qualify async scheduling admission:
accepted Tasks retain the owner and count toward disposal before a worker enters
the original API. Completion settles before that accounting is released. A
deterministic scheduling pause proves owner disposal waits, followed by valid
traffic on owner B. These APIs are absent from NO_SYS, whose compiled applicable
implementation is unchanged. Earlier owner-lifecycle/pool-recovery receipts retain
their scoped unchanged behavior. Nine pipeline checks, layout and syntax checks
also pass. Input snapshot: `artifacts/owning-api-snapshot-20260929-1227.json`.

Older contrib and HTTP-content publications must be refreshed before referencing
this facade because they lack the now-required capability metadata.

Initial failure `20260929-114817-d24c28ac` is preserved: authored IPv6 fixed-buffer
address expressions in the new facade were corrected before passing runs.
Failure `20260929-121229-14960450` exposed the SDK restore-selection issue; failure
`20260929-122333-540a599b` came from a fixture polling an intentionally disposed
owner and was corrected to use independent loopback owners.
These finite owning suites do not close full socket options, typed interface
identity, pending protocol-specific services or the complete lifecycle gate.

Owning radio/ZEP/BLE implementation is now under qualification. Refreshed
raw/processed publications pass threaded `20260929-123615-d0fe9319`, NO_SYS
`20260929-123913-71c8944e` and locking `20260929-124411-8570ce3e`. The first
managed/native receipt `20260929-124849-beeaef37` passes **threaded raw JIT** for
valid ZEP/FCS and radio/BLE ingress, shared radio timer leases, fragments surviving
another link's close, pending-fragment cleanup, duplicate-bind rollback, foreign
transport rejection, restart and owner independence. Matching native inputs use
the production wrapper and record original/staged include hashes. Three-profile
native controls live under `artifacts/zep-native`.

This finite pass does not qualify output/compression/MTU expansion, the remaining
managed matrix or regressions of the changed shared service-registration helpers.
Initial receipt `20260929-124649-91471845` is retained: an authored fixture enum
reference failed compilation and was corrected before the passing run. No new
compiler repair or upstream text patch was required.

The interrupted worker's handoff recorded that valid native output passed payloads
17, 900 and 1232 bytes, then stopped at 1233 with the original ten-buffer IPv6
reassembly budget (`ip6_frag.memerr=1`). Its reused per-profile log was overwritten
by resumed native execution; the handoff summary remains, but that original log
is no longer an independently inspectable receipt. A fresh isolated resource-budget
control is being recorded separately. Raising the configured limit to 32 and
repairing valid fragment insertion are qualified only by the fresh scoped evidence
below, not by the interrupted experiment.

The resumed native valid-output controls now pass all three product profiles at
`artifacts/zep-output-native/<profile>/report.json`. Identical-fixture original
head-insertion behavior fails the valid reorder assertion (exit 92) under
`artifacts/zep-output-original-order/<profile>/failure.json`; the staged repair
preserves the queued fragment tail and passes delivery/expiry and natural
heap/pbuf/timer baseline. The original acquired source is unchanged.

Checkpoint **`537bfc3d`** records this finite owning LOWPAN implementation.
Fresh raw/processed publications pass threaded `20260929-141507-80ed4f8d`,
NO_SYS `20260929-142007-5b497849` and locking `20260929-142357-b36aa0ed`.
The owning LOWPAN suite passes all three profiles × four cells:

| Profile | LOWPAN, owning API and owner lifecycle | Final pool recovery | Consumer |
| --- | --- | --- | --- |
| Threaded | `20260929-141705-edad1d9e` | `20260929-142916-95783640` | `20260929-141705-edad1d9e` |
| NO_SYS | `20260929-142119-7e9e1e2e` | `20260929-142927-2dfeb959` | `20260929-142119-7e9e1e2e` |
| Locking | specialist reports in `20260929-142515-eba9318e` | `20260929-142834-20facba6` | `20260929-142834-20facba6` |

LOWPAN covers copied radio/BLE output at 17/900/1232/1233-byte payloads,
compression, IPv6 MTU crossing, unchanged duplicate/reordered frames, normal
expiry, shared radio timer leases, pending-fragment cleanup, constructor rollback
and independent-owner continuation. Native production-wrapper controls exercise
queued loopback with stop/FIFO/free. Managed common-interface tests qualify the
shared facade barrier; this is finite lifetime evidence, not all protocol teardown.

The locking batch's overall receipt remains **failed**: after its LOWPAN, owning
API and lifecycle cells passed, PoolRecovery processed AOT encountered MSB3883
while copying a referenced product assembly. Its legacy project reference was
replaced with the established canonical product import; final pool matrices and
pending locking consumer then passed in the fresh receipts above. Preserve the
failed receipt, and use its completed specialist reports only for their scopes.

The configured reassembly budget is 32 against the existing 256-buffer pool.
Fresh isolated `artifacts/zep-resource-limit10/nosys/report.json` reproduces the
valid MTU-crossing resource failure with budget ten. Its paired headers differ
only by that setting; it is new evidence, not recovery of the overwritten initial
worker log. `artifacts/lowpan-reorder-comparison-20260929.json` records identical
fixtures/profile headers and the sole pointer-assignment repair across all three
native profiles. Coordinator independently verified those bytes, 20 checkpoint
path hashes, 148 authored policy hashes, 11 owning and four LOWPAN implementation
hashes, and 11 original upstream exception records. Snapshot:
`artifacts/lowpan-qualified-snapshot-20260929-1430.json`. Ten pipeline checks,
layout, syntax and whitespace validation pass. No compiler repair or external
library was added. Auxiliary product refresh and the remaining required-work audit
in [remaining-work-audit.md](remaining-work-audit.md) remain open.

Native NO_SYS application characterization passes five finite cases under
`artifacts/udp-native-20260929/<case>-attempt-1/report.json`: `tftp-server`,
`tftp-get`, `sntp`, `netbios` and `mdns`. Native TFTP PUT remains failed; diagnostic
attempt2 confirms ordinary ERROR5 after valid ACK0. Each attempt has exact frozen
input hashes and compile/link/log records. SNTP uses a test-only notification
contract, and original mDNS retains one global UDP PCB after service removal.
These are not owning product-service acceptance or managed matrix results.
Owning TFTP/SNTP implementation and option-profile work subsequently passed
source-snapshot review and entered qualification. Native enhanced attempt
`artifacts/enhanced-udp-native-20260929/enhanced-01/report.json` passes all ten
designated options, its assertion control, original timer cancellation/removal,
and natural heap/pbuf/TCP PCB baselines. Six peer/eight stack frames pass
independent IPv4/TCP/UDP checksum/length checks. Exact native ABI observations
are recorded for the managed receive-info comparison.

The same directory contains original PUT failure `put-original-01`, adapted
success `put-adapted-01`, and owning C boundary success `owned-native-01` for
completion, explicit pending cancellation, ordinary storage error and original
server no-ACK expiry. `put-comparison.json` confirms 295 identical shared input
hashes and the same profile header. The guarded typed entry helper sets block0
before the original request serializer, without a new text patch. These native
results do not qualify the managed facade or broaden the original client's
request-timeout behavior.

Threaded translation `150928-70d1e79d` failed before publication because DotCC does
not support GCC forced-header `-include`. An authored SNTP include-wrapper now
expresses the same upstream macro hook; retry `151138-f5c825ab` passes raw and
processed publication. Enhanced publication `151412-e242ced2` also passes.
Coordinator verified all 533 inputs in snapshot
`artifacts/enhanced-udp-readiness-20260929-02.json`. Seven UDP cases are registered
on baseline product profiles; the ten-option suite is designated to additive
`threaded-enhanced`. All new managed behavior remains pending. Review identified
that the initial enhanced natural baseline covered only three counters; stronger
fixture checks for UDP PCB, netconn, netbuf and listening TCP PCB pools are being
added before the managed runs, without broadening the earlier native claim.

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
