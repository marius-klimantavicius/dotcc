# Bounded DuckDB accessor-tail trace

Status: the two-body public-reader trace passed with zero issues. The
[scoped receipt](../acceptance/duckdb/accessor-tail-trace-report.json) is
152,426 bytes, SHA-256
`5e69d607e9ae3e64c5728e721ca07823144c1205015fb30aba6e79bc730c5461`.
`t5/duckdb-accessor-tail-facts-1.json` is 148,772 bytes, SHA-256
`6f2a2946cb81c5b3402bfaab6cd446adbd75d4ed9821401cb625d4ccb96b6e7f`.
This is scoped inspection evidence, with no product, link, execution, ownership
or closed no-throw acceptance.

The unchanged input is configured core entry 316,
`/mnt/storage/roslynobj-duckdb-20261014/export/major2/0316/unit.roslynobj`,
SHA-256 `cf385364234784aa0d748781d06f74161ad51d28a6a34215449c3ae212810b65`.
The `--mode accessor-tail` selectors reproduce the And-to-accessor identity
intersection, select its actual non-Boolean unsigned-byte direct call, then
select the declared ordinary-pointer-returning direct call inside that call's
receiver. Callee Reference.Target and TargetMethod agree at each selection.
No method name, prior locator or source operator interpretation supplies binding.
All labels below are local to this report.

## Selection and declared/value views

And identity inventories contain 117/85 operations; the accessor inventory
contains 22. The receiver inventory contains four expressions. These are
selection-only and excluded from the two bodies' typed counts.

Parent Call 42 selects unsigned-byte Function 2/signature 45 and records LTR
evaluation. Its receiver 44 is also argument 0/descriptor 46/raw operand 1:
an ordinary pointer cast around Call 52. Call 52 directly selects pointer-return
Function 55/signature 56 through callee Reference 53. Its receiver 54 aliases
argument 0/descriptor 57/raw operand 1. Both Call 52 order flags are false;
the enclosing call's LTR flag does not supply an order for this inner call.

Function 55's **declared** return type 41 is natural 8/8, ordinary nonreference
pointer to VectorBuffer 48, identical to Call 52.Type. The selector therefore
excludes the getter's declared reference result even though that other call's
effective value type appears as an ordinary pointer. Actual receiver 54 reads
local 60: value pointer 59 points through const 66 and buffer_ptr alias 152 to
shared_ptr record 62, while the local's declared type 61 is a reference to the
same const 66. Formal receiver 63 instead points through const 67 directly to
record 62. Actual/formal pointer and pointee identities remain distinct.

## Two actual bodies

Function 2 is an ordinary defined method with five typed identities/occurrences:
block 51, Return 69, points-to-field 70 and receiver/field References 71/72.
The field is actual vector_type 74 at offset 24 in complete VectorBuffer 48,
size/alignment 56/8. These are saved shallow type/layout rows, not a source
layout calculation. Receiver symbol 50 retains top-level const pointer wrapper
73 over selected pointer 49 to const VectorBuffer 165. There are no locals or
calls. Result type 16 is arithmetic unsigned integer, eight bits and 1/1,
not Boolean. Enum declaration 147 names that exact type with explicit underlying
uint8_t alias 148, then __uint8_t 174 and natural unsigned-byte type 178.
No general enum/native ABI admission follows from those storage facts.

Function 55 is defined `sfk_operator` with 35 typed identities/occurrences.
Retained If 80 tests signed-32-bit Constant 81, value 1. Both branches are
traced as containment; their inclusion does not assert both execute at runtime.
In its then block, executable `dik_expression` Initialization 77 targets
automatic local ptr 75 using LTR direct Call 78 to Function 90/signature 92.
Its receiver 99 is address-of generated-base member 101 over internal member
103, selecting exact fields 111/110. The local type 76 is a top-level const
wrapper around ordinary pointer 93. Initialization has no constructor,
destructor, copy-source, constant, lifetime, master or destruction-link payload.

