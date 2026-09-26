# Public managed QUIC sample

This executable references `src/ManagedApi/ManagedApi.csproj` as a normal project
dependency. Its assembly is `PublicQuicSample`, with no friend access, unsafe
code, generated types, source-linked library files, or test callback hooks.
It needs the generated MsQuic and picotls projects used by `BclHost.csproj`.

The server and client run in separate processes. Each connection exchanges
65,537 bytes in each direction and checks every byte and FIN. By default they
make two connections: the first performs a full handshake and obtains a ticket;
the second supplies that ticket and requires actual resumption in connection
statistics. Both exchange fresh stream data after the handshake. Tickets stay
in memory and are scoped to the same server name, trust policy, and ALPN.

Build from the repository root:

```bash
dotnet build msquic/samples/ManagedConsumer/ManagedConsumer.csproj -c Release
```

Create a new directory of short-lived local certificates. The generated leaf
covers `localhost`, `127.0.0.1`, and `::1`; the key file is created with owner-only
permissions on Unix. These are development credentials.

```bash
dotnet msquic/samples/ManagedConsumer/bin/Release/net10.0/PublicQuicSample.dll certificates /tmp/quic-sample-certificates
```

Run the server in one terminal. It writes its bound port to the ready file only
after the listener has started, and exits after two connections:

```bash
dotnet msquic/samples/ManagedConsumer/bin/Release/net10.0/PublicQuicSample.dll server /tmp/quic-sample-certificates/server.pem /tmp/quic-sample-certificates/server-key.pem 127.0.0.1 45678 /tmp/quic-sample-ready
```

Run the client in another terminal:

```bash
dotnet msquic/samples/ManagedConsumer/bin/Release/net10.0/PublicQuicSample.dll client /tmp/quic-sample-certificates/root.pem localhost 127.0.0.1 45678
```

Use `::1` in both commands for IPv6. A server port of `0` selects an available
port; read the ready file and pass that port to the client. An optional final
argument selects 1–32 connections on both processes. Every connection after the
first must resume. Each round prints payload, FIN, and resumption observations;
the final `passed`/`clean_close` record is printed after all owners have drained.

The client explicitly selects `X509RevocationMode.NoCheck` for the local sample
certificates. Certificate-chain and server-name validation remain enabled.
For an authentication rejection, run a fresh server and use `wrong.example`
as the client's server name. The client must exit unsuccessfully and print the
actual QUIC status and any available TLS alert. A timeout is also a failure;
it does not establish authentication rejection. Cancel the waiting server with
Ctrl+C afterward. Both processes have a two-minute bound and dispose their
owners on cancellation or failure.

The optional final `--alpn=protocol` argument selects 1–255 printable ASCII
bytes; the default remains `dotcc-public-sample`. Use the same value on both
peers for a positive exchange. To exercise ALPN rejection, leave the server at
its default and append `--alpn=dotcc-public-mismatch` to the client command.
The client must report ALPN status92, QUIC crypto error0x178/TLS alert120 and
zero application bytes. No listener is selected on the server for that protocol,
so cancel its still-pending accept afterward and require completed disposal.

The request's FIN follows the checked server response and received ticket. This
application protocol prevents server shutdown from racing ticket delivery.
The code uses `await using` in child-before-parent order: stream, connection,
listener, configuration, registration, runtime. Send completion releases the
copied send buffer; `CompleteWritesAsync` observes graceful write shutdown.
`ReadAsync` handles receive offers internally. Applications that instead use
`ReceiveAsync` must complete or dispose each receive lease before awaiting owner
disposal, because a live lease deliberately retains receive ownership.

Publish the same external consumer with NativeAOT:

```bash
dotnet publish msquic/samples/ManagedConsumer/ManagedConsumer.csproj -c Release -r linux-x64 --self-contained true -p:PublishAot=true -o msquic/artifacts/public-sample-aot
```

Then substitute `msquic/artifacts/public-sample-aot/PublicQuicSample` for the
`dotnet ...PublicQuicSample.dll` prefix above. No test-only source is added during
publish. The sample's JIT/NativeAOT execution matrix is pending; source authoring
alone is not a qualification result.

The qualification driver runs the actual project through both translated
variants and both runtimes, using separate IPv4/IPv6 processes. It checks two
connections, actual resumption, payloads, FIN, authentication rejection, runtime
identity, and completed disposal:

```bash
python3 msquic/scripts/test-public-consumer.py
```

`--variants optimized --jit-only` selects a shorter diagnostic run. Such a run
can set `targeted_passed`, but never the full `passed` flag. A full run also
requires the actual library source-revision getter to equal the immutable pin;
it cannot substitute metadata from configuration files. Receipts bind the
sample, project dependencies, staged source, generated manifests, picotls
translation, and executed binaries. Logs are preserved under unique directories
in `msquic/artifacts/public-consumer`, with the latest receipt at `results.json`.
The driver also checks wrong-ALPN negotiation separately from wrong trust/name,
including the exact client error, zero bytes on both roles and canceled server
accept disposal. This consumer gate does not replace the wider owning-API
qualification matrix.
