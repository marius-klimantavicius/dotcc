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


## Requirements observed in trace 3

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

At that checkpoint the next capture was same-type class payload ownership: selected copy
construction into exception storage, recorded payload destruction before final
free, and a borrowed const-reference catch. A small canary/int Payload avoids
claiming the full polymorphic InternalException/base/string/catch hierarchy.
Actual layout, initializer/receiver/result roles, catch binding and exact type
identity must be inspected before a representation or runtime change. Failed
payload-copy construction, by-value catch copies, class rethrow, native unwinding
and whole-project linking remain separate obligations.

## Focused accessor inspection

The optional `--mode accessor` selects defined `GetVectorType` candidates
through public `FindSymbols` in the same pinned entry 316. The first immutable
attempt correctly stopped with an incomplete report because there are two
defined candidates; its default-mode regression reproduced accepted trace 3.
The next version intersects these actual public entities with retained direct
calls in a separate bounded identity inventory of the two defined `And`
bodies. A matching call must have a direct callee ReferenceOperation and a
TargetMethod naming the same candidate FunctionSymbol. Exactly one distinct
target must remain. It never binds a prior report locator, reconstructs source
overloads or selects by class/display name. Without the option, the accepted
four-root selectors, traversal and bounds remain unchanged.

The focused mode reuses the typed walker for only that actual body and records
its execution/signature parameter associations, receiver symbol and pointer
object const wrapper, actual return/field operations, arithmetic and retained
enum declaration/storage facets, and complete/nonvirtual/subobject layouts.
The partial comparison with the current ordinary unsigned-byte field-return
rule is explicitly non-authorizing. Nested field paths, conversions or calls
remain the actual operations; the selected name does not imply an accessor
implementation or translator acceptance.

Only the chosen target receives typed containment tracing. The selection-only
inventory uses public `DescendantOperations` with 256 operation identities per
`And` root, two roots, and at most 16 matching direct call identities. These
separate counters are not a typed occurrence trace or execution schedule; no
callee bodies are traversed. Actual caller signatures and matching call facets
are retained as selection evidence, not runtime invocation evidence.

Focused target limits are one root, 64 distinct operations, 256 typed occurrences,
depth 32, 16 one-hop function/storage dependencies, 64 related types, 32 fields
per type, 16 parameters, 10,000 commands and 20,000 rows. Other finite metadata
and cache limits stay as reported above. Unknowns or truncation still produce
an incomplete report and exit 3; no callee bodies are expanded. The coordinator
builds and runs a fresh frozen snapshot with the existing 120-second/1-GiB
external budget, preserving trace 3 unchanged:

```sh
dotnet PATH/TO/FROZEN/DuckdbSliceFacts.dll --mode accessor \
  --input /mnt/storage/roslynobj-duckdb-20261014/export/major2/0316/unit.roslynobj \
  --output /mnt/storage/roslynobj-csharp-20261015/t5/NEW-duckdb-accessor-facts.json
```

## Accessor results and next inspection

The [compact accessor receipt](../acceptance/duckdb/accessor-trace-report.json)
has status `body-observed-related-types-partial`, SHA-256
`39bec0741b200277dd17cd171f1cdfd1f67b11317b53291fb9c4f9e454d26023`.
Its five measured phases are two successful tool builds, the successful
default-mode regression, and two expected diagnostic exit-3 observations.
There are no unexpected outcomes or budget violations. Both 48-file source
snapshots and all earlier evidence are retained; the second snapshot matches
the frozen current executable sources. This note is updated separately from
those immutable source manifests.

Both frozen tool attempts compile. Attempt 1 preserves
`t5/duckdb-accessor-facts-1.json`: two defined retained-name candidates, no
selected root, exit 3. The same frozen binary's default four-root mode passes
and reproduces trace 3's semantic report after excluding elapsed time and
reader counters; the actual measured values remain in its report. That
selection failure is evidence, not a reason to bind a class name or old locator.

