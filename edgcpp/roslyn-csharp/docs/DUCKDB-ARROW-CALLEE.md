# Bounded DuckDB arrow-callee trace

The measured `arrow-callee` public-reader trace passes within its one-body,
shallow-type scope: 11 operation identities, 11 typed occurrences, 16 target
types, one declared dependency and zero issues. ProbeVersion 10 records 40
separate selection-type associations and `targetTypeQueueInitiallyEmpty=true`.
This is inspection evidence, with no product, link, generated execution,
ownership or closed no-throw acceptance.

The [compact receipt](../acceptance/duckdb/arrow-callee-report.json) is 76,834
bytes, SHA-256
`eabbebcdc1e37da45b916e6a0e71afb0ec2580d82007283690eeb243564ec9b8`.

The immutable facts are
`/mnt/storage/roslynobj-csharp-20261015/t5/duckdb-arrow-callee-facts-1.json`
(89,446 bytes). The unchanged input remains configured core entry 316,
`/mnt/storage/roslynobj-duckdb-20261014/export/major2/0316/unit.roslynobj`
(156,354,560 bytes),
SHA-256 `cf385364234784aa0d748781d06f74161ad51d28a6a34215449c3ae212810b65`.
No other unit is inspected.

## Selection and type scope

The tool reproduces the current public And-to-accessor identity intersection,
then the accessor's unsigned-byte call and pointer-returning receiver call.
Within that pointer body, its actual return-call context selects a defined
ordinary-pointer callee. That body's sole executable direct call selects the
body traced here. Each step requires callee Reference.Target, TargetMethod and
selected signature identity to agree. Names are display facts; old report
locators are not handles or selectors.

And inventories have 117/85 identities; subsequent parent inventories have
22, 35 and 11. These and receiver operand inventories are selection-only,
outside the selected body's counts. During selection, type references record
stable public-equality locators in a separate association set without entering
the target type dictionary or queue. The tool asserts both are empty, then
enables normal target type collection immediately before the sole `TraceRoot`.
Parent-only type locators stay explicitly unexpanded. The two type counts do
not imply disjoint identities or complete metadata closure.

## Actual selected body

All labels below belong only to this report. Selected Function 104 (`_M_get`
for display) is defined `sfk_none`/`cdk_none`, fixed, default-convention C++
with one `this_parameter` and no hidden result/copy role. Signature 105 declares
ordinary pointer result 68, natural 8/8. Receiver pointer 82 points to const
135 over fieldless record 81, complete/nonvirtual size/alignment 1/1. Execution
symbol 108 has separate top-level const pointer-object type 110 over pointer 82.

Block 109 contains block 117. The retained operations are:

```text
RTL assignment 120 -> automatic pointer local 114, type 115
  LTR direct call 122 -> selected get Function 123 / signature 124
    callee Reference 129 == selected target
    receiver cast 130, type 125
      Reference 132 -> this symbol 108, value type 82
Return 119 -> Reference 133 -> the same local 114, value type 115
```

Receiver 130 is argument 0, `this_parameter` descriptor 126 and raw operand 1;
callee 129 is raw operand 0. These aliases add no evaluations. Its cast targets
pointer 125 to const 89 over `__shared_ptr` record 98, complete/nonvirtual 16/8.
The report retains that cast from the distinct fieldless receiver view; it does
not establish a source base conversion, adjustment formula or permission to
reinterpret arbitrary records.

Declared result pointer 68 reaches element_type alias 134 then VectorBuffer 48.
Call/local/returned pointer 115 instead reaches element_type 136, type alias
141, then that same record. Both pointers are ordinary 8/8, but their identities
and alias chains remain distinct. VectorBuffer's captured shallow extent is
56/8; field/base/table associations are not recursively expanded.

Local 114 is automatic `initk_none` without an initializer association. The
explicit assignment performs initialization. There are no retained lifetime
associations, EH tables, states, prologue, epilogues or regions. Traced volatile
and virtual-call flags are false. Captured noexcept is true for the selected
function and its declared callee; neither fact proves recursive behavior.
The callee's body association 128 is **not traversed**. Earlier get-body evidence
remains separately attributed in [the partial batch](DUCKDB-TAIL-DEPENDENCIES.md);
this run adds no recursive body or cross-report identity claim.

## Bounds and immutable attribution

Target limits remain 64 identities, 256 occurrences, depth 32, 16 dependencies,
64 shallow types, 32 fields and 16 parameters; reader budgets are 10,000 commands
and 20,000 rows. Actual counts are 1,068 commands, 2,774 rows and maximum batch
12. Build and trace both exit zero without a budget violation. Measured elapsed
times are 7.212/0.904 seconds, with sampled process-tree RSS
465,887,232/90,857,472 bytes. Budgets are 600 seconds/4 GiB for the build and
120 seconds/1 GiB for the trace.

Artifact paths below are relative to
`/mnt/storage/roslynobj-csharp-20261015/t5/`:

| Artifact | SHA-256 |
| --- | --- |
| `duckdb-arrow-callee-facts-1.json` | `59e7ea04a4abda8c80408ecd71f4017bed80710084572d1200ec6ee2b02335f4` |
| `duckdb-arrow-callee-source-1/source-snapshot.json` (48 files) | `1de1201b2a50c36410d4a9721f4072c55e732987b8406856452b9f9a75a69aff` |
| Frozen `tests/DuckdbSliceFacts/Program.cs` | `922dabd924c12de7e16e47a0fb239cc1209f88de09bfac7339ed10c511ecf4cb` |
| `duckdb-arrow-callee-build-1/run.json` | `5bb177cd8fb72f6f98dfbab60161694371e90fc1cf81d2690139c543e8c7e943` |
| `duckdb-arrow-callee-trace-1/run.json` | `2cf8978988c520cb0c78cbbeba6faa5938f1b64516c55eb86affd6bae0817598` |

`traceComplete=true` concerns this bounded typed body and shallow queue only.
`fullRelatedTypeClosure`, `closedNoThrowBodyProof`, `translatorAccepted`,
`linkClosureComplete` and `generatedExecutionAccepted` remain false. Prior
partial reports are preserved; no cap increase, repeated batch or new product
admission is implied.
