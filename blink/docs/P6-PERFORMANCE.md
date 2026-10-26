# Measured interpreter optimization

This work replaces the owned-mapping list scan with an owner-scoped AVL range
index, skips deadline locking when an execution's immutable timeout is absent,
and caches stable pthread mutex identities within their runtime context. The
interpreter, managed callback ABI, instruction accounting, guest permissions and
RW/NX host backing remain in place. No native-code backend was enabled.

The baseline is `sqlite` at `65059c2f571301707053a4145e19c7380da6e1ed`.
The candidate's strict producer is
`artifacts/campaign/20261025-193713-37214280/receipt.json`: compiler/runtime
resources rebuilt, all 110 C objects freshly emitted, zero object reuse, raw and
processed builds passed. Measurements and failed attempts are retained under
`artifacts/performance-implementation/`. This is uncommitted implementation for
root review; no commit, push or PR is part of this work.

## Implementation and accounting

`src/Host/HostMemory.c` indexes disjoint live allocation ranges. Shared leases
may repeat the same single-page base; equal-key insertion order and rightmost
predecessor/removal preserve the former newest-alias precedence through rotations.
The existing shared-memory gate protects every shared operation. Protection bytes,
EOF state and shared-lease callbacks are read live. There is no raw-pointer-only
or permission cache. Successful publication/removal updates the index; failed
lease release leaves its mapping live. Disposal releases every surviving lease.

The private x64 mapping record grows from 24 to 40 bytes. Its links, height and
padding are included in the existing `sizeof(OwnedMapping)` charge, alongside
protection bytes and the unchanged shared-lease charge. The owner root replaces
the list head without another allocation or owner-header charge. Exact-budget
fixtures account for the additional metadata; defaults and limits are unchanged.
Structural checks at 4,096 mappings validated height 13 and predecessor paths
bounded by tree height, including misses and seeded random queries.

`HostExecutionStop.RefreshDeadline` returns before locking for a null immutable
timeout. Reason still uses a volatile read, and existing token propagation,
instruction-boundary polling, timed deadline handling and monotonic latching
remain. Shared instruction counters and exact budgets were not amortized.

`DotCC.Libc/PthreadLib.cs` resolves a cached mutex only within its owning
`RuntimeContext`, by its immutable, monotonically assigned ID. The existing state
lock still checks destruction, ownership, recursion and waiters. Registry misses,
initialization and destruction retain the registry gate. A context with more
than 64 live mutexes disables cache-slot reads/publications; shrinking repopulates
at most 64 slots under the gate. Destroy/dispose release cache references.
Measured x64 managed metadata increases by 544 bytes per runtime object graph
and eight bytes per live mutex object. This is runtime metadata, separate from
the emulator's guest backing/virtual/memory-file accounting.

## Matched measurements

The primary comparison uses identical frozen guest bytes and the same harness,
processed JIT host, tracing off, with host tiering disabled for this diagnostic
stratum. Builds and runs are serial. ABBA process order retains ten observations
per variant after per-process warmups. Import/hash checking is outside execution.
Ranges below are observed minima–maxima, not confidence intervals.

| Case | Before wall ms, median [range] | After wall ms, median [range] | Before → after CPU ms | Instructions |
|---|---:|---:|---:|---:|
| CoreCLR host information | 914.34 [886.17–936.33] | 201.81 [198.37–208.42] | 913.93 → 201.54 | 2,087,891 |
| Fixed CoreCLR service prefix | 921.89 [873.30–943.11] | 191.73 [186.41–199.50] | 921.33 → 191.78 | 2,000,000 |
| Register loop | 1,424.96 [1,352.06–1,619.38] | 1,080.85 [1,061.06–1,188.82] | 1,424.21 → 1,080.48 | 30,000,009 |
| Stack loop | 2,828.52 [2,789.83–3,159.09] | 2,529.89 [2,498.08–2,561.43] | 2,827.94 → 2,529.01 | 30,000,010 |
| File-data loop | 3,781.97 [3,734.51–4,009.51] | 3,583.35 [3,566.07–3,597.50] | 3,772.36 → 3,580.73 | 30,000,010 |

All 25 compared case/form/host strata preserve deterministic instruction totals
and functional results. No median regression occurred in the supplemental
raw/processed default-tiering JIT and NativeAOT host comparisons. Those strata
have fewer retained samples and are not pooled with the primary diagnostic.
The changeset is measured together; these results do not attribute additive
percentages to individual changes or predict an arbitrary application's latency.
See `integrated/summary.json`, per-process rows, source/library hashes and command
receipts. `tests/Performance` contains the durable harness and loop sources.

