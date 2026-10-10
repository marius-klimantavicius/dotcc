# DuckDB translation priority queue

This queue prioritizes observed shared blockers by the number of real units
currently stopped, the dependency order, and the scope of a correct fix.
Counts are first failures, not independent feature frequencies or promises of
successful generation after one fix. The selected sample is deliberately varied,
not statistically representative. Preserve all required source-type information;
do not omit metadata to bypass a gate. Re-rank after each expanded probe.

## Current queue (running full 2,635-unit native119 / managed134 inventory)

Snapshot: 1,712 processed, 1,670 packaged, 42 first failures. Exact retained
snapshot: `dotobj-full-failure-priorities-119-v50.json`. This inventory remains
frozen at v48. Accepted v49/v50 changes are measured separately, not projected
onto untested units. Profile50 clears all17 measured virtual array-delete units
and five previously managed-rejected units;23actual units plus2RTTI providers
now link/generate5,943 definitions. It does not complete the full project.

| Priority | Measured first blocker | Count | Current action |
| --- | --- | ---: | --- |
| 1 | Raw lowered record assignment / result construction | 9 + 2 | Lifetime worker stages exact complete-object add_bitwise_copy/add_init_assignment observations. Existing source-copy rules remain; accepted v49 covers other stock storage copies. |
| 2 | Source destruction lacks original object type | 6 | Supervisor inspects retained creation/capture provenance; preserve precise missing allocation/type reason before implementing ownership. |
| 3 | Member-function-pointer by-value representation transport | 2 | Base worker staged existing observed scalar ABI record transport; source member owner/signature remains distinct. |
| 4 | Result-buffer forwarding through receiver-adjusting wrapper | 1 | Base worker reduced exact secondary-base thunk; stage unchanged caller result pointer and only stock this adjustment. |
| Newly observed | Calling-method ABI; original parameter identity | 1 + 1 | Keep exact original declarations and physical calling slots; reduce after current shared transfer batch. |
| Newly observed | Array allocation rollback action coverage | 2 | Preserve cleanup ownership; investigate make_uniq_array/make_unsafe_uniq_array stock elision/rollback evidence. |
| Cleared in newer focused real probes | Virtual array delete; alias-array stride | 17 + 1 earlier first failures | v50 clears17actual units; v49 alias-array stride fixture+actual probe accepted. |
| Cleared managed family | Anonymous ctor spelling; opaque alignment; label lifetime overlap | 2 + 2 + 3 current rejects | v50 actual5probe covers first occurrences and all three mechanisms; later two label units await fresh export. |
| Required source-only metadata | Constrained dependent _Iter; dynamic typed exception specification | 3 + 1 latest libc++ source probes | Exact diagnostic121 identifies actual concept constraints. Preserve semantic constraints, not omitted source types; behind current project transfer families. |

Continue the full inventory concurrently with coherent focused milestones.
Production virtual-vector runtime closure remains later; generation acceptance
uses exact native/stock semantics and owned graph checks without fake providers.

## Previous queue (completed 500-unit native115 / managed126 inventory)

The expanded sample includes the previous 160 and spans 430 library, 48 test,
12 libc++ and 10 libc units. Completed: 456 packaged, 447 individually verified,
38 explicit exporter gates, one packaging failure and five test-configuration
failures; no exporter crashes. Receipt `dotobj-duckdb-expanded-summary-115.json`.
These are earlier v47 tools, not the final117/128 exception-policy query.
Final v47 is committed as `83f0e6dc0`, with 1,042 broad and 143 focused checks.

| Priority | Measured blocker | Affected selected units | Work and owner |
| --- | --- | ---: | --- |
| Integration prerequisite | Partial callback ABI storage has different physical observations across TUs | Combined real pair rejects ReplacementScan layout | Base worker: normalize only identical original semantic signatures, compare known decisions, preserve partial observations; actual calls stay gated until complete ABI. |
| 1 | Incomplete by-value external declaration signatures | 16 return + 4 parameter export failures | Base worker: source-owned deferred imported declarations; link to complete providers before calls. Current v48 batch. |
| 2 | Floating classification builtins treated as ordinary varargs | 4 managed rejects | Lifetime worker: exact EDG builtin identity, typed intrinsic f32/f64/f80, BCL/bit classification; unchanged varargs validation. Current v48 batch. |
| 3 | Primary virtual base carries vptr | 2 managed rejects | Supervisor: exact owned zero-offset primary placement; both actual Parquet units verify in scratch. Current v48 batch. |
| 4 | Record assignment / result construction | 4 assignment + 1 unique_ptr construction export failures | Lifetime worker: next scratch observation of selected copy/construction semantics. |
| 5 | Dependent source parameter constraints | 4 exporter gates in libc++ source | Required source metadata; behind selected library generation blockers, not a runtime implementation detour. |
| Small managed fixes | Constant byte-offset alignment; incomplete array storage view; RHS-first call order | 1 managed reject each | Supervisor alignment scratch verifies task_scheduler; base diagnoses fmt against stock C array-view rule; Arrow sequencing already passes final v47. |
| Later observed | Missing allocation object type; variadic indirect ABI; lambda parameter identity | 2 export failures each | Preserve precise allocation, ABI and parameter ownership; no omission workaround. |
| Later observed | Dependent constant expression; partial array rollback; contention-table layout | 1 export failure each | Required paths retained behind larger measured shared blockers. |
| Configuration/package | Static-array destructor observes typedef size zero; test POSIX frontend configuration | 1 packaging + 5 frontend failures | Distinct from EDG parsing capability; retain invocations and stderr. |

All counts are first failures: clearing a gate may reveal another, and these are
not additive promises of full generation. The next integration must measure
combined C# generation and expand toward the complete project, rather than
polishing this sample indefinitely. No runtime/libc++ closure detour is active.

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
