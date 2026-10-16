# Ordinary generated const unsigned-byte field accessor

Status: this bounded slice passed production CLI publication, complete
native/JIT/NativeAOT comparisons and physical layout/storage checks. The
[verified receipt](../acceptance/lifetimes/ordinary-const-method-report.json),
SHA-256 `57036cea331ccde345e8427dd17401fd40ed4b6da25534dbed5b696cf3bf773c`,
records all 31 expected outcomes: 27 successful phases and four intended
unsupported-case rejections, with no unexpected outcome or budget violation.
The preceding mutable-reference milestone is checkpoint `cd36a75d1`; runtime
and providers remain unchanged from that checkpoint. Broader T5 remains open.

[ordinary-const-method.cpp](../tests/Fixtures/T5/ordinary-const-method.cpp)
defines header-free trivial `Cell { unsigned char kind; }` and ordinary method
`unsigned char read() const { return kind; }`. The method has no noexcept
annotation. The fixture declares its local without aggregate initialization,
stores 5 into the field, calls the method, stores high-bit value 201, calls the
same method again, and observes the final field. Event calls occur after each
store and each method result. No constructor/destructor, allocation, exception
construct, explicit reference, virtual dispatch or native provider is requested.

The unsigned high-bit value tests the actual captured unsigned scalar
representation; neither C# signedness nor a source spelling substitutes for
the retained type facts. The input contains two call sites to exercise the
same receiver storage before and after mutation. This is not a proposal to
special-case those sites or a fixed number of methods in consumer admission.

The existing native scalar harness used `VALUE_COUNT=9` and invoked the fixture
twice. Its scalar exports are `fixture` and the existing `lifetime_*` observers.
Values 0/1 are the first/second method results, value 2 is the final kind, and
values 3–8 are unused zero protocol slots. `lifetime_live()` is also explicitly
zero. Cell has no counted resources; these zeros are not lifetime, ownership
or cleanup evidence. The event buffer has capacity 64 and observable overflow.
Actual native execution established the complete comparison oracle below;
source predictions did not supply acceptance.

## Frozen capture and separate native evidence

Artifacts are relative to `/mnt/storage/roslynobj-csharp-20261015/t5/`.

| Evidence | SHA-256 |
| --- | --- |
| Fixture source | `30e56d8d13c24afb85d064a369ba54ef00dd621963ee5a81fdb40848246a51f6` |
| `capture/ordinary-const-method.roslynobj` | `c08954ba17043666b11734180dac43581b926852a530f301a080193d9bb009dc` |
| `capture/ordinary-const-method-inspect.json` | `06a1e599295530aa86d73be19945d1454503ab6fdb32ecd9d8870f1dae1a42cd` |
| `ordinary-const-method-facts-1.json` | `4ab36ee343feb3601f46cc7ffa735b476d2786ae5659167bd442ce140827f5d8` |
| `ordinary-const-method-capture-provenance-1.json` | `8fd174f6a07bdbc15ddb838687de5bab1ce8fab3d0b74a9731908b9607866f01` |
| `native-ordinary-const-method-run-1/stdout.txt` | `ebd65acad76d2d9141c22d392454f9fa2968b01952e9c470b064c1f8d4fba56e` |

Provenance records the frozen producer SHA-256
`cf619053cbedabca46da3eab3e8278de43fa4a25ef770a56572454abf9ff25d4`,
exact source/input hashes, inspector, runner, all six measured phases and
unchanged source. Capture flags remain `--g++ --c++17 --gnu_version 130300
--exceptions`, with RTTI enabled. ProbeVersion 6 used the frozen
`virtual-facts-bin-7/LifetimeFacts.dll`, SHA-256
`82c6933d380168c85216ccd3e6d595473be48f63be5e1d28c47f7d49cc1bfe82`,
with source manifest SHA-256
`bd3cd339c30250d8bf496b01a6936710c66712ee1932d6fa13b52662c59732a8`.
The native binary `native/ordinary-const-method/native` has SHA-256
`6ee7c8f7de90232c61b383a148a55bf2bf292538289ba3553fe03e4986c4ee6b`.
It used the existing `tests/Lifetimes/native.c` harness, whose SHA-256 is
`587755a8b10cb7366007e13cb5d41d7a9bcfebadc6bde3d868f75f2d3cc3693c`.

