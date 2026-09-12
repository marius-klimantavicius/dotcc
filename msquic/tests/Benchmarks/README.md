# Public API endpoint benchmark (full P9 matrix pending)

`scripts/benchmark-product.py` builds and runs these independent endpoint
processes, records source/binary provenance and matched process affinity, and
writes a JSON receipt and Markdown report. The first optimized JIT IPv4/AES-128
native/native and managed/managed pairs pass one 16 MiB warmup and one measured
16 MiB transfer per direction, with verified payload, FIN and clean shutdown.
See `artifacts/benchmarks-handshake-fixed/results.json` and `report.md`.
This diagnostic run does not establish a performance baseline or complete P9:
raw/optimized, JIT/AOT, both IP families/ciphers and exact compiled source
metadata remain required. No throughput threshold is encoded. Failed payloads,
missing FIN, unclean shutdown, profile mismatch or timeout invalidate the run.

```sh
python3 msquic/scripts/benchmark-product.py
# Small diagnostic subset:
python3 msquic/scripts/benchmark-product.py --variants optimized --jit-only \
  --families ipv4 --ciphers 128 --pairs native both --iterations 1
```

The first managed attempt exposed upstream TLS-state retirement with resumption
disabled. The owning facade now captures negotiated handshake information during
CONNECTED, before publishing the connection to the consumer. The benchmark
reads that immutable snapshot after accept. The native endpoint likewise samples
its actual handshake parameters inside CONNECTED. No TLS context is artificially
retained for measurement, and the original failed receipts remain available.

`ManagedEndpoint/ManagedEndpoint.csproj` is an ordinary public consumer of the
actual ManagedApi project. Its separate assembly `PublicQuicBenchmark` has no
friend access, source-linked library files, generated types, unsafe code, or
extra trimming roots. `native-endpoint.c` uses the pinned public native API and
is linked only into an independent native endpoint process.

Both programs accept exactly the same positional arguments:

```text
endpoint client|server certificate-or-root key-or-server-name ip port ready-file bytes warmups iterations chunk-bytes pipeline cipher128|256
```

Server: certificate and private-key PEM paths, bind IP, port (zero allowed), and
ready-file path. Client: explicit trust-root PEM path, expected server name,
remote IP, nonzero remote port; the ready-file argument is present but unused.
The server writes its actual listening port after ListenerStart. Use plain IPv4
or IPv6 addresses; scoped-address parsing is not a benchmark feature.

Suggested bounded numeric configuration:

```text
16777216 1 3 1048576 1 128
```

This means 16 MiB **per direction per stream**, one warmup, three measured
transfers, 1 MiB send slices, one outstanding send, and AES-128-GCM/SHA-256.
AES-256-GCM/SHA-384 is selected with `256`. Both reject pipeline values other
than one: the owning stream API permits one in-flight send per stream. Neither
endpoint gains additional pipelining by silently changing the product's write
ownership. Accepted bounds are 16–256 MiB, 1–4 warmups, 1–16 measurements, and
4 KiB–4 MiB slices. Orchestration should normally use the suggested minimum
payload and short transfer count.

Both set the same settings before connection establishment:

- ALPN `dotcc-bench-v1`, QUIC v1 only, P-256 and the selected AES cipher.
- One transport worker: managed runtime ProcessorCount=1; native actual
  QUIC_PARAM_GLOBAL_EXECUTION_CONFIG ProcessorCount=1, NO_IDEAL_PROC.
  The orchestrator should record and equalize process affinity externally.
- Stream receive window and receive buffer 1 MiB; connection flow-control
  window 8 MiB; 32 peer bidirectional streams and no unidirectional streams.
- Send buffering disabled, handshake timeout 10 seconds, idle timeout 60
  seconds, resumption disabled, pacing enabled, ECN and encryption offload
  explicitly disabled. Every timed stream uses a single established
  connection, so only the separate connect metric includes TLS establishment.
- The managed provider uses explicit roots with NoCheck for development
  certificate revocation. Trust and server-name verification remain enabled.
  The native endpoint uses CA_CERTIFICATE_FILE and built-in verification.
  Supply fresh local certificates without revocation services; document the
  provider-policy distinction rather than implying identical crypto internals.

