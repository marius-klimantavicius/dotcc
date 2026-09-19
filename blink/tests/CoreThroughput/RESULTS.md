# Fixed integer-loop throughput result

On 2026-09-19, all five modes passed the exact semantic checks for all 75 measured
batches: 75,000,000 guest instructions total, plus fixed untimed warmups. Nine
negative preparation/provenance checks also passed. This measures only the hot
four-instruction loop described in [README.md](README.md).

| Mode | Median M instructions/s | Minimum | Maximum | Sample standard deviation |
| --- | ---: | ---: | ---: | ---: |
| Pinned native interpreter | 84.203 | 81.641 | 85.150 | 1.053 |
| Raw JIT | 5.500 | 5.457 | 5.546 | 0.026 |
| Raw NativeAOT | 42.580 | 41.845 | 42.785 | 0.233 |
| Optimized JIT | 14.676 | 14.471 | 14.744 | 0.082 |
| Optimized NativeAOT | 47.759 | 46.496 | 47.915 | 0.480 |

Each mode has 15 observations: five batches of 1,000,000 instructions in each of
three fresh processes. Each process runs the same 40,000-instruction warmup.
All checks/reset/output occur outside the dispatch timing interval. Full per-batch
ticks/frequency, elapsed seconds, and separately observed process wall time are
retained in the receipt. No count or warmup was adjusted after seeing a score.

The host was an AMD Ryzen 9 7950X, Linux x64, using .NET 10.0.11. Every benchmark
child was pinned to permitted logical CPU 31. All recorded known tiering/PGO/JIT/GC
tuning overrides were unset. Other campaign builds were paused during final
measurement; uncontrolled external background activity remained. The observed
one-minute host load ranged approximately 3.44–3.66 across this short run.

These figures do not establish steady-state tiered-JIT behavior, end-to-end
service performance, another instruction mix, another CPU, or general speed
ratios. In particular, the fixed warmup and short process lifetime are part of
this workload and may influence differences between JIT and NativeAOT.

The canonical input is the narrowed CPUID profile `attempt-7i4_ajz4`, qualified
by `artifacts/core-execution/attempt-n9ligxbc/receipt.json`. The harness retains
108 exact producer objects and replaces only the authored frontend. The native
reference links the unchanged pinned interpreter archive with JIT and linear
mapping disabled. The managed input includes its separately reviewed profile
adapters; no source or generated instruction implementation was edited for this
benchmark.

Evidence below is under `blink/` and has been preserved separately from earlier
development/preparation attempts:

| Evidence | SHA256 |
| --- | --- |
| Final timing `artifacts/core-throughput/attempt-h4zo2vxu/receipt.json` | `ae0e7eaf1596221ebf89286d11983a55aa67546b0fe14f4a336cb583cf469763` |
| Immutable preparation `artifacts/core-throughput/attempt-h4zo2vxu/prepared-receipt.json` | `b5f2d1db00f88a327f36aceb61b95fccfe5a765d480c8c01a10efea704e3f807` |
| Nine negative controls `artifacts/core-throughput-controls/attempt-10wi772e/receipt.json` | `8da6fa741805cb86f4dba6d0ca47b11118775200d21ab6ad1132b36f4c7926f9` |

The final receipt verifies the complete frozen artifact set before and after
measurement, exact five-mode executable paths and semantic preflights, the
canonical core/assembly/profile identity chain, compiler identity, and .NET
runtime binary/tuning identity. Failed or partial checks cannot retain a passing
receipt. Preparation was rerun after review fixes; earlier receipts remain
historical and were not relabeled as final timing evidence.
