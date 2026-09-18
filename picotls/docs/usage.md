# Build and use the translated picotls campaign

The product is the pinned picotls core translated to C#, with a BCL crypto
provider and an owning managed connection API. It targets .NET 10 and NativeAOT.
The native OpenSSL adapter and independent `SslStream` peer are test oracles.

**Execution status:** the complete Linux x64 raw/optimized × JIT/NativeAOT
campaign passes, including actual ABI, provider/failure, upstream, TLS and
independent-peer checks. The dependency audit and broad compiler/SQLite
regressions also pass. Receipt: `artifacts/tests/PASS.json`, run
`artifacts/tests/run-_ofyv5ic`; detailed evidence is in [validation.md](validation.md).
Windows/macOS and other architectures remain unverified.

## Prepare, translate, build, test

Run commands serially from any working directory. These examples start in
`picotls/`; scripts resolve their own paths. The current executable campaign
profile is Linux x64 System V LP64. Native prerequisites are `cc`, CMake,
pkg-config and OpenSSL development libraries; managed prerequisites are the
repository's .NET 10 SDK and NativeAOT toolchain when requested.

```sh
./scripts/fetch.sh
./scripts/oracle.sh
./scripts/translate.sh
./scripts/build-only.sh
./scripts/test.sh
./scripts/test.sh --all --aot --runtime linux-x64
```

`fetch.sh` verifies pinned archives. `oracle.sh` explicitly prepares the native
reference libraries and runs its selected upstream tests. `translate.sh` builds
the compiler and semantic postprocessor, source-links the unchanged three core
units followed by the authored host adapter, emits the managed library, preserves
a raw snapshot and postprocesses the product in place. `--no-build-tools` reuses
already-built tools. `--no-fetch` uses the two pinned source directories already
under `ref/` instead of downloading their archives. Upstream source and generated
C# are never patched manually.

`build-only.sh` builds the existing provider/owning product project. `--core`
builds just the optimized translated project; `--raw` builds just its raw
counterpart. It performs no fetch, translation, tests or NativeAOT publish.

`test.sh` uses existing translations and prepared native reference inputs. It
builds test consumers, including a direct copied-source consumer, then runs
actual emitted ABI storage checks (92 native sizes/alignments/member offsets),
provider vectors, upstream test ports, the TLS
API suite, and the managed/native/independent process peers in that order. It
does not fetch or translate anything. Missing products, source provenance or
native oracle libraries cause a clear failure.

The default selects optimized JIT. `--raw` selects the raw product. `--all` runs
raw then optimized and compares deterministic suite output and peer public
results: acceptance/rejection, negotiated protocol/cipher/ALPN, application
payload length and digest. Handshake ciphertext, random secrets and generated
certificates are not parity inputs. Each vector executable checks its actual
values internally before reporting success. `--aot` additionally publishes and
runs each consumer and managed peer as NativeAOT, comparing the same results
with JIT. Other runtime/platform ABIs require their own native baseline before
this driver can accept them.

`PICOTLS_BUILD_TIMEOUT`, `PICOTLS_TEST_TIMEOUT` and `PICOTLS_PEER_TIMEOUT` set
positive limits in seconds (defaults: 900, 180 and 3600). Temporary files and
per-command stdout/stderr stay under a unique `artifacts/tests/run-*` directory.
The driver retains exact argv, exit status, timeout information, source hashes
and public results. It removes `artifacts/tests/PASS.json` before attempting a
new campaign and writes it only after every selected check and parity comparison
passes with unchanged inputs. Older run receipts remain historical evidence for
their recorded inputs.

## Consume the managed API

The generated wrapper is `Managed.Security.PicoTls`. Translation uses
`--nest-types --runtime=c`, matching SQLite's isolated source layout. Generated
types and runtime helpers are nested inside `PicoTls`; callback aliases are
file-local, so no `PicoTls.GlobalUsings.g.cs` is emitted. For direct unsafe API
types, add `using static Managed.Security.PicoTls;` to the individual consumer
files that need them, or qualify names such as `PicoTls.st_ptls_context_t` and
`PicoTls.Libc`.

