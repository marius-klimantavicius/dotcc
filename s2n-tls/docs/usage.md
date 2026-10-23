# Managed s2n-tls usage and qualification

The owning API is authored in `src/Managed.S2nTls`. It calls the actual
translated C engine in `Managed.Security.S2n.S2nTls`; it contains no replacement
TLS implementation. The frozen `223542` engine with the final ASN.1 provider repair passed all 1,076
consumer, compatibility, independent-peer and lifecycle rows across raw/processed
JIT/NativeAOT. Its separate copied-consumer and whole-library-rooted delivery
gate passed all 12 rows in fresh builds against provider source `0158d36c`.
Earlier provider receipts remain historical evidence. The original managed
upstream C and Python execution gates remain **unqualified** until their own
rows pass. A missing product or required
harness returns a failing exit code.

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
and native s2n/libssl/libcrypto product dependencies. All four external-copy JIT/AOT raw/processed delivery variants pass the three
loopbacks and notice, authored-reference and dependency audits. The independent
native oracles remain test-only executables.

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
`RequestKeyUpdate` requests the actual TLS 1.3 key-update behavior. This upstream
revision supports updating the local sending key; `requestPeerUpdate: true`
returns its original invalid-argument error.

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
forced negotiation, payload lengths and close-notify observation. The qualification
runner freezes both selected product trees before building. Each private JIT/AOT
build records the actual executable/assembly, dependency and provider output,
loaded runtime files, product snapshot hashes and current authored source hashes.
Each exchange captures the actual native endpoint and certificate/key/DH hashes
before execution and checks them again afterward. Historical discovery receipts
remain separate from fresh current-product executions. The lifecycle
runner has 30 named cases for ownership/freeze, cancellation, I/O and callback
drain/errors, distinct-connection concurrency, per-connection serialization,
malformed credentials, partial RC4/3DES/TLS1.3 I/O, GC-rooted callbacks,
asynchronous validation, authentication rejection/bypass, record corruption and
truncated EOF, reuse, session resumption, state transfer, TLS 1.3 key updates,
ALPN, allocation failures and initialization-failure quarantine. It uses transport
faults around the actual engine, without a mock TLS implementation. All 30 cases
pass in every required form/mode in
[`final-provider-asn1-main/report.json`](../artifacts/managed/final-provider-asn1-main/report.json),
whose complete consumer/compatibility/interop/lifecycle matrix passes 1,076 rows.
Fresh copied-consumer and rooted delivery evidence passes all 12 rows in
[`final-provider-asn1-delivery/report.json`](../artifacts/managed/final-provider-asn1-delivery/report.json).
The independent receipt audit checks the actual executable outputs, mapped runtime
files, product snapshots, authored sources and fixture/peer identities in
[`final-asn1-main-delivery-deep-audit.json`](../artifacts/managed-contract/final-asn1-main-delivery-deep-audit.json),
covering 5,023 distinct files. The fresh eight original CLI applications have
separate build/provider and actual host-runtime identities in
[`final-asn1-cli-eight-identity-proof.json`](../artifacts/managed-contract/final-asn1-cli-eight-identity-proof.json).
These builds reused the exact retained CLI/support C emissions and 175 engine
objects; no C regeneration was needed.
New runs isolate build intermediates per application as well as per form/mode.

