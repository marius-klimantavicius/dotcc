# Bounded DuckDB tail-dependency trace

Status: all three selected bodies were traced within their operation bounds;
the overall report is **partial**. The process exited 3 because the shallow
related-type inventory reached its limit of 64, after body traversal. There was
one `truncation-or-bound` issue and no resource-budget violation. The
[receipt](../acceptance/duckdb/tail-dependencies-report.json) is 150,148 bytes,
SHA-256 `f6ead30ed106af32063f451b2ed20d9654763dd82365f8c7507424405d20fcd2`.
This is public-reader inspection evidence, with no translator, link, generated
execution, ownership or closed no-throw acceptance.

`t5/duckdb-tail-dependencies-facts-1.json` is 173,413 bytes, SHA-256
`ed652a26c1b843e5f6b7427ab1ba95d096c1a5b712a8cf1fc5d6244f938e600e`.
It uses ProbeVersion 9 and mode `tail-dependencies`. The unchanged core-entry
316 input remains SHA-256
`cf385364234784aa0d748781d06f74161ad51d28a6a34215449c3ae212810b65`.
All entity labels below are local to this report.

## Selection and three bodies

Selectors reproduce the existing And/accessor/receiver chain and inventory the
pointer-returning parent Function 55. Its 35 operation identities select exactly
three direct calls, with callee-reference Target equal to selected TargetMethod.
Those inventories are excluded from the target-body counts. The actual selecting
calls 68/92/102 have LTR true, RTL false and no return slot. Call 68 is linked
through executable expression Initialization 123 to local 124, whose declared
type 125 is a const pointer-object wrapper around value pointer 69.

| Selected body | Typed identities / occurrences | Signature and selected roles |
| --- | ---: | --- |
| get 72 | 10 / 10 | Signature 73 returns ordinary pointer 69; sole this 77, descriptor 74, execution symbol 78; noexcept true. |
| AssertNotNull 95 | 12 / 12 | Signature 96 returns void 5; sole ordinary Boolean 40, declared const wrapper 99, descriptor 97, execution symbol 100; noexcept null. |
| base operator-> 106 | 11 / 11 | Signature 107 returns ordinary pointer 103; sole this 111, descriptor 108, execution symbol 112; noexcept true. |

All three signatures retain prototypes, default calling convention, C++ external
linkage, no variadic or hidden return/copy roles. Receiver symbols 78 and 112
retain distinct top-level const pointer wrappers 145 and 187 over selected
pointer types 77 and 111. Declared, selected and expression views are not collapsed.

get 72 assigns the actual points-to-field read 142 of `_M_ptr` Field 146 to
automatic pointer local 135, then returns that local at Reference 147.
Assignment 140 records RTL true. Receiver Reference 143 names the selected this
symbol 78, using expression pointer type 77. Field 146 and the local both have
pointer type 69. No call occurs in this body.

AssertNotNull 95 retains If 149. Its condition is LTR direct Call 151 to
declaration-only builtin 153, discriminant `bfk_expect`. Signature 154 returns
signed 64-bit type 155 and takes two ordinary parameters of that same type.
Actual argument 159 casts Boolean Reference 162 of symbol 100 to signed 64-bit;
argument 160 is exact integer zero. The then block contains LTR direct Call 165
to declaration-only Function 166, captured identity
`_ZN6duckdb29ThrowNullSharedPtrDereferenceEv`. Its signature 167 is void,
prototype, no arguments and nullable noexcept null. A void Return 150 follows.
The call cannot be erased or treated as an implemented exception action from
its display name.

base operator-> 106 has an empty statement followed by a block assigning LTR
direct Call 178 to automatic pointer local 170, then returning the same local
at Reference 188. Assignment 176 records RTL true. Call 178 selects defined
Function 179, `_M_get`, signature 180, with result pointer 103. Its receiver
185 is the selected this symbol 112, expression type 111, also the sole argument
through descriptor 181/raw operand 1. Function 179's noexcept is true, but its
body association 183 is not traversed.

These three bodies total 33 typed identities and 33 occurrences. Their saved
EH/lifetime metadata has no region/array table, state, prologue, epilogue, region
or reachable lifetime. That does not prove their transitive calls cannot throw.

## Partial types and remaining obligations

Exactly 64 shallow type rows were saved. Pointer 69's pointee 210 is only a
partial `element_type` row; pointer 103's pointee 215 and receiver pointer 111's
pointee 216 remain explicitly pending. Complete alias chains saved in the
[earlier tail report](DUCKDB-ACCESSOR-TAIL.md) belong to that report; this batch
does not newly establish them. Record member/base/partner associations and child
dependency bodies remain unexpanded. No storage dependencies were reported.

The concrete open call obligations are the recorded builtin-expect contract,
the unresolved application declaration 166 and its actual exception/link
contract, and defined but untraversed `_M_get` 179. The retained guard and else
base-view path remain obligations; there is no guard-elimination, smart-pointer
ownership, native fallback or general pointer-return admission here. Nullable
noexcept metadata supplies no closed body proof.

## Attribution and limits

The frozen `t5/duckdb-tail-dependencies-source-1/source-snapshot.json` contains
48 source/configuration files, SHA-256
`21900ec04b9b0609c356338d81941be4cc503205b4f96cd242ec88f1dcda24d9`.
Frozen Program.cs is SHA-256
`370479b459dc201dd6bba3fcdd408b9e6f4739cd86248532b0e192fa746231cc`;
the built DuckdbSliceFacts.dll is SHA-256
`4ce9f4552b87d79be8844005b178118fe6ae3a139c33940f9013ec91463705e2`.

Measured build time was 6.1025 seconds, sampled process-tree RSS 457,588,736
bytes; trace time was 1.00475 seconds, sampled RSS 92,303,360 bytes.
The saved run receipts are `t5/duckdb-tail-dependencies-{build,trace}-1/run.json`.
Bounds remained three roots, 64 operations/256 occurrences per root, depth 32,
16 dependencies, 64 types, 10,000 commands and 20,000 rows. Actual public-reader
counts were 1,473 commands and 3,158 rows; cache bounds remained 4,096 rows and
128 pages. The type-bound failure is preserved, not relabeled as full completion.
