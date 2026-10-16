# Composite Boolean call-chain capture

Status: the bounded composite passed production CLI, native/JIT/NativeAOT
comparison, physical checks and affected regressions. The
[final receipt](../acceptance/lifetimes/boolean-call-chain-report.json) is
101,369 bytes, SHA-256
`c9204394017d2120258c9b1f85f6e0f5b328bbd1c5545f624b7168fc99a2786c`.
All 51 expected outcomes passed: 45 ordinary successes and six intentional
rejections, with no unexpected outcome or budget violation. Broader T5 remains
open. The genuine fixture is
[boolean-call-chain.cpp](../tests/Fixtures/T5/boolean-call-chain.cpp).
The coordinator's receipt separately attributes the frozen captures, product,
checker builds and observations. Runtime is unchanged from the borrowed-member
checkpoint; earlier evidence retains its original source/runtime attribution.

The header-free source combines the observed requirements without implementing
DuckDB: an EmptyTag base; Storage with an abstract Opaque pointer and an inert
by-value RefCountCarrier containing another Opaque pointer; DerivedStorage;
Wrapper with a DerivedAlias member and delegating const Boolean conversion;
and Owner with a wide signed-64-bit canary before mutable WrapperAlias payload.
The borrowed getter has no noexcept annotation. Both explicit conversions have
source noexcept annotations. The actual selected roles and body facts below
do not themselves grant no-throw-cache or cleanup-omission permission.

Opaque is complete and abstract, with a pure virtual declaration and an int
field, but no Opaque object is constructed or accessed. Explicit pointer casts
of the two scalar canary addresses carry sentinel address bits only. No pointee
field access, virtual call, allocation, dereference, callback or native adapter
uses those values. The captured target representation/alignment and actual
cast/comparison operations govern any later policy; this source gives no
arbitrary pointee equivalence or dereference permission. Actual capture records
both the signed-64-bit scalar and the Opaque target alignment as 8; the values
remain address-only and do not create an Opaque object in scalar storage.

Two Owner locals are fully initialized by ordinary field stores. Both wide
canaries are representable signed long-long literals and are compared at full
64-bit width. Each borrowed const alias local is initialized by its owner's
actual getter. The first pointer transitions from null to its canary-address
sentinel and back to null; fresh explicit conversions and actual negated
conditions observe each state.
The other owner retains its distinct canary-address sentinel. RefCountCarrier
control pointers use the opposite owner's canary-address view and remain
untouched inert cookies, with no reference count or ownership operations.

The separate native scalar harness used `VALUE_COUNT=19` and invoked the
fixture twice. Event capacity is 64; live is explicitly zero and the unused
last value is zero protocol only, not lifetime or resource evidence.

| Values | Observation |
| --- | --- |
| 0/1 | Initial first-owner conversion / negated condition |
| 2 | Initial other-owner conversion |
| 3/4 | First-owner conversion / negated condition after setting sentinel |
| 5/6 | Borrowed pointer equals its payload / differs from other payload |
| 7 | Borrowed alias member address equals the actual owner's member |
| 8 | Actual Storage base-view address equals DerivedStorage member address |
| 9 | First pointer equals its exact nonnull sentinel |
| 10/11 | Fresh first-owner conversion / negated condition after clearing |
| 12/13 | Other-owner fresh conversion / unchanged pointer sentinel |
| 14/15 | Both inert control cookies remain unchanged |
| 16/17 | Both full-width canaries remain unchanged |
| 18 | Zero unused protocol slot |

No predicted layout, event sequence or result supplied test authority. Actual
capture/native evidence follows; report-local locators are public identity
labels, not persistent handles or a source lookup mechanism.

## Captured composition

The public report describes 1,074 entities and ten bodies. These identity and
structural inventories are not an execution schedule. Owner 764 is complete
24/8, canary field 904 at 0 and mutable payload field 911 at 8. The latter has
alias type 910 (WrapperAlias) over Wrapper 769, complete 16/8. Wrapper's internal
field 912 at 0 has alias 909 (DerivedAlias) over DerivedStorage 855, complete
16/8. Its actual generated base field 913 is Storage 773 at offset 0; Storage
is complete 16/8, pointer field 914 at 0 and RefCountCarrier field 916 at 8.
RefCountCarrier 834 is complete 8/8 with pointer field 917 at 0. These records
have matching complete/nonvirtual quantities and self subobject partners.

EmptyTag 823 is complete/NV 1/1, self-partnered and captured `IsEmpty=true`.
Storage's direct subobject 1027 is at 0 with `IsOptimizedEmptyBase=true`.
DerivedStorage's direct Storage subobject 1039 is at 0 with that flag false;
its indirect EmptyTag subobject 1040 also has the flag false and OriginalType
null. The actual field 913 supplies the base storage/address path; metadata
does not authorize synthesizing an occupied extent or another empty-base field.

