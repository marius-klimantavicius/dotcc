# Borrowed const reference to an alias-typed field

Status: the bounded borrowed-member-reference slice passed production CLI,
native comparison, JIT, NativeAOT, physical checks and focused regressions for
both independent captures. The accepted
[receipt](../acceptance/lifetimes/borrowed-field-reference-report.json) is
92,898 bytes, SHA-256
`dfb3271496ef2111bbd46e6b4b0e2caf1125908ea4eff39344a84e6855d2f737`.
All 48 expected outcomes passed: 43 successful phases and five intended
rejections, with no unexpected outcomes or budget violations. Broader T5
remains open.

[borrowed-field-reference.cpp](../tests/Fixtures/T5/borrowed-field-reference.cpp)
defines trivial `Member { int first; int second; }`, `using FieldAlias = Member`,
and Owner with a signed `long long` canary before its alias-typed payload. The
ordinary method `const FieldAlias& view() const { return payload; }` has no
noexcept annotation. No source method or helper in this fixture has one.
The alias, const layers, receiver and returned-reference representation must
come from the actual captured public type relationships; spelling is not a
binding or ownership rule.

Two Owner locals are initialized using ordinary field stores only. The first
has canary `-0x6123456789ABCDE0LL` and payload fields 5/7; the other has canary
`0x5123456789ABCDE0LL` and payload fields 101/103. The signed literals are
representable without out-of-range unsigned-to-signed conversions. Their
nonzero upper bits expose truncation. No record size or field offset was assumed
before capture; the actual layout is recorded below.

The fixture binds `const FieldAlias& borrowed = first.view()`, reads both
borrowed fields, changes the original first payload fields to 11/13, then
performs fresh reads through the same borrowed reference. Pointer comparisons
record whether it is exactly the first payload address and distinct from the
other payload address. Both original canaries are compared directly against
their full signed-64-bit literals, storing only boolean results; no narrowed
canary value supplies either comparison.

The existing scalar native harness can use `VALUE_COUNT=9` and invoke the
fixture twice. Ordered bounded events surround the initial observations,
mutation and fresh observations. Actual native execution established the
complete result/value/event oracle below; source predictions did not supply
acceptance.

| Value index | Observation |
| --- | --- |
| 0, 1 | Initial borrowed first/second fields |
| 2, 3 | Fresh borrowed first/second fields after owner mutation |
| 4 | Borrowed address equals first.payload address |
| 5 | Borrowed address differs from other.payload address |
| 6, 7 | Full-width first/other canaries remain equal to their original literals |
| 8 | Zero unused protocol slot |

`lifetime_live()` is explicitly zero. The trivial records own no counted
resources, so zero live/unused slots do not prove ownership, lifetime duration
or cleanup. The event capacity is 64 with observable overflow. Address equality
is compared as scalar booleans, never as native/managed raw-address equality.
There is no source constructor/destructor, EH construct, reference parameter,
heap allocation, callback, virtual dispatch or native provider.

The public-model audit below records selected return and receiver roles,
the full alias/const/reference chains, exact returned field identity and actual
byte offset, return-body category/address operations, caller binding and fresh
loads. It distinguishes a borrowed pointer/reference result from a returned
record value or copied initializer. Wide signed canary types and comparison
operands remain full width.
Metadata identity inventory is not an execution or destruction schedule.

Any consumer proposal follows those actual facts. Nullable noexcept does not
authorize no-throw cache insertion or caller cleanup omission. General reference
returns, ownership extension, native method ABI, polymorphic/base conversions,
throwing methods and DuckDB Vector/InternalException/string/link closure remain
separate obligations. Runtime and providers are unchanged by this slice.

## Frozen capture and separate native observations

Artifacts are relative to `/mnt/storage/roslynobj-csharp-20261015/t5/`.

| Evidence | SHA-256 |
| --- | --- |
| Fixture source | `231d266a0bcfbb7f9bc1bb6d1897e5e4b68d44792ec0d27071b68d5d7c7f3110` |
| `capture/borrowed-field-reference.roslynobj` | `2b5f1c47b0951f6244fd46757a10a6624819e67d7471060f4f00e75713a8be78` |
| `capture/borrowed-field-reference-inspect.json` | `842a99fca8e652ae518da50e618da2f1c566ec2772581d96eeb24fda261d5a7b` |
| `borrowed-field-reference-facts-1.json` | `44a6514fadb0b6840ac4226906c6552e38a904355784437eadcc7eaacbd23abd` |
| `borrowed-field-reference-capture-provenance-1.json` | `3303ed839b7cfc172b503af832a996efa2d6e13e3650db61abdc5775c96861fb` |
| `native-borrowed-field-reference-run-1/stdout.txt` | `53809edd68f0d46e5796ee2dec6af502acd8e9b91ee72102ecf928a86f898f3b` |

