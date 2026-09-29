# Source and configuration inventory

The campaign acquires lwIP 2.2.1 using the pinned metadata in
`config/sources.json`. The initial local reference was copied from the planning
checkout; acquisition receipts describe observed provenance. Preparation never
modifies this reference. Five narrow upstream repairs are recorded in `config/adaptations.json`: generator checksum-loop progress, IPv4 software-checksum predicates, EAP control-pbuf release, the IEEE802.15.4 short-source constant and LOWPAN reassembly list insertion. Only staged copies change. IPv6 hop expiry uses a scoped macro override, and lifecycle support uses authored include-wrappers around unchanged algorithms. Diagnostic applicability is recorded per repair; no diagnostic assertion or fixture is modified.

`config/source-groups.json` enumerates all C translation units declared by
upstream `src/Filelists.cmake`, including its conditional SLIP append. The stack
closure comprises 119 units: 38 core, nine API, four interface, four 6LoWPAN,
30 PPP, 21 SNMP, four HTTP and nine remaining application units. The three
mbedTLS adapters are explicitly excluded. Uncompressed makefsdata is separately
inventoried for tool delivery. `config/contrib-inventory.json` classifies every
file in contrib, including inputs absent from the aggregate CMake list.

Profiles and feature-to-suite rows are implementation drafts until effective
preprocessing and behavioral checks have qualified them. Source presence does
not prove an active compile guard or tested behavior. The unit diagnostic profile layers the original options and simplified scheduler without changing upstream fixtures or assertions. All137 actual registrations match native Check and pass raw/processed JIT/NativeAOT (081512-9608394d, 081538-a7d37662). The corresponding unit-nosys132 cases also pass all four cells (083055-ad449f5f). The uncompressed generator has a native-matched four-cell checksum fixture (083157-23870a49); the subsequent actual generated-content HTTP roundtrip passes093554-eba3af0c. Full contrib behavior remains open. Dated results below are chronological milestones; docs/validation.md records current evidence.

Packing uses the official `PACK_STRUCT_USE_INCLUDES` port hook with paired
`#pragma pack(push, 1)` / `pop` headers. Host handles use `intptr_t`; these are C
`long` on the declared LP64 ABI. Every shared host binding must receive an explicit
translated owner. Object emission uses instance methods; the link uses both
instance methods and the shared C runtime profile.

The initial source manifest excludes external TLS/DTLS/mbedTLS and SNMPv3 crypto,
but retains bundled PPP MD4, MD5, SHA1, DES and ARC4. EAP MD5-Challenge is included;
EAP SRP branches require an external SRP implementation and are excluded.
Unsupported CBCP/ECP/demand modes remain disabled as upstream limitations.

PPP option audit: `multilink.c` documents an unported Samba TDB requirement and
is classified as an external-dependency exclusion. Deflate/BSD/Predictor options
in `ccp.c` retain negotiation code but require `ccp_test`, whose only definition
is disabled by `#if 0` in `ppp.c`. The actual CCP packet transmit/receive switches
support MPPE only. These absent codecs are upstream-unimplemented; VJ and MPPE
remain required. `LQR_SUPPORT` explicitly implements LCP negotiation only, which
is the behavior to qualify rather than claiming a full quality-report engine.

The required `makefsdata` profile publishes its project at
`generated/profiles/makefsdata/TranslatedLwip/TranslatedLwip.csproj` (and `.Raw`).
The assembly is named `TranslatedLwipMakeFsData`, with class
`Managed.Networking.Tools.MakeFsData`, to keep the generator separate from the
stack while reusing the existing transactional profile layout. Its only source
repair is the upstream checksum-loop progress fix documented in the blocker
ledger; the native bounded oracle applies the same staging helper.

`THIRD-PARTY-NOTICES` preserves complete source-specific comment notices from the
acquired tree, including bundled PPP primitives and tinydir. The recipe regenerates
and includes it alongside COPYING in published outputs.

Native diagnostic controls use pinned Check0.15.2 acquired through the `tests`
source group. The managed test-only compatibility header exposes the Check0.12
registration ABI selected by upstream lwip_check.h, retaining all conditions and
callbacks. Its string.h include matches the native Check transitive declarations.
A separate deliberate-failure control verifies nonzero exit and fixture counts
(three setups, two bodies, two teardowns, two failures), including skipping a body
and teardown when setup fails. Check is not a product dependency.

The NO_SYS diagnostic selection excludes exactly five SOCKETS registrations,
matching the original empty-suite branch; unguarded threaded helpers in that
source file require omitting the translation unit and supplying the same empty
getter in authored test support. The unchanged IPv4/IPv6 teardown requests a
scheduler poll; the NO_SYS adapter executes netif_poll_all and sys_check_timeouts
to perform actual mainloop cleanup. The native counterpart and raw/processed JIT/NativeAOT builds all pass132 cases. No upstream test assertions are changed.

