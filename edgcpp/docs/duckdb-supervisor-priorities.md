# DuckDB translation priority queue

This queue prioritizes observed shared blockers by the number of real units
currently stopped, the dependency order, and the scope of a correct fix.
Counts are first failures, not independent feature frequencies or promises of
successful generation after one fix. The selected sample is deliberately varied,
not statistically representative. Preserve all required source-type information;
do not omit metadata to bypass a gate. Re-rank after each expanded probe.

## Current queue (native113 / managed124)

The complete identical 160-unit checkpoint is
`dotobj-duckdb-expanded-summary-113.json` in persistent campaign storage:
135 packaged and 127 individually verified, versus 128 packaged and 82 verified
with native112/managed123. The 45 additional verifiable units demonstrate the
benefit of batching the previous 33 static-lifetime ordering, nine generated
exception-specification and eight field-destructor first failures. Some units
advance to another gate; these counts are not additive promises.

| Priority | Observed blocker | Current affected sample units | Work and owner |
| --- | --- | ---: | --- |
| Integration prerequisite | Defaulted exception specification unresolved in one TU, resolved in another | Combined 48-unit generation rejects one shared vector constructor signature | Lifetime worker: inspect stock semantic completion API while original frontend context is valid; preserve provenance, never ignore signature disagreement. |
| 1 | By-value transport | 7 return + 2 parameter export failures | Lifetime worker after current integration: inspect selected ABI and stock C transport. |
| 2 | Discarded record expression context | 3 nested initialization sequences + 1 for-step reference expression | Supervisor: propagate semantic discard through sequence tails without permitting copies; base worker validates native/stock reducers. |
| 3 | Uninitialized placement-new rollback ownership | 2 former crash units now precise export gates | Base worker: retain exact original allocation ownership and proof that no initialization/rollback is executed. |
| Later observed | Record assignment | 2 export failures | Preserve selected source construction/assignment distinction. |
| Later observed | Lambda parameter correspondence | 2 export failures | Preserve original lambda/invoker ownership. |
| Later observed | Virtual base, signbit intrinsic, cached storage type, initialization evaluation order | 1 managed reject each | Keep exact evidence; promote shared causes when a wider scan measures impact. |
| Later observed | Uninitialized array allocation, variadic result ABI, partial fixed-array rollback, constrained source parameter, contention-table layout | 1 export failure each | Required project paths, not discarded; defer behind larger measured families. |
| Configuration/package | Zero-size assembly operand packaging and test frontend configuration | 1 packaging failure + 4 test configuration diagnostics | Explicitly distinct from EDG semantic capability; retain original stderr and invocation. |

Source-only template/formal and nested-declaration blockers were prerequisites
for measuring these downstream execution and linking errors. Their required type
information remains represented. No libc++ runtime implementation detour is in
this queue. The first whole-project scan supplied historical priorities; later
selection deliberately spreads over path/role groups and is not statistical.
After the next coherent integration, expand beyond these 160 units and rerank
against new evidence rather than repeatedly polishing the same selected units.

## Historical whole-project evidence

The frozen094 baseline processed all 2,635 units: 1,402 packaged, 1,223 explicit
exporter gates, 2 exporter crashes and 8 test-configuration diagnostics. Its
largest first-failure categories were 316 static partial-aggregate containment,
257 allocation ownership, 163 virtual-base correspondence, 135 by-value return,
80 empty asm operand constraints and 65 missing destruction ownership.
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
