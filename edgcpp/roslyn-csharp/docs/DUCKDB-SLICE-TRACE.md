# Bounded DuckDB core slice trace

Status: the bounded source-3 trace now passes with four roots, 404 operation
identities, 40 calls and zero issues. [The compact evidence](../acceptance/duckdb/slice-trace-report.json)
preserves the first compile failure and source-2 partial trace. This is
public-reader evidence for choosing the next adapter, with no translator,
linker or execution acceptance claim.

The input is the unchanged configured core entry 316,
`/mnt/storage/roslynobj-duckdb-20261014/export/major2/0316/unit.roslynobj`,
156,354,560 bytes, SHA-256
`cf385364234784aa0d748781d06f74161ad51d28a6a34215449c3ae212810b65`.
The accepted [core corpus](../../roslyn/docs/DUCKDB.md) and
[sampler receipt](../../roslyn/docs/duckdb-project-sampler-ready.json) establish
four untruncated retained `And`/`Or` roots, 404 sampled operation identities and
40 call identities. Those counts describe the prior reader sample, not a typed
execution schedule or required dependency closure. The 164 core objects remain
independent inputs and are not a complete linked DuckDB program.

[DuckdbSliceFacts](../tests/DuckdbSliceFacts/Program.cs) repeats only the two exact
public retained-name/function-kind selectors. It requires the same two defined
functions per name and four distinct roots, records their current public
identities, and follows selected typed containment facets. It never traverses
callee bodies, enumerates all TU functions or scans the other 163 objects.
Names locate the sample; they do not perform source-language lookup or rebinding.

The report separates distinct operation identities from repeated typed paths,
raw operand order constraints from evaluation schedules, and association-only
metadata from executable occurrences. Receiver/result/argument call facets add
no duplicate traversal. Independent structural child auditing matches identity,
role and ordinal; unclassified execution relations remain unknown and are never
used as fallback execution edges. Association cycles are deduplicated normally;
an active execution containment cycle makes the trace incomplete.

One-hop dependencies include direct routine references, retained selected targets
even with nontrivial callees, unresolved indirect sets, referenced storage and
actual exceptional cleanup targets. The output retains body/declaration state,
signatures, hidden roles, layouts and initialization/lifetime associations.
Reference counts are not runtime invocation evidence. Related type/constant
descriptions are bounded metadata fields, not a second semantic model or a
claim of complete project closure.

The initial bounds are one object; 64 materialized name matches per key; four
symbols per key; four roots; 256 distinct operation identities and 4,096 typed
occurrences per root; depth 128; 256 combined function/storage dependencies;
256 related types; 64 fields per type; 32 parameters; 128 locals, 64 regions,
128 cleanup actions and 128 recipes per root; 256 constant units/elements.
Reader caches are 4,096 rows and 128 pages of 128 rows. Observed ceilings are
20,000 commands, 100,000 rows and batch 900. The actual report repeats all limits.
Paged name materialization does not guarantee a bound on internal search work.
The coordinator separately enforces 120 seconds and 1 GiB process memory.

The tool records before/after input SHA, exact manifest/target, public local
locators, reader counters, partial data and explicit issues. Unknown execution
forms, mismatched child accounting or bounds yield `traceComplete=false` and
exit 3. They narrow the evidence instead of silently expanding support. Exit 0
means the bounded selected containment trace completed; `translatorAccepted`,
`linkClosureComplete` and `generatedExecutionAccepted` remain false.

The immutable partial report `t5/duckdb-slice-facts-2.json` records all four roots,
404 operation identities, 40 call identities and 192 related types in 1.31 s at
about 99 MB, with no budget violation. Its three audit issues per root are exact
public facet labels: `Conditional.Condition` is structural `operand/0`,
`Lifetime.Value` is `body/0`, and `Throw.Operand` is `operand/1`. Source 3 adjusts
only those audit aliases, preserving typed traversal paths and ordering, and
records the retained throw-payload destructor as a one-hop dependency without
walking its body. The partial report is not overwritten or promoted to complete.

Coordinator commands, with fresh snapshot/build/report paths:

```sh
dotnet build tests/DuckdbSliceFacts/DuckdbSliceFacts.csproj -c Release --disable-build-servers
dotnet tests/DuckdbSliceFacts/bin/Release/net10.0/DuckdbSliceFacts.dll \
  --input /mnt/storage/roslynobj-duckdb-20261014/export/major2/0316/unit.roslynobj \
  --output /mnt/storage/roslynobj-csharp-20261015/t5/NEW-duckdb-slice-facts.json
```

Build and run through `tools/run-measured.py`; preserve nonzero partial reports
and failed attempts. No producer rebuild or object export is involved. A new
genuine scalar-observation fixture and adapter should be selected only after
the actual trace identifies its smallest meaningful missing requirement.


## Observed next requirement

Final measured build/trace take 5.18/1.21 seconds with no budget violations.
Input and 49-file snapshot hashes verify. Reader counters are 4,750 commands,
6,519 rows, cache 4,096 rows/128 pages and maximum batch 10. The final report is
`t5/duckdb-slice-facts-3.json`, SHA-256
`c4e89c8346ecda3abe07f000391e5c68da4e22ecc97c6a75527492d556cf81b8`.
It records 13 one-hop function dependencies and 193 related types. The frozen
snapshot contains this note's pre-run version; this current result text is
attributed separately from unchanged executable/build sources.

All four roots contain an actual class throw of retained `InternalException`
(size/alignment 16/8), with selected complete D1 destructor, defined body and
noexcept this-only signature. Each has two actual direct-variable exceptional
cleanup regions for string/allocator temporary storage. Current signatures
include const Vector references and a mutable output Vector reference. A
selected declaration-only `size` dependency remains outside the sampled closure.
These are observed forms, not evidence they already translate or run.

The next capture should isolate same-type class payload ownership: selected copy
construction into exception storage, recorded payload destruction before final
free, and a borrowed const-reference catch. A small canary/int Payload avoids
claiming the full polymorphic InternalException/base/string/catch hierarchy.
Actual layout, initializer/receiver/result roles, catch binding and exact type
identity must be inspected before a representation or runtime change. Failed
payload-copy construction, by-value catch copies, class rethrow, native unwinding
and whole-project linking remain separate obligations.