The finite `tool-generator` suite covers bounded checksum image generation and
native byte equality. Its initial receipt083157-23870a49 used the provisional
`tools` name; generated content is now exercised through the translated HTTP
server in the dedicated http-content matrix noted below.

The `http-content` profile consumes a file emitted by the actual processed/JIT
`makefsdata` product through upstream `HTTPD_FSDATA_FILE`, with
`HTTPD_PRECALCULATED_CHECKSUM=1`. Preparation records the generator project,
translation receipt, command, exact input hashes and generated image hash in the
HTTP translation receipt; the image is retained beside that receipt. Explicit
translation requires an already published makefsdata profile. Full verification
prepares makefsdata before http-content without recursively entering campaign
or tool locks. Native HTTP control compiles that same durable generated image.
Portable preparation retry093414-95dafdc6 and the actual native-matched raw/processed JIT/NativeAOT HTTP roundtrip093554-eba3af0c pass. The generator and generated-content tools gate is qualified on Linux x64; Windows execution remains unqualified.

Finite contrib-addon raw/JIT controls passed090220-366b245d: seven extended ABI
sizes/offsets, two-owner extension callbacks, route-table operations and fixed
IPv4/IPv6 TCP ISN vectors. The empty IPv6 route table's low-level lookup returns
zero for an unused zero-prefix slot in this revision; the forwarding hook
correctly returns no route, and both native and managed tests preserve that
behavior. Other contrib application workflows remain open.

Finite bundled PPP raw/JIT controls passed090230-b19a87b0: five bundled primitive
vectors, RFC2759 authentication acceptance/tampered rejection, RFC3079 key,
600 stateful/stateless MPPE packets through a rekey boundary, and malformed
headers. These controls do not close complete PPP link/negotiation qualification.

Finite addon controls now pass all four cells092742-40fc80aa. Bundled PPP crypto,
SLIP and HTTP server baseline controls pass all four cells in each product
profile: threaded092814-c3aef247, nosys092953-8a3a53fc and
threaded-locking093126-d3ef1043. SLIP includes actual IPv4/IPv6 UDP ingress,
fragmented escape sequences and a live incomplete decoder frame whose pbuf and
heap usage return to baseline after explicit worker stop/join. Unknown escaped
bytes are preserved as the pinned implementation does. HTTP includes custom
content, SSI, raw CGI parameters (upstream does not URL-decode them), POST,404,
fragmented requests and released file state. The earlier aggregate092312-37ea469b
failed on an invalid NULL CGI unregister call during HTTP teardown; its passing
SLIP constituent is superseded by the complete matrices above. Clean HTTP retry
092443-6eedfe86 fixed only fixture lifetime handling.

`artifacts/finite-source-snapshot-20260929-0937.json` records the host/facade
source state after these matrices. The host worker authored new registered-service
and failed-shutdown retry behavior during earlier cells; these finite protocol
receipts do not qualify that newer service contract. Its dedicated lifecycle
qualification remains separate. No full PPP/link/application/contrib gate is
closed by these subsets.

After migration to the main checkout, merged toolchain refreshes publish all eight
profiles: threaded100938-1e61b771, nosys101126-d7f9a0c3,
threaded-locking101258-8d10f70a, contrib101436-e4f2dfc8,
makefsdata101623-bba174b7, http-content101639-f50e3581,
unit101818-531970c1 and unit-nosys101941-06379f74. Historical receipts preserve
old absolute paths as provenance; new qualification uses the main-checkout products.

The third narrow staged upstream repair releases an untransferred pbuf in
`eap_send_request`'s invalid-state default branch. Real EAP-MD5 wrong credentials
reproduce the552-byte retained heap allocation after both sessions are dead and
freed. The reference is unchanged. A unique larger local switch-tail guard is
mandatory while the file hash remains observational, preserving unrelated local
edits under warn/off policy. Native exact-fixture original/adapted controls and
symbolized allocation evidence are under artifacts/ppp-links-native and
artifacts/native-eap-leak. Product/contrib/http-content native and translated
closures use the identical staged unit. Diagnostics keep original EAP source,
which is inactive in the upstream diagnostic configuration.

