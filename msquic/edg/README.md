# EDG translation of MsQuic

This port uses the pinned MsQuic source and the existing dotcc managed-host
contract. It translates the selected 47 C units plus one managed API root unit
with EDG's C# backend and links
them to the existing BCL host and **EDG-translated picotls**. The dotcc-generated
MsQuic and picotls libraries are not product dependencies.

The source archive is verified before staging. The three existing managed-host
header overlays and four host adapter/portable fragment units are recorded in
`source/manifest.json`; upstream transport implementation files are unchanged.
`api_roots.c` retains native header types and an inline helper used only by the
managed consumer, which would otherwise be absent from the C call graph.
Microsoft anonymous-member syntax is enabled explicitly, alongside the portable
LP64 target and GNU C11 mode. Selected musl implementation pruning is enabled by
default. Status values and referenced object macros are exported by EDG.

```sh
python3 msquic/edg/translate.py --output-dir /path/to/new-translation --build
python3 msquic/edg/tests/verify.py \
  --core /path/to/new-translation/managed \
  --stage /path/to/new-translation/source \
  --picotls-core /path/to/edg-picotls/managed \
  --output-dir /path/to/new-verification --aot \
  --modes configuration transport packets resumption
```

`provider.py` links generated C# unchanged. It records explicit ABI spelling
adaptations to copies of the authored BCL host and managed facade sources:
signed native `char`, typedef names, inline arrays and anonymous aggregate
members can differ from dotcc's representation. A partial-class bridge exposes
EDG's named `MsQuicLib` global to the existing host shutdown sequence.
Transient platform allocations use tracked, 16-byte-aligned `NativeMemory`
storage; allocator tags and independent ownership remain checked. Global/TLS
storage remains governed by the translator.

The managed callback table supplies platform/UDP and packet cryptography through
BCL implementations. QUIC TLS uses the existing picotls adapter and its translated
handshake state machine. No native QUIC/TLS implementation is a product fallback.
Native code and aioquic are verification oracles/peers only.

See [verification scope and commands](tests/README.md). A successful translation
or library build is not evidence of a working transport. Receipts in each fresh
output directory identify the executed checks and retain failures; this port
does not inherit the legacy dotcc campaign's broader qualification results.

## Verified output (2026-10-07)

The selected QUIC v1 profile passes on Linux x64 with the pinned source revision
`80a065112426bce68c1da42d026478d3e40fd45e`. Strict Release builds report zero
warnings/errors. Native/JIT/NativeAOT ABI comparisons match 65 observations;
the allocator passes 33 checks per runtime. Both JIT and NativeAOT pass
configuration, IPv4/IPv6 stream/lifetime, AES-128/256 packet protection,
key updates, DATAGRAM and session-resumption controls (including rejection,
full-handshake fallback, copied application state and pending-close handling).

All 16 separate aioquic exchanges pass: managed client/server × IPv4/IPv6 ×
AES-128/256 × JIT/NativeAOT, with 65,537-byte payloads, FIN, authenticated TLS
negotiation and drained ownership. The regenerated picotls also passes its ABI,
upstream-vector, provider and TLS JIT/NativeAOT suites. Windows/macOS and other
architectures have not been executed; broader legacy recovery/fault campaigns
remain separate evidence.

Review links:

- `edgcpp/artifacts/review/msquic/`: generated core, 127 C# files / 5,054,267 bytes.
  `Translated.source.connection.c.cs` and `Translated.source.stream.c.cs` contain
  the main connection/stream code; `dot-gen.sources.json` maps original locations.
- `edgcpp/artifacts/review/msquic-managed/ManagedApi/ManagedApi.csproj`: owning API
  project with the BCL host and both EDG-generated cores as project dependencies.
- `edgcpp/artifacts/review/picotls/`: the regenerated TLS core used by this port.

Persistent artifacts under `/mnt/storage/dotcc-edg-campaign-20261003/`:

- `msquic-edg-011/` and `msquic-picotls-edg-002/`: final translation/build receipts,
  original inputs, raw C# and packaged sources.
- `msquic-edg-tests-004/`: final ABI, allocation and four-mode facade acceptance.
  Its receipt references the frozen provider projects under `msquic-edg-tests-003/provider/`.
- `msquic-edg-independent-004/`: final independent peer receipts and executables.
- `msquic-picotls-verified-001/`: final standalone picotls regression results.

[Tracked acceptance evidence](../../edgcpp/evidence/msquic-20261007.json) records
the exact input/output hashes, verification receipts and limits. Earlier failed
attempts are preserved. Generic translation fixes and their native/C-backend
comparisons are recorded separately in
[prerequisite evidence](../../edgcpp/evidence/msquic-prerequisites-20261007.json)
and [backend evidence](../../edgcpp/evidence/msquic-backend-20261007.json).
