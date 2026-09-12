# First managed API profile

This is the selected scope for `managed-linux-x64-quic-v1`, pinned to
`80a065112426bce68c1da42d026478d3e40fd45e`. It is an implementation contract, **not a claim that any managed API is ready**.
Every selected operation remains pending its transport, lifetime, error-path and raw/optimized JIT/NativeAOT gates.
The [machine-readable profile](../config/api-profile.json) contains every entry from the
[source inventory](../config/public-api-inventory.json), source locations, direction-specific behavior, settings masks and capability policy.

The inventory has **39 API-table slots and 72 parameter identifiers**. Of the latter, **63 are actual parameters**, eight are namespace prefixes, and one is a priority modifier. Prefix values overlap valid first parameters: `QUIC_PARAM_PREFIX_GLOBAL` equals `QUIC_PARAM_GLOBAL_RETRY_MEMORY_PERCENT`. Classify identifiers for documentation, but dispatch full numeric IDs and handle scopes; a prefix blacklist would reject valid operations.

## Selection and enforcement

`selected-pending` means required or accepted in the first profile after qualification. `deferred` means a possible later feature, with an explicit first-profile rejection. `excluded` means a native-provider or insecure facility outside this host. `syntax-only` marks namespace constants rather than operations. None means implemented.

The owning facade must reject recognized deferred/excluded operations and unsupported option values before dispatch or mutation: `QUIC_STATUS_NOT_SUPPORTED` at status-bearing C boundaries, or `NotSupportedException` in C#. Unknown IDs, wrong scope/direction, reserved bits and malformed sizes produce `QUIC_STATUS_INVALID_PARAMETER`; selected calls retain upstream state and buffer-size errors. Validate an entire settings/credential payload before forwarding it. Unsupported void-returning slots are not exposed: do not invent a C return status or a successful no-op.

This policy is **not yet wired into the generated core or owning facade**. The raw table keeps its upstream ABI shape and may contain compiled optional operations. Callers must not treat raw-table presence as public managed support. Runtime enforcement and negative tests remain required before exposure.

## Required transport and optional facilities

QUIC v1 over IPv4/IPv6 UDP, client/server operation, concurrent bidirectional/unidirectional streams, reliable reset/FIN through baseline shutdown operations, backpressure, QUIC DATAGRAM, and ticket resumption with fresh 1-RTT data are selected. Here baseline stream reset does not mean the optional reliable-reset-offset extension. Core recovery, CUBIC, retry, stateless reset, key updates, CID rotation and NAT rebinding/path validation remain required. Missing required BCL binding/source-selection behavior blocks completion.

0-RTT, QUIC v2, BBR qualification, multipath, client authentication, native credential APIs, ECN/DSCP controls, RSS, UDP segmentation/coalescing, hardware encryption offload, raw datapath, TCP/QTIP, XDP, io_uring, RIO, external worker queues and persistent platform storage are not advertised. HTTP/3, WebTransport and native binary exports are also outside the first profile. Ordinary core scheduling and stream priority do not imply OS affinity or an external execution API.

Capability queries must distinguish **compiled**, **qualified**, and **negotiated** behavior. The upstream supported-version getter reports the compiled version list; the owning facade separately identifies its v1-only qualification and restricts all effective version settings to v1. `CONN_DATAGRAM_SEND_ENABLED`, negotiated ALPN and handshake queries report actual peer/session state. Host datapath feature bits remain false for absent facilities. A declaration or compiled algorithm never qualifies a capability.

`QUIC_TLS_PROVIDER` defines only Schannel and OpenSSL upstream. The managed host explicitly extends that ABI with `MSQUIC_HOST_TLS_PROVIDER_PICOTLS = (QUIC_TLS_PROVIDER)0x10000`; the selected provider query returns this value after the host identity test passes. It must not impersonate OpenSSL.

## API-table slots

Indices preserve the inventoried preview-enabled table order. All rows currently have unqualified implementation status. Base lifecycle/transport slots are selected; preview partition/pool/receive-buffer/external-loop/exporter slots remain deferred.