The next expression is LTR direct Call 112 to Function 113/signature 114.
Its ordinary Boolean argument 120 is cast of negation 122 of the local pointer
read 123; it has no receiver. Return 89 then reads the same local at 124.
The else Return 125 contains LTR direct Call 126 to Function 127/signature 129.
Receiver 136 is cast of address 138 of lvalue-adjust 140, whose input is the
nested generated-base/internal member path 141/142. The adjustment's type 128
is the saved fieldless 1/1 __shared_ptr_access record, also retained as a
nonvirtual direct-base association of record 91. This operation is not evidence
for a synthesized empty-base field or inferred occupied extent.

The then pointer value 93 points through element_type alias 170 and type alias
176 to VectorBuffer 48. The else call value 130 points through separate
element_type alias 172 to that same record. Both differ from declared return
pointer 41; the exact alias/view relationships are preserved, not normalized.
Both selected functions have only one `this_parameter`, no variadic, hidden
result-slot or copy-constructor return role, and nullable noexcept null.
Their body reports contain no EH tables/state/regions/prologue/epilogues or
reachable lifetime associations. All traced expression volatile flags are false.

## Dependencies, bounds and attribution

The three one-hop function dependencies are defined but untraversed:

| Actual target | Selected result/roles | Captured noexcept | Body association |
| --- | --- | --- | --- |
| 90, get | Ordinary pointer 93; this only | true | 97 |
| 113, AssertNotNull | void; one Boolean ordinary parameter, declared const view 115 | null | 118 |
| 127, base operator-> | Ordinary pointer 130; this only | true | 134 |

There are no storage dependencies. Their declaration metadata, including
noexcept true, is not recursive body or exception-behavior proof. Concrete
remaining gaps are the selected pointer-local/result/return alias contexts,
these three actual callees, and the retained base-view receiver operation.
The unsigned-byte method additionally reads an actual field of polymorphic
VectorBuffer; the earlier address-only pointer profile grants no pointee field
access or full representation of its unexpanded base/member/table associations.
No arbitrary smart-pointer, enum, no-throw or ownership support is established.

ProbeVersion 8 reports 40 total typed identities/occurrences, 59 shallow types,
three function dependencies and zero issues. Bounds remain two roots,
64 identities/256 occurrences per root, depth 32, 16 combined dependencies,
64 types and 32 fields per type; no recursive callee bodies or type closure.
`traceComplete=true` applies only to that scope. `fullRelatedTypeClosure=false`,
`closedNoThrowBodyProof=false`, `inventoryOnly=true` and all translator/link/
generated-execution acceptance flags false remain explicit.

The build and trace measured 6.602/0.904 seconds, both exit 0 with no budget
violation. Trace peak sampled process-tree RSS was 91,926,528 bytes under
120 seconds/1 GiB. Internal trace elapsed time was 0.719 seconds; reader
counters were 1,311 commands, 2,987 rows and maximum batch 12, with caches
4,096 rows / 128 pages of 128 rows. Input before/after hashes match.

The build used the 48-file immutable `t5/duckdb-accessor-tail-source-1` snapshot,
including the public reader, tool and build/schema configuration. Its
`source-snapshot.json` SHA-256 is
`d989f0847b7382fca6192f71ebdb361eed01a8cf337054682b18a998abf8ce75`.
Frozen Program.cs SHA-256 is
`3d41ef1355526226a7dce1202d7d5b4d981ed12d6b598ccc80936f8abf90a270`;
built DuckdbSliceFacts.dll SHA-256 is
`052c536b7b2a6077dc4b96a02aab371da032cd32d560c15afc68bf469d5806b8`.
Measured build/trace receipt hashes are respectively
`ce4181b7dd688f5186282942cc50fd7298dab9289f8f8c5c78835a517fa1e1bb` and
`eba4af123dfd4cebfd274c806ae63aa2c8871d520129800ca34320c424a1a7cd`.
No producer rebuild, export, native application execution or product change
was involved. Earlier accessor and callee reports retain their original scope.