Provenance pins unchanged source/input, all six measured phases, native harness,
binary, inspector and public probe. Frozen producer SHA-256 is
`cf619053cbedabca46da3eab3e8278de43fa4a25ef770a56572454abf9ff25d4`;
flags remain `--g++ --c++17 --gnu_version 130300 --exceptions` with RTTI enabled.
ProbeVersion 6 used `virtual-facts-bin-7/LifetimeFacts.dll`, SHA-256
`82c6933d380168c85216ccd3e6d595473be48f63be5e1d28c47f7d49cc1bfe82`,
and source manifest
`bd3cd339c30250d8bf496b01a6936710c66712ee1932d6fa13b52662c59732a8`.
The native binary `native/borrowed-field-reference/native` has SHA-256
`585f2f6750ed405ea657f80a09fd343d85e87fb53d3339777f34e8dcff4ec6c1`.
It uses the existing scalar harness, SHA-256
`587755a8b10cb7366007e13cb5d41d7a9bcfebadc6bde3d868f75f2d3cc3693c`.

The public report records 584 semantic identities, eight bodies and 338
structural relationship occurrences. Those are inventory counts, not a
schedule. Locators below are local public semantic-equality labels.
The separate native executable reports this complete observation twice:

```json
{"result":36,"overflow":0,"live":0,"values":[5,7,11,13,1,1,1,1,0],"events":[100,205,307,400,511,613,704]}
```

Fresh borrowed reads reflect owner mutation, both address comparisons succeed,
and both wide canary checks succeed. Zero live/unused slots retain their
protocol-only meaning. This is native comparison evidence, not generated
execution or a lifetime-extension/ownership proof.

## Actual types, aliases and selected receiver

Complete Owner 429 is size/alignment 16/8. Canary field 488 is at byte offset
0 and has signed-64-bit type 487, size/alignment 8/8. Payload field 492 is at
byte offset 8, and its declared type is alias typeref 491, FieldAlias, whose
UnderlyingType is Member 452. Complete Member is size/alignment 8/4; first 493
and second 496 are signed-32-bit fields at offsets 0/4. The records have no
bases, virtual tables/VTT, separate subobject layout or typeinfo variable.

Defined method 15 is ordinary `sfk_none` / `cdk_none`, nonvirtual and unadjusted.
Its captured current name `_ZNK5Owner4viewEv` is descriptive, not a lookup or
binding recipe. COMDAT is true; weak, weakref, GNU alias, ifunc and alias-entry
flags are false, with no primary/alternate entries or GNU alias/version links.
Signature 427 has prototype, default convention, C++ linkage, ThisClass 429
and one selected/execution `this_parameter`, type 430 / descriptor 431 /
variable 35. There are no source parameters, variadic, result-slot or
copy-constructor return roles. Both method and fixture retain nullable
`NoexceptEffective=null`.

The receiver signature/value type 430 is an ordinary 8/8 pointer to const
typeref 510, whose UnderlyingType is Owner 429. Receiver variable 35 has its
own top-level const typeref 473 around pointer 430. These receiver const layers
are separate from the returned field's alias/qualification layers.

| Facet | Actual captured type |
| --- | --- |
| Signature ReturnType and borrowed variable 34.Type | 428: 8/8 pointer with IsReference=true, IsRValueReference=false |
| Pointee of reference 428 | 554: const, nonvolatile typeref → alias 491 → Member 452 |
| Return expression 336 and call expression 140 | 508: ordinary nonreference 8/8 pointer to identical const typeref 554 |
| Declared payload field 492 | Alias 491 → Member 452, without const on that declaration |
| Returned member lvalue 337 | Const typeref 554, preserving the effective const receiver view |

Descriptor reference type 428 and effective pointer value type 508 are not
identical entities and do not have the same IsReference flag. Their actual
shared pointee chain establishes the representation relationship. Erasing
either the alias or const layer by name, size or source reconstruction would
lose that evidence.

## Actual return and caller binding

Method block 334 contains only Return 335 of address-of 336, whose operand is
member lvalue 337 (`eok_points_to_field`). That member's operands are receiver
Reference 338 targeting variable 35 with pointer type 430 and field Reference
339 targeting payload 492 with declared alias type 491. The effective member
lvalue is const type 554, and address-of 336 is pointer prvalue 508. The return
has no target/common-lifetime association. There are no method locals, calls,
reachable lifetime associations, cleanup tables/regions, prologue or epilogue.
This is a retained field address, not a record-value return or an inferred
receiver-plus-offset shortcut. Capture 1 already has a const field place and
contains no separate lvalue-adjust in the return body. It does not dynamically
exercise DuckDB's explicit qualification-adjustment branch.