Real PPPoS and independent PPPoE client suites now pass raw/processed JIT and
NativeAOT in all three profiles: threaded102218-436a6eb5,
nosys102323-8af6d3ee and threaded-locking102423-3e0a37ed. PPPoS covers six successful
sessions (PAP, CHAP-MD5, MSCHAPv1/v2, EAP-MD5, required128-bit MPPE), five wrong
credential outcomes, actual IPCP/IPv6CP, UDP and TCP payloads, bad FCS, independent
FCS/VJ/MPPE wire inspection and natural resource baselines after close/free.
Managed peers have separate owning stacks. Locking also requests and observes
LQR option acknowledgements; the pin supplies negotiation only, no report engine.
PPPoE uses independently encoded RFC2516/RFC1661 frames, including service and
concentrator names, wrong host-unique rejection, cookie echo, malformed PADS,
LCP rejection/progress, dual-stack negotiation, bidirectional UDP and orderly
termination/PADT. Upstream's PPPOE_SERVER branch explicitly emits #error and is
classified as unimplemented; this fixture independently emulates that peer.

Fresh main-checkout upstream diagnostics also pass every cell: original137 cases
102529-f99f8fc4 and132 NO_SYS-applicable cases102607-122fea01, including native
registration parity and deliberate assertion/setup controls. These checkpoints do
not close full link/application gates: L2TP data/managed qualification, broader
IPv6 link data and reconnect cases, registered PPP/SLIP service lifetimes,
6LoWPAN/ZEP and remaining applications/configuration variants remain required.

The IPv6 hop-limit repair uses a macro override only for `src/core/ipv6/ip6.c`.
The original forwarding decrement promotes zero to a negative integer before the
setter truncates it to255. The authored typed helper clamps negative input to
zero, allowing the existing expiry path to reject it; ordinary u8 output values
remain unchanged. `config/ip6-hop-contract.json` guards the exact original
macro parameters/body and both setter call roles. Object reports must show one
selected definition and exactly two expansions in the intended source unit.
Native builds force-include the equivalent reviewed header for this unit only.
Diagnostics retain the original setter because forwarding is disabled.

`src/port/bridge_service.c` includes the two original bridge implementation files
unchanged and supplies owner validation, a real queued FIFO barrier, and private
FDB/timer teardown. The product selects this wrapper instead of the two standalone
bridge units. Receipts preserve both logical upstream source closure and the
wrapper-to-original inclusion mapping. Teardown requires a barrier outside the
core before stop and between stop/free; this also drains pending loopback in the
locking profile. These requirements are tested through ordinary consumer code.

The fourth minimal staged source repair corrects the source-short address
constant in the IEEE802.15.4 parser. Its source bits were compared with the
unrelated destination-short encoding. The unique larger source-branch guard
leaves destination parsing intact; unrelated local edits remain allowed. Native
controls compile the same fixture against original and repaired lowpan6.c:
original short-source input fails, while all ten raw/IPHC radio address
combinations and BLE receive vectors pass in all three profiles. Diagnostic
closures do not select lowpan6.c. This finite receive-vector witness does not
qualify fragmentation, reassembly, ZEP lifecycle, or every compression option.


The four additional finite suites pass raw/processed JIT and NativeAOT in every
product profile: threaded110105-df4730c6, nosys110524-6852b698 and
threaded-locking111004-d11b3587. These cover independent authenticated L2TP
control/data, ten native BLE/radio vectors and thirty managed deliveries across
two owners, IPv6 forwarding hop-expiry/error paths, and bridge forwarding/FDB
aging/queued-loopback teardown and owner isolation. Product publications are
105719-74e8e2eb,110311-4b343b75 and110757-36b627d9 respectively. Auxiliary
contrib/http-content publications still precede these latest adapters and require
refresh with the next shared wrapper batch. The original native LOWPAN parser
must fail the same short-source fixture; the corrected parser must pass all
address combinations. Nine focused recipe/staging/hop-contract checks pass.

The source snapshot is artifacts/link-routing-source-snapshot-20260929-1114.json.
ZEP prototype code remains outside the selected product and this checkpoint.
Full link/application/feature-variant qualification remains open.

### Owning API and selected-product capabilities (2026-09-29)

Product publication now writes `Lwip.Capabilities.props` next to each project.
The facade and ordinary consumer import this metadata through
`src/LwipProduct.props`, so NO_SYS selection removes socket/netconn references
from the managed API without relying on a caller-supplied profile hint. Explicit
mismatches fail before compilation. The import selects static graph restore and
an explicit product reference; consumer facade references forward normalized
`LwipProject` and matching `LwipProfile` metadata. The local-relative selection
witness checks both the generated constant and the facade API surface.
`src/port/socket_refs.c` includes the original
sockets unit and exposes only original get/done reference operations; source
closure reports retain the original logical unit mapping.

