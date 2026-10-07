# Picotls translated with EDG

The pinned picotls core now runs as C# with the shared BCL crypto provider under
.NET JIT and NativeAOT. This port is independent of the existing dotcc-generated
output and retains the `Managed.Security.PicoTls` class and public C function
names. EDG uses public typedef names such as `ptls_context_t` for records.

The input is revision `3598470df01264da85157025ed10db0f7e103790`, already pinned
in [the campaign source manifest](../config/inputs.json). Translation verifies
the archive hash and every archived upstream file. The real protocol state
machine is translated from the unchanged core; cryptographic callbacks reuse
[the existing BCL provider](managed/README.md). Original generated C# is never
patched to accommodate that provider.

## Build

From the repository root, with the pinned inputs fetched:

```sh
picotls/scripts/fetch.sh
EDG_BUILD_TARGET=cpfe-dot bash edgcpp/build.sh
python3 picotls/edg/translate.py --output-dir /absolute/new/picotls-edg --build
python3 picotls/edg/provider.py \
  --core /absolute/new/picotls-edg/managed \
  --output /absolute/new/picotls-provider
dotnet build /absolute/new/picotls-provider/BclProvider/BclProvider.csproj -c Release
```

Reference the prepared `BclProvider.csproj` for the owning managed API, or
`Core/EdgPicotls.csproj` for the translated pointer API and provider allocation
bridge. See the existing [API usage](../docs/usage.md) for contexts and connections.
The generated core uses musl plus the shared `Edg.Runtime` implementation.
Ordinary allocations use the native heap; immutable provider names use pinned
managed storage. Sources, projects, commands and failure logs are retained.

The picotls recipe defaults to `--libc-pruning reachable`: all project functions
remain available, while only the musl definitions reachable from them are emitted.
Use `--libc-pruning all` to retain the entire provider for comparison or to expose
otherwise unused libc functions directly to managed callers. The generic
`edgcpp/dotnet.py` driver also defaults to `reachable` for `--libc musl`.
A source wrapper calling a desired libc function also keeps it.

For this profile, pruning reduces generated C# from 286 to 72 files and from
4,452,485 to 3,299,933 bytes. `c32rtomb` is absent; all 102 picotls API names and
all 240 project function names remain. Twenty of 449 eligible provider routines
and four of 66 eligible provider variables are retained. These figures include
the copied runtime files. EDG still parses all provider inputs to resolve their
dependencies; the optimization removes unused C# emission.

The trimmed output passes the complete JIT/NativeAOT verification below, including
both 56-case peer suites. A separate used-`c32rtomb` test confirms that it remains
available when called. See [pruning evidence](../../edgcpp/evidence/picotls-pruning-20261007.json).

## Sharing libc code between translated projects

A referenced shared library is feasible but is not implemented by this pruning
change. The authored `Edg.Runtime` helpers are already largely independent and
could be built once and referenced, including shared `VaList`/`VaArg` types and
timezone assets. Translated external partial implementations must stay with the
translated project because C# partial classes cannot span assemblies.

Translated musl currently shares the project's record types, globals, TLS,
allocation registry and callback signatures. A separate musl assembly therefore
needs a stable ABI with shared or opaque record types, an explicit owner context,
allocation ownership and callback adapters. Sharing code need not share mutable
state: separate contexts can preserve independent library instances. That design
can use direct calls and remain NativeAOT-compatible, while also avoiding repeated
musl parsing and translation.

## Verified results

Each configuration—JIT and Linux-x64 NativeAOT—passes:

| Suite | Result |
| --- | --- |
| Native ABI layout | 92 size, alignment and member-offset checks |
| Upstream utility vectors | 8 cases, 232 checks |
| Crypto provider | 2,962 checks, including 124 handshake allocation-failure boundaries |
| TLS behavior | 9,963 assertions |
| Independent peers | 56 cases against native picotls/OpenSSL and .NET SslStream |

The peer suite covers client/server roles, both AES-GCM suites, RSA/ECDSA
certificates, mutual authentication, empty and large records, key updates, and
certificate/name/ALPN rejection. The TLS suite additionally covers retry,
exporters, resumption, concurrency and cleanup. A separate C fixture compares
native/JIT/NativeAOT output for the unchanged core's buffers, integer decoding,
base64, PEM files, memory callbacks and IP recognition.

The selected profile uses P-256, AES-128/256-GCM and SHA-256/384. Optional logging
and DTrace are disabled. TLS 1.2, 0-RTT and complete QUIC transport are outside
this acceptance profile. Windows, macOS and other architectures have not been
executed. No application-owned native TLS/crypto library is linked into the
managed product; native picotls/OpenSSL are separate test peers.

See [core test instructions](tests/README.md),
[provider and peer verification](managed/README.md), and
[the recorded acceptance evidence](../../edgcpp/evidence/picotls-tls-20261007.json).

## Review artifacts

Current generated source:
`edgcpp/artifacts/review/picotls/` (links to `picotls-edg-pruned-002/managed`).
The main protocol implementation is `Translated.source.picotls.c.cs`; public
record declarations are in `Translated.cs`, and exported constants are grouped
in `Translated.source.pembase64.c.cs`. The exact source index is
`dot-gen.sources.json`.
All 17 selected integral macros are public C# constants; their unused private
probe bodies and lifecycle wrappers are removed from packaged C#.

Persistent evidence lives under
`/mnt/storage/dotcc-edg-campaign-20261003/`:

- `picotls-edg-pruned-001/`: trimmed core translation and strict library build.
- `picotls-edg-pruned-002/`: regenerated core with constant macro probes removed.
- `picotls-edg-pruned-provider-002/`: provider JIT/AOT verification after probe removal.
- `picotls-edg-pruned-provider-001/`: provider projects, JIT/AOT tests and executables.
- `picotls-edg-pruned-tests-001/`: native differential tests and both peer receipts.
- `picotls-edg-004/`: retained complete-provider baseline for comparison.
- `picotls-inet-001/`: 1,186-case native/JIT/AOT `inet_pton` differential test.

Earlier attempts remain available alongside these directories.