The isolated mapping comparison freshly translates baseline/candidate C with
identical frozen tools, runtime and I/O. It covers 1/64/1,024/4,096 mappings,
oldest/newest/random/interleaved/sequential/miss patterns, and anonymous, private,
mutable regular shared and memfd backing. At 1,024 anonymous mappings, oldest
lookup changes from 1,394 to 97 ns and random lookup from 745 to 138 ns.
At 4,096 mappings, random lookup is about 161 ns. The small helper uses C99
`inline`, which the backend translates to an inlining hint.
Filtered full-optimization JIT disassembly confirms that the first AVL helper
has an out-of-line call and the hinted helper has none. The inspected caller
plus helper changes from 234 + 189 bytes to a 393-byte caller. This confirms
this JIT decision; NativeAOT qualification and timing do not establish that every
NativeAOT caller was inlined. The inspected `JitlessDispatch` and `GetOp` methods
retain their 219- and 237-byte sizes before/after, respectively.

The first AVL candidate regressed newest/one-map lookup by roughly 5–14 ns.
The hint recovered the one-map and much of the newest cost. One isolated
1,024-map newest cell still rises from about 91 to 97 ns; this constant tradeoff
is retained explicitly. The bounded index is accepted with the integrated
complementary-workload improvements above, without adding cache lifetime and
accounting complexity to recover a few nanoseconds. See `mapping/inline-followup.txt`
and `mapping/ab/inline-b3n3ox2t/summary.json`.

A matched null-timeout Reason read changes from 7.18 ns [7.03–7.30] to 1.34 ns
[1.31–1.37]. This is a small isolated saving, not the explanation for the full
CoreCLR improvement. Pthread one-worker acquisitions change from 69.03 to
47.81 ns/pair. Longer four-worker private controls change from 114.72 to 58.75.
Initial 257-handle cache-thrashing regressions were retained and rejected; with
capacity fallback, the longer two-worker miss control is 59.05 → 59.29 ns/pair
with overlapping ranges. Four-worker miss observations remain too variable to
rank. Mutex state locks remain; the old Monitor stack percentage was not used
as CPU utilization or contention evidence.

## Guest NativeAOT and disposition

The unchanged minimal middleware source in `GuestKestrelMinimal` and
`CoreClrReadyToRun` publishes successfully with SDK 10.0.112, runtime 10.0.12,
Linux x64 glibc NativeAOT, invariant globalization and ordinary apphost output.
No service-code AOT adaptation is needed for this fixture. The historical
static-musl source/pin was inspected, but its ELF is not retained in this checkout.
The new glibc guest exercises the current ELF loader rather than relying on the
obsolete loader failure. Native Linux passes three health requests, two echo
requests and normal HTTP stop. The guest remains interpreted x86 code; compiling
the emulator host with NativeAOT is a separate axis.

The direct-owner before/after service comparison retains three fresh guests per
host/form, with one cold-host observation and two warm-host/cold-guest samples.
All baseline and candidate cases pass exact HTTP bodies, stdout and cleanup.
Candidate readiness is approximately 2.5–2.9 seconds after host warmup in these
forms. Readiness instruction snapshots are sampled after the stdout marker;
they are upper-bound observations, not an exact instruction at the write.
First and repeated request timing/counts, CPU, allocations and process peak
working set are separate fields in `nativeaot-service/summary.json`. Process peak
working set is a host high-water value, not peak guest backing consumption.

Matched NativeAOT-guest results below retain two warm-host/fresh-guest samples
per side after the separate cold-host observation. This small sample supports
the observed fixture improvement; it does not establish general service latency.

| Form / host | Before ready s, median [range] | After ready s, median [range] | Before → after total CPU s | Before → after first health s |
|---|---:|---:|---:|---:|
| Processed / JIT | 11.967 [11.652–12.283] | 2.507 [2.476–2.539] | 14.742 → 3.703 | 0.879 → 0.186 |
| Processed / NativeAOT | 14.668 [13.737–15.599] | 2.848 [2.834–2.863] | 17.555 → 3.774 | 1.186 → 0.196 |
| Raw / JIT | 11.849 [11.764–11.934] | 2.590 [2.590–2.590] | 14.726 → 3.799 | 0.880 → 0.195 |
| Raw / NativeAOT | 13.991 [13.416–14.565] | 2.913 [2.913–2.914] | 16.717 → 3.886 | 1.048 → 0.208 |

Public NativeAOT guest qualification also passes all eight raw/processed ×
JIT/NativeAOT host × InProcess/SeparateProcess cases, each with six exact HTTP
exchanges and normal resource/worker cleanup. Public readiness observes port
publication and includes import/start, so it is not pooled with the stdout-based
direct-owner measurement. `tests/PerformanceService` contains that consumer.

Current-producer service phase controls use the same processed NativeAOT host,
default environment and unchanged minimal service source. The following are
single cold-host observations, with a fresh guest in each process; they establish
the size of this fixture's differences, not an IL/R2R ranking or throughput result.
NativeAOT guest publication changes deployment/runtime contents as well as the
compilation mode, but needs no middleware source adaptation.