| Index | Slot | Policy | Required behavior or reason |
| --- | --- | --- | --- |
| 0 | `SetContext` | selected-pending | Facade owns rooted contexts; no movable managed object pointer in C storage. |
| 1 | `GetContext` | selected-pending | Return only the owning facade context association with lifetime checks. |
| 2 | `SetCallbackHandler` | selected-pending | Static AOT-compatible callbacks with exact signature; replacement follows upstream handle/state rules. |
| 3 | `SetParam` | selected-pending | Validate profile policy, directions, flags and entire setting payload before forwarding; no partial mutation on profile rejection. |
| 4 | `GetParam` | selected-pending | Apply per-parameter query policy, report actual state and upstream size/state errors; never infer qualification from compiled fields. |
| 5 | `RegistrationOpen` | selected-pending | Default low-latency profile only initially; reject unqualified execution profiles. |
| 6 | `RegistrationClose` | selected-pending | Drain all child handles off callback/worker threads; upstream close may block. |
| 7 | `RegistrationShutdown` | selected-pending | Retain upstream transport, callback, state and handle ownership semantics; selected first-profile runtime gate remains pending. |
| 8 | `ConfigurationOpen` | selected-pending | QUIC v1 only; validate the complete settings mask and ALPN lifetime. |
| 9 | `ConfigurationClose` | selected-pending | Retain upstream transport, callback, state and handle ownership semantics; selected first-profile runtime gate remains pending. |
| 10 | `ConfigurationLoadCredential` | selected-pending | Only explicitly owned managed credentials and real trust/name validation under credential_policy; reject native credential handles and insecure flags. |
| 11 | `ListenerOpen` | selected-pending | Retain upstream transport, callback, state and handle ownership semantics; selected first-profile runtime gate remains pending. |
| 12 | `ListenerClose` | selected-pending | Retain upstream transport, callback, state and handle ownership semantics; selected first-profile runtime gate remains pending. |
| 13 | `ListenerStart` | selected-pending | Retain upstream transport, callback, state and handle ownership semantics; selected first-profile runtime gate remains pending. |
| 14 | `ListenerStop` | selected-pending | Retain upstream transport, callback, state and handle ownership semantics; selected first-profile runtime gate remains pending. |
| 15 | `ConnectionOpen` | selected-pending | Retain upstream transport, callback, state and handle ownership semantics; selected first-profile runtime gate remains pending. |
| 16 | `ConnectionClose` | selected-pending | Retain upstream transport, callback, state and handle ownership semantics; selected first-profile runtime gate remains pending. |
| 17 | `ConnectionShutdown` | selected-pending | Retain upstream transport, callback, state and handle ownership semantics; selected first-profile runtime gate remains pending. |
| 18 | `ConnectionStart` | selected-pending | Retain upstream transport, callback, state and handle ownership semantics; selected first-profile runtime gate remains pending. |
| 19 | `ConnectionSetConfiguration` | selected-pending | Retain upstream transport, callback, state and handle ownership semantics; selected first-profile runtime gate remains pending. |
| 20 | `ConnectionSendResumptionTicket` | selected-pending | Preserve ticket envelope and callback ownership; do not enable 0-RTT. |
| 21 | `StreamOpen` | selected-pending | Retain upstream transport, callback, state and handle ownership semantics; selected first-profile runtime gate remains pending. |
| 22 | `StreamClose` | selected-pending | Retain upstream transport, callback, state and handle ownership semantics; selected first-profile runtime gate remains pending. |
| 23 | `StreamStart` | selected-pending | Retain upstream transport, callback, state and handle ownership semantics; selected first-profile runtime gate remains pending. |
| 24 | `StreamShutdown` | selected-pending | Retain upstream transport, callback, state and handle ownership semantics; selected first-profile runtime gate remains pending. |
| 25 | `StreamSend` | selected-pending | Preserve buffer lifetime until completion; apply api_flags.send and reject unqualified early-data/optional send flags. |
| 26 | `StreamReceiveComplete` | selected-pending | Complete only outstanding received bytes; preserve backpressure and callback lifetime. |
| 27 | `StreamReceiveSetEnabled` | selected-pending | Preserve upstream flow-control/backpressure transitions. |
| 28 | `DatagramSend` | selected-pending | Require negotiated datagram capability and payload limit; preserve completion/cancellation ownership. |
| 29 | `ConnectionResumptionTicketValidationComplete` | selected-pending | Complete actual pending ticket validation exactly once; no 0-RTT acceptance policy. |
| 30 | `ConnectionCertificateValidationComplete` | selected-pending | Only the owning validator completes a pending validation with the actual trust/name result and TLS alert. |
| 31 | `ConnectionOpenInPartition` | deferred | Explicit worker partition assignment is deferred. |
| 32 | `StreamProvideReceiveBuffers` | deferred | Application-owned receive-buffer provisioning is deferred; baseline receive-complete/backpressure remains selected. |
| 33 | `ConnectionPoolCreate` | deferred | Native preview pooled-connection/partition API is deferred; the owning facade manages individual connections. |
| 34 | `ExecutionCreate` | deferred | External event-loop execution is deferred; retain translated platform_worker.c with managed PAL services. |
| 35 | `ExecutionDelete` | deferred | No external execution context is exposed or created in this profile. |
| 36 | `ExecutionPoll` | deferred | No external polling loop is exposed in this profile. |
| 37 | `RegistrationClose2` | deferred | Preview callback-based registration close is deferred; facade drains child objects and uses baseline close off callbacks. |
| 38 | `ConnectionExportKeyingMaterial` | deferred | TLS exporter API is deferred beyond transport authentication/resumption requirements. |