Getter 16 is an ordinary nonvirtual, unadjusted method, selected signature 762,
one `this_parameter` (765/descriptor 766), no source/result/copy/variadic roles
and nullable noexcept null. Return 631 contains address-of 632 of explicit
lvalue-adjust 633 to const alias 1009, over mutable payload member place 634
(declared alias 910). The signature and borrowed locals 54/56 use reference
descriptor 763, while return/call values use ordinary pointer 922. Both are
natural 8/8 and point to the identical const type 1009 over alias 910. The
reference and pointer identities remain distinct.

Executable `dik_expression` initializations 196/204 reciprocally target locals
54/56 and use direct getter Calls 197/205. Their callee references select the
identical Function 16/signature 762. Each actual receiver aliases argument 0,
selected descriptor 766 and raw operand 1, evaluated once with retained LTR
flags. Neither initializer has constructor, destructor, copy, constant, lifetime,
master or destruction-link payload. Owner locals first 52 and other 53 have
`initk_none`; their field stores supply their data.

Wrapper conversion 17 and Storage conversion 18 are defined `sfk_conversion`
functions, each with one natural const-record receiver and captured noexcept
true. Their result type 768 is arithmetic kind Boolean, eight bits and 1/1.
The actual wrapper assigns a Boolean local using cast 643 of inequality 644:
its left operand is integer cast 645 of direct Storage Call 646, and its right
operand is signed-integer zero 654. The call's callee Reference 647 selects
Function 18/signature 772; receiver 648 is address-of generated-base member 649
over internal member 650, with actual field identities 913/912. It aliases the
sole `this_parameter` argument and raw operand 1. The call is LTR; the outer
assignment 641 is RTL. These retained normalization nodes are preserved rather
than replaced by the previous DuckDB wrapper body's shape.

Storage's assignment 661 stores cast 663 of inequality 664 into Boolean local
60, then Return 669 reads that local. Member read 665 names pointer field 914
through receiver 61; zero 668 has the identical ordinary pointer type 795.
Pointer 795 is natural 8/8 to complete abstract Opaque 718, whose layout is
16/8 with NV 12/8 and a separate 12/8 subobject partner. Actual reinterpret
cast 151, for example, converts address 152 of other.canary (signed-64-bit
field type 903, alignment 8) to pointer 795. No Opaque object is constructed,
its fields are not selected by executable field references, and its virtual
method is not called. Pointer operations carry/test address values only.
Getter, conversions and fixture have no retained EH/lifetime associations.

## Frozen provenance and native observations

Artifacts are under `/mnt/storage/roslynobj-csharp-20261015/t5/`.

| Evidence | SHA-256 |
| --- | --- |
| Positive source | `c2f5bf85e5ca97c5e4c95b8464712927b7f02aba25f26eca6bf72c18ab42b3da` |
| `capture/boolean-call-chain.roslynobj` | `07c3198fd8d09f6dda99baeebdcf9cff98ef814bb4387ae5cff9be0b56c6bfed` |
| `capture/boolean-call-chain-inspect.json` | `79a7beebb85e7a2882df9e2dd407df7d92aaa9ca835fa30d1b782b21412cb1af` |
| `boolean-call-chain-facts-1.json` | `616a883d29140252858b1ebd63a0bbc1b8b7e52fd814fac7f1c13892422b285d` |
| `boolean-call-chain-capture-provenance-1.json` | `1db6ad5547497a76e78a0dd8214f78fe326f670cd3490f3c7b76fbb591cbb6ea` |
| `native-boolean-call-chain-run-1/stdout.txt` | `4f2e41e18240e45c5c22d05e3a174bfa7db1d0df5214c699dd001026e4793261` |

The 4,980-byte provenance pins the unchanged 3,898-byte source, 903,168-byte
object, all six measured phases, inspector/probe/harness/binary and tool hashes.
Producer SHA-256 is `cf619053cbedabca46da3eab3e8278de43fa4a25ef770a56572454abf9ff25d4`;
flags are `--g++ --c++17 --gnu_version 130300 --exceptions`, with RTTI enabled.
The unchanged public input is read through ProbeVersion 6. Native binary SHA-256
is `bc49b020ed78cad2c48edefd274896f68fdebb47792af03c145cb8e302f67480`.
Both independent native invocations report exactly:

```json
{"result":3,"overflow":0,"live":0,"values":[0,1,1,1,0,1,1,1,1,1,0,1,1,1,1,1,1,1,0],"events":[100,201,301,400,500,601,700,801,901]}
```

