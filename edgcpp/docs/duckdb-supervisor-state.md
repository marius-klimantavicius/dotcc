# DuckDB translation supervisor — resumed 2026-10-10

The user authorized continuous coordinated implementation of complete DuckDB
translation. First finish EDG/owned `.dotobj` semantic export, packaging,
linking and readable C# generation for the complete project. Only then complete
missing libc++/shared runtime bindings and compiled JIT/NativeAOT execution.
Earlier phase-boundary stop instructions and the old merged-IL restart recipe
are superseded. There is no authorization to weaken semantic checks or omit
required bodies, initialization, destruction or exception handling.

## Ownership and current work

Supervisor `/root/duckdb_supervisor` owns build/prepare scheduling, integration,
project probes, durable evidence and Git staging/commits. Root is read-only while
that ownership is active. Native edits are canonical `backend/` files; generated
upstream overlays and unrelated existing work remain unstaged.

- `lifetime_export`: original DuckDB `exception.cpp`/`date.cpp` unsupported
  exception construct kind 10, then `allocator.cpp` initialization with destructor.
  Owns lifetime portions of `dot_gen_object.h` and focused lifecycle fixtures.
- `base_provenance`: original DuckDB `string_util.cpp` physical base lacking direct
  original declaration. Owns record/base portions of `dot_gen_object.h`, relevant
  source-base capture and focused base fixtures.
- Supervisor: schema/managed integration as required, full-manifest provenance,
  broader bounded-memory frontier probes and milestone acceptance.

Workers consult stock EDG C lowering/backend before choosing representations.
Overlapping native files have function-level ownership, with edits coordinated.
Workers do not run preparation, compiler builds or Git mutations.

## Baseline and project identity

- Baseline commit `35030a956`, object profile `scalar-control-v37`.
- Persistent root `/mnt/storage/dotcc-edg-campaign-20261003`.
- Frozen native `dotobj-native075-verified-snapshot/cpfe-dot`.
- Frozen managed `dotobj-project-managed-build-085/bin/DotObj.Linker/debug/dotobj-link.dll`.
- Full manifest `dotobj-duckdb-runtime-manifest-003/project.json`: **2,635 units**,
  bundled dependencies, seven static extensions, native extension loading disabled.
- Five-unit `dotobj-duckdb-frontier-075-types/results.json` is a sample only.
  `hugeint.cpp` packages and generates 280 definitions; four other units reject
  the gates assigned above. No complete DuckDB generation/build/run is claimed.
- Existing source stage `duckdb-portable-correspondence-001/source`; preserve its
  pinned acquisition/adapter provenance. Do not adopt unrelated uncommitted
  `duckdb/edg/portable.py` changes silently.

## Acceptance and continuation

1. Capture exact original/lowered semantics for current gates, add native and
   stock-C reference fixtures, validate owned metadata and malformed inputs.
2. Freeze tools, run focused and appropriate broad regressions, rerun the original
   failing units, retain failure evidence and commit the verified milestone.
3. Expand beyond the sample across the full manifest; advance exporter, verifier,
   linker and emitter failure frontiers until every required unit is accounted
   for and complete C# is emitted without source/EDG access during linking.
4. Then implement remaining libc++/runtime requirements; verify native DuckDB
   behavior/tests under managed JIT and NativeAOT, recording tested platforms.

Use independent original-source per-unit EDG invocations, initially at most two
workers with 8-GiB per-worker caps. Never restore whole-program IL union or an
external preprocessing pipeline. Preserve intermediates and monitor storage.
Generation, compilation and runtime achievements are separate. Update this file
and the campaign ledger at significant transitions; do not stop at a small
milestone unless the user pauses or a genuine external blocker prevents progress.

## Live frontier after first implementation build

Native076/managed086 implements internal try expressions plus recursive original
base/field ID reservation. The base regression passes14 checks; all four failing
sample units advance. `string_util.cpp` now needs adjusting/covariant wrapper
provenance. `exception.cpp` and `date.cpp` now need source allocation-release
obligations (these are not destructor obligations); the placement-new focused
fixture reproduces the same missing capture. `allocator.cpp` advances to an
unsupported cleanup action flag. The native/stock-C lifetime oracle passes12
scenarios but owned export is not yet accepted. Broad regression076/086 is running.

