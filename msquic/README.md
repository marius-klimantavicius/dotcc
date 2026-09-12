# MsQuic translation campaign

Downloaded the latest `main` snapshot resolved on 2026-09-12 into `ref/` and
attempted dotcc translation. The user subsequently authorized implementation of
all phases with a coordinator and sub-agents. Completed changes are being
committed as authorized. Final toolchain-bound regeneration, the service matrices,
80 transport pairs and all 68 owning API mode executions pass. The separate 32-case public-consumer matrix also passes. Remaining external
interop, recovery and delivery qualification is in progress.

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

Current compiler iteration over **unchanged upstream source**, using only clearly
marked diagnostic host declarations (still not a product PAL):

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
failure paths retain targeted evidence while their final complete matrix is pending.
The owning API now passes all 17 modes under raw/optimized JIT/NativeAOT,
including streams, lifetime races, authentication, Retry/reset, key updates,
DATAGRAM, ticket-key rotation, callback errors, registration-wide shutdown,
settings state boundaries and network parameters. Actual runtime metadata reports
the pinned source revision. Final SQLite validation, its normal optimized output
restoration, and fresh picotls regression passed with the frozen compiler.
The regenerated platform/UDP service matrices and all 80 transport peer pairs
pass, with exact bidirectional payloads and clean drain. The peer harness now includes a test-only shared-binding option for CID rotation;
all 80 refreshed baseline pairs pass, and targeted actual rotation passes both
roles against the independent peer.

The candidate product stage now contains 47 units: 43 unchanged upstream units
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
