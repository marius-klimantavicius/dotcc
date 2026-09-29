# TCP application qualification

The five original application cases and the actual owning MQTT case pass native
controls and raw/processed JIT/AOT on all three baseline profiles. Each baseline
receipt also requalifies shared owning API, TFTP, SNTP, LOWPAN and canonical consumer
lifetime behavior after completed-service unregistration:

| Profile | Completed matrix receipt |
| --- | --- |
| threaded | `20260929-160809-dd5468af` |
| nosys | `20260929-161345-b55f9edf` |
| threaded-locking | `20260929-161858-576439ba` |

`OwnedMqttCases.cs` uses the public facade for copied QoS1 publish/inbound messages,
empty messages, queue/request admission and recovery, cancellation/reconnect,
ordinary broker FIN/CONNACK refusal, multiple clients, service/owner disposal and
an independent surviving owner. Its native prerequisite checks original MQTT wire
and callback behavior; separate managed assertions check Tasks/Channels and a
no-inline weak-reference collection test with unread copied data on a live owner.
An idle create/dispose loop checks natural original heap recovery independently.
Disposal removes completed built-ins from active stack registration; failed stops
remain registered. Resolver's separate post-core-stop list intentionally retains
even empty disposed wrappers until owner shutdown; that remaining limit is not
closed by MQTT collection evidence.

`LwiperfRefusalCases.cs` is excluded from project compilation and its native
counterpart remains unselected. Public original lwiperf abort/refusal and proposed
ownership boundaries are unqualified. Positive lwiperf keeps one original listener
until whole-owner shutdown; the public client fixes ten seconds and retains an
upstream duration/amount API TODO. MQTT QoS0/QoS2, keepalive, Will, credentials and
IPv6 remain required unqualified variants. CONNECT status parsing is an upstream
TODO; split terminator/coalesced tunnel-data and pending cancellation remain open.

Preserved failures and corrections:

- `tcp-native-20260929/mqtt-01`: missing original SNTP provider for the baseline
  DHCP NTP callback caused native link failure. Adding the original SNTP source
  closes that dependency; no SNTP workload or authored stub was substituted.
- `smtp-01` and `smtp-original-diagnostic-02`: valid ASCII CRLF body returns
  `ERR_ARG`. Original body validation incorrectly passes the header-only flag0.
  The sixth guarded staged repair changes only that body's argument to1 while
  preserving verifier and to/from/subject checks. `smtp-comparison.json` proves
  identical fixture/profile inputs, exact original-fail/staged-pass bytes and
  successful copied/static mail plus ordinary550/FIN cases. No reference edit.
- `20260929-155534-b2ac9319`: fixture C# local shadowing prevented execution;
  only the nested local name changed.
- `20260929-155601-87d33ef1` and `20260929-160023-843753dc`: authored lwiperf
  clock advancement after the first24-byte settings header produced an ordinary
  successful report of24 bytes/10001ms, below the fixture's unchanged48-byte
  expectation. The pre-advance observation now requires at least48 bytes of
  coherent emitted TCP sequence extent, excluding padding/retransmit duplicates.
  `20260929-160204-5d300d31` and all complete matrices pass; original send logic
  and report threshold are unchanged.

All traffic is coherent; no custom corruption test is present. The README MQTT
usage snippet requires supplied stack/transport/broker and is not a separately
executed example project. Actual API execution is recorded by the owning fixture.

The following original draft handoff is retained as historical design context;
its unregistered/unbuilt labels describe the pre-integration handoff, not the
current registration. The completed receipts above supersede its pre-execution status.

# Unqualified TCP application workflow drafts

These authored fixtures are **unregistered, unbuilt and unexecuted**. They do not
close a campaign acceptance item. No product, shared script, configuration,
verification policy or upstream source is changed by this directory. Integration
requires native original-source counterparts, source/policy review, declared
profile applicability and an explicit build-ownership transfer.

`Applications.Tcp.csproj` imports canonical `src/LwipProduct.props`. `Program.cs`
requires a matching successful native report. Native counterparts are authored
but have not been built or run. A success line follows fixture assertions and owner
teardown, not translation or build alone.
The terminal report uses `Utf8JsonWriter`, without reflection-based serialization
or an AOT reflection opt-in.

