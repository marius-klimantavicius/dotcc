# Managed s2n-tls usage and qualification

The owning API is authored in `src/Managed.S2nTls`. It calls the actual
translated C engine in `Managed.Security.S2n.S2nTls`; it contains no replacement
TLS implementation. The owning API and consumer compile against canonical raw,
and the lifecycle executable compiles against canonical processed. A bounded
native lifecycle control also passed. The first actual consumer run stopped at
`s2n_init` with `S2N_ERR_SAFETY` while provider initialization was gated; no
handshake executed. Managed TLS execution and
delivery remain **unqualified** until the complete product builds and the runtime
matrix passes. A missing product or required harness returns a failing exit code.

## Build and run

After the supervisor publishes the canonical products:

```bash
dotnet build s2n-tls/samples/ManagedConsumer/ManagedConsumer.csproj -c Release
dotnet run --project s2n-tls/samples/ManagedConsumer/ManagedConsumer.csproj -c Release -- \
  --fixtures "$PWD/s2n-tls/build/native/credentials" --source "$PWD/s2n-tls/ref/s2n-tls"
python3 s2n-tls/scripts/managed-qualification.py --suite consumer --form all --mode all
```

The consumer opens real TCP loopback sockets and runs SSLv3/RC4-MD5,
SSLv3/3DES-CBC-SHA and TLS 1.3/AES-128-GCM. Each endpoint asserts its negotiated
protocol, IANA ID and cipher name, then sends/verifies 1,027 binary bytes in three
application writes and completes bidirectional close-notify. It also checks that
the final runtime references and final-cleanup counts balance.

`S2nTlsProject` selects an ordinary project reference to the real raw or processed
product. Pass an absolute path when overriding it; the property is forwarded to
the owning project and its dependencies. The owning project reports a specific
build error when that file is absent.

```bash
dotnet build s2n-tls/samples/ManagedConsumer/ManagedConsumer.csproj -c Release \
  -p:S2nTlsProject="$PWD/s2n-tls/generated/TranslatedS2nTls.Raw/TranslatedS2nTls.csproj" \
  --artifacts-path "$PWD/s2n-tls/build/my-consumer"
```

The sample can be copied outside the repository. Set `S2nManagedProject` to the
absolute authored owning-project path, and `S2nTlsProject` to the chosen product.
`tests/Delivery/run.py` performs that external-copy build and real TLS execution,
with a normally trimmed consumer and a separate whole-library-rooted AOT run.
It audits the resulting source/dependency closure for replacement TLS engines
and native s2n/libssl/libcrypto product dependencies. The independent native
oracles remain test-only executables.

## Ownership and connection use

```csharp
using Managed.Security.S2n;

using var runtime = new S2nRuntime();
using var config = new S2nConfiguration(runtime);
config.AddTrustPem(File.ReadAllText("trusted-ca.pem"));
await using var client = new S2nConnection(config, S2nRole.Client, connectedSocket,
    new S2nPolicy(S2nProtocol.Ssl3, S2nProtocol.Ssl3, 0x0004));
client.SetServerName("localhost");
await client.NegotiateAsync(cancellationToken);
S2nNegotiated actual = client.Negotiated;
// SendAsync may return a short count. ReceiveAsync returns zero on close-notify.
int sent = await client.SendAsync(binaryMessage, cancellationToken);
int received = await client.ReceiveAsync(receiveBuffer, cancellationToken);
await client.ShutdownAsync(cancellationToken);
```

The default configuration permits SSLv3 through TLS 1.3, all 36 applicable cipher
suites and the adapter's 17 classical signature schemes, without a test-mode
environment variable or separate legacy switch. Certificate verification stays
enabled. Omitting the connection policy preserves the configuration's selected
policy. Supplying `S2nPolicy` narrows versions and optionally forces one IANA suite;
zero retains the compatible suite set. Protocol enum values come from the actual
emitted `S2N_*` constants.

For servers, construct `S2nCredentials(runtime, certificateChainPem, privateKeyPem)`
and call `config.AddCredentials(credentials)`. Configurations retain credentials;
connections retain their configuration. Public owners may be disposed earlier
without invalidating those retained objects. Configurations freeze on their first
connection creation and reject subsequent mutation. Parsed credential handles are
immutable. Temporary managed private-key byte copies are cleared after loading;
the caller still owns its original PEM string.

Each connection owns its narrowed policy until after `s2n_connection_free`.
Callback roots are released after entry and asynchronous certificate callbacks
drain. Configurations and credentials release their runtime leases only after
their C objects are freed. The last lease calls **`s2n_cleanup_final`**. Upstream
`s2n_cleanup` is a retained no-op and is not the finalization mechanism.