Attempt 2's `t5/duckdb-accessor-facts-2.json` is 105,102 bytes, SHA-256
`db98fc7a9ca9dc4242f1b07bbd7a8f223428fe7e80f003e7d4dc930883c13ea5`.
The separate selection inventory contains 117 and 85 operation identities for
the two `And` roots. Two matching direct calls in the first root select the
same actual `Vector::GetVectorType` entity; the second root adds no match.
These are public identity observations, not runtime invocation counts.

The selected target's typed containment and structural audit complete with
22 operation identities and 22 occurrences. Its actual body contains one
`buffer_ref` local, an executable `dik_expression` initializer, four direct
calls, an `if`, and two returns. It has no retained EH tables, regions,
prologue, epilogues or lifetime associations. The reference initializer has
no constructor, destructor, copy-source or lifetime payload. This is not the
single direct field-return body admitted by the ordinary-method milestone.
No source noexcept fact is inferred: the selected target's value is null.

The four selected call dependencies are:

| Retained function | Actual result and selected facts | Body evidence |
| --- | --- | --- |
| `Vector::GetBufferRef` | Natural 8/8 lvalue reference to const `buffer_ptr`, whose underlying `shared_ptr` record is 16/8; noexcept fact null | Defined, not traversed |
| `shared_ptr::operator bool` | Boolean arithmetic descriptor, 8 bits, 1/1; noexcept true | Defined, not traversed |
| `VectorBuffer::GetVectorType` | The same captured unsigned-byte `VectorType` result; noexcept fact null | Defined, not traversed |
| `shared_ptr::operator->` | Natural 8/8 non-reference pointer to `VectorBuffer`; noexcept fact null | Defined, not traversed |

The initializer's GetBufferRef call and the bool and VectorBuffer method calls
retain left-to-right flags. The operator-arrow call has neither order flag
set. Receiver facets alias the actual argument/raw operand; these facts do not
authorize replacing calls with field access or adding receiver evaluations.

`VectorType` is represented by an actual `ArithmeticType` with `tk_integer`,
unsigned 8 bits and size/alignment 1/1. Its retained EnumSymbol names that same
type and has an explicit underlying `uint8_t` typeref, then `__uint8_t`, then
the natural unsigned-byte descriptor. The enum relationship and storage
representation are both retained. This does not imply the whole method is
covered by an arithmetic accessor rule. Complete `Vector` is 24/8, with
nonvirtual 24/8, self subobject partner, no bases or virtual functions, a
LogicalType field at 0 and buffer at 8. VectorBuffer is separately 56/8 and
polymorphic, with a generated base at 8 and `vector_type` at 24; its body and
virtual ownership closure are not inspected here.

The overall report remains `traceComplete=false` and exit 3. Its sole issue
occurs later in related-type metadata expansion at the explicit 64-type bound;
some queued type rows remain pending. This does not undo the completed target
body trace or turn the metadata inventory into a complete closure. There are
1,287 reader commands and 2,889 rows, maximum batch 12, and no external budget
violation (90,943,488 bytes sampled RSS, about 91 MB). The input hash is unchanged. Neither attempt
is promoted to translator, linker or generated-execution acceptance; trace 3
and both partial accessor reports remain immutable. No limit increase or
rerun is required to establish this finding.

The smallest concrete next question is the actual selected GetBufferRef body,
not another synthetic scalar accessor. A separately approved bounded public
inspection should start from this report's retained direct call identity in
the same input: reproduce the public selection chain and use the actual
call's TargetMethod, never the report locator as a persistent handle or a
fresh name lookup for GetBufferRef. It should record the returned place, reference/value categories,
const wrappers, owner/field association, and any calls, lifetime or cleanup
obligations. Its signature establishes an unmet reference-return boundary;
its untraversed body does not yet establish a safe adapter. If a direct borrowed
field-reference body is actually retained, a later finite alias/lifetime
fixture can target that contract. Reference-return/native/callback/ownership
admission and the other three callee bodies remain open; no implementation or
new fixture follows from the current partial comparison alone.
