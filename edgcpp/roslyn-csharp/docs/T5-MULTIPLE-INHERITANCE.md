# Ordinary multiple inheritance and secondary dispatch

Status: bounded production CLI, native/JIT/NativeAOT execution, physical layout,
all five canonical entries and borrowed class/VMI provider checks pass. Broader
T5 remains open. The [consolidated receipt](../acceptance/lifetimes/multiple-inheritance-report.json)
records 50 expected outcomes: 44 ordinary successes, two prior termination
signals and four intended unsupported rejections.

The header-free C++17 candidate is
[multiple-inheritance.cpp](../tests/Fixtures/T5/multiple-inheritance.cpp).
Its SHA-256 is
`1a6c050ed3b387d74842d8d9196cad6bb8e477389243e421d388a287f873f3db`.
It declares two polymorphic bases, `Left` and `Right`, and `Derived : Left,
Right`. Every selected source constructor, virtual method and explicitly
nonvirtual destructor is `noexcept`. Separate `dispatch_left(Left*, int)` and
`dispatch_right(Right*, int)` helpers perform virtual calls. Their receiver
types remain supplied by callers rather than recovered by source lookup.

One automatic scope contains complete Left, Right and Derived objects. Stored
Left and Right pointer views of the derived object pass through the respective
helpers and expose their corresponding scalar fields. Both derived overrides
read both base ids and the derived bonus, so an incorrect receiver adjustment
affects observable method results. The complete base objects also call the
same helpers, activating the original base method bodies and their distinct
events. There is no throw, allocation expression, virtual base, virtual
destructor, RTTI query, member pointer, native application call or native
exception unwinding. Capture must preserve the ordinary RTTI flags.

## Finite comparison protocol

The existing `fixture()` / `lifetime_*` API, nine values and 64 event slots are
unchanged. The NormalLifetime checker can invoke the fixture twice and compare
the entire separately produced native JSON, including ordered events, live
count, overflow and prior-live evidence. Each invocation resets only the
observation arrays/count/overflow; it never resets live ownership.

| Value slot | Observation |
| --- | --- |
| 0 | Complete Left dispatch result |
| 1 | Complete Right dispatch result |
| 2 | Derived dispatch through its stored Left pointer view |
| 3 | Derived dispatch through its stored Right pointer view |
| 4 | Scalar id read through the stored Left view |
| 5 | Scalar id read through the stored Right view |
| 6 | Derived bonus |
| 7 | Live constructor units after the automatic scope ends |
| 8 | Prior live constructor units before the current invocation |

The fixture returns the sum of its four dispatch results. Each completed base
constructor and the derived constructor owns one matched test live unit;
actual source destructors release those units. This is finite observation
bookkeeping, not inferred runtime object ownership. Event families distinguish
Left construction/destruction, Right construction/destruction, Derived body
construction/destruction, both helper entries, both base implementations and
both overrides. No predicted event array or layout constant is encoded as a
native oracle. Actual captured facts and independent native observations must
establish them before generated execution is claimed.

## Questions for the unchanged public-model capture

The first inspection must retain complete and occupied-base record types,
layout sizes/alignment, physical fields and base-subobject paths. The actual
secondary offset must come from those descriptors, including any reused tail
storage. Constructor/destructor entry roles and body forwarding must remain
selected identities; no source-name remapping or lexical cleanup inference is
permitted.

For both stored base pointer conversions and both indirect helper calls,
inspect the actual typed operands, receiver category, adjustment path and
evaluation flags. Follow the selected application vtable constants, including
every signed offset-to-top value and exact address-point byte offset. Preserve
positive and negative constants without normalizing them to a guessed primary
table. Follow each actual routine address to its selected entry and any
generated wrapper/thunk, with its retained receiver/return adjustments and
operation body. An override declaration or virtual ordinal alone does not
authorize a reconstructed target or adjustment.

Typeinfo and any additional external ABI data dependencies must be inventoried
through their actual public constant/address closure. Generated application
tables and generated typeinfo objects remain distinct from borrowed native
provider data. The existing exact two-symbol RTTI address provider does not
pre-admit another export, offset, descriptor, provider file or native RTTI
method call. Any new required provider scope needs its own concrete audit and
review. Metadata recipes and catalog rows remain separate from actual
initialization and execution occurrences.

