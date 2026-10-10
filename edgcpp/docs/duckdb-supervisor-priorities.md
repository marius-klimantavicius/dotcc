# DuckDB translation priority queue

This queue prioritizes observed shared blockers by the number of real units
currently stopped, the dependency order, and the scope of a correct fix.
Counts are first failures, not independent feature frequencies or promises of
successful generation after one fix. The selected sample is deliberately varied,
not statistically representative. Preserve all required source-type information;
do not omit metadata to bypass a gate. Re-rank after each expanded probe.

## Current queue (native112 / managed123)

The complete 160-unit checkpoint is `dotobj-duckdb-expanded-summary-112.json`
in persistent campaign storage: 128 packaged and 82 verified. The earlier
99-unit ranking snapshot is retained separately. Packaging failures comprise
25 explicit exporter gates, two exporter diagnostic crashes, one SQLite
packaging constraint failure and four frontend/configuration diagnostics.

| Priority | Observed blocker | Current affected sample units | Work and owner |
| --- | --- | ---: | --- |
| 1 | Static lifetime sibling ordering | 33 managed rejects | Supervisor: retain exact child-list position when an earlier captured field subtree is revisited; no verifier relaxation. |
| 2 | Generated/defaulted exception-spec finalization | 9 managed rejects | Lifetime worker: observe exact stock finalization and preserve initial versus resolved semantic policy. |
| 3 | Default-member destructor finalization | 8 export failures | Base worker: capture finalized original selection at pre-copy boundary; exact reducer already reproduces. |
| 4 | By-value transport | 7 return + 2 parameter export failures | Next shared execution family after current integration; inspect selected ABI and stock C transport. |
| Parallel diagnostic | Exporter range exceptions | 2 export crashes | Base worker: exact stack/reducer for task_scheduler and buffer_pool; inventory integrity and potentially shared zero-ID handling. |
| Later observed | Virtual-base metadata, discarded record sequence, signbit intrinsic, uninitialized array allocation | 1 observed unit each so far | Keep exact evidence; promote if expanded counts or shared root causes warrant. |

The previous common source-only blockers were dependent templates/formals and
original nested declarations. They had to be represented before downstream
execution and linking failures could be measured. The same 33-unit sample went
from 9 packaged before nested-type support to most units reaching verification;
the expanded sample now discovers execution/lifetime families. This was a
prerequisite, not a libc++ runtime implementation detour.

## Historical whole-project evidence

The frozen094 baseline processed all 2,635 units: 1,402 packaged, 1,223 explicit
exporter gates, 2 exporter crashes and 8 test-configuration diagnostics. Its
largest first-failure categories were 316 static partial-aggregate containment,
257 allocation ownership,163 virtual-base correspondence,135 by-value return,
80 empty asm operand constraints and65 missing destruction ownership.
These are old-tool counts. Later changes advanced these families in selected
real units, but only a new whole-project run can establish current full totals.
Do not present those historical counts as remaining current failures, or claim
that clearing one representative fixes its entire historical category.

## Scheduling and acceptance

Batch related source-capture fixes, use focused stock/native oracles and owned
metadata rejection checks, then expand real-unit packaging/verification and
combined C# generation. Run broad/AOT integration checks when lifecycle/runtime
semantics change substantially; avoid re-running the whole suite for every
metadata edit. Keep rare unrelated language/library features and libc++ runtime
closure behind the current full-generation checkpoint. Both historical crash
units remain explicitly selected. Later stages still include full linking,
C# compilation, shared runtime closure, NativeAOT and DuckDB execution/tests.
