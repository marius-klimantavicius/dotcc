# Selected DuckDB Boolean condition callee

Status: the measured one-body inspection completes within its declared scope.
The [scoped receipt](../acceptance/duckdb/boolean-conversion-trace-report.json)
is 82,623 bytes, SHA-256
`190389570907e489242aba5128410b35a4b791e3d096c7412ee4403c9486a1e1`.
`t5/duckdb-boolean-facts-1.json` is 80,022 bytes, SHA-256
`a4d23b49b7437e30aa96c94e544ef8bf9e14d41fea24f4030295d5d5ba9cf21d`.
It records 15 typed operation identities/occurrences, 33 shallow types, one
one-hop function dependency and zero issues. This is public-reader evidence,
with no translator, link, closed no-throw or generated-execution acceptance.
Product and Runtime stay at the borrowed-member checkpoint `de39c8bb9`.

The unchanged input is configured core entry 316,
`/mnt/storage/roslynobj-duckdb-20261014/export/major2/0316/unit.roslynobj`,
SHA-256 `cf385364234784aa0d748781d06f74161ad51d28a6a34215449c3ae212810b65`.
The `--mode boolean-conversion` mode reproduces the public And-to-accessor
selection, then inspects the actual direct callee under the accessor's retained
`eok_not` condition. Prior report locators are citations only; they are not
persistent handles, source overload lookup or a new name selector.

## Existing evidence and required distinctions

The [accessor receipt](../acceptance/duckdb/accessor-trace-report.json) preserves
`t5/duckdb-accessor-facts-2.json`, SHA-256
`db98fc7a9ca9dc4242f1b07bbd7a8f223428fe7e80f003e7d4dc930883c13ea5`.
Its two And selection inventories contain 117/85 operation identities. Only
the selected accessor has a typed body trace: 22 identities/22 occurrences.
That body trace completes, but the report remains partial at its 64-related-type
bound. Parent selector inventory is not body tracing or a runtime call count.

In that report, condition 59 is `eok_not` over direct Call 62. Its raw callee
Reference 71 and selected TargetMethod both name Function 63; its receiver 72
is also argument 0 and raw operand 1. The retained target is `sfk_conversion`,
has a defined but untraversed body 70, and a selected `this_parameter` role.
The Boolean result is an arithmetic descriptor with `tk_integer`, kind
`boolean`, eight bits and size/alignment 1/1. Those storage facts do not
describe the conversion's body, make it a signed-byte conversion, or authorize
replacing a call by a field/pointer test. Its captured noexcept value is true;
the parent accessor's value is null. Neither fact replaces body inspection or
grants no-throw-cache/cleanup-omission permission.

Actual receiver value type 54 is an ordinary 8/8 pointer to const typeref 105,
whose underlying type is alias 103. The formal selected receiver type 67 is
an ordinary 8/8 pointer to const typeref 108 over shared_ptr record 64.
These actual/formal wrapper chains must remain separate facts, including the
execution receiver symbol's own type. A common size, record spelling or alias
target does not make the pointer/const/alias descriptors identical.

The [buffer-reference receipt](../acceptance/duckdb/buffer-reference-trace-report.json)
preserves `t5/duckdb-buffer-ref-facts-1.json`, SHA-256
`99bfbe8f0103c4cca39bc62d8b49e74fa3d8530730f3f0ce45aec7bfb9c81158`.
That separate body trace completes within seven identities/seven occurrences,
21 described shallow types and no dependencies or issues. It distinguishes a
declared reference from an ordinary pointer value with the same exact const
alias pointee. Its unexpanded field/base/partner associations are not full
type, dependency, ownership or link closure. Neither older report changes
status when the next body is inspected.

## Measured selected body

The following labels belong only to the Boolean report; earlier report labels
above are separate citations. And selection inventories still contain 117/85
operation identities, with two/zero matching calls to the same selected accessor.
The parent accessor inventory contains 22 identities. All are selection-only,
excluded from the selected body's 15 identity/occurrence counters.

Parent conditional 43 has negated typed condition 44 and direct call 46. Its
callee Reference 47 and TargetMethod both select Function 49/signature 50.
Receiver 48 aliases argument 0, descriptor 51 and raw operand 1; it is a read
of parent local 55. Formal receiver pointer 41 points to const typeref 53 over
shared_ptr record 52. Actual receiver value pointer 42 points to const typeref
54 over buffer_ptr alias 58, then the same record 52. Pointer and pointee
identities differ; only their captured leaf record matches. The local's declared
type 56 is a reference, distinct again from its ordinary pointer expression
type 42. Its initialization association 57 is not expanded by this mode.