`Fixture.cs` creates two existing `VirtualPair` owners connected by copied
Ethernet frames. It retains callback roots and native strings/structures until
both cores stop. `TcpPeer.cs` is an independent coherent application peer using
original translated raw TCP. It captures bounded copied bytes, handles ordinary
FIN, and detaches callbacks before releasing its root. It does not require the
threaded socket profile. Original MQTT/SMTP/lwiperf client/server algorithms run
from the translated C; the authored peer is test code, not a product replacement.

## First source batch

| File/case | Authored ordinary behavior | Original entrypoints |
| --- | --- | --- |
| `MqttCases.cs` / `mqtt` | Clean-session CONNECT, QoS1 SUBSCRIBE/SUBACK, copied incoming topic/payload and PUBACK, outgoing QoS1 publish/PUBACK, unsubscribe, normal broker FIN, valid server-unavailable CONNACK, explicit disconnect with an unanswered subscription | `mqtt_client_new`, `mqtt_client_connect`, `mqtt_set_inpub_callback`, `mqtt_sub_unsub`, `mqtt_publish`, `mqtt_disconnect`, `mqtt_client_free` |
| `SmtpCases.cs` / `smtp` | Copied and retained-static mail, greeting/EHLO/envelope/DATA/QUIT, exact headers/body, ordinary recipient 550 refusal, ordinary peer FIN before greeting | `smtp_set_server_addr`, `smtp_set_server_port`, `smtp_set_auth`, `smtp_send_mail`, `smtp_send_mail_static` |
| `LwiperfCases.cs` / `lwiperf` | Original client to independent TCP sink, original client to original server, ordinary successful FIN/report completion | `lwiperf_start_tcp_client`, `lwiperf_start_tcp_server`, original sent/poll/close paths |
| `HttpClientCases.cs` / `http-client` | Segmented valid response headers, copied complete 200/404 bodies and distinct status reporting, normal application header-policy rejection, ordinary early peer FIN | `httpc_get_file`, original headers/body/result callbacks |
| `ProxyConnectCases.cs` / `proxy-connect` | Cleartext CONNECT target serialization, coherent 200 handshake, copied bidirectional tunnel payload and normal FIN/close | `altcp_proxyconnect_new_tcp`, `altcp_connect`, `altcp_write`, `altcp_recved`, `altcp_close` |

All inputs are coherent protocol messages or ordinary transport closure/refusal.
There are no custom malformed, mutation, corruption, signature-tampering or
payload-corruption cases. Bounds checks validate captured output. The lwiperf
clock case advances only the existing host clock and invokes original timeout
scheduling; it does not call a private protocol timer or replace its decisions.
Its peer copies only an initial settings prefix, then counts/discards payload
without retaining unbounded data. A second original client/server flow observes
valid emitted data before advancing time. Both use the original fixed ten-second
public-client duration. Ethernet observation does not alter frames.

Baseline requirements are IPv4/TCP/raw callbacks, MQTT, SMTP, lwiperf and the
campaign's copied Ethernet host integration. lwiperf's first batch assumes
`LWIPERF_CHECK_RX_DATA=0`; enabling its optional original data-check mode requires
a separate native comparison. No input corruption test is proposed for that mode.
TLS and external crypto are absent. The cleartext SMTP cases disable credentials;
bundled AUTH LOGIN/PLAIN are additional in-scope variants, not exclusions.

## Exact callback and lifetime contracts

All generated callbacks add the explicit `Lwip owner` argument before the
original C parameters. Original declarations live under
`src/include/lwip/apps/{mqtt,smtp,lwiperf}.h` in pinned revision
`77dcd25a72509eb83f72b033d219b1d40cd8eb95` (use campaign source metadata as the
authoritative revision; fixtures are not a replacement pin).

MQTT callback signatures are:

```
void Connection(Lwip, mqtt_client_s*, void*, mqtt_connection_status_t)
void Request(Lwip, void*, sbyte)
void Published(Lwip, void*, byte* borrowedTopic, uint totalLength)
void Data(Lwip, void*, byte* borrowedBytes, ushort length, byte flags)
```