Every one of the 644 upstream inventory entries is retained: 275 required
(257 C sources and 18 Python families), 287 explicit exclusions and 82 support
entries. The original record-stuffer test whose complete workload is disabled
with `#if 0` has an explicit source-checked exclusion. `tests/Upstream/build.py` reuses a
complete campaign object/tool snapshot, translates original C test/support units
and links private test executables. Prelude/host includes preserve assertions;
recv/shutdown/key-update kTLS regions additionally use the exact recorded outside-PLAN
feature guards in `tests/Upstream/test-adaptations.json`. Every guarded body and
assertion byte remains unchanged, with identical native control metadata.
The builder omits a core object only when compiler dependencies prove its entire
staged C implementation is explicitly included by the harness and all original
external roots remain in that actual harness object. `tests/Upstream/run.py` rejects missing
harnesses, changed inputs, skipped required capabilities, absent assertion counts
and differing native/translated counts. The original 18 Python families have
an endpoint adapter that preserves their test bodies and parameterization;
initial collection selected 22,195 parameter cases with no product capability vetoes.
The original SSLv2 ClientHello family has executed successfully; broader
execution and native comparison remain required. Peer command prefixes and assets come from
`build/test-prerequisites/peers.json`. Declared services start with private logs,
must reach socket readiness and terminate/drain in a `finally` path. Original
CLI argument parsing uses test-only GNU libc `getopt_long` bindings, including
its original long options, argument permutation and `optind` state. Standard
descriptor byte I/O and mixed standard/socket polling use test-only Linux host
services, with virtual socket handles borrowed from the actual embedded table.
The exact upstream `AvailablePorts(low, high)` constructor can receive explicit
`--port-low/--port-high` bounds; translated and native runs use disjoint ranges,
while the original Apache port 7777 remains exclusive during its family. Remaining
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

Use `run.py --c-only` to execute and compare the complete required C inventory
without launching Python controllers; its report explicitly covers C only.
To assemble the complete gate from already executed Python families, pass
`--python-report <translated-report>` and `--native-python-report <native-report>`.
Both options can be repeated for disjoint family reports, such as separate Apache
and non-Apache runs. The runner rejects duplicate families, mismatched execution
variants, incomplete parameter executions, failures/skips and undrained processes,
and compares the original native/translated parameters, exclusions and source
inventories. It never starts another Python controller when `--python-report`
is supplied.

`run.py --c-report <completed-provider-progress.json> --native-report <matching.json>
[--native-equivalence <proof.json>] [--campaign-run <immutable-run>]` consumes the
provider's completed 257-by-four original-C receipts read-only. It verifies the
original sources/assertions, native profile, application/runtime identities and
retained snapshots through `tests/Provider/required_c_receipt.py`; it launches no
C compiler, build or test controller. `--python-report` also accepts a durable
Python matrix report and selects its exact requested execution variant.
Native equivalence is required for controls retained from a different campaign.
The builder uses `scripts/translation_evidence.py` to select the closed
translation receipt and records its actual path and hash; a parent verification
can continue without changing that build-only receipt.

The repeatable Python driver is:

```bash
python3 -B s2n-tls/tests/Upstream/python-matrix.py \
  --executables s2n-tls/build/managed-upstream/python-endpoints.json \
  --peer-commands s2n-tls/build/test-prerequisites/peers-legacy-policy-v2.json \
  --psk-peer-commands s2n-tls/build/test-prerequisites/peers-psk-measured-v3.json \
  --python s2n-tls/build/python-integration/bin/python \
  --endpoint-kind translated --form all --mode all --output <matrix.json>
```

Each whole original family gets a separate retained leaf report. `--resume`
rechecks passed leaves; `--adopt-report` accepts already completed strict original
families after deep validation, including passing families retained in a failed
parent report. Only explicit `--retry-failed` starts another attempt, preserving
the old one. The default is one worker and no retry plugin. Apache uses an
exclusive port-7777 fixture lease. Both matrix and managed qualification entry
points forward repeatable Python report inputs instead of duplicating controllers.

`--available-port-probe` is an opt-in transport fixture adapter, enabled by the
matrix driver. It consumes the original finite `AvailablePorts` candidates in
order, probes kernel bind availability on IPv4 and IPv6 loopback, and removes
only actual `EADDRINUSE` candidates. Every candidate/result/errno and the original
method/adapter hashes are retained. It never listens, connects, cycles the
iterator or excludes a test parameter. A contract using real held IPv4 and IPv6
listeners proves the returned sequence equals the original sequence minus the
occupied ports in
[`available-port-probe-contract-r4.json`](../artifacts/managed-contract/available-port-probe-contract-r4.json).
Earlier strict failures at the resident service on port 27036 remain failures.