The runtime is one shared translated global domain. It serializes initialization
and cleanup, while distinct connections can enter C concurrently. Calling raw
initialization/cleanup or freeing borrowed handles outside this ownership model
invalidates its reference counts. Initialization or cleanup failure quarantines
the domain and rejects further acquisition; partially initialized upstream state
is not advertised as recoverable.

`S2nRuntimeOptions(CallerOwnsCryptoInitialization: true)` invokes
`s2n_crypto_disable_init` before the first initialization; the caller must have
initialized the configured provider. Crypto initialization ownership cannot change
later in the process. The native probe confirms `crypto_initialized` remains set
after final cleanup, even though ordinary global initialization can restart.
Translated restart behavior still requires the managed runtime tests.

All operations on one connection are serialized, including time spent waiting
for I/O. Different connections have independent entry gates. `Reset` reuses a
connection after a completed TLS shutdown, reapplies its config/policy/I/O and
returns it to setup state. Set the server name and other per-connection overrides
again before the next negotiation. The transport must supply the next peer.

## Transport, callbacks and cancellation

`S2nSocketTransport` uses BCL nonblocking socket reads/writes in actual translated
send/receive callbacks. Would-block sets the embedded `S2nTls.Libc.errno` to
`EAGAIN`; other transport exceptions are contained, mapped to `EIO` and rethrown
on the owning operation. Readiness probes consume no encrypted bytes. Short
reads/writes, EOF and cancellation are supplied to the translated state machine.

Custom `IS2nTransport` implementations must return promptly, report would-block
explicitly and honor cancellation in `WaitAsync`. A zero receive means transport
EOF; s2n decides whether close-notify was received. A zero-length application
buffer does not establish EOF.

Cancellation while queued leaves the connection available. Cancellation after C
entry makes it terminal, preventing a partially completed send from being retried
with a different buffer. `DisposeAsync` cancels pending waits, acquires/drains the
entry gate, drains an outstanding certificate-validation task and then frees C
objects and roots. The socket overload owns its transport wrapper; `ownsSocket:
false` leaves the caller's socket open and restores its original blocking mode.
Disposal does not manufacture a clean TLS shutdown; call `ShutdownAsync` first
when close-notify is required.

Hostname, key-log, renegotiation and certificate-validation delegates remain
rooted through their last connection. Callback exceptions never unwind through
translated C. Callbacks must not reenter or dispose a connection. Asynchronous
certificate validators receive a cancellation token and must finish when it is
cancelled; disposal waits for their completion. Their continuations never receive
or retain a C validation pointer. Reentry protection follows asynchronous callback
continuations as well as synchronous callbacks.

Connections select self-service blinding and honor `s2n_connection_get_delay`
before reporting a terminal error or freeing the connection. Cancellation does
not shorten that delay. `SetBlinding(BuiltIn)` exposes upstream's built-in behavior;
its synchronous sleep and synchronous crypto execution cannot be interrupted by
an asynchronous cancellation token. Error tests explicitly select zero maximum
blinding delay where their specified behavior needs immediate failure.

## Compatibility controls

The following methods preserve the upstream controls in
`config/compatibility.json`. Presence in the API is not a claim that its required
behavior row has already executed.

| Upstream behavior | Owning API |
| --- | --- |
| X.509 verification, time and intent exceptions | `DisableCertificateVerification`, `DisableCertificateTimeVerification`, `DisableCertificateIntentVerification` |
| IP-in-CN fallback without SAN | `AllowIpInCommonName` |
| Stapled OCSP and maximum chain depth | `CheckStapledOcsp`, `SetMaximumCertificateDepth` |
| Config and connection hostname verification | `SetHostnameVerifier` on the respective owner |
| Config and connection named security policies | `SetNamedPolicy` on the respective owner |
| Blinding mode and maximum delay | `connection.SetBlinding`, `config.SetMaximumBlindingDelay` |
| Legacy DSS type in CertificateRequest | `EnableLegacyDssCertificateRequest` |
| Config and connection client authentication | `SetClientAuthentication` on the respective owner |
| Warning-alert and verify-after-sign behavior | `IgnoreWarningAlerts`, `VerifyAfterSigning` |
| Explicit session-secret logging | `SetKeyLogger` |
| Legacy NPN and ALPN preferences | `EnableNpn`, `SetApplicationProtocols` |
| Connection-state serialization | `EnableSerialization`, `ExportUnprotectedState`, `ImportUnprotectedState` |
| Additional synchronous/asynchronous certificate decision | `SetCertificateValidator` |
| Caller crypto init, atexit disabling, real final cleanup | `S2nRuntimeOptions` and the reference-counted runtime |
| Upstream renegotiation-request decision | `SetRenegotiationHandler`; low-level renegotiation calls remain on `S2nTls` |
| Retained random callback API | Raw `s2n_rand_set_callbacks` is a no-op; actual entropy remains at the provider RAND/OS boundary |
| kTLS unsafe TLS 1.3 control | Explicitly excluded by the selected product plan |