`mqtt_client_connect` copies CONNECT strings into its ring buffer; incoming topic
and payload pointers are borrowed only during the callback and are copied here.
The fixture catches ordinary callback failures. In original `mqtt.c`,
`mqtt_close` clears pending requests through `mqtt_clear_requests`, then removes
its cyclic timer and optionally reports disconnect. `mqtt_disconnect` first marks
the client disconnected, deliberately suppressing the connection callback;
cleared request callbacks are not invoked. The fixture explicitly observes this
contract. A future owning MQTT service must settle its own pending Tasks, stop
original protocol work, drain the core, then free the client and callback roots.
`mqtt_client_free` alone only frees memory and is not shutdown. No product task
cancellation is claimed by this raw fixture.

SMTP result signature is:

```
void Result(Lwip, void*, byte result, ushort serverCode, sbyte transportError)
```

`smtp_send_mail` copies message inputs; `smtp_send_mail_static` borrows them until
result. `smtp_free` saves the callback, frees original session/input state, then
calls it. In the normal `smtp_process` success path the result fires after QUIT
is queued, before the peer's 221/FIN. Fixtures finish that ordinary transport
closure before leaving the case. Server address/auth settings are original
owner-global state; simultaneous consumers require an owning configuration
contract. There is no public session handle or cancellation API. A product owner
may need a reviewed narrow include-wrapper/hook associating sessions with their
consumer and invoking original `smtp_close`/`smtp_free`. No such boundary or
source patch is authored here, and no active SMTP root is freed prematurely.

lwiperf result signature is:

```
void Report(Lwip, void*, lwiperf_report_type,
            ip_addr* local, ushort localPort,
            ip_addr* remote, ushort remotePort,
            uint bytes, uint milliseconds, uint bandwidthKbitPerSecond)
```

Original `lwiperf_tcp_close` removes the session, reports, detaches callbacks,
closes/aborts TCP and frees session memory. `lwiperf_start_tcp_client` hardcodes
`settings.amount = htonl((u32_t)-1000)` and has a TODO for public duration/amount
selection. **Public `lwiperf_abort` has not been executed:** source review finds
that it frees matching linked records without the corresponding callback/PCB
teardown or head update visible in the normal close helper. It must not be
assumed safe because of its name. The server listener and callback root in these
finite fixtures remain until whole-owner shutdown; this is not owning-service
cancellation or listener-restart qualification. A guarded original-close
boundary and safe native control need review before exercising public abort or
implementing product cancellation.

A second source-observed limitation affects ordinary refused connections:
`lwiperf_tcp_err` sets `conn->conn_pcb = NULL`, then calls `lwiperf_tcp_close`;
`lwip_tcp_conn_report` dereferences `conn->conn_pcb` for endpoint fields whenever
`report_fn` is non-null. This has not been executed. The positive `lwiperf` case
does **not** include refused connect or claim it passes. Separate
`LwiperfRefusalCases.cs` and `native-lwiperf-refusal.c` preserve an ordinary
closed-port control for later source/policy/repair review. Neither is selected
by the entrypoint/native runner; any crash or nonzero result must stay a failed
attempt, not an expected pass or skip. A native comparison and an approved narrow
boundary are required before claiming ordinary refusal completion.

## Required integration and remaining scope

- Review all authored/transitive paths and ordinary scenarios in verification
  policy. Preserve per-attempt input/source hashes and failures.
- Review and qualify authored native controls with independent TCP peers or matched
  original global-stack client/server flow as appropriate. Define observations
  independently; do not manufacture native reports to bypass the entrypoint.
- Review generated callback/struct signatures against the selected product, then
  run native controls and raw/processed JIT/AOT only under assigned build ownership.
- Implement actual owning MQTT/SMTP/lwiperf consumer services and examples, with
  admission, bounded copied notifications/data, cancellation completion and
  callback-root release after original cleanup. These fixtures are not services.
- Extend MQTT QoS0/QoS2, retained/Will/credentials/keepalive/timeouts and reconnect,
  SMTP bundled AUTH PLAIN/LOGIN and dynamic body callback,
  lwiperf dual/tradeoff/check-data/restart, and IPv6 through coherent
  profile-specific cases. No feature is excluded by omission from this first batch.

## HTTP client and cleartext CONNECT contracts