The process-control adapter recognizes the original readiness, send, close and
kill markers across consecutive reads from the same pipe. It does not combine
stdout and stderr or change test assertions and timeouts. Actual fragmented
native payload markers and a managed SEND marker passed focused original-node
replays. String markers retain the original comparison against `repr(bytes)`,
including the stripped byte representations used by original record-padding
tests; arbitrary plaintext escapes are not decoded. Fifty marker cases and five
additional scenarios on real OS pipes pass in
[`fragmented-control-markers-r4-v3.json`](../artifacts/managed-contract/fragmented-control-markers-r4-v3.json).
Two unchanged original record-padding cases also pass against the fresh provider
CLI, observing both actual stripped send and close markers, in
[`record-padding-marker-repair-proof.json`](../artifacts/managed-contract/record-padding-marker-repair-proof.json).
Those actual matches were not split; fragmented cases are proven separately by
the OS contract. Focused diagnostics never qualify a whole original family.
The earlier timeouts remain failed attempts.

The PSK-specific manifest keeps ordinary/version OpenSSL commands unchanged and
routes only `OpenSSL:openssl` to the separately labelled app whose unchanged
pinned TLS libraries support an explicitly selected PSK suite. The bounded helper
adaptation verifies the complete native proof, build inputs/outputs and actual
endpoint transcripts before bypassing only the original OpenSSL/SHA384 skip.
The original `None` skip, parameters, test bodies and assertions remain unchanged.
Fresh native execution passes all 1,999 cases, including all 351 previously
skipped cases, in
[`psk-measured-v3/full-family.json`](../artifacts/native-python/psk-measured-v3/full-family.json).

The original HRR client test's exact `curve == S2N_DEFAULT_CURVE` predicate marks
nine X25519 combinations inapplicable to an HRR assertion. Source and AST guards
record those nine node IDs, parameters and reasons as explicit inapplicable
exclusions. Before classification, a separate nine-control execution must prove
TLS 1.3, the exact suite, X25519, actual payload and absence of both HRR markers.
Those authored controls have separate counts and never count as original HRR
assertions. Native evidence passes nine controls and all 118 applicable unchanged
original assertions in
[`hrr-no-retry-r4/controls-nine.json`](../artifacts/native-python/hrr-no-retry-r4/controls-nine.json)
and [`original-applicable.json`](../artifacts/native-python/hrr-no-retry-r4/original-applicable.json).
Native and translated original inventories and inapplicable node metadata must
match. Other skips and every retry outcome remain failures.

The strict native comparator now passes all 18 original families and 22,186
applicable parameter cases in
[`final-eighteen-strict-comparator.json`](../artifacts/native-python/final-eighteen-strict-comparator.json),
after deep validation of 49,656 retained files. Its happy-path family executes
all 8,840 original cases without skips or retries. The nine original inapplicable
HRR cases and nine separate authored controls retain their distinct inventories.
This native result is independent of the managed 18-by-four Python matrix and
the managed 257-by-four C gate; neither is inferred from native success.

For a test-only provider surface, `build.py --provider-project <project.csproj>
--provider-binding <extra-binding.cs>` selects one frozen test assembly and
additional binding sources per harness. The binding option is repeatable.
The provider-owned snapshot helper retains the project, linked production
sources/resources, locks and test additions; its hashes are embedded in both
the executable manifest and actual application identity. The production provider
project and its 183-declaration profile remain separate from this test surface.
Alternatively, `--provider-policy <catalogue.json>` selects that surface only
for exact C cases listed in `bindings_by_case`; other cases use production.
The builder freezes the policy bytes and records its hash and selected entry.
Policy selection cannot be combined with explicit provider project/binding flags.

For a bounded replay, `python-integration.py --node 'file.py::test[parameters]'`
selects the exact unchanged original pytest node and freezes a new adapter/source
snapshot. The resulting `focused-original-python-integration` receipt leaves
whole-family qualification unrun and cannot be consumed as a complete family
by `run.py`. Explicit port bounds keep these replays separate from active native
and translated trials. Command provenance records the current pytest node and
hashes actual file arguments without treating long PSK values as filenames.
Final native and translated trials use explicit ranges 8000–18000 and
18001–30000, respectively, below the qualification host's ephemeral range.
Worker receipts record every unchanged allocator result and any ports reused by
different workers, so crossing an upstream worker's nominal stride is visible.

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