The bounded projection and physical receiver/thunk proof follow these retained
relationships. Virtual inheritance, virtual destructors,
covariant returns, RTTI queries, member pointers and native unwinding remain
separate unsupported forms, rather than implied by this fixture.

## Actual capture and native observations

The six measured capture/inspect/public-facts/native phases passed with unchanged
source and input. Provenance is `t5/multiple-capture-provenance-1.json`, SHA-256
`638dcc48f4dd00cc7b6711f49291442de746cd3c43b8161a3e0a0abf06396644`.
The exact input SHA-256 is
`a9564cf5ea57d5a84aa11c981138e91e03574f28f9d44b0d69d372a873ffcf17`.
The inspect report SHA-256 is
`a62fd0a235e30097df627cd238746124a4a710c7112a5b3e3950c6c98b328ee5`.
Public facts are `t5/multiple-inheritance-facts-1.json`, SHA-256
`324e8a00d781c628513c14e44453ffcf9e7150133fc56eeb69106d5077bcce02`,
with 1,321 public entities and 27 bodies. The capture uses the frozen EDG
revision `89e67e07c7f6d0fc38622f9778d8e79f40deaa0f` and unchanged flags
`--g++ --c++17 --gnu_version 130300 --exceptions`; RTTI was not disabled.
Entity numbers below are public probe locators, not an execution schedule.

The separate native binary SHA-256 is
`fdcbb1983d809f973073476982983a6b2d2ca9a31eb207702ed0107ef4673972`.
Its two-run observations at `t5/native-multiple-inheritance-run-1/stdout.txt`
have SHA-256
`39bf0d3409f7182d1a0e1b85f611b22e846990cc23a878c509138fa43985f274`.
Both invocations return 109 with values `5,11,36,57,5,7,11,0,0`, overflow 0,
live count 0 and events
`102,203,105,207,511,1100,602,1200,703,1100,805,1200,907,1023,407,305,403,302`.
These are native observations, not generated evidence.

## Captured records, tables and selected thunk

| Public record | Complete size/alignment | Occupied-base facts and physical fields |
| --- | --- | --- |
| Left 869 | 16/8 | Nonvirtual size 12; partner 1138 is 12/8; vptr at 0, left id at 8 |
| Right 877 | 16/8 | Nonvirtual size 12; partner 1067 is 12/8; vptr at 0, right id at 8 |
| Derived 883 | 32/8 | Left field 1139 at 0; Right field 1068 at 16; bonus field 1069 at 28 |

Direct base subobject 881 selects Left at offset 0 and shares virtual-function
information. Direct base subobject 887 selects Right at offset 16 and does not
share that information. Both are public, direct and nonvirtual. The occupied
12-byte base views are captured storage views inside complete owners; these
facts alone do not grant standalone CLR layout or a native by-value ABI.

Application tables use const signed-64 elements, each size/alignment 8/8:

| Generated table and initializer | Actual retained elements in ordinal order | Address-point byte offsets |
| --- | --- | --- |
| Left variable 55, constant 1034; 24/8 | `0`, RTTI variable 53 + 0, method 1 + 0 | 16 |
| Right variable 65, constant 1040; 24/8 | `0`, RTTI variable 63 + 0, method 3 + 0 | 16 |
| Derived variable 74, constant 1046; 56/8 | `0`, RTTI variable 69 + 0, method 4 + 0, method 5 + 0, `-16`, RTTI variable 69 + 0, thunk 6 + 0 | 16 and 48 |

The signed offset-to-top values are 0 and -16; this input has no positive
offset-to-top value. Actual constant values 1142 and 1144 preserve the two
Derived address points. Both helper bodies remain actual indirect calls:
Left call 289 and Right call 311 load slot 0 from their respective receiver's
vptr, with an ordinary scalar argument and the selected this argument.
No source-name dispatch rebinding is needed or permitted.

Thunk function 6 is the actual defined
`_ZThn16_N7Derived14evaluate_rightEi`, with `sfk_none`, noexcept effective true,
selected this parameter plus one ordinary integer, `ThisAdjustment=-16` and
`ReturnAdjustment=0`. Public wrapper links select overriding function 5 and
overridden function 3. Its body explicitly assigns its captured this variable
through a byte-pointer adjustment of -16, then uses execution node 276,
`enk_result_of_overriding_function`, to obtain the overriding result and returns
it. That node has no generic operands. Projection must follow this actual body
and wrapper contract; applying the metadata adjustment again would duplicate
the recorded adjustment.