HTTP client settings are retained original pointers, so `httpc_connection_t`
(`_httpc_connection` in generated C#), callback roots and settings live through
completion and owner stop. Headers are borrowed pbuf data; body pbuf ownership
passes to the supplied receive callback, which copies/acknowledges/frees it.
The signatures, prefixed with the generated `Lwip` owner, are:

```
sbyte Headers(Lwip, _httpc_state*, void*, pbuf*, ushort headerLength, uint contentLength)
sbyte Body(Lwip, void*, altcp_pcb*, pbuf*, sbyte)
void Result(Lwip, void*, ehttpc_result, uint receivedLength, uint serverStatus, sbyte)
```

The ordinary header-policy case returns `ERR_ABRT` for a valid response: original
`httpc_tcp_recv` calls `httpc_close(...HTTPC_RESULT_LOCAL_ABORT...)`. This is an
implemented consumer rejection hook, not arbitrary cancellation of a pending
request. `httpc_close` invokes the result callback before `httpc_free_state`
detaches callbacks, closes TCP and frees state; settings cannot be released
reentrantly from the result callback. A complete ordinary 404 response returns
`HTTPC_RESULT_OK` together with status 404; a product API must retain that status.
No general public request-abort entry exists in the pinned header. DNS/proxy GET,
file-I/O examples and owning pending cancellation remain separate work.

The original CONNECT layer retains `altcp_proxyconnect_config`; the fixture
keeps it until the core stops and unregisters callbacks on normal close. Its
connected/receive/error callbacks have the ordinary generated ALTCP signatures.
The positive case sends the complete valid 200 response header together, waits
for connection notification, then exchanges tunnel payload separately. TLS is
not needed for this cleartext layer.

In original `src/apps/http/altcp_proxyconnect.c`,
`altcp_proxyconnect_lower_recv` explicitly says `@todo: parse setup phase rx data`
and currently only searches for `\r\n\r\n`. **HTTP status interpretation is
upstream-unimplemented**, not a campaign promise to invent a parser. The
implemented header-end scan operates on each receive pbuf separately: an
ordinary terminator split across receives is an unresolved segmentation limit.
It also frees the whole handshake pbuf, so coalesced first tunnel data is not
qualified by this separate-payload positive case. A peer FIN during setup
closes the outer layer without an application terminal callback in the visible
branch; an owning service needs an explicit lifetime/completion design. None of
these observations is a tested behavior claim or a reason to replace the
original protocol with host code.

## Native and campaign handoff

`native-common.h`, `native-main.c` and `native-peer.c` provide one original native
stack, a coherent independent raw-TCP peer, bounded capture and static callback
lifetimes. `native-{mqtt,smtp,lwiperf,http,proxy}.c` implement the matching finite
cases. The native stack uses its local interface/loopback and address 10.0.0.1;
managed counterparts exchange copied Ethernet between independent owners at
10.0.0.1/10.0.0.2. Expected envelope/Host/address fields identify the correct
endpoint in each topology. Native lwiperf's original client/server flow lets the
fixed ten-second duration elapse; its independent sink retains only 48 settings
bytes regardless of whether loopback completes before the waiting thread runs.
No public `lwiperf_abort` control is invoked. One retained original lwiperf
listener is explicitly recorded through process/core shutdown; TCP active and
TIME_WAIT counts are observations, not an assertion of complete service teardown.

`native.py --profile <nosys|threaded|threaded-locking> --case <case> --output
<new-attempt-directory>` refuses to build without a reviewed
`tcp_application_draft_snapshot` in verification policy. That future section
must inventory `paths`, `native_dependency_paths`, and `original_units`, each
with exact `path` and `sha256`; no section is added by these drafts. It should
include every local file, relevant campaign configuration/helpers, original
selected units, native host bridge/sys_arch prerequisites and shared headers.
The runner records selected/staged/header/port hashes, every compile/link/run
command, per-command logs, nonzero exits and a final input-hash comparison. It
uses unique output directories. Its assertion-control process exits 92 before
initialization and sends no protocol input. No other oracle workload is invoked.
All transitive inputs must stay frozen during qualification.

Selected original groups are core/core4/core6/API/PPP/MQTT/SMTP/lwiperf, plus
Ethernet, `http_client.c` and `altcp_proxyconnect.c`. Native prerequisites reuse
`src/Host/bridge.c` and `tests/NativeOracle/sys_arch.c`; renaming only host
`sys_now` supplies the optional clock offset without replacing a protocol timer.
`campaign-runner.py` is an unregistered proposed dispatcher for explicit
`tcp-app-{mqtt,smtp,lwiperf,http-client,proxy-connect}` suite names. It requires
native success before raw/processed JIT/AOT. No build/recipe/policy registration
has occurred, and all reports remain absent until actual execution.