| Guest | Readiness s / sampled instructions | First health s | Next health s (two requests) | Total wall / CPU s | Peak host MiB |
|---|---:|---:|---:|---:|---:|
| IL/CoreCLR | 50.89 / 557,792,651 | 24.145 | 1.632 / 1.028 | 86.06 / 130.39 | 439.95 |
| R2R/CoreCLR | 51.86 / 558,146,916 | 23.265 | 1.627 / 0.998 | 87.07 / 134.02 | 440.16 |
| NativeAOT guest | 2.88 / 34,471,688 | 0.194 | 0.024 / 0.023 | 3.30 / 3.91 | 119.05 |

All three controls complete the same health/echo/stop exchanges, exact response
bodies and normal cleanup. Import is outside these intervals. Phase CPU and
instruction snapshots, all six request rows, allocations, host peak memory and
retained guest accounting are in `service-phases/summary.json`. Retained bytes
before release are not peak guest memory. HTTP scheduling changes total guest
instruction counts, so this table does not require identical counts across runs.
The selected public IL control and all three historical public R2R cells also
pass using the current producer. The ordinary CoreCLR controls still have slow
readiness and first health; this work does not qualify arbitrary ASP.NET services
or establish that local interpreter changes alone meet ordinary service goals.

The global shared-page lease lookup is measured but unchanged: about 18 ns for
one worker in isolation. Two/four-worker measurements distinguish shared-owner
serialization from separate owners, but no post-optimization service lookup
frequency or contention attribution supports a new lease-handle design here.
It would broaden lifetime, unbinding, truncation and registry ownership work.

Managed dispatch, decoded blocks and counter credits are deferred. The existing
512-slot instruction cache and 32-entry TLB remain. A post-change opcode/cache
profile is still needed before a block engine or larger dispatch rewrite;
alias writes, exact IP/flags/faults, stash locks, signals and budgets require a
separate design. The measured local mapping-helper hint is the selected helper
optimization in this changeset.

Native-code backend implementation is outside scope. The independent
`artifacts/jit-research/astra-65059c2-20261025/analysis.md` describes the proposed
unmanaged execution-frame ABI for Linux/Windows x64 and CoreCLR/NativeAOT hosts.
Its tiny Linux CoreCLR ABI probe does not qualify Windows, NativeAOT or a Blink
backend. No upstream JIT flag, executable guest backing or copied managed method
body was enabled here.

## Validation and evidence

The completed index is `artifacts/performance-implementation/evidence-index.json`.
Its `final-audit.py` rechecked all 369 producer inputs with zero mismatches and
all 22 generated raw/processed output files. The strict producer emitted all
110 objects with zero reuse. No generated C# was edited.

Validation passed: 61 pthread/runtime unit tests and 25 compiler/runtime tests;
native legacy/shared mapping ASan+UBSan and all eight mapping-index modes;
HostMemory and HostFileMapping in raw/processed JIT/NativeAOT; shared-memory
native and managed modes; ELF (104 cases); live shared-file coherence and
descriptor pinning; shared identity, virtual/backing limits and memfd aliases;
futex bitsets/shared-private identity, TLS, worker churn/cancellation and recursive
signals; process maps, segment reads and CPU identity; Machine API lifecycle,
simultaneous owners, natural deadline and exact instruction budget. Selected
functional SIMD/flags/cross-page controls pass hardware and baseline/current
managed execution. Pinned native Blink exits 1 on that SIMD control; its retained
unsuccessful comparison is not treated as an oracle for the managed result.

All 24 selected console/workload cells pass: host information, smoke and workload
across raw/processed, JIT/NativeAOT host and both public execution modes. Their
receipts are `artifacts/coreclr-runtime/guest/attempt-mwk95dpt/receipt.json` and
`attempt-s51z199i/receipt.json`. They verify the fresh producer but deliberately
select only these three scenarios; the runner's broader `fresh_qualification`
flag is false because it also requires other scenarios, including its service
and supplementary memfd cells. Separate semantic and service receipts supply
the gates described above. No full unselected CoreClrRuntime matrix is claimed.

The integrated comparison has 312 passing per-trial observations and 25 matched cells.
Public service qualification has eight NativeAOT-guest cells and four current
CoreCLR controls, with exact six-request behavior and disposed/worker-gone checks.
Both implementation children are terminal with no queued turns. The final audit
checked 394 recorded process groups: none live, no signals required by the final
gates. `git diff --check` passes. Branch and HEAD remain `sqlite` / `65059c2f…`;
changes are uncommitted and no push or PR was made.

Failed experiments and corrected harness/test assumptions are retained beside
successful receipts. Some first outer qualification logs used duplicate labels
and were overwritten; their nested failed source/input closures remain. The
NativeAOT publication provenance's early `guest_interpretation=pending` field is
superseded by the successful executions and completed evidence index. The index
lists these limits, exact source hashes and unsuccessful command rows explicitly.
