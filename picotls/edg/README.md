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
`edgcpp/artifacts/review/picotls/` (links to the final `picotls-edg-004/managed`).
The main protocol implementation is `Translated.source.picotls.c.cs`; public
record declarations are in `Translated.cs`, and exported constants are grouped
in `Translated.source.pembase64.c.cs`. The exact source index is
`dot-gen.sources.json`.

Persistent evidence lives under
`/mnt/storage/dotcc-edg-campaign-20261003/`:

- `picotls-edg-004/`: final core translation and strict library build.
- `picotls-edg-provider-002/`: provider projects, JIT/AOT tests and executables.
- `picotls-edg-tests-002/`: native differential tests and both peer receipts.
- `picotls-inet-001/`: 1,186-case native/JIT/AOT `inet_pton` differential test.

Earlier attempts remain available alongside these directories.