## Independent pointer-slot rejection

[boolean-call-chain-pointer-slot.cpp](../tests/Fixtures/T5/boolean-call-chain-pointer-slot.cpp)
preserves the positive source independently and adds automatic
`Opaque** pointer_slot = &first.payload.internal.pointer`, reads its pointer
value, stores null then the saved pointer through it, and observes the resulting
pointer in value 9. VALUE_COUNT stays 19. This accesses only the real pointer
field, never an Opaque object. Source SHA-256 is
`c2ba30b670c65bde30d38b4c510e3c647d37b94d4e6b02e4189e25371f9b10c3`.
All six independent capture/native phases passed. Actual automatic pointer_slot
57 has ordinary 8/8 pointer type 913 to pointer 820, which in turn points to
complete abstract Opaque 743. Saved_pointer 58 has type 820. Execution
initializations 294/307 target those exact locals using expression operations
295/308, with no constructor, destructor or lifetime association. Thus this is
an actual P** transport input, not a source-name guess or an Opaque field access.
The independent native observations equal the positive's complete two-run JSON;
that equality does not authorize unsupported representation transport.

Negative object SHA-256 is
`d84a7ac6234df7d8d415986536894aa9796c74281524ed2dabbdfb56b21d5c5e`;
public facts SHA-256 is
`36db1b755ce5278955720505ab113147ccee2e271b2cb40ab7eed55a9e900a94`.
Its 5,227-byte `boolean-call-chain-pointer-slot-capture-provenance-1.json`,
SHA-256 `b32d43c134e48106f3d2f9c9d4c0b7375010708cae6540eda52b3abb656437fa`,
pins the separate immutable source/object/probe/native tools and all six phases.
The new product rejects this companion with `CSHARP2502`, family
`address-only-pointer`, before publication. Both source-valid inputs have the
same native output, but only the positive uses the admitted pointer transports.
The five prior negative inputs also remain unpublished. No corruption test or
further negative matrix was introduced; the positive source is unchanged.

## Completed generated and physical proof

The [BooleanCallChain checker](../tests/BooleanCallChain/README.md) matched the
complete positive native JSON in JIT and NativeAOT: result 3, all 19 values and
all nine events, twice in each mode. It used explicit JsonNode output and
separate supplied capture provenance, bound by the receipt to actual compilation
and publication. Its actual source-2 entry points were
`Translated.Program.view(Owner*)`, `operator_bool(Wrapper*)` and
`operator_bool__2(Storage*)`, the latter two returning captured signed-byte
Boolean storage as exact sbyte 0/1, not a new native Boolean ABI.

Both physical modes measured five complete carrier sizes 24/16/16/16/8,
seven offsets 0/8/0/0/0/8/0, sequential-prefix alignment 8 and matching array
strides. They used separately initialized Owner storage, actual getter calls on
both owners returning distinct exact payload pointers at +8, and both actual
conversion bodies. Each independently returned `[0,1,1,0,1]` across null first,
nonnull other, nonnull first, cleared first and unchanged other states. All
actual scalar address bits/alignment, alias/generated-base identity, borrowed
views, inert control cookies and full-width canary checks passed. No Opaque or
EmptyTag CLR carrier, pointee access or virtual execution was introduced.

The receipt verified 255 product source files, nine checker snapshot files and
23 immutable inputs unchanged. Repeat publication preserved generated source/
project bytes and normalized report content. Six affected prior C++ profiles
preserved source bytes and JIT/NativeAOT observations; three virtual profiles
preserved generated source bytes with their original runtime attribution. One
existing pinned native callback regression separately retained result 751 and
seven exact events in JIT/NativeAOT. The first product snapshot's compile-only
RoutineType discrimination failure is preserved; no translation ran from it.
Source 2 supplies the final proof. No additional rerun is needed for this note.

There are no source constructors/destructors, heap operations, EH constructs,
native calls or virtual method execution. This is one capture-first composite,
not a general smart-pointer implementation, ownership proof or DuckDB root
acceptance. The reusable method rule remains limited to selected fixed const
receiver Boolean methods/conversions with the captured Boolean/int32 operations
and individually validated direct calls; it is not generic arbitrary-method
admission. Explicit complete nonvirtual field carriers and the frozen per-use
address-only pointer audit grant no P**, pointee dereference/stride/value layout,
unregistered escape, native/unknown/variadic/callback or nested ABI transport.
Captured noexcept true on conversions and null on the getter remain facts,
without NoThrow-cache or cleanup-omission permission. There is no ownership,
lifetime-extension, general smart-pointer or actual DuckDB execution claim.