Worker assignments continue: base worker owns exact stock wrapper provenance;
lifetime worker owns allocation-release source obligations and their internal-try
correspondence. Supervisor integrates additive schema/managed contracts. These
features must retain original obligation types and boundaries rather than hiding
allocation rollback among fake destructors. All076 attempts are retained.

## Accepted v38 milestone and next work

Final native078/managed088:1,042 broad JIT checks,37 lifetime checks/24 native+
stock-C/JIT/NativeAOT scenarios,14 base checks,18 thunk metadata/link checks.
Date now packages but verifier rejects its narrow switch selector. Hugeint still
generates. The 38-subsystem sample packages17 units and all17 independently emit;
combined linking rejects fabricated anonymous-record counter identities.
Evidence: `evidence/dotobj-duckdb-boundaries-20261010.json`.

Next ownership: base worker fixes stable original anonymous nested record identity
and later record storage initialization; lifetime worker implements ordinary-new
rollback and local-static guard cleanup actions; supervisor integrates schema,
managed verification/emission and constexpr/switch selection. Native078 sources
and all v38 validation are frozen for the milestone commit. No libc++ runtime
closure work starts before full generation. New rollback/storage-init fixtures
are next-phase preparation and are excluded from this commit.

## v39 integration in progress

Native079/managed089 built. Stable anonymous-member identities pass35 focused
checks including source-hidden multiunit linking and execution. Constexpr branch
selection and narrow integer switch promotion pass34 checks against native and
stock C, JIT and NativeAOT. Shared BCL guard runtime passes16 direct ABI checks,
including contention/publication, abort/retry and NativeAOT. These are focused
results, not full DuckDB acceptance.

Allocation/guard semantic actions are now represented separately from object
destructor and handler cleanup states. Native080 corrects the stock sized-delete
supplement's zero element-count convention. Original guarded-local-static capture
is still under investigation; full translated rollback acceptance is pending.
Workers: lifetime worker owns guard/allocation capture and acceptance; base worker
investigates original file-scope destruction provenance for the next project gate.
Supervisor owns schema/action verification, emission, selection closure, tools,
project batches and milestone commits. v39 is not yet committed.

## v39 focused acceptance and final verification

Native084/managed090 are frozen. Native080 broad suite passed 1,042 checks; final
native084 broad rerun passed 1,042 checks at `dotobj-full-v39-002`. Focused acceptance:
35anonymous identity,34constexpr/switch,16shared guard ABI,65allocation/localstatic
rollback (native/stock sequential,JIT/AOT,32thread publication),19foldedvoid.
Review link `artifacts/review/dotobj-duckdb-v39` contains retained source outputs.
The38unit sample now packages22 and independently emits21. Combined linking
passes anonymous identity and reaches record transport. Fiveunit final084 sample
packages3:date/hugeint emit,string_util verifier rejects discarded recordplace.
Exception still has physical byvalue transport gate. Allocator now reaches exact
missing ABI for stock make_cxa_dtor_type(void(void*)) used by __cxa_atexit.

Nextworker findings are preserved but excluded from v39implementation acceptance:
`docs/dotobj-global-lifetime-design.md` and27native/stock oraclechecks; global/static
registration must occur at observed successful registration call, including local
static registration before guard publication. Source original global lifetime
ownership and generated array destructor callback provenance must be explicit.
Baseworker also identified string_util's Split nonfinal sequence operand as a
discarded noncopyable basic_istream place, currently incorrectly treated as value
transport by managed verification. No EDG semantic defect is alleged.

V39 acceptance is complete and committed as the allocation/guard/selection
milestone. Native084 and managed090 are immutable acceptance tools. Continue
with the next source/static lifetime and record-value gates; full DuckDB
generation and runtime verification are still incomplete.
