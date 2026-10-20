# Codec comparison

The benchmark calls the owning API against an ordinary copied original product
or the current generated product. Both variants use the **preserved original
owning API**, the same `CodecBench` source and the same build and runtime settings.
No generated source is changed by this harness.

The original snapshot is
`zstd/artifacts/optimizations/baseline-00373046c-20261020T021419Z`.
Its `identity.json`, `manifest.json` and `original-head.tar` preserve clean HEAD
`00373046cf90d8b44bb282f8760589e6417233fd`, the successful verification receipt
`20261019-204241-00db2fbc`, original raw/processed products (12 generated sources
each), authored API and consumer, root MSBuild metadata, upstream source and
archive, compiled verification artifacts and the original tool closures.
All 5,440 ordinary files were hashed and checked before the snapshot was made
read-only. There are no symlinks. Benchmark build intermediates and results live
outside that snapshot. The runner verifies every preserved file before running.

Run the original matrix first, in a new output directory:

```sh
DOTCC_CAMPAIGN_COMMAND_LOCKS=production python3 zstd/benchmarks/run.py \
  --baseline zstd/artifacts/optimizations/baseline-00373046c-20261020T021419Z \
  --output zstd/artifacts/optimizations/original-benchmark-20261020 \
  --variant original
```

After generating a candidate, compare in another new directory:

```sh
DOTCC_CAMPAIGN_COMMAND_LOCKS=production python3 zstd/benchmarks/run.py \
  --baseline zstd/artifacts/optimizations/baseline-00373046c-20261020T021419Z \
  --output zstd/artifacts/optimizations/candidate-benchmark-20261020 \
  --variant candidate \
  --compare-to zstd/artifacts/optimizations/original-benchmark-20261020/report.json
```

The default matrix is raw/processed × JIT/NativeAOT on Linux x64. Each cell uses
private outputs and intermediates. Builds use the production command lock and
one MSBuild node. Processes run serially on one CPU selected from the allowed
affinity. The report retains SDK/runtime information, CPU information, load,
affinity, source and binary hashes, exact commands and logs. `--forms`, `--modes`
and `--cpu` can narrow a run; before/after settings must match exactly.

The 29 workloads comprise compression and decompression of deterministic log
records at 4 KiB, 128 KiB and 1 MiB with levels 1, 3 and 9; mixed binary and
incompressible 128 KiB inputs at level 3; 128 KiB logs at level 19; 1 MiB streaming
with 32 KiB chunks; trained-dictionary 4 KiB records; and the allocating
convenience API roundtrip. Dictionary training, corpus generation and full-byte
validation run outside the timer. Every measured sample is followed by exact
roundtrip and deterministic-frame checks. Paired comparisons also require
identical input and compressed-frame hashes and lengths.

Context reuse workloads keep their codec contexts and destination buffers.
Streaming measures the public `Process` API, including per-frame reset and
pledged-size calls. The separate convenience workload includes context creation,
output allocation and both encode and decode. Throughput always uses original
input bytes per operation, including for this combined roundtrip. These cases
are reported separately so allocation costs do not become primitive speedups.

JIT runs explicitly set `DOTNET_TieredCompilation=0` for fully optimized code
without tier transitions; they still warm each case for at least 200 ms and
three calls. Calibration chooses a batch of at least approximately 100 ms,
then five measured batches run with full GC between batches. Reports retain
every timing, batch size, managed thread allocation count and GC count, then
calculate median, quartiles, median absolute deviation and min/max. The
allocation metric is managed bytes allocated on the benchmark thread; it does
not measure native allocator traffic or peak resident memory.

A paired candidate run reuses the original benchmark binaries after checking
their hashes, builds the candidate with the unchanged harness, then executes
original/candidate/candidate/original for each cell. Each process performs its
own warmup and calibration. Summaries pool ten measured batches per variant.
The initial original figures remain available independently. Timing is local
machine evidence, with dispersion retained; it is not a portability claim.

This benchmark supplements the complete correctness, native interchange,
upstream-case and delivery-audit matrices. A faster benchmark cannot substitute
for those gates.