To copy the translated core into another project, take the `.cs` files listed in
`generated/TranslatedPicotls/Dotcc.SourceFiles.txt`. Its manifest also removes the
old `Picotls.*` files when regenerating in place. The copied-source test compiles
the actual output in a consumer assembly with implicit usings disabled and
colliding consumer type names, under JIT and NativeAOT. The BCL provider/owning API
is supplied separately under `src/BclProvider/`.

Reference `src/BclProvider/BclProvider.csproj`; it references the generated
`TranslatedPicotls.csproj`. The public namespace is `Managed.Security`. The
[ManagedConsumer example](../ManagedConsumer/README.md) demonstrates the complete
transport loop and explicit certificate policy. Build it independently after
translation:

```sh
dotnet build ManagedConsumer/ManagedConsumer.csproj -c Release
dotnet publish ManagedConsumer/ManagedConsumer.csproj -c Release \
  -r linux-x64 -p:PublishAot=true -o build/managed-consumer-aot
```

Construct `PicotlsContext` with the required certificate verifier and signing
credentials, create a connection, and pass received TLS bytes to `Process`.
Forward its outbound ciphertext and consume its authenticated plaintext. Start
a client by processing empty input. Feed post-handshake records too. `Send`,
`UpdateKey`, `ExportSecret` and `CloseNotify` require the appropriate completed
handshake state; `CompleteInput` reports transport EOF. A `PicotlsException`
can carry alert bytes for the transport to forward.

Dispose connections, contexts, credential registrations and saved tickets.
Contexts lease credentials and remain alive for their existing connections after
their owner disposes them; disposal prevents new connections. Each connection
serializes its operations. Independent connections may share a context. Exported
ticket bytes are caller-owned secrets; clear the copy after use. Direct access
to the unsafe translated API requires a valid `CallbackScope`, retained callback
state and explicit storage/lifetime management.

The validated Linux x64 managed profile is TLS 1.3, P-256 ECDHE, AES-128-GCM/SHA-256 and
AES-256-GCM/SHA-384, with ECDSA P-256/SHA-256 and RSA-PSS/SHA-256 authentication.
The managed facade includes ALPN, mutual authentication, record processing,
exporters, key updates and opt-in tickets; these pass the complete campaign. HPKE remains in the translated source closure
but is not a public managed feature; TLS 1.2, 0-RTT and the native backend's wider
algorithm lists are outside this profile. The disposable offline certificate
fixtures explicitly choose `NoCheck` revocation; application verification policy
must be supplied by the application. No broader OS/architecture support is
established by Linux x64 evidence.

## Reproduce or upgrade the source

[inputs.json](../config/inputs.json) records upstream revisions, archive digests
and licenses. [core-sources.txt](../config/core-sources.txt) selects the unchanged
three upstream units; [core-defines.txt](../config/core-defines.txt) defines the
campaign configuration. [host-sources.txt](../config/host-sources.txt) separately
lists authored adapters. Translation provenance records those inputs, actual
core/header and adapter hashes, compiler/postprocessor assembly hashes, and every
manifest-owned raw/optimized generated file.

[core-wrappers.json](../config/core-wrappers.json) records the authored same-unit
QUIC ticket boundary around unchanged `lib/picotls.c`. It reuses the existing
session encoder at a completed handshake transcript and does not change TLS
state or add QUIC transport to the picotls facade. Native controls are reproducible
with `python3 scripts/test-quic-ticket-helper.py`; the separate MsQuic adapter's
raw/optimized JIT/NativeAOT tests validate its managed use.

For an upgrade, edit the pinned revision/archive hash deliberately, fetch it,
review upstream header/core changes and license notices, rerun the native oracle
and compiler/boundary probes, then translate, build and run `test.sh --all --aot`.
Resolve compiler gaps in the shared compiler and provider changes against actual
emitted types. Do not edit generated fields, paste offset tables or substitute
mirrored product types. Keep the raw snapshot for semantic postprocessor parity;
manifest cleanup removes only previous compiler-owned files. Preserve receipts
for each source/toolchain combination and update the executed validation record
only after the commands finish successfully.
