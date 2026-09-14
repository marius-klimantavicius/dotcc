# Receipt workload measurements

`python3 pinta/scripts/benchmark.py` runs a native reference and all four managed
forms serially on Linux x64. Add `--profile debug` to select diagnostic source.
Each iteration creates one engine, loads the authored receipt module from memory,
supplies the same globals, executes once, collects and compacts the Pinta heap,
copies and validates all 52 output bytes, and disposes the engine. Five warmups
precede the default 1,000 measured iterations. JIT tiered compilation is disabled.

Both final profile runs passed every output check using the final staged core and
owning API. Release selects `PINTA_DEBUG=0`, and diagnostic selects `PINTA_DEBUG=1`.
The native benchmark uses `-O2` and `-O0` respectively; both managed profiles use
.NET Release builds. Values below are total seconds across 1,000 iterations,
observed on this machine, not performance gates. The profiles are kept separate.

Release profile:

| Form | Arena/engine creation | Execute | Collect/compact | Image bytes |
| --- | ---: | ---: | ---: | ---: |
| Native | 0.087192 | 0.000676 | 0.000613 | 211,744 |
| Raw JIT | 0.086177 | 0.000878 | 0.000866 | 8,192 |
| Raw NativeAOT | 0.088110 | 0.000934 | 0.000883 | 3,817,640 |
| Optimized JIT | 0.086671 | 0.000770 | 0.000706 | 8,192 |
| Optimized NativeAOT | 0.086572 | 0.000811 | 0.000776 | 3,788,968 |

Diagnostic profile:

| Form | Arena/engine creation | Execute | Collect/compact | Image bytes |
| --- | ---: | ---: | ---: | ---: |
| Native | 0.087805 | 0.003550 | 0.002071 | 384,328 |
| Raw JIT | 0.086586 | 0.002042 | 0.001413 | 8,192 |
| Raw NativeAOT | 0.086697 | 0.001957 | 0.001412 | 3,956,904 |
| Optimized JIT | 0.086284 | 0.001759 | 0.001216 | 8,192 |
| Optimized NativeAOT | 0.087049 | 0.001706 | 0.001306 | 3,928,232 |

Each managed form records 1,224 CLR-allocated bytes per iteration and zero CLR
collections during the measured interval. This excludes the 4 MiB unmanaged
arena and its 64 guard bytes. The interpreter uses a 1 MiB heap and 64 KiB stack
within that arena. Native shares a preloaded immutable module buffer; the owning
managed facade clones its module dictionary per engine. Native frees its copied
output during disposal; the CLR reclaims managed output arrays later. These host
allocation differences must be retained when interpreting timings.

Detailed receipts are `artifacts/benchmark/receipt.json` and
`artifacts/benchmark/debug/receipt.json`. They record input and binary hashes,
commands, load/global-setting time, copying/disposal time, peak process working
set, image sizes and complete output logs. Peak working set includes process
startup and warmups. JIT image bytes count only the entry assembly, excluding its
runtime and dependencies; they cannot be compared directly with complete native
images. Image sizes exclude any separate symbol sidecars.

The explicit collection/compaction measurement uses a small live heap. It is not
a GC stress or throughput benchmark. The separate consumer exercises bounded-heap
allocation and combined Pinta/CLR collection. Small timing differences across
forms are insufficient to claim a general speed advantage. Earlier measurements
without forced collection are preserved under `artifacts/benchmark-initial/`;
they are a different workload and are not pooled with these runs.