The native OpenSSL provider must be configured externally to offer P-256
(`OPENSSL_CONF` with `Groups = P-256`, as in the existing native peer harness).
Both programs read the actual negotiated cipher/group/version/ALPN and reject
any mismatch. The native provider is OpenSSL; the managed provider is picotls
plus BCL crypto. Their provider, runtime and ownership differences are part of
what is being measured, not an isolated compiler-only comparison.
Native UDP batching/segmentation may still differ from the BCL datapath: there
is no selected public facade switch that makes these implementation capabilities
identical. Record this limitation when interpreting throughput or CPU ratios.

The wire protocol creates a fresh bidirectional stream for every transfer,
starting with all warmups and then measured iterations. Both directions send a
single byte `0xa7`, then wait until the peer marker has been received before
sending payload. For payload offset `o`, stream index `i` (including warmups),
and sender role, the expected byte is:

```text
(o * 31 + (server ? 83 : 17) + i * 7) mod 256
```

Both simultaneously send the configured payload length in bounded chunks and
attach FIN to the last chunk. Both verify every received byte, its position,
the complete length, peer FIN, and graceful local write completion. Payload
arrays are generated before stream open/accept and before timing. The managed
endpoint must still copy/pin each public SendAsync slice and copy/complete
receive offers through the owning API. The native endpoint lends the prepared
payload directly to StreamSend, owns its descriptor until SEND_COMPLETE and
verifies native receives inside its callback. These are explicit API-consumer
costs; do not label their difference pure C-to-C# code-generation overhead.

The transfer wall interval starts immediately before sending the readiness
marker and ends after verified receive FIN plus graceful write completion. It
includes the marker exchange, all payload processing/copies/pinning and send
completion, but excludes payload generation, stream open/accept, result output,
and StreamClose. The open/accept duration is reported separately. Client
handshake timing spans the complete connection-open/start/connected operation;
server `server_accept_wait` includes waiting for an external client and must
**not** be presented as server handshake latency. Wall-clock microseconds use
monotonic Stopwatch/CLOCK_MONOTONIC, not civil time.

JSON-lines records:

- `configuration`: exact CLI transfer sizes/counts and fixed window/profile
  settings. `identity`: actual library revision and provider; managed also
  reports actual dynamic-code/AOT identity. Set DOTCC_REQUIRED_SOURCE_REVISION
  to require compiled metadata to equal the pin; no revision is manufactured.
- `handshake`: labeled scope, wall microseconds and checked cipher/group/v1.
- `transfer`: index, warmup flag, verified bytes each direction, FIN, wall/open
  microseconds, process CPU delta, and process-wide managed allocation delta
  (`null` on native). No transfer record is emitted for invalid data or FIN.
- `memory`: idle after handshake, load_begin/load_end per transfer, and drained
  after all stream/connection/configuration/registration/library owners close.
  Current RSS and OS peak RSS are process values. CPU deltas are sampled around
  the wall interval and include small observer overhead. Peak RSS is a lifetime
  high-water mark, not a post-close retained-allocation count. Managed heap and
  allocation counters are approximate process-wide samples; normal background
  GC and other runtime work contribute. No endpoint forces a GC or requests a
  full collection. There is no idle-time plateau guarantee in one snapshot.
- Final `passed` and `clean_close`: both must be true and both processes must
  exit zero before any aggregate rate is reported. Exclude all warmup transfer
  records from throughput and latency summaries. Derive payload throughput
  from `payload_sent + payload_received` and `wall_us`; identify it explicitly
  as aggregate bidirectional payload rate, excluding the marker bytes.

The client initiates connection shutdown only after every transfer finishes.
The server waits for the peer's clean shutdown. Raw StreamClose is called only
from the native producer/main thread, never from its callback, and drains borrowed
send storage before it is freed. Managed owners use async disposal in child-first
order. The managed operation bound is two minutes per transfer and fifteen
minutes for the session. Native condition waits are individually bounded by two
minutes; orchestration must apply a whole-process timeout and reap both peers.
A killed/timed-out process cannot contribute a passing throughput result.

Native build ingredients for the coordinator: gcc, `-std=c17 -D_GNU_SOURCE
-DCX_PLATFORM_LINUX`, immutable native `src/inc` include directory, this source,
`-pthread`, and the already qualified native libmsquic with its explicit runtime
library path. The source enables the preview API only for actual v1 policy and
one-worker execution configuration. Freeze/hash the actual included headers,
native library, compiler invocation, managed ProjectReference graph, generated
raw/optimized MsQuic and picotls manifests, source pin/profile and executed
binaries. Publish the actual managed project to test trimming without added roots.
Run native/native, managed/managed and optionally mixed provider pairs as clearly
separate cases; never combine samples from different pair types in one ratio.