`MsQuicOpenVersion`/`MsQuicClose` are entry points outside these 39 slots. The owning lifetime layer must also qualify their API-version-2 reference-counting and initialization/unload contract. “QUIC v1” here is the transport version, not obsolete `QUIC_API_VERSION_1`.

## Parameter identifiers

`r`/`w` indicate the pinned native directions, not present managed availability. On selected rows, unsupported directions reject as invalid; deferred/excluded rows reject both queries and setters as not supported. The release retry-secret getter is the explicit exception noted below. Full setter/getter policies and source references are in JSON.

| Identifier | Native direction / kind | Policy | Constraint or reason |
| --- | --- | --- | --- |
| `QUIC_PARAM_PREFIX_GLOBAL` | namespace-prefix | syntax-only | Decode by full parameter ID and handle scope. Prefix values may equal valid parameter IDs; never blacklist this numeric value as a prefix. |
| `QUIC_PARAM_PREFIX_REGISTRATION` | namespace-prefix | syntax-only | Decode by full parameter ID and handle scope. Prefix values may equal valid parameter IDs; never blacklist this numeric value as a prefix. |
| `QUIC_PARAM_PREFIX_CONFIGURATION` | namespace-prefix | syntax-only | Decode by full parameter ID and handle scope. Prefix values may equal valid parameter IDs; never blacklist this numeric value as a prefix. |
| `QUIC_PARAM_PREFIX_LISTENER` | namespace-prefix | syntax-only | Decode by full parameter ID and handle scope. Prefix values may equal valid parameter IDs; never blacklist this numeric value as a prefix. |
| `QUIC_PARAM_PREFIX_CONNECTION` | namespace-prefix | syntax-only | Decode by full parameter ID and handle scope. Prefix values may equal valid parameter IDs; never blacklist this numeric value as a prefix. |
| `QUIC_PARAM_PREFIX_TLS` | namespace-prefix | syntax-only | Decode by full parameter ID and handle scope. Prefix values may equal valid parameter IDs; never blacklist this numeric value as a prefix. |
| `QUIC_PARAM_PREFIX_TLS_SCHANNEL` | namespace-prefix | syntax-only | Decode by full parameter ID and handle scope. Prefix values may equal valid parameter IDs; never blacklist this numeric value as a prefix. |
| `QUIC_PARAM_PREFIX_STREAM` | namespace-prefix | syntax-only | Decode by full parameter ID and handle scope. Prefix values may equal valid parameter IDs; never blacklist this numeric value as a prefix. |
| `QUIC_PARAM_HIGH_PRIORITY` | priority-modifier | selected-pending | Mask for dispatch and preserve upstream operation priority; does not bypass profile restrictions. Bare modifier is invalid. |
| `QUIC_PARAM_GLOBAL_RETRY_MEMORY_PERCENT` | rw | selected-pending | Upstream retry resource threshold; preserve range and pre-initialization restrictions. |
| `QUIC_PARAM_GLOBAL_SUPPORTED_VERSIONS` | r | selected-pending | Returns actual compiled version list; not a managed qualification list. Effective connection/configuration version lists must contain only QUIC v1. |
| `QUIC_PARAM_GLOBAL_LOAD_BALACING_MODE` | rw | selected-pending | Only QUIC_LOAD_BALANCING_DISABLED; reject server-ID/IP/fixed load-balancing modes. |
| `QUIC_PARAM_GLOBAL_PERF_COUNTERS` | r | selected-pending | Actual translated counters with upstream output-size negotiation; unavailable measurements must not be invented. |
| `QUIC_PARAM_GLOBAL_LIBRARY_VERSION` | r | selected-pending | Report pinned upstream library version; distinguish owning-facade version separately. |
| `QUIC_PARAM_GLOBAL_SETTINGS` | rw | selected-pending | Apply settings_policy to every set IsSet bit, then upstream validation; get actual effective settings. |
| `QUIC_PARAM_GLOBAL_GLOBAL_SETTINGS` | rw | selected-pending | RetryMemoryLimit allowed; LoadBalancingMode only DISABLED; FixedServerID and reserved bits rejected. |
| `QUIC_PARAM_GLOBAL_VERSION_SETTINGS` | rw | selected-pending | Acceptable/Offered/FullyDeployed version lists restricted to QUIC v1; no QUIC v2 negotiation. |
| `QUIC_PARAM_GLOBAL_LIBRARY_GIT_HASH` | r | selected-pending | Report the actual pinned source revision. |
| `QUIC_PARAM_GLOBAL_EXECUTION_CONFIG` | rw | deferred | External execution, processor affinity and polling configuration are not exposed; retain internal upstream worker policy. |
| `QUIC_PARAM_GLOBAL_TLS_PROVIDER` | r | selected-pending | Return MSQUIC_HOST_TLS_PROVIDER_PICOTLS = (QUIC_TLS_PROVIDER)0x10000, an explicit managed-host ABI extension. Never report SCHANNEL or OPENSSL for this provider. |
| `QUIC_PARAM_GLOBAL_STATELESS_RESET_KEY` | w | selected-pending | 32-byte key provisioning through translated core; do not expose key material through GetParam. |
| `QUIC_PARAM_GLOBAL_STATISTICS_V2_SIZES` | r | selected-pending | Report sizes actually supported by this compiled statistics layout, not native-platform guesses. |
| `QUIC_PARAM_GLOBAL_STATELESS_RETRY_CONFIG` | w; r only in DEBUG builds | selected-pending | Retain upstream AES-GCM key/rotation validation for setters. Secret-bearing getter is DEBUG-only upstream and NOT_SUPPORTED in this NDEBUG first profile. |
| `QUIC_PARAM_GLOBAL_XDP_MAP_CONFIG` | rw | excluded | Native XDP maps are outside the BCL UDP host. |
| `QUIC_PARAM_CONFIGURATION_SETTINGS` | rw | selected-pending | Apply settings_policy and preserve upstream configuration state rules. |
| `QUIC_PARAM_CONFIGURATION_TICKET_KEYS` | w | selected-pending | Map explicit ticket keys into the picotls ticket-envelope provider; preserve ownership, rotation and failure behavior. |
| `QUIC_PARAM_CONFIGURATION_VERSION_SETTINGS` | rw | selected-pending | All effective version lists restricted to QUIC v1. |
| `QUIC_PARAM_CONFIGURATION_SCHANNEL_CREDENTIAL_ATTRIBUTE_W` | w | excluded | Schannel credential handles/attributes are not managed picotls credentials. |
| `QUIC_PARAM_LISTENER_LOCAL_ADDRESS` | r | selected-pending | Report actual bound IPv4/IPv6 address, selected interface and port. |
| `QUIC_PARAM_LISTENER_STATS` | r | selected-pending | Actual listener acceptance/rejection/drop counters. |
| `QUIC_PARAM_LISTENER_CIBIR_ID` | rw | deferred | Custom connection-ID routing is not needed for core-managed CID rotation or NAT rebinding. |
| `QUIC_PARAM_LISTENER_PARTITION_INDEX` | rw | deferred | Explicit listener/worker partition affinity requires separate host qualification. |
| `QUIC_PARAM_DOS_MODE_EVENTS` | rw | selected-pending | Preserve upstream notification and retry-mode transitions; no managed substitute for core retry policy. |
| `QUIC_PARAM_CONN_QUIC_VERSION` | r | selected-pending | Report actual negotiated version; a qualified first-profile connection must use v1. |
| `QUIC_PARAM_CONN_LOCAL_ADDRESS` | rw | selected-pending | Preserve upstream state rules and validated IPv4/IPv6 source/bind selection; unqualified required BCL behavior blocks the profile. |
| `QUIC_PARAM_CONN_REMOTE_ADDRESS` | rw | selected-pending | Preserve upstream client/start-state restrictions and actual peer address updates. |
| `QUIC_PARAM_CONN_IDEAL_PROCESSOR` | r | selected-pending | Report actual upstream logical worker assignment; do not imply OS thread affinity. |
| `QUIC_PARAM_CONN_SETTINGS` | rw | selected-pending | Apply settings_policy and upstream per-connection mutability rules. |
| `QUIC_PARAM_CONN_STATISTICS` | r | selected-pending | Actual legacy core statistics with upstream buffer-size rules. |
| `QUIC_PARAM_CONN_STATISTICS_PLAT` | r | selected-pending | Actual platform-clock statistics; host clock is monotonic microseconds and CxPlatTimeUs64ToPlat is identity. |
| `QUIC_PARAM_CONN_SHARE_UDP_BINDING` | rw | selected-pending | Retain requested upstream binding semantics; actual same-port/wildcard integration must qualify before advertising support. |
| `QUIC_PARAM_CONN_LOCAL_BIDI_STREAM_COUNT` | r | selected-pending | Actual peer-advertised remaining local bidirectional stream capacity. |
| `QUIC_PARAM_CONN_LOCAL_UNIDI_STREAM_COUNT` | r | selected-pending | Actual peer-advertised remaining local unidirectional stream capacity. |
| `QUIC_PARAM_CONN_MAX_STREAM_IDS` | r | selected-pending | Actual core-maintained stream limits. |
| `QUIC_PARAM_CONN_CLOSE_REASON_PHRASE` | rw | selected-pending | Preserve byte length, termination and shutdown state validation. |
| `QUIC_PARAM_CONN_STREAM_SCHEDULING_SCHEME` | rw | selected-pending | Upstream FIFO/round-robin stream scheduling; no competing managed stream scheduler. |
| `QUIC_PARAM_CONN_DATAGRAM_RECEIVE_ENABLED` | rw | selected-pending | Selected QUIC DATAGRAM behavior; preserve pre-start negotiation constraints. |
| `QUIC_PARAM_CONN_DATAGRAM_SEND_ENABLED` | r | selected-pending | Actual peer-negotiated send capability; false until negotiation permits datagrams. |
| `QUIC_PARAM_CONN_DISABLE_1RTT_ENCRYPTION` | rw | excluded | Insecure transport bypass is excluded; no setter or getter exposure through the first owning facade. |
| `QUIC_PARAM_CONN_RESUMPTION_TICKET` | w | selected-pending | Accept the upstream MsQuic ticket envelope and use fresh 1-RTT application data only; preserve pre-start validation. |
| `QUIC_PARAM_CONN_PEER_CERTIFICATE_VALID` | w | deferred | Legacy certificate-validation completion parameter is hidden; use the typed completion operation through the owning validator. |
| `QUIC_PARAM_CONN_LOCAL_INTERFACE` | w | selected-pending | Client pre-start interface selection with actual BCL IPv4/IPv6 binding; required missing support is a blocker. |
| `QUIC_PARAM_CONN_TLS_SECRETS` | w | deferred | Diagnostic key-log pointer ownership/lifetime needs separate opt-in API; no raw secret export in the first facade. |
| `QUIC_PARAM_CONN_VERSION_SETTINGS` | rw | selected-pending | All effective version lists restricted to QUIC v1. |
| `QUIC_PARAM_CONN_CIBIR_ID` | rw | deferred | Custom connection-ID routing is deferred; ordinary upstream CID rotation remains required. |
| `QUIC_PARAM_CONN_STATISTICS_V2` | r | selected-pending | Actual size-versioned core counters; do not fabricate unavailable ECN/TTL/platform observations. |
| `QUIC_PARAM_CONN_STATISTICS_V2_PLAT` | r | selected-pending | Actual platform-clock variant with the documented monotonic-microsecond host clock. |
| `QUIC_PARAM_CONN_ORIG_DEST_CID` | r | selected-pending | Read actual original destination CID with upstream availability and length checks. |
| `QUIC_PARAM_CONN_SEND_DSCP` | rw | deferred | Per-send DSCP behavior has not been qualified in the BCL datapath; effective initial marking remains zero. |
| `QUIC_PARAM_CONN_NETWORK_STATISTICS` | r | selected-pending | Actual translated congestion/network statistics, without claiming extra datapath telemetry. |
| `QUIC_PARAM_CONN_CLOSE_ASYNC` | rw | deferred | Preview native async-close parameter is hidden; first facade supplies ownership and draining over baseline close/shutdown operations. |
| `QUIC_PARAM_TLS_HANDSHAKE_INFO` | r | selected-pending | Populate from the real picotls negotiated version/cipher/group and report only actual handshake results. |
| `QUIC_PARAM_TLS_NEGOTIATED_ALPN` | r | selected-pending | Return actual negotiated ALPN bytes with upstream output-size behavior. |
| `QUIC_PARAM_TLS_SCHANNEL_CONTEXT_ATTRIBUTE_W` | r | excluded | No Schannel security context exists in the managed picotls provider. |
| `QUIC_PARAM_TLS_SCHANNEL_CONTEXT_ATTRIBUTE_EX_W` | r | excluded | No Schannel security context exists in the managed picotls provider. |
| `QUIC_PARAM_TLS_SCHANNEL_SECURITY_CONTEXT_TOKEN` | r | excluded | No native Windows security token exists in this host. |
| `QUIC_PARAM_STREAM_ID` | r | selected-pending | Return the actual stream identifier with upstream start-state checks. |
| `QUIC_PARAM_STREAM_0RTT_LENGTH` | r | selected-pending | Report actual early-data length, necessarily zero in this profile; this getter does not enable 0-RTT. |
| `QUIC_PARAM_STREAM_IDEAL_SEND_BUFFER_SIZE` | r | selected-pending | Actual core send-buffer recommendation for backpressure. |
| `QUIC_PARAM_STREAM_PRIORITY` | rw | selected-pending | Upstream uint16 stream priority and scheduling behavior. |
| `QUIC_PARAM_STREAM_STATISTICS` | r | selected-pending | Actual translated stream counters and upstream output-size behavior. |
| `QUIC_PARAM_STREAM_RELIABLE_OFFSET` | rw | deferred | Preview reliable-reset extension is outside the initial stream reset profile. |