The passing physical proof separately executes the primary
Derived address point's slot 1 (method 5) with a complete Derived receiver and
the secondary address point's slot 0 (thunk 6) with the actual Right subobject
address. Distinct canonical entries reach the same selected override with result 57 and
method event 907. The two ordinary helper
sites still exercise their actual loaded slot 0. Both JIT and NativeAOT execute these entries.

## Additional external data and exact initializer role

This input requires existing external variable 51,
`_ZTVN10__cxxabiv117__class_type_infoE`, and new external variable 52,
`_ZTVN10__cxxabiv121__vmi_class_type_infoE`. The earlier single-inheritance
`__si_class_type_info` table is absent here. Both declarations are
`sc_extern`, static duration, `initk_none`, lowering-generated and address-taken,
with no initializer, alias, weak or local-static initialization facts.
Their public types 1027 and 1028 are incomplete unbounded arrays without
size/alignment. The element is const typeref 1200, size/alignment 8/8, whose
underlying type 1074 is signed 64-bit integer. No 88-byte EDG array is inferred.

Address value 1289 selects exact external variable 51 + 16. It occurs in both
Left RTTI variable 53's constant chain `1030 -> 1203 -> 1254` and Right RTTI
variable 63's chain `1036 -> 1211 -> 1259`. The new address value 1314 selects
exact external variable 52 + 16, with `OnePastEnd=false`, null SubobjectPath,
no reflection flag and no parameter number. Its complete data-parent closure
is the Derived RTTI initializer:

```text
Derived record 883.TypeinfoVariable = generated variable 69
  variable 69.ConstantInitializer = constant 1042
    component 0 constant 1219: __class_type_info 1201
      component 0 constant 1271: __EDG_type_info 1252
        component 0 constant 1298: pointer 1081
          AddressValue 1314: exact external variable 52, ByteOffset 16
```

The nested fields are root base field 1263 at offset 0, class metadata base
field 1250 at offset 0, and EDG metadata vptr field 1286 at offset 0. The leaf
constant retains an implicit/reinterpret-like conversion. The root variable
type is const typeref 1041 over anonymous lowered record 1217, size/alignment
56/8. Its other fields are flags at 16, base_count at 20 and base_info at 24.
Their constants retain flags 0, count 2 and two generated base-typeinfo
addresses with raw offset_flags integers 2 and 4098. These bytes are captured
constant storage; this note does not execute an RTTI algorithm or reconstruct
the encoding.

The anonymous root has no VirtualFunctionTableVariable link. Its embedded
`__class_type_info` record 1201 links the ordinary class table 51, not table 52.
Thus a VMI admission rule must anchor the generated root in the actual
application record's TypeinfoVariable relation and follow the exact typed
initializer field/value path. A root type name, size or fabricated canonical
VMI type relationship would not establish that use.

All non-type data parents of address 1314 close to that initializer. No body
operation references external variables 51/52 or generated RTTI variables
53/63/69. No operation targets the anonymous root metadata fields or the EDG
metadata vptr/name fields. Application table initializers refer to those
generated RTTI objects and generated routines; the external provider table is
not an application dispatch table. Metadata cleanup recipes add no execution
to this constant closure.

## Approved bounded provider addition

The attributable read-only audit is `t5/multiple-provider-audit-1.json`, SHA-256
`726eec58b8c91897616cd7fc8ce3eba0ab733cd2fab05686e505a855b1cab758`.
It preserves exact relevant command rows, references the prior unchanged-file
ELF/tool audit, and records the actual public initializer closure. That raw audit predates runtime execution; the final receipt below supplies
new-range loaded-owner and map-permission evidence.

Read-only `ldd`, `readelf --dyn-syms --wide`, `objdump -T` and relocation/program
header inspection select the same pinned provider:
`/usr/lib/x86_64-linux-gnu/libstdc++.so.6.0.33`, SHA-256
`1fd75fe70354a416d75aef22bcae68c47bd25d20e2d0568c30b1a9838cf62f11`.
The new exact VMI export is `OBJECT`, `WEAK`, default visibility, default
version `CXXABI_1.3`, ELF symbol size 88, RVA `0x26ec98`. Its range
`[0x26ec98,0x26ecf0)` lies in `.data.rel.ro` and wholly inside PT_GNU_RELRO
`[0x26c310,0x277000)`. The native oracle has an `R_X86_64_64` relocation to
the same versioned export with addend `0x10`; it has no COPY relocation for it.
The earlier provider audit's file identity/build ID remains applicable to the
unchanged bytes. ELF symbol size is a provider fact, not an EDG type size.

