# Selected callee of the DuckDB Boolean wrapper

Status: the measured `boolean-callee` trace completes within its one-body,
shallow-type scope. The [scoped receipt](../acceptance/duckdb/boolean-callee-trace-report.json)
is 96,535 bytes, SHA-256
`656dc52a2fe233c94b510dba83193b27fd4657fcb257e1e0a115ab0a5a3981b4`.
Both measured phases pass. `t5/duckdb-boolean-callee-facts-1.json` is 93,761 bytes,
SHA-256 `7921bd625c8a40fd32f73df66b08be7e03f6fbb928555346056f99f55141763a`.
It records 13 typed operation identities/occurrences, 40 shallow types and no
one-hop dependencies or issues. No product, Runtime, provider or translation
admission change follows. The preceding tool checkpoint is `0f4849b67`; the
verified product remains the borrowed-member checkpoint `de39c8bb9`.

The unchanged configured core entry 316 is
`/mnt/storage/roslynobj-duckdb-20261014/export/major2/0316/unit.roslynobj`,
SHA-256 `cf385364234784aa0d748781d06f74161ad51d28a6a34215449c3ae212810b65`.
The [preceding Boolean report](../acceptance/duckdb/boolean-conversion-trace-report.json),
SHA-256 `190389570907e489242aba5128410b35a4b791e3d096c7412ee4403c9486a1e1`,
records a wrapper with 15 typed identities/occurrences. It assigns one direct
conversion call's Boolean result to a local, then returns that local.

In that report only, Call 68 has callee Reference 76 targeting the identical
selected Function 69/signature 71. Function 69 is a defined conversion with
captured noexcept true; its body 75 is untraversed there. These labels cite
that immutable report and are not persistent handles or a name selector.
The mode reproduces And-to-accessor-to-Boolean-wrapper selection in the current
input, then requires the sole direct body call's actual callee reference and
TargetMethod to agree before selecting its target body.

The preceding call receiver is address-of 77 over generated-base dot-field
79, nested in points-to-field 81 through internal field 88. Its const alias
and base field relationships are actual retained operations. They do not
establish how the newly selected body uses its own receiver. Both the nested
caller expression and selected formal/execution receiver views must remain
separately attributable; none may be replaced by a class/name/size shortcut.

## Actual selected call and receiver

The following labels belong to the new report, not the earlier report cited
above. And inventories contain 117/85 identities; the accessor inventory has
22 and the Boolean-wrapper inventory has 15 with one executable call identity.
All selection inventories, including the nested receiver inventory, are
excluded from the selected body's 13 typed identities/occurrences.

The wrapper's sole direct Call 62 records left-to-right evaluation. Callee
Reference 63 and TargetMethod both select Function 65/signature 66. Receiver
64 is also argument 0, `this_parameter` descriptor 67 and raw operand 1; these
aliases add no evaluation. Actual and formal receiver types are identical
ordinary 8/8 pointer 61 to const typeref 70 over __shared_ptr record 69.
The selected execution receiver symbol 82 has separate top-level const
pointer-object typeref 98 around pointer 61.

Parent receiver 64 is address-of dot-field 71, selecting actual generated-base
field 80 at offset 0 in shared_ptr record 81. Its object is points-to-field
72 selecting internal field 79 at offset 0 in wrapper record 52. That field's
declared alias 78 (`original`) names record 81; the effective member place
uses const typeref 74. The address operand is const type 70. These nested
operations and qualifiers remain actual receiver expressions, not an inferred
receiver adjustment. Record sizes 16/8 and the two offset-zero field facts do
not establish anything about unexpanded bases.

## Actual body and unresolved associations

Function 65 is a defined `sfk_conversion` / `cdk_none` with a prototype,
default convention, C++ linkage and only the selected `this_parameter`.
There is no variadic, hidden result-slot or copy-constructor return role.
Its Boolean result is type 40: `tk_integer`, arithmetic kind `boolean`,
`IsBool=true`, eight bits, reported signedness true and size/alignment 1/1.
Captured `NoexceptEffective=true` remains metadata; the report explicitly
records `closedNoThrowBodyProof=false`.

Body block 83 contains block 86 with expression statement 87 and Return 88.
The retained expression assigns the Boolean conversion result to automatic
local 84, then the return reads that same local through Reference 102:

```text
eok_assign 89: Boolean type 40, right-to-left
  Reference 90 -> local 84, Boolean lvalue
  eok_cast 91 -> Boolean type 40
    eok_ne 92: signed-32-bit type 45
      eok_points_to_field 93: ordinary pointer type 95
        Reference 96 -> execution receiver 82, value pointer 61
        field Reference 97 -> _M_ptr field 99, pointer type 95
      ConstantOperation 94 -> Constant 100
        ck_integer value 0, exact same pointer type 95
```

Constant 100 retains a separate OriginalType 101; it is not silently replaced
by that descriptor. Local 84 is `initk_none` with no initializer association.
All traced expression volatile flags are false. This body has no calls,
initialization-association rows, lifetime associations, EH tables, initial/null
state, prologue, epilogues or regions. Both function and storage dependency
lists are empty. These are observed body facts, not a recursive class or
program-closure result.

Complete __shared_ptr record 69 is 16/8, with pointer field 99 at offset 0
and refcount field 108 at offset 8. The refcount's type 109 remains unexpanded.
The record also retains a nonvirtual direct-base association to type 110 and
subobject 111, neither described by this shallow trace. Record 81's base
subobject 112 and record partner associations remain unexpanded. Pointer 95
has its captured pointee chain, but VectorBuffer's field/base/table associations
are shallow metadata, not application layout or ownership closure. No empty-base
claim or permission to discard these associations follows from the method
touching only `_M_ptr`.

The input before/after hashes match and all 48 frozen source files verify.
Measured build/trace times are 5.793/0.804 seconds. ProbeVersion 6 separately
records internal elapsed time 0.633 seconds, 1,034 commands, 2,736 rows and
maximum batch 12. One-body limits remain 64 identities, 256 occurrences,
depth 32, 16 dependencies and 64 shallow types. There are no issues, but
`traceComplete=true` is scoped: `inventoryOnly=true`,
`fullRelatedTypeClosure=false`, `closedNoThrowBodyProof=false`,
`translatorAccepted=false`, `linkClosureComplete=false` and
`generatedExecutionAccepted=false` remain explicit.

Any product proposal must separately address the actual receiver's member/base
path and the required storage representation without inventing empty-base
semantics, ownership or no-throw permission. This evidence adds no general
shared_ptr implementation or DuckDB root execution claim. Earlier accessor,
buffer-reference and Boolean reports retain their exact original attribution.