Caller Call 140 directly selects the identical method 15 and function type
427. Its raw operands are callee Reference 141 and receiver address-of 142.
The sole argument has ordinal 0, role `this_parameter`, descriptor 431 and
value 142, identical to the Receiver facet and raw operand 1. It records
left-to-right evaluation and has no return slot or virtual/source dispatch.
Receiver address 142 has type 430 and takes the address of const Owner
lvalue-adjust 143, whose Reference 144 targets actual local first 32.
Those aliasing facets must not add another receiver evaluation or argument.

Borrowed variable 34 has reference type 428 and execution Initialization 139,
`dik_expression`, targeting that exact variable/type and naming Call 140 as its
ExpressionAssociation. The call result has effective pointer type 508. There
is no constructor, destructor, bitwise-copy source, constant, extra operand,
lifetime, master or destruction-link association. Only
FollowsExecutableStatement is true among the recorded flags. Locals first 32
and other 33 are `initk_none` with no initializer/constant association; their
actual field stores supply values. No record copy or owner is implied by the
reference initializer.

Initial borrowed field reads 151/160 and fresh reads 207/216 all use pointer
values of type 508 targeting the same borrowed variable 34. The intervening
assignments 182/190 store 11/13 to the original first owner's payload fields,
with recorded right-to-left assignment evaluation. No second initializer or
rebinding supplies the later reads. Pointer equality 244 compares a borrowed
pointer read with the qualified address of first.payload; inequality 258 uses
the other owner's payload address. These comparisons are actual retained
pointer operations, not source-derived alias assertions.

Canary equalities 272/284 compare actual field loads and integer constants all
of signed-64-bit type 487. Constants are `-6999514556857568736` and
`5846593052250721760`; no narrow cast is present on either comparison operand.
Only their boolean comparison results enter the scalar observation array.
These capture facts alone do not establish generated execution, arbitrary
reference-return admission, extended storage lifetime or no-throw cache
permission. The measured consumer/checker status is recorded separately below.

## Actual mutable-field companion capture

The authorized separate
[borrowed-field-reference-mutable.cpp](../tests/Fixtures/T5/borrowed-field-reference-mutable.cpp)
differs from the original source only by declaring
`mutable FieldAlias payload`. Both owners, full-width canaries, field stores,
borrowed observations, address comparisons and zero-live protocol remain
unchanged; no noexcept annotation or adapter is added. The original source,
capture 1 and native observations above remain preserved with their exact
attribution.

The companion also passed its six measured capture/native phases, with the
same frozen producer, flags, public probe and scalar harness identified above.
Its separate provenance records all exact inputs/tool/phase hashes and unchanged
source. Native binary `native/borrowed-field-reference-mutable/native` has
SHA-256 `5a9773bb03c9ae18560494993a21888812c9988796a2aaded15542d5cbf9b2c9`.

| Companion evidence | SHA-256 |
| --- | --- |
| Fixture source | `563d9142640c5df9639d5b63904c79eda99db79fd8a9eac2d0e5f4d7a00b3a7a` |
| `capture/borrowed-field-reference-mutable.roslynobj` | `ee91bb52a2a545678a7fde7140ced9d9463eccd160a9bf37de5bef225d9b6d8c` |
| `capture/borrowed-field-reference-mutable-inspect.json` | `fc6168c061899ef79062f73bc0fac59533f4be377432ba70e96e4b712ef61df7` |
| `borrowed-field-reference-mutable-facts-1.json` | `faeed642cdd1ae9fc8ef648b689a456fa1041b9ef537e66e4fa41d643d4c148e` |
| `borrowed-field-reference-mutable-capture-provenance-1.json` | `c72d27c0bb3e62e72a405db213025423ee65240bb392012930f3e9fcea5b8958` |
| `native-borrowed-field-reference-mutable-run-1/stdout.txt` | `53809edd68f0d46e5796ee2dec6af502acd8e9b91ee72102ecf928a86f898f3b` |

ProbeVersion 6 reports 585 entities, eight bodies and 339 structural occurrences.
The following locators belong to the companion report, not shared identities
across the two input objects. Public field 493 has `IsMutable=true`, declared
alias type 492 and containing Owner 430. Alias 492 directly names Member 453.
Owner remains complete 16/8 with canary at 0 and payload field 493 at byte
offset 8; Member remains complete 8/4 with fields at 0/4.

The actual return is now a distinct typed shape:

```text
Return 335
  address_of 336: ordinary pointer 509, prvalue
    lvalue_adjust 337: const alias 555, lvalue
      points_to_field 338: unqualified alias 492, lvalue
        receiver Reference 339 -> parameter 35, pointer type 431
        field Reference 340 -> mutable payload 493, alias type 492
```

Const typeref 555 points through alias 492 to Member 453. The explicit
lvalue-adjust adds that const view to the actual mutable field place before
its address is taken. Capture 1 instead directly returned an already-const
member place. This is a genuine captured qualification branch, rather than
an operation inferred from the source declaration or inserted to resemble
DuckDB. Neither candidate establishes actual DuckDB execution.

