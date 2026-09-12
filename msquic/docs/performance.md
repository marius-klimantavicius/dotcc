# Selected-profile performance measurements

The benchmark compares complete native/native and managed/managed connections on
Linux x64. It does not isolate compiler overhead. Every reported pair must first
verify every payload byte, FIN, negotiated profile and clean shutdown.

## Workload and measurement scopes

`scripts/benchmark-product.py` runs 20 pairs: four native IP/cipher combinations
and sixteen raw/optimized × JIT/NativeAOT × IPv4/IPv6 × AES-128/AES-256 pairs.
Each process is pinned to one recorded CPU. Each connection has one transport
worker, a 1 MiB stream window, an 8 MiB connection window, pacing enabled and send
buffering disabled. The workload has one warmup and three measured exchanges,
each carrying 16 MiB in each direction with 1 MiB application writes and a
pipeline depth of one. Receipt hashes bind source, generated inputs, binaries
and native dependencies before and after execution. Every command is recorded.

Throughput is aggregate bidirectional application payload divided by the client
transfer interval. The interval includes marker synchronization, ownership and
byte verification, but excludes payload preparation and stream opening/disposal.
The receipt retains both endpoints' intervals and process CPU observations.
Client connection timing covers the first connect call; it is not a warmed
steady-state handshake distribution. Server accept wait also includes launch
coordination and must not be compared as handshake latency.

Shutdown timing separates client-requested close from server waiting for peer
close, then measures complete local owner disposal. These are single observations
per endpoint. Server waiting includes peer coordination and the managed test's
one-millisecond close-state polling. Managed disposal includes the enclosing
runtime, registration, configuration and credential scopes; native disposal
closes its public handle tree through `MsQuicClose`. Neither includes process
termination, and the two ownership implementations have different costs.

## Results

The final complete 20-pair matrix passes, including shutdown and owner-disposal
measurements for all 40 endpoints. The receipt and full report are
`artifacts/benchmarks/results.json` and `artifacts/benchmarks/report.md`.
The following ranges span the four client IP/cipher profiles; transfer figures
are each profile's median of three measured exchanges.

| Implementation | Payload Mbit/s | First connect ms | Transfer CPU ms | Managed allocation MiB per exchange |
| --- | ---: | ---: | ---: | ---: |
| Native | 4,648–8,756 | 1.21–1.32 | 28.2–29.4 | Unmeasured |
| Raw JIT | 420–446 | 318.9–334.5 | 525.4–552.1 | 61.8–66.4 |
| Optimized JIT | 567–629 | 265.0–273.7 | 380.9–394.9 | 62.1–66.6 |
| Raw NativeAOT | 904–1,051 | 34.2–36.9 | 237.4–248.0 | 62.2–67.0 |
| Optimized NativeAOT | 971–1,112 | 34.1–34.7 | 231.3–236.5 | 62.4–67.4 |

| Implementation | Client requested close ms | Local owner disposal ms | Idle / peak / drained client RSS MiB |
| --- | ---: | ---: | --- |
| Native | 0.05–0.10 | 0.19–0.25 | 6.7–6.8 / 24.5–25.4 / 24.6–25.6 |
| Raw JIT | 6.89–7.41 | 20.23–21.95 | 76.0–76.2 / 122.5–127.7 / 122.5–126.1 |
| Optimized JIT | 6.39–6.85 | 19.95–21.28 | 72.2–72.4 / 119.8–125.6 / 119.4–122.9 |
| Raw NativeAOT | 0.12–0.17 | 0.38–0.42 | 27.5–27.8 / 73.5–87.6 / 58.1–74.0 |
| Optimized NativeAOT | 0.11–0.14 | 0.34–0.38 | 27.4–27.8 / 73.5–75.4 / 72.9–73.8 |

Optimized JIT is faster than raw JIT in these observations, but the complete
managed implementation remains substantially slower than native. Matched
managed/native payload-rate ratios range from 0.048× to 0.237× across forms and
profiles. These small local samples do not establish stable ratios or a required
performance threshold. The report retains per-profile results and both endpoint
roles, including the differently scoped server close wait.

The prior matrix, binaries, sources and profiling evidence are preserved with a
recursive hash manifest in
`artifacts/final-qualification/benchmark-before-shutdown-metrics/`.

## Investigation of the managed/native gap

The initial matrix showed a substantial throughput and process-CPU gap. A
separate 30-second `dotnet-trace` CPU-sampling session inspected the optimized JIT
IPv4/AES-128 client using the qualified binaries, with 256 MiB per direction,
one warmup and eight measured exchanges. Both instrumented endpoints verified their exact
payloads, FIN and clean close. Input and binary hashes matched before and after.
The raw trace and successful Speedscope conversion are retained under
`artifacts/benchmark-profile/`, together with a source-bound diagnostic receipt.
This instrumented run is not a performance qualification baseline.

The converted trace contains 17 evented thread profiles and 633 frame entries.
It repeatedly shows the datagram receive task handoff, platform queue operations,
owning API read continuation, thread-pool synchronization and the benchmark's
byte-verification loop. Send/receive socket calls and translated packet processing
also appear. `stack-observations.json` records converted span-opening counts and
their provenance. Those counts are not original sample counts or CPU accounting:
adjacent observations can merge, inclusive frames overlap, and wait, unmanaged
and unresolved frames prevent precise attribution. Summing displayed thread-stack
durations would incorrectly count blocked time as CPU time.

Source inspection identifies concrete costs consistent with those stacks:

- `MsQuicHost.Datapath.Lifetime.cs` converts socket receives to tasks and waits
  synchronously on a dedicated receive thread before handing work to the
  transport worker. Already-completed receives need not block.
- `MsQuicHost.Datapath.Send.cs` allocates pinned packet buffers and host descriptors and
  submits individual socket sends.
- `QuicSendOperation.cs` copies application send data; `QuicReceiveLease.cs`
  copies received data into an owned lease, then `QuicStream.ReadAsync` copies it
  into the caller's destination.
- The benchmark verifies every received byte inside its transfer interval.

These observations identify candidates for a subsequent controlled optimization
experiment. They do not quantify each candidate's contribution, prove a compiler
regression or establish a single cause for the whole gap. No product optimization
was introduced based solely on this trace. The later benchmark extension changes
only shutdown timing; this earlier diagnostic remains tied to its preserved
sources and binaries, with the same qualified product implementation.

## Interpretation limits

Native uses quictls/OpenSSL and borrowed send buffers. Managed uses translated
picotls, BCL cryptography, copied ownership buffers and GC. UDP batching and
offload capabilities may differ. Managed allocation counters are approximate;
native allocation volume is not measured. RSS includes runtime and prepared
payloads, peak RSS is a process-lifetime high-water mark, and drained RSS does not
prove memory reclamation. Neither endpoint forces GC.

Three sequential local samples do not establish statistical significance,
multi-connection scalability, deployment latency or platform portability. No
performance threshold was invented after observing these measurements.
