# Bounded DuckDB receiver layout audit

Status: the explicit `receiver-layout` type-only audit passed its frozen build
and measured read. The [scoped receipt](../acceptance/duckdb/receiver-layout-report.json)
is 96,038 bytes, SHA-256
`8ba1c79c88dd1aaeb488338c585f90a9875263870bccac22e2f70901562c7f9e`.
`t5/duckdb-receiver-layout-facts-1.json` is 93,998 bytes,
SHA-256 `15cf427ff2a30e623988cb24c9c3945e791e8fa7db06cb31e60016d7c7c7fe36`.
It describes four record layouts and 33 type descriptors, with zero typed body
nodes, identities or occurrences and zero issues. No body, ownership, closed
no-throw, translator or generated-execution acceptance is claimed.

The unchanged configured core entry 316 is
`/mnt/storage/roslynobj-duckdb-20261014/export/major2/0316/unit.roslynobj`,
SHA-256 `cf385364234784aa0d748781d06f74161ad51d28a6a34215449c3ae212810b65`.
The mode reproduces the current And/accessor/Boolean-wrapper/sole-direct-callee
identity chain described in [the callee trace](DUCKDB-BOOLEAN-CALLEE.md).
Parent and nested-receiver inventories remain selection-only, excluded from
body counts. Function 65 and Call 62 are declaration/call associations in this
report, not another body trace. All locators below are report-local public
semantic-equality labels, never persistent handles or source lookup keys.

## Actual layout and field associations

| Explicit record | Complete size/alignment | Nonvirtual size/alignment | Partner | Retained fields |
| --- | --- | --- | --- | --- |
| Receiver 69, __shared_ptr | 16/8 | 16/8 | Same entity | Pointer 90 at 0; value field 91 at 8 |
| Parent field owner 81, shared_ptr | 16/8 | 16/8 | Same entity | Generated base field 80, exact type 69, at 0 |
| Direct base 86, __shared_ptr_access | 1/1 | 1/1 | Same entity | None; captured IsEmpty=true |
| Immediate value field record 88, __shared_count | 8/8 | 8/8 | Same entity | Pointer 97 at 0 |

All four records are complete, unpacked, nonvirtual and nonpolymorphic, retain
`HasSubobjectType=false`, and have complete layouts with matching type identity.
Each partner is reciprocal and identical; no separate smaller subobject carrier
is captured for these seeds.

The actual nested receiver's internal field 79 belongs to wrapper record 52
and is at offset 0. Its declared alias 78 (`original`) directly names record
81. The alias has effective/helper size/alignment 16/8, while its raw descriptor
is 0/1; those quantities remain distinct. Generated base field 80 belongs to
record 81, has exact declared/leaf type 69 and size/alignment 16/8, and is also
at offset 0. Both declaring owners contain the identical fields and exactly one
matching field-layout row. The member alias leaf matches owner 81, and the
generated field's leaf matches the selected receiver record 69 and ThisClass.

Receiver field 90 (`_M_ptr`) is ordinary pointer type 87, size/alignment 8/8.
Field 91 (`_M_refcount`) is by-value record 88, size/alignment 8/8. Its sole
field 97 (`_M_pi`) is ordinary pointer type 98, also 8/8. Pointer pointee
identities are recorded but their graphs are not expanded. No pointee field,
vtable, allocator, counter operation or ownership behavior is inferred.

Direct base 86 is linked to receiver 69 by subobject 92 at offset 0, with
`IsDirect=true` and captured `IsOptimizedEmptyBase=true`. Parent 81's generated
base field 80 corresponds to direct base type 69 and subobject 93, also at 0,
with matching actual BaseType identity. Parent 81 additionally retains indirect
subobject 94 of type 86 at 0; its own optimized-empty flag is false. Each
subobject's `CompleteSubobject` fact is null, and indirect subobject 94 has
`OriginalType=null`. These separate context-specific
flags are preserved rather than generalized from the base's name or size.
They do not require materializing indirect subobject 94 or matching all indirect
flags: the explicit parent field 80 already embeds receiver record 69.

BaseSubobject exposes no occupied-size quantity. The audit deliberately reports
`occupiedSize=null` and `occupiedSizeSynthesized=false`, alongside complete,
nonvirtual and partner facts. This absence is not an audit failure and is not
needed to describe the explicit carrier fields and their captured offsets.
No zero extent, overlapping field, padding formula or C++ empty-base layout is
reconstructed from the source-level base association. The selected lowered
field identities, sizes and offsets remain authoritative.

## Scope and attribution

All 48 frozen source files and the input before/after hashes remain unchanged.
ProbeVersion 7 records internal elapsed time 0.547 seconds, 923 reader commands,
2,586 rows and maximum batch 12. Bounds are eight record layouts, 64 types,
16 fields per record, four direct bases, eight subobjects and wrapper depth 16;
reader caches remain 4,096 rows and 128 pages of 128 rows. No method enumeration,
target body traversal, constant/RTTI traversal or recursive layout closure occurs.

`noBodyTrace=true` and all typed body counts are zero. `traceComplete=true`
means completion of this finite type audit only. `inventoryOnly=true`,
`fullRelatedTypeClosure=false`, `closedNoThrowBodyProof=false`,
`translatorAccepted=false`, `linkClosureComplete=false` and
`generatedExecutionAccepted=false` remain explicit. Pointer representation
policy and any cohesive product proposal require separate review; these facts
grant no native ABI, pointee equivalence, ownership or no-throw permission.
Earlier body traces retain their original scope and attribution. Program.cs,
product and Runtime remain frozen; no application execution was added here.