## Composite settings, flags and credentials

`GLOBAL_SETTINGS`, `CONFIGURATION_SETTINGS`, `CONN_SETTINGS` and `ConfigurationOpen` share the same full `QUIC_SETTINGS` policy. Ordinary pinned transport tuning fields remain selected, subject to upstream size/range/state validation. The JSON explicitly enumerates all 46 defined `IsSet` field names. Congestion control is CUBIC only; server resumption accepts `NO_RESUME` or `RESUME_ONLY`, never `RESUME_AND_ZERORTT`. Global load balancing is disabled; fixed server IDs are rejected.

ECN, encryption offload, reliable-reset extension, one-way delay, extra network-statistics events, multi-receive buffers, XDP, QTIP and reserved RIO remain false. Explicit false is permitted; true rejects before any settings mutation. Reject reserved/unknown `IsSet` bits and nonzero reserved flags. Apply the same checks at every settings entry point so configuration inheritance cannot re-enable excluded facilities.

The JSON also lists selected and rejected stream-open/start/shutdown, send, receive and resumption flags. Baseline FIN/abort/backpressure and callback reentrancy remain selected; 0-RTT and application-owned receive buffers do not. Retain operation-specific valid flag combinations from upstream.

The selected authentication path uses translated picotls TLS 1.3, P256, AES-128-GCM/SHA-256 or AES-256-GCM/SHA-384, and ECDSA/RSA-PSS server authentication with explicit trust and hostname checks. The facade owns credential/key/trust objects or parsed bytes and translates only its declared adapter flags. Native hash/store/context/token handles, unrestricted raw credential payloads, insecure no-validation, native provider flags and client authentication reject. Typed certificate-validation completion remains selected for the owning validator; the legacy boolean parameter is hidden. Key logging and TLS exporters need a later ownership/security profile.

## Evidence and remaining gate

Classification follows the pinned `src/inc/msquic.h` declarations and actual dispatch in `src/core/library.c`, `configuration.c`, `listener.c`, `connection.c` and `stream.c`. The native TLS query reference is `src/platform/tls_openssl.c`; it describes expected outputs, not permission to substitute a native provider. For example, ticket-key configuration calls `CxPlatTlsSecConfigSetTicketKeys`, resumption input decodes the MsQuic ticket envelope, local-interface selection writes the route scope ID before client start, and retry-secret querying is DEBUG-only. Per-entry source locations are recorded in JSON.

This completes the selection inventory only. The [plan](PLAN.md), [host boundary](host-boundary.md) and [UDP feasibility limits](datapath-feasibility.md) still govern qualification. Implement facade/adapter rejection and selected behavior, verify capability responses and invalid mixed settings, then exercise every selected operation and excluded option in the runtime matrix. The unchanged raw table is not an enforcement boundary by itself.
