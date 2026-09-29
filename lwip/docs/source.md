# Source and configuration inventory

The campaign acquires lwIP 2.2.1 using the pinned metadata in
`config/sources.json`. The initial local reference was copied from the planning
checkout; acquisition receipts describe observed provenance. Preparation never
modifies this reference. The stack uses no text adaptations. The separate makefsdata tool has one uniquely guarded upstream checksum-loop repair in config/adaptations.json.

`config/source-groups.json` enumerates all C translation units declared by
upstream `src/Filelists.cmake`, including its conditional SLIP append. The stack
closure comprises 119 units: 38 core, nine API, four interface, four 6LoWPAN,
30 PPP, 21 SNMP, four HTTP and nine remaining application units. The three
mbedTLS adapters are explicitly excluded. Uncompressed makefsdata is separately
inventoried for tool delivery. `config/contrib-inventory.json` classifies every
file in contrib, including inputs absent from the aggregate CMake list.

Profiles and feature-to-suite rows are implementation drafts until effective
preprocessing and behavioral checks have qualified them. Source presence does
not prove an active compile guard or tested behavior. Diagnostic profiles now layer the upstream unit options without modifying them; their effective case execution remains unqualified. Auxiliary tool/contrib products remain to be completed.

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