The report contains 368 public semantic identities, eight bodies and 184
structural relationship occurrences. These are inventory counts, not an
execution schedule. Locators below are local public semantic-equality labels.
The separate native executable reports this full observation twice:

```json
{"result":206,"overflow":0,"live":0,"values":[5,201,201,0,0,0,0,0,0],"events":[15,105,221,401]}
```

This establishes the actual unsigned high-bit observation and finite event
oracle. It is not generated execution evidence or a lifetime/ownership proof.

## Actual selected method, receiver and body

Method entity 15 is a defined `sfk_none` / `cdk_none` ordinary method. Its
captured current name is `_ZNK4Cell4readEv`; that retained name is descriptive,
not a reconstructed selection rule. It is nonvirtual, with a null primary entry,
no alternate entries and zero recorded receiver/return adjustments. IsUseComdat
is true; weak, weakref, GNU alias, ifunc and alias-entry flags are false, and
GNU alias/version/representative relationships are null.

Selected signature 269 has prototype, default calling convention, C++ language
linkage, ThisClass 271 and `HasThisParameter=true`. Its return type 270 is
`unsigned_integer`, 8 bits, size/alignment 1/1. There is no variadic, hidden
result or return-copy convention. The sole selected/execution parameter has
ordinal 0, role `this_parameter`, type 272 and descriptor 273; the execution
variable is 32, not a source parameter. `PassedViaCopyConstructor=false`.
The method and caller fixture both retain `NoexceptEffective=null`, not true.

Receiver type 272 is an ordinary nonreference pointer, size/alignment 8/8,
to const typeref 326. That typeref has `IsConst=true`, `IsVolatile=false` and
underlying complete Cell 271. Receiver variable 32 separately has type 304,
a top-level const typeref around pointer type 272. Its actual body value read
uses type 272. These two const layers must not be confused with a reference
type or a copied object.

Cell 271 is complete size/alignment 1/1 with one field, kind 318, at offset 0
and type 270. Its occupied size is also 1. It has no base/subobject graph,
virtual methods, vtable/VTT or typeinfo-variable association. Method body block
177 contains only Return 178 of operation 179, `eok_points_to_field` with
unsigned-byte prvalue type 270. Its operands are receiver Reference 180,
targeting variable 32 with type 272, and field Reference 181 targeting field
318. The return has no target/common-lifetime association. There are no locals,
reachable lifetime associations, cleanup regions/tables, prologue or epilogue.

## Actual direct calls and field-store timeline

The fixture local `cell`, variable 30, has type Cell 271, `initk_none` and no
initializer/constant association. Its value is supplied by actual field stores,
not by an aggregate or constructor adapter.

| Call | Exact retained receiver relationships |
| --- | --- |
| 106 | Raw operands are callee 107 and receiver 108. Address-of 108 has type 272; operand 109 is an lvalue-adjust to const type 326 of Reference 110 targeting local 30. |
| 142 | Raw operands are callee 143 and receiver 144. Address-of 144 has type 272; operand 145 is the same const lvalue adjustment of Reference 146 targeting the identical local 30. |

Both calls directly target method 15, select function type 269, return prvalue
type 270 and record left-to-right operand evaluation. Each has exactly one
argument, ordinal 0 / `this_parameter` / descriptor 273, whose value is identical
to the Receiver facet and raw operand 1. There is no return slot, source target
or retained virtual call. The aliasing facets must not introduce another
receiver evaluation or argument. Public target identity selects the method;
neither the captured mangling nor a source method lookup supplies it.