Signature 428 retains reference ReturnType 429, size/alignment 8/8, pointing
to const type 555. Effective return/call type 509 is an ordinary nonreference
8/8 pointer to the identical type 555. Caller Initialization 139 remains
execution `dik_expression`, target borrowed 34/type 429, expression Call 140
of effective pointer type 509, with no constructor, destructor, copy source or
lifetime. The same receiver/argument alias protocol is present: Call 140 targets
method 15/signature 428; Receiver 142 is also argument 0, raw operand 1 and
selected this descriptor 432. Method NoexceptEffective remains null, with no
locals, lifetime associations or EH tables/regions. There is no record-value
return or newly owned object to invent for either view.

The companion's complete two-run native JSON is byte-identical to the original
native output: result 36, values `[5,7,11,13,1,1,1,1,0]` and events
`[100,205,307,400,511,613,704]`, with zero overflow/protocol live. The preserved
original fixture hash remains
`231d266a0bcfbb7f9bc1bb6d1897e5e4b68d44792ec0d27071b68d5d7c7f3110`.
This is capture/native comparison evidence. The subsequent bounded consumer
implementation and generated execution proof are separate from these preserved
capture facts; Runtime and providers remain unchanged.

## Completed bounded implementation and proof

Both production publications contain eight required bodies and no required
imports. Each separate JIT and NativeAOT checker build matched its own complete
two-run native JSON: result 36, all nine values and all seven events recorded
above, with zero overflow/protocol live. The original and mutable-field
`Generated.cs` files are byte-identical, SHA-256
`19222355ceb84fe5fd72d90cc5f62e1346e042702af9b0b2dc39ed1426c12600`;
their distinct input, report, completion and binary attributions are preserved
in the receipt. Repeat publications preserved generated source/project bytes
and reports after output-directory normalization.

The [typed checker](../tests/BorrowedFieldReferences/README.md) used the actual
published `Translated.Owner`, `Translated.Member` and
`Translated.Program.view(Owner*)`. In both modes and both variants it measured
Member size/alignment 8/4, fields 0/4, Owner size/alignment 16/8, canary/payload
0/8, sequential-prefix offsets 4/8 and array strides 8/16. It called the actual
getter on two distinct owners: both pointers equaled their corresponding
payload at byte offset 8, were distinct, and exposed fresh mutations of each
owner while both full-width canaries remained intact. Explicit JsonNode output
kept native observations, physical results and `suppliedCaptureProvenance`
separate; the receipt binds supplied provenance to the actual compilation and
publication rather than treating a supplied hash as proof of linked code.

The receipt verified all 248 product snapshot files, all nine final checker
snapshot files and 20 immutable inputs unchanged. Runtime sources were
unchanged from the ordinary-const-method checkpoint. All 29 focused regression
phases passed, including the four previous negative inputs. Earlier normal,
mutable-reference, ordinary-method and class-payload sources retained byte
identity and JIT/NativeAOT observations; single/multiple/virtual-inheritance
checks retained source identity with their original runtime evidence. The
preserved first physical-checker attempt failed only on four cast-parenthesis
syntax expressions before physical execution. Checker source 2 passed all eight
physical phases without changing the product snapshot.

Admission remains per selected function, direct call and reciprocal automatic
local binding, using captured identities rather than names, fixed call counts
or record sizes. Only direct local borrowed-member bindings are covered. There
is no general reference escape, ownership/lifetime extension, native return ABI
or DuckDB acceptance claim. Nullable noexcept remains null and grants no
no-throw-cache insertion or cleanup omission permission. Zero protocol live
fields do not prove resource ownership, lifetime duration or destruction.

## Focused unregistered-local rejection

The separate
[borrowed-field-reference-unregistered-local.cpp](../tests/Fixtures/T5/borrowed-field-reference-unregistered-local.cpp)
preserves the original fixture's getter-bound `borrowed` reference and adds
only `const FieldAlias& unregistered = other.payload` plus an event reading
both fields through that local. It is a valid source input checking
that admitting the getter's reference representation does not silently admit
every local with the same reference type. Neither positive source is changed.
The actual public facts establish the identical declared reference type
`entity/000449` for valid getter-bound local 34 and direct-field-bound local 36.
The valid initializer 140 selects getter call 141; the unregistered initializer
148 uses address operation 149 directly. These labels are local to the negative
report. Translation rejected the input with `CSHARP2502`, requiring every
applicable reference local to have its own prevalidated reciprocal call binding,
and published no output. Exact source/object/public-facts/report attribution is
preserved in `t5/borrowed-field-reference-negative-result-1.json` under the
artifact root and in the accepted receipt. Both positive captures remained
unchanged; no corrupt input or broader negative matrix was used.