Serialized connection state contains plaintext traffic keys without intrinsic
integrity protection. The caller must authenticate and protect it. Upstream
rejects RC4 serialization and buffered I/O; those checks remain in C. Successful
export closes the source connection; import reapplies this owner's config/policy.
`ExportSession`/`ImportSession` expose upstream session blobs separately, and
`RequestKeyUpdate` requests the actual TLS 1.3 key-update behavior.

Low-level declarations are public nested types/functions on `S2nTls`. In
particular, `s2n_get_openssl_version` describes the selected OpenSSL declaration
ABI, not a claim that native OpenSSL supplies product crypto. Provider identity
and capabilities have their separate managed boundary contract.

## Qualification commands and current limits

```bash
python3 s2n-tls/scripts/managed-qualification.py --suite compatibility --form all --mode all
python3 s2n-tls/scripts/managed-qualification.py --suite interop --form all --mode all
python3 s2n-tls/scripts/managed-qualification.py --suite lifecycle --form all --mode all
python3 s2n-tls/scripts/managed-qualification.py --suite delivery --form all --mode all
python3 s2n-tls/scripts/managed-qualification.py --suite rooted-aot --form all --mode aot
```

Compatibility runs all 78 applicable version/suite pairs in both managed roles,
plus validating rejection and explicit bypass controls. Interop uses the existing
proven native OpenSSL endpoints in both roles for every pair, including the
separate SSLv3 endpoint. Each process preserves its transcript, exit status,
forced negotiation, payload lengths and close-notify observation. The lifecycle
runner has 30 named cases for ownership/freeze, cancellation, I/O and callback
drain/errors, distinct-connection concurrency, per-connection serialization,
malformed credentials, partial RC4/3DES/TLS1.3 I/O, GC-rooted callbacks,
asynchronous validation, authentication rejection/bypass, record corruption and
truncated EOF, reuse, session resumption, state transfer, TLS 1.3 key updates,
ALPN, allocation failures and initialization-failure quarantine. It uses transport faults around the actual engine, without a mock
TLS implementation. These executable cases are currently **unrun**.

Every one of the 644 upstream inventory entries is retained: 276 required,
286 explicit exclusions and 82 support entries. `tests/Upstream/build.py` reuses a
complete campaign object/tool snapshot, translates original C test/support units
and links private test executables. It adds only prelude and host declaration includes to
test source, preserving assertions. `tests/Upstream/run.py` rejects missing
harnesses, changed inputs, skipped required capabilities, absent assertion counts
and differing native/translated counts. The original 18 Python families have
an endpoint adapter that preserves their test bodies and parameterization;
collection selected 22,195 parameter cases with no product capability vetoes.
They remain unrun. Peer command prefixes and assets come from
`build/test-prerequisites/peers.json`. Declared services start with private logs,
must reach socket readiness and terminate/drain in a `finally` path. Original
CLI argument parsing uses test-only GNU libc `getopt_long` bindings, including
its original long options, argument permutation and `optind` state. Remaining
behavior and upstream execution rows still require actual assertions; source
availability and compilation do not close them.

```bash
python3 s2n-tls/tests/Upstream/build.py \
  --campaign-run s2n-tls/build/campaign/<complete-run> \
  --output s2n-tls/build/managed-upstream/executables.json
python3 s2n-tls/tests/Upstream/run.py --form raw --mode jit \
  --native-report s2n-tls/artifacts/native/<matching-run>/report.json \
  --output s2n-tls/artifacts/my-upstream.json
```

The bounded native lifecycle evidence is
[`20261023T173507Z-c95586e4/report.json`](../artifacts/managed-native-lifecycle/20261023T173507Z-c95586e4/report.json).
It uses cached adapted native libraries and does not rebuild the native baseline.
Static emitted-call audit results are in
[`complete-emission.json`](../artifacts/managed-contract/complete-emission.json).
The non-executable reference compilation checks authored consumer/lifecycle code
against signatures extracted from the real linked output; it supplies no runtime
implementations and establishes no TLS or layout evidence.

Reproduction tools accept absolute script paths from another working directory.
`managed-qualification.py --restore normal|locked|none` controls managed restore
separately from native fixture/source acquisition. All specialist build outputs
use private paths and no specialist rebuilds the DotCC toolchain. The supervisor
retains the shared `scripts/qualification.py` integration point, plan and acceptance
ledgers. Only actual executed raw/processed JIT/AOT Linux-x64 results can close the
corresponding product rows.
