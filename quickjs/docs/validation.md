# Validation results

**P0–P6 passed on Linux x64**, using .NET SDK 10.0.111 and Bellard QuickJS
2026-06-04. The final [verification receipt](../artifacts/campaign/20260927-062444-292bece5/receipt.json)
records fresh full-source translation, raw and processed builds, all seven
suites, all four execution cells and the final dependency/distribution audit.

The final run used the absolute wrapper with local source inputs,
the default `warn` provenance policy and the final compiler snapshot. The link
uses the shared 256 KB split, literal-pool and inline-deduplication defaults:

```bash
bash /home/marius/p/dotcc/quickjs/scripts/verify.sh \
  --tools reuse --fetch never --form all --mode all --rid linux-x64 --jobs 2
```

| Suite / embedded profile | Raw JIT | Processed JIT | Raw NativeAOT | Processed NativeAOT |
| --- | --- | --- | --- | --- |
| Separate useful consumer, two fresh runs | Passed | Passed | Passed | Passed |
| Actual ABI/allocator, 5 cases | Passed | Passed | Passed | Passed |
| Behavior, 5 harness + 16 JavaScript cases | Passed | Passed | Passed | Passed |
| Lifetimes/limits/concurrency, 23 cases | Passed | Passed | Passed | Passed |
| Upstream, 76 functions + cyclic module | Passed | Passed | Passed | Passed |
| Binary, dependency and notice audit | Passed | Passed | Passed | Passed |

Each consumer run produces exactly `{"values":[4,6,10],"sum":20}` and checks
three managed callbacks, one imported module, settled Promise work and zero
owned allocations after disposal. Every managed harness first proves that a
deliberately failing assertion exits with code 1 and the expected message.
Case reports are retained beside the receipt as `abi-cases.json`,
`behavior-cases.json`, `lifecycle-cases.json` and `upstream-cases.json`.

The [native control report](../artifacts/campaign/20260927-062444-292bece5/native/native-report.json)
passes 36/36 records. Stock computed-goto and embedded switch controls, both with Atomics enabled,
agree on the exact workflow and actual-header ABI transcripts, selected upstream
cases, authored features and error controls. The single selected-test exclusion
is `test_builtin.js:test_finalization_registry`, which requires the excluded
`os.setTimeout` event loop. See [the complete inventory](upstream-tests.md).

The [dependency audit](../artifacts/campaign/20260927-062444-292bece5/dependency-audit.json)
examines actual PE references/imports, current consumer manifests, exact NativeAOT
input assemblies, ELF dependencies and the executed output directories. No native
QuickJS or alternate JavaScript backend is delivered. Each rooted AOT map contains
3,597 compiled translated method bodies, including evaluator, closure, RegExp,
Unicode, BigInt, upstream ownership-inline and Atomics code. The ordinary consumer uses normal
trimming. Both products (14 generated C# files each) and actual built/published applications
preserve the archive license, version and 14 additional source notices. The
five selected header inlines are exported once under their upstream names; no
`DotCC_JS_*` forwarding wrappers or extra bridge translation unit remain. All
five original inline bodies and the Atomics load/store, read-modify-write, wait,
notify, lock-free and pause paths are present in each rooted AOT map. Authored
controls cover all eight integer/BigInt typed views and adjacent-memory guards.
Independent runtimes share test-owned backing for Int32 and BigInt64 concurrent
increments and wait/notify; both finalizers run and owned allocations return to
zero. The facade keeps upstream `can_block=false`; only raw test hosts opt in
to blocking waits.

[Branch-hint evidence](../artifacts/branch-hint-overrides-20260927/report.json)
records all four source definitions with literal expected values and all five
object comparisons: 426 `__builtin_expect` calls become zero. Guarded replacements
retain `(!!(x))` and evaluate the first value once. Native controls retain their
original branch hints; all behavioral comparisons and four execution cells pass.

[Disposable reproduction evidence](../artifacts/reproduction-branch-hints-20260927/report.json)
exercises actual shared acquisition, staging, full-core object emission and
transactional publication using legitimately edited local inputs. `warn` and
`off` accept the edits without network fetches; strict checks reject provenance
drift, and the dispatch macro-override guard rejects a changed original macro
body in both permissive modes before any object is emitted. The branch-hint
guard also rejects an expected argument changed from a literal to a function
call, under both policies. Clean and
repeat publication preserve unrelated files; failed publication retains the
previous product. Authored implementation and acquired-source hashes are unchanged.
The original-path linked host uses the upstream Atomics-enabled class-ID mutex;
concurrent runtime lifecycle tests exercise registration and disposal.
An additional [absolute-path fetch from /tmp](../artifacts/campaign/20260926-212617-218acabe/receipt.json)
passes with `--fetch never --hashes off`.

[Shared regression evidence](../artifacts/shared-regressions/report.json) retains
the earlier 2,819-test unit pass, affected native/emitted fixtures, before/after
failure controls and focused runtime/compiler tests. Verification of existing
projects is recorded separately in the
[cross-project verification report](../../docs/plans/translation-regressions-2026-09-27.md).
The [current repository suites](../artifacts/repository-regressions-atomics-20260927/report.json)
pass 2,828 unit tests, 731 functional tests
(1,179 opt-in/platform skips), and retain the unchanged 103-test postprocessor pass. The
collectible literal-storage test runs apart from other tests that enumerate
loaded assemblies, and joins its concurrent first-access workers before checking
unloading; its lifetime and concurrent-initialization assertions remain enabled.
[Recipe checks](../artifacts/recipe-validation/report.json) retain framework,
layout, script and staging validation. Resolved defects and their regressions are
listed in [the blocker ledger](blockers.md); failed historical attempts remain
available and are superseded by the final passing receipt.

Qualification covers the declared embedded Linux x64 profile. Other targets,
external Test262, shell/worker APIs and public bytecode delivery remain outside
scope. The completed `__js_rc` investigation is recorded in
[the plan](PLAN.md#follow-ups). Enabling the recipe's missing
`--deduplicate-inline` option leaves exactly one `__js_rc` in each product;
the final audit enforces this. The [focused evidence](../artifacts/inline-deduplication/report.json)
records 33 passing inline unit tests and six managed execution cases, including
header-pointer arithmetic, distinct function addresses and independent state.
