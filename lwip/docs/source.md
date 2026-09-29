# Source and configuration inventory

The campaign acquires lwIP 2.2.1 using the pinned metadata in
`config/sources.json`. The initial local reference was copied from the planning
checkout; acquisition receipts describe observed provenance. Preparation never
modifies this reference. Two narrow upstream repairs are recorded in config/adaptations.json: the separate generator checksum-loop progress fix and four IPv4 forwarding software-checksum predicates. Only staged copies change. Product/contrib forwarding sources use the latter; diagnostic options explicitly keep IP_FORWARD=0 and retain original ip4.c in both native and managed builds.

`config/source-groups.json` enumerates all C translation units declared by
upstream `src/Filelists.cmake`, including its conditional SLIP append. The stack
closure comprises 119 units: 38 core, nine API, four interface, four 6LoWPAN,
30 PPP, 21 SNMP, four HTTP and nine remaining application units. The three
mbedTLS adapters are explicitly excluded. Uncompressed makefsdata is separately
inventoried for tool delivery. `config/contrib-inventory.json` classifies every
file in contrib, including inputs absent from the aggregate CMake list.

Profiles and feature-to-suite rows are implementation drafts until effective
preprocessing and behavioral checks have qualified them. Source presence does
not prove an active compile guard or tested behavior. The unit diagnostic profile layers the original options and simplified scheduler without changing upstream fixtures or assertions. All137 actual registrations match native Check and pass raw/processed JIT/NativeAOT (081512-9608394d, 081538-a7d37662). The corresponding unit-nosys132 cases also pass all four cells (083055-ad449f5f). The uncompressed generator has a native-matched four-cell checksum fixture (083157-23870a49); generated-content HTTP consumption and contrib behavior remain open.

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