Actual assignment 85 writes field kind through local 30 with constant 5,
whose recorded type is already unsigned byte 270. The first result uses
Call 106, with captured cast 105 to signed integer type 266 for the observer.
Later assignment 121 writes constant 201 of the same unsigned-byte type 270
to that same local/field before Call 142. Cast 141 promotes the second result
to type 266; the final field observation has its own captured cast 162. Both
field assignments record right-to-left evaluation. The intervening note calls
and method results remain actual retained statement occurrences. No source
lookup, inferred signed narrowing or cached first result supplies the second.

## Reusable admission and finite scope

The accepted rule derives from each selected public function and its actual
field-return body. It requires an ordinary nonvirtual, unadjusted const
receiver with natural 8/8 pointer representation and an unsigned-byte return
of that receiver's own exact captured field, with no calls, locals, lifetimes
or EH. It preserves the receiver variable's const-pointer wrapper separately
from its actual value/signature type and consumes the selected argument/receiver
alias once. Admission is not tied to a fixture name, fixed method/call count,
record size or field count. This campaign exercised one actual method and its
two captured direct calls; it did not execute every shape the rule might admit.

The captured nullable noexcept fact remains null. This separate body-capability
proof neither inserts the method into `validatedNoThrowFunctions` nor grants
`EnsureGeneratedNoThrow` permission to omit caller cleanup or EH markers.
No native/callback method ABI, adjusted/virtual/indirect/member-pointer dispatch,
reference return, general enum return, unknown/throwing callee or arbitrary
class/string/link contract is accepted. The actual DuckDB GetVectorType body
and complete Vector dependencies remain unproven; no Vector or DuckDB root
admission follows from this prerequisite.

## Production CLI, JIT and NativeAOT proof

The coordinator froze product snapshot `ordinary-const-method-source-1` and
the nine-file `ordinary-const-method-checker-source-1`. Their source manifests
have SHA-256
`60ec4e4cec2823baf11cf76a81eea16b31853ba9290cab7ab115492c1ebb6557`
and `fe32e27c746465a5262616c684cffb7f7dcbc4ff62b133bee21d1f0804918dcb`.
Both freezes, all 17 recorded input/content hashes and every publication
completion were verified. The [focused checker](../tests/OrdinaryConstMethods/Program.cs)
used complete two-run native JSON comparison with explicit JsonNode arrays and
protocol-only zero-live checks. The immutable production output is
`t5/ordinary-const-method-cli-generated-1/Generated.cs`, from product snapshot
`ordinary-const-method-source-1`. It declares actual `Translated.Cell` with
explicit size 1/pack 1, public byte `kind` at offset 0, and generated method
`Program.read(Cell*)` returning that byte directly. All eight required bodies
published with no required imports. JIT and NativeAOT matched the entire native
result/value/event object twice, including unsigned result 201. The physical
checker used actual published Cell/field names for direct measurements, with no
reflection or guessed aliases. Both modes also produced:

```json
{"ordinaryConstMethodPhysical":{"cellSize":1,"kindOffset":0,"sequentialPrefixOffset":1,"unsignedHighBitValue":201}}
```

The high-bit observation read the typed field and raw byte after storing 201.
The native-compatible application trace separately checked the generated
method return; the physical probe did not substitute for execution. Zero live
and unused slots retained their protocol-only meaning. Dependencies contained
only Checks, Generated and the existing runtime, with no native application
fallback. Runtime source hashes remained unchanged from the mutable-reference
checkpoint; no runtime or provider adapter was added.

The normal const-reference, mutable-reference aliasing and exact class-payload
regressions retained byte-identical generated application sources and passed
JIT/NativeAOT execution. Prior single-inheritance, multiple-inheritance and
virtual-inheritance re-emissions also retained source bytes; those were
emission-only checks, not new runtime proofs. Four existing negatives remained
unpublished. Repeat translation produced byte-identical source/project and
equal program reports after output-directory normalization. No broad SQLite or
inheritance execution rerun was claimed. Broader T5/T6, general enum/Vector
admission and DuckDB class/string/base/allocator/link closure remain open.
