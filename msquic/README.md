# MsQuic translation campaign

See the [2026-09-24 compiler refresh verification](docs/verification-20260924.md)
for freshly regenerated PicoTLS/MsQuic results and the retained recovery failures.

Regenerate the selected core and its postprocessed product with:

```sh
msquic/scripts/translate.sh
```

To switch source versions first, run `python3 msquic/scripts/fetch.py --ref stable`
(or `--ref main`, a release tag, branch, or commit). Platform headers are reused;
source inventories, portable function fragments, and native reference inputs
follow the selected version automatically. See [source selection](docs/source.md).

The final project is
`msquic/generated/TranslatedMsQuic/TranslatedMsQuic.csproj`. Translation links
with `--literal-pool --nest-types --runtime=c`; ABI types and runtime helpers live inside
`Managed.Transport.MsQuic`. Low-level consumers can import these types with
`using static Managed.Transport.MsQuic;`. The owning API keeps its existing
`Managed.Transport.Api` surface and references this final project by default.

Full macro expansion and typed detection automatically expose status values such
as `MsQuic.QUIC_STATUS_PENDING`, preserving their unsigned C values. Object
compilation retains `--emit-define 'QUIC_STATUS_*'` to require these API exports. Macro-generated function helpers
receive canonical pointer fields only when translated C uses their addresses.

The script preserves raw nested output at `generated/raw/TranslatedMsQuic`,
postprocesses the final project in place, and checks native/JIT/NativeAOT ABI and
whole-assembly consumers. `--no-build-tools` reuses already-built compiler and
postprocessor binaries. Earlier closure evidence is archived before regeneration;
the new closure does not claim a fresh SQLite or full transport campaign unless
those gates have actually been rerun.

For a development-only translation without the qualification and closure gates,
run:

```sh
msquic/scripts/translate.sh --fast [--no-build-tools] [--jobs 8]
```

This stages the selected source, translates its independent C translation units
in parallel, links the raw project, and postprocesses the final project. It writes
the same generated directories and archives any locally qualified checkpoint before
replacing its files. It does not run native/JIT/AOT ABI or consumer tests, fetch
source, or freeze a product closure. The exact pinned
source tree must already exist under `msquic/ref/`. The full script uses separate
objects because a monolithic compile retains parser state for every translation
unit; unlike the qualification path, those object compilations run concurrently.
See [nested-layout validation](docs/nested-layout.md) for the refreshed ABI,
consumer and service checks.

The selected Linux x64 product translates and links all 47 source units from the
MsQuic snapshot resolved on 2026-09-12. Raw and postprocessed libraries work under
JIT and NativeAOT. The translated core uses a managed platform/UDP host, BCL packet
cryptography and the existing translated picotls provider. Separate native MsQuic
and aioquic processes serve as test peers.

Before the nested-layout change, qualification passed 80 basic transport pairs, 68 owning API mode
executions, 32 separate-consumer cases, 88 independent-peer cases, 152 CID cases,
480 positive recovery exchanges and 64 live endpoint input/amplification profiles.
That campaign also freshly regenerated SQLite; its normal generated
output includes the postprocessor optimizations and passes JIT/NativeAOT checks.
The fixed native recovery observations and full 20-pair performance comparison
are complete. Two stronger recovery checks retain strict failures, including one
managed harness-deadline outcome unmatched by its native control. Qualification
is therefore not an unconditional pass. See the
[qualification ledger](docs/qualification.md) and
[performance measurements](docs/performance.md) for evidence and limits.

See the [implementation plan and phase status](docs/PLAN.md),
[initial compiler scope and evidence](docs/compiler-scope.md),
[source and native reference provenance](docs/source.md),
[validated compiler implementation](docs/compiler-implementation.md),
[pinned source](config/source.json), and [reduced examples](probes/).

The reference source is unchanged. Diagnostic source copies, generated C#,
binaries, and logs are ignored. Generic compiler fixes and reduced regression fixtures have passed the complete
compiler suite. The initial scope report
is historical evidence from before those fixes. Its receipts are preserved in
`artifacts/initial-compiler-scope.tar.gz` before subsequent probe runs.

```sh
dotnet build DotCC/DotCC.csproj -c Release --nologo
python3 msquic/scripts/fetch.py
python3 msquic/scripts/probe.py --stage baseline
python3 msquic/scripts/probe.py --stage syntax
python3 msquic/scripts/build-probe.py
python3 msquic/scripts/probe-reduced.py --build
```

Probe scripts save failures as evidence; their successful completion does not
mean the generated library compiles. Inspect the recorded stage exit codes.
These diagnostic probes do not establish transport correctness.

Separate native reference (test peer only):

```sh
python3 msquic/scripts/inventory.py
python3 msquic/scripts/native-oracle.py --jobs 4
python3 msquic/scripts/test-native-sample.py
python3 msquic/scripts/test-native-peer.py
python3 msquic/scripts/independent-peer.py
python3 msquic/scripts/test-independent-peer.py
```

Historical compiler probes over **unchanged upstream source**, using clearly
marked diagnostic host declarations rather than the product host:

```sh
python3 msquic/scripts/probe.py --stage headers --label unchanged-core
python3 msquic/scripts/build-probe.py --label unchanged-core
```

P1 BCL UDP and raw translated-picotls QUIC feasibility are independently validated
under JIT/NativeAOT; see [UDP evidence](docs/datapath-feasibility.md) and
[TLS evidence](docs/tls-feasibility.md). The committed service baseline passes the actual platform, packet crypto, TLS
adapter and UDP gates in all four raw/optimized × JIT/NativeAOT combinations.
Packet crypto passes 162 checks per combination; the TLS adapter passes 20 cases
per combination, and the fresh picotls regression campaign passes. Injected
failure paths pass all 40 cases, with another 48 actual-core keepalive/idle controls.
The owning API now passes all 17 modes under raw/optimized JIT/NativeAOT,
including streams, lifetime races, authentication, Retry/reset, key updates,
DATAGRAM, ticket-key rotation, callback errors, registration-wide shutdown,
settings state boundaries and network parameters. Actual runtime metadata reports
the pinned source revision. Final SQLite validation, its normal optimized output
restoration, and fresh picotls regression passed with the frozen compiler.
The regenerated platform/UDP service matrices and all 80 transport peer pairs
pass, with exact bidirectional payloads and clean drain. The peer harness includes
a test-only shared-binding option for CID rotation. All 152 ordinary, rotation
and authentication-negative cases pass against the independent peer.

The product stage contains 47 units: 43 unchanged upstream units
and four host adapters/fragments. It emits and links as separate objects. The
complete core ABI matches 60 native observations under JIT and NativeAOT. Raw and
optimized managed libraries and whole-assembly-rooted AOT consumers also pass.
The exact closure and evidence hashes are pinned in `config/product-closure.json`.
See the [managed host contract](docs/host-boundary.md) and the explicit
[API selection policy](docs/api-profile.md), which covers all 39 table slots and
72 parameter identifiers without claiming runtime support.

The product gate scripts use actual staged inputs and native layout controls:

```sh
python3 msquic/scripts/test-host-contract.py
python3 msquic/scripts/build-product.py
```

The second command requires a passing complete host ABI receipt and checks its
per-object source, compiler and object hashes before reuse. It preserves raw
output, runs the repository postprocessor, and builds both managed libraries
and NativeAOT consumers that root the entire generated assembly. Its executable
check only verifies rejection of an invalid host table; it does not initialize
host services or establish transport correctness.