`owned-api` exercises production facade sources, not copies of prototype classes.
It includes native descriptor-reference and interface-lifetime controls, valid
DNS traffic, copied buffers, cancellation, accepted-child adoption races, disposal
retry and partial interface construction. Verification-policy metadata records
exact permitted source hashes and receipts. Superseded prototypes are archived
under `artifacts/owned-api-prototype-history-20260929/manifest.json`, outside the
consumer project. ZEP and broader application/variant work remain unqualified.

Current main product publications carry this metadata; older contrib and
http-content publications must be refreshed before referencing the new facade.
Historical auxiliary protocol receipts do not qualify that new integration.

The final narrow project-reference graph passes owning-api and ordinary consumer
in all four cells for NO_SYS (121848-8f47f868), threaded (122007-dac94da8), and
core-locking (122125-d03caecf). Subsequent socket/netconn scheduling admission
retains/accounting before queueing passed owning-api all four cells for threaded
(122521-090f75f3) and core-locking (122622-a0980575); those APIs are compiled out
of NO_SYS. Paused public Tasks settle before owner disposal completes. The
original fixed-buffer pointer failure (114817) and local restore graph failure
(121229) remain preserved. Nine pipeline checks and layout validation pass.

## Owning LOWPAN output checkpoint

The original LOWPAN and ZEP bodies now share an authored include-wrapper that
exposes only lifecycle boundaries: radio timer leases, stop, core FIFO drain and
backing release. BLE retains its separate original contexts. The owning facade
provides copied radio/BLE frames and ZEP UDP transport, owner-relative interface
properties, shared PAN/context controls, constructor rollback and retryable
disposal. Original algorithms remain translated. Public usage/contracts are in
`tests/Zep/README.md` and the campaign README.

A fifth minimal staged source repair adds the missing `p->next=q` when a valid
FRAGN is inserted before the existing first queued fragment. Without it, valid
original output delivered out of order loses the queued tail. There is no local
port hook/configuration for that assignment; whole-function substitution would
duplicate the parser. Exact guards preserve prior short-address adaptation,
original references and unrelated content. Native before/after input/profile
bytes match, with only this assignment changed. The permitted fixture delivers
unchanged complete frames and does not generate corruption.

Product IPv6 reassembly is configured for 32 packet buffers against the existing
256-buffer pool, permitting the observed LOWPAN/IPv6 MTU-crossing reassembly. A
fresh isolated ten-buffer control records the prior resource limit. Three native
profiles and three managed profiles × raw/processed × JIT/NativeAOT pass the
finite LOWPAN suite, with shared owning/lifecycle/consumer regressions recorded
in validation.md. The PoolRecovery project uses canonical product property
propagation after an AOT reference-output collision. No shared compiler repair
was needed. Remaining option/application/contrib work is explicitly inventoried
in remaining-work-audit.md; this checkpoint does not complete those gates.

## UDP applications and enhanced option checkpoint

An authored TFTP include-wrapper retains the original parser, request serializer
and retry engine while exposing singleton admission, original cleanup and close
context. A guarded typed `tftp_put` override invokes a small C setup helper whose
initial block number is zero before original serialization. Native controls show
the original valid ACK0 failure and corrected transfer with otherwise identical
inputs. Exact local body/state/context guards protect this dependency; the whole
original file hash records provenance without rejecting unrelated source edits.
No sixth text patch was added.

SNTP uses its documented raw-NTP time setter macro, supplied before unchanged
original `sntp.c` through an authored include-wrapper. The typed explicit-owner
binding publishes copied seconds/fraction values and never changes the operating
system clock. The owning client bounds notifications and completes readers at
shutdown. Owning TFTP bounds payload bytes and file count separately, retains
copied read snapshots, stops the original client/server before draining callbacks,
and releases roots only afterward. A borrowed original instance survives failed
owning initialization, and subsequent owning restart succeeds. Original client
request retransmission is not invented; server retry/expiry and explicit pending
client cancellation are distinct qualified behaviors.

Additive `threaded-enhanced` enables ten compatible implemented TCP/socket/core
options. `config/feature-options.json` records effective macros, local compile
guards, required definitions, dependent literals, and distinct behavior/lifetime
contracts. Its initial ten rows do not claim complete option inventory. SACK,
timestamps, backlog, window scaling, linger, receive metadata, reusable broadcast
delivery, next-datagram FIONREAD, core timeouts and interface-removal callbacks
pass native and all four managed cells. Canonical consumer/ABI/owning gates and
separately rooted AOT also pass. UDP's seven finite application cases pass all
three baseline profiles and four cells; receipts are in validation.md. Other
applications, variants, owning services and final full-product audits remain open.
