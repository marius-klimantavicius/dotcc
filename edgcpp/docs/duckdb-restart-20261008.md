# DuckDB restart point — 2026-10-08

Stopped at the user's request. No compiler, test, pipeline or worker operation
should be restarted until the user resumes work. All intermediates are retained.
Full DuckDB C# translation/build/execution has **not** succeeded.

## Committed and verified

- `c30aeb9bc`: recovered diagnostic identifies
  `pcg_detail::no_stream<uint64_t>::is_mcg`; original stale running receipt
  retained. Its exit code/time and launcher interruption cause are unknown.
- `f48dad769`: reviewed termcolor staging adapter, 13 canonical controls pass.
- `53323635f`: 24-file BCL socket/musl parser bundle integrated; canonical
  compiler rebuild, 55 socket/parser commands, existing network comparisons
  and 93 descriptor checks per JIT/NativeAOT mode pass on Linux x64.
- `0c1635706`: fresh checkpoint tools003, all 25 replay checks including AOT pass.

## Incomplete owned working-tree change

`duckdb/edg/portable.py` contains an uncommitted PCG staging adapter and call.
It adds exactly `static_assert(is_mcg, "PCG no_stream must use MCG")` after
the existing true bool in the pinned no_stream template. The isolated candidate
is independently reviewed and passed native and managed tests; canonical
reusable-verifier integration/revalidation and fresh staging are unfinished.
Do not silently treat this working-tree change as a committed milestone.
A copy of the diff is retained in
`/mnt/storage/dotcc-edg-campaign-20261003/duckdb-restart-handoff-20261008/`.

The assertion changes early-instantiation behavior for explicit member
specializations. Clang accepts a retained original counterexample and rejects
the adaptation; GCC rejects both. Adopt only for the audited pinned DuckDB
graph, which has no such uses, not as a generic PCG/library modification.
No EDG frontend semantics were changed or assertions suppressed.

## Evidence and pending drafts

All paths below are under `/mnt/storage/dotcc-edg-campaign-20261003/`.

- `duckdb-pcg-reduction-001/receipt.json`: tiny and real-header reproduction,
  native output comparisons, full source/preprocessed token audit, explicit
  specialization counterexample, and static/instance JIT/NativeAOT parity.
- `duckdb-pcg-independent-review-001/review.json`: independent scoped approval.
- `duckdb-pcg-reduction-001/reusable/`: unfinished reusable verifier and fixture
  drafts. Review current files and worker handoff before copying canonically.
  Latest provenance/RID draft includes full acquisition/runtime/PCG inventories,
  compiler invocation fingerprints and platform-derived AOT RID; this latest
  version has not executed. See `duckdb-pcg-reduction-001/pause-handoff.json`.
- `duckdb-socket-canonical-001/acceptance.json`: accepted canonical socket proof.
- `duckdb-il-replay-integrated-tools-003/tools.json`: fresh normal writer/reader.
- `duckdb-il-replay-integrated-003/results.json`: 25 checks pass.

The actual-header stock C PCG candidate clears correspondence962 but reaches
the separately retained `read_memory_region` limitation; do not call that full
C emission success. Tiny stock-C execution passes. The native PCG128 stream
roundtrip failure matches original/adapted and is excluded from passing
roundtrip assertions; RNG/state output comparisons remain separate.

## Stopped larger diagnostic

`duckdb-pcg-two-input-diagnostic-001` copies the old core and unity0012 .ii and
changes exactly one PCG line in each. This is diagnostic-only, not accepted
pipeline provenance or a complete source composition. User-requested stop
was sent through the isolated writer process group. EDG handled SIGTERM with
exit code4 and "Compilation terminated." after about150 seconds; peak RSS was
8,775,143,424 bytes. Writer2322678 and launcher2322675 no longer exist. Original
and copied input hashes are unchanged. Logs, stop request, raw/annotated
receipts and any partial IL are preserved; partial IL is not a valid checkpoint.

After the user resumes, the exact diagnostic can be rerun in a **new** directory:

```sh
mkdir /mnt/storage/dotcc-edg-campaign-20261003/duckdb-pcg-two-input-diagnostic-002
cp /mnt/storage/dotcc-edg-campaign-20261003/duckdb-pcg-two-input-diagnostic-001/run.py /mnt/storage/dotcc-edg-campaign-20261003/duckdb-pcg-two-input-diagnostic-002/run.py
python3 -B /mnt/storage/dotcc-edg-campaign-20261003/duckdb-pcg-two-input-diagnostic-002/run.py
```

This retains the 20-GiB compiler/16-GiB available-memory/15-minute diagnostic
guards and uses the existing supported DEBUG writer. Never rerun001 in place.

## Resume order

1. Read this handoff and worker receipts; inspect the owned portable.py diff.
2. Finish reusable PCG verifier integration and scoped canonical validation;
   retain the specialization limitation and commit only after those pass.
3. Complete the bounded two-input diagnostic before another hour-long full run.
4. Freeze all recipe/test filenames, then create a fresh complete source stage,
   preprocessing, provider and129-input composition. Socket headers and
   termcolor/PCG changes invalidate old fingerprints; do not relabel old .ii.
   Use pair-signature acquisition001 and existing verified native unity group
   evidence, preserving all2207 source TUs and seven static extensions.
5. Prepare a new launcher bound to tools003 (old launcher002 pins tools002).
   Preserve guard policies and provenance. Emit/checkpoint/build/execute only
   after preprocessing/composition acceptance.

The generic integral_constant overlay remains isolated/unadopted. The
compressed-pair address-retention candidate is rejected. Old partial Hugeint
review output remains linked; no failed/empty output replaced it.

Unrelated docs/plans, libc-wscanf, libuv, root .ti files and old untracked
backend tests/bindings remain untouched and unstaged. Coordinator owned Git
mutations through this stop; ownership returns to root with acknowledgment.