Selected Function 49 is defined `sfk_conversion` / `cdk_none`, with a prototype,
default calling convention, C++ linkage and one `this_parameter`. It has no
variadic, return-slot or copy-constructor return roles. Descriptor 51 and
execution symbol 59 use pointer type 41; symbol 59 also retains top-level const
pointer-object typeref 86 around that type. `NoexceptEffective=true` is captured
metadata. The parent accessor's nullable noexcept remains null.

The actual body is not a terminal pointer test. Its outer block 60 contains
block 63, whose expression statement 64 executes assignment 66. That assignment
records right-to-left evaluation and stores direct Call 68 into automatic local
61 through lvalue Reference 67. Call 68 records left-to-right evaluation. Its
callee Reference 76 names the identical selected Function 69, using signature
71. Return 65 then reads local 61 through Reference 90. Local 61 is Boolean type
40, `initk_none`, with no initializer association; its value comes from the
actual assignment. Type 40 is `tk_integer`, arithmetic kind `boolean`,
`IsBool=true`, eight bits, reported signedness true and size/alignment 1/1.
The semantic Boolean kind is retained independently of that storage descriptor.

Call 68's only argument is receiver 77, also raw operand 1 and selected
`this_parameter` descriptor 73. Its actual retained address expression is:

```text
address_of 77: pointer 72
  dot_field 79: const __shared_ptr type 80
    points_to_field 81: const alias type 83
      Reference 84 -> execution this symbol 59
      field Reference 85 -> internal field 88, declared alias 87
    field Reference 82 -> generated base field 89, type 70
```

The complete shared_ptr record 52 is 16/8, with internal field 88 at offset 0.
Alias 87 (`original`) names separate shared_ptr record 102, also 16/8, whose
generated base field 89 is at offset 0 and has __shared_ptr type 70. Record 70
is 16/8 and describes `_M_ptr` at 0 and `_M_refcount` at 8, but their field-type
associations and its nonvirtual base association remain unexpanded. Those shallow
layout facts do not replace either field operation, prove ownership, or establish
the dependency's body behavior.

Function 69 is the sole one-hop dependency: a defined `sfk_conversion` on
__shared_ptr record 70, with Boolean result 40, captured noexcept true and one
selected/execution `this_parameter` of pointer type 72 to const type 80.
Its body 75 is explicitly **not traversed**. No terminal pointer comparison or
closed no-throw proof follows from the declaration or record field names.
The traced Function 49 has no retained EH tables, initial/null state, prologue,
epilogues, regions or reachable lifetime associations; there are no one-hop
storage dependencies or initialization-association rows. This absence is scoped
to the traced body and does not extend to dependency 69.

The input's before/after hashes and all 48 frozen source files match. Both
measured phases pass without budget violations: build 6.318 seconds, trace
0.804 seconds and about 90.8 MB sampled memory. ProbeVersion 5 records an internal
elapsed time of 0.630 seconds, 913 reader commands, 2,585 rows, maximum batch 12
and caches of 4,096 rows / 128 pages. Internal trace time and measured process
time are separate observations. No producer rebuild or re-export was needed.

## Evidence boundary and next question

Bounds remain one body, 64 operation identities, 256 typed occurrences, depth
32, 16 one-hop dependencies and 64 shallow related types. Parent inventories
are separate from body tracing; receiver/argument/raw-operand aliases add no
evaluation. Typed containment and order flags are not a reconstructed runtime
schedule. Field/base/partner associations and callee bodies remain unexpanded;
captured noexcept metadata does not supply no-throw permission.

Here `traceComplete=true` and zero issues mean completion only of this selected
body and the explicitly shallow type scope. `fullRelatedTypeClosure=false`,
`inventoryOnly=true`, `translatorAccepted=false`, `linkClosureComplete=false`
and `generatedExecutionAccepted=false` remain unchanged. The earlier partial
accessor report and scoped buffer-reference report retain their own attribution.

The smallest next question is the actual body of Function 69 reached by Call 68.
A separately approved bounded tool mode can reproduce this current public
selection chain and require callee-reference/TargetMethod identity equality,
then inspect only that one body. No name lookup, persistent locator, guessed
pointer test, recursive callee closure or new product adapter is warranted by
the current report. Native/reference/ownership support and DuckDB root execution
remain unproven.