The approved runtime change adds only that exact descriptor to the existing
resolver allowlist, preserving the previously admitted descriptors. The
allowlist addition is exercised in JIT/NativeAOT; existing hash, owner, RVA, read-only
extent, cache and process-lifetime behavior are unchanged. Generated code must
store the resolved export base plus the actual captured 16-byte offset once,
retain the provider handle for process lifetime and borrow its data.
Default-export version metadata is not a runtime versioned lookup claim.
Actual loaded owner/RVA and read-only/non-executable mapping for this new range
pass resolver and independent JIT/NativeAOT physical checks. The finite descriptor
addition is not a general RTTI or native data-provider admission.

Calls through native RTTI method slots, arbitrary data reads/writes/frees,
RTTI queries, virtual inheritance/destructors, member pointers and native
unwinding remain outside the proposal. The anonymous generated metadata
aggregate, generated application tables and selected thunk all remain consumer
projection obligations; borrowing one native data address implements none of
those application bodies.

## Verified publication and execution

Production `t5/multiple-cli-generated-4` contains all 27 required bodies and
exactly two required DATA bindings, with no diagnostics or native function
imports. Both complete fixture invocations match the native result 109, nine
values and all 18 events. Separate physical JIT/NativeAOT checks establish
complete sizes 16/16/32, borrowed views 12, complete alignment 8, bases at 0/16,
bonus at 28, address points 16/48 and signed prefixes 0/-16. All five loaded
canonical entries actually execute: results 5/11/36/57/57 and method events
602/703/805/907/907. Two complete objects of each type balance ten live units
through the full 25-event physical trace.

The thunk UCO entry retains its captured Derived* formal. Its caller uses the
captured Right* signature and actual secondary-subobject address under the
separately exercised natural 8-byte pointer ABI; neither dispatcher nor UCO
pre-adjusts the bits. The actual body performs -16 once, then the payloadless
result occurrence calls the exact selected overriding body with current
execution parameters. The typed plan carries Call/MayThrow effects and the
closed no-throw graph follows that selected edge. Pinned stock source/blob/hash
provenance is `t5/multiple-contract-source-1/provenance.json`; in particular
`c_gen_be.c:5190–5223` records this current-parameter contract.

The class export serves two actual generated typeinfo roots through one shared
AddressValue; VMI serves one. Admission tracks allowed (root, value) pairs.
Both provider rows independently verify hash, export BASE, owner/RVA/read-only
mapping and generated BASE + 16 once. The anonymous VMI root retains raw flags,
count and base-info constants, without a fabricated vtable-type link or RTTI
algorithm. The alignment anchor is a CLR-only alias of existing vptr bytes,
not an EDG field. Packed views have no standalone/by-value/native ABI claim.

The product snapshot is `t5/multiple-source-4` (222 files), and the checker
snapshot is `t5/multiple-checker-source-1` (nine files). All 13 retained input
hashes, provider content and completion-file hashes verify. Repeat source,
project and DATA manifest are byte-identical; reports agree after output-path
normalization. Seven prior C++ publications retain byte-identical application
sources and pass their relevant JIT/NativeAOT checks. Generated dependencies
remain checker + Generated + Runtime, without the reader or native SQLite.

Receipt SHA-256:
`4e82c5124a6a2b296c672fa52515e052350ed6a58b6dbd39b399cf79f7e7c329`.
Artifacts remain under `/mnt/storage/roslynobj-csharp-20261015/t5`. The initial
compile-only accessor error and two fail-closed traversal attempts are preserved
separately; none published output. Their corrections consume only the actual
payloadless execution leaf and the two retained automatic pointer initializer
values, with explicit role/target/type/payload guards.

This proves one fixed -16 receiver adjustor, two indirect sites and the exact
three-record/two-base profile. Virtual inheritance/VTT, virtual destructors,
covariant returns, dynamic vcall/vbase adjustments, RTTI queries, member pointers,
static guards/TLS, native unwinding and broader ABI/scale remain open. The next
capture-first target is ordinary scalar dispatch through a virtual base with
its actual retained VTT/constructor roles; no capability is assumed in advance.
