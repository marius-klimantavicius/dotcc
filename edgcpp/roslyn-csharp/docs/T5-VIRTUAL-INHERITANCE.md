# Virtual-base construction and destruction dispatch

Status: the bounded production CLI, native/JIT/NativeAOT and physical VTT
proof now pass. All 28 required bodies and three address-only DATA imports
publish. [The compact receipt](../acceptance/lifetimes/virtual-inheritance-report.json)
records 55 expected outcomes; broader T5 remains open. Runtime behavior and its
existing three data descriptors remain unchanged. Earlier nonvirtual-base
evidence retains its original source attribution.

The candidate is
[virtual-inheritance.cpp](../tests/Fixtures/T5/virtual-inheritance.cpp).
Its SHA-256 is
`ad7b78d8d6c7e56a8aa64c6de87a2e6caedd46bdefd69df21d3e3408d6395e34`.
Root has an integer id and a noexcept virtual scalar `evaluate(int)` method.
Branch virtually inherits Root, adds an integer branch id and overrides the
method. Final inherits Branch, adds a `long long` bonus and supplies another
override. All source constructors, methods, helpers and explicitly nonvirtual
destructors are noexcept. The long bonus is a layout stimulus; neither its
declaration nor host C++ expectations determine a captured offset.

The application constructs only one complete automatic Final object. Branch's
constructor calls a separate `dispatch(Root*, int)` helper after initializing
its scalar field. The complete Final path stores a Root pointer view and calls
the same helper. Final's destructor calls it before its derived observation
unit is released, and Branch's destructor calls it again. Both overrides read
the actual virtual Root id and their own scalar fields. Distinct body events
and returned values make the four lifetime phases observable without source
native calls or introspection callbacks.

There is no throw, heap allocation, virtual destructor, RTTI query, member
pointer, custom handler or native exception unwinding. Capture must preserve
ordinary RTTI flags. The projection adds no exception runtime or native RTTI execution capability.

## Existing finite protocol

The `fixture()` / `lifetime_*` scalar API retains nine values and 64 event slots
for the existing NormalLifetime checker and separate two-invocation native
oracle. Each fixture call resets only the event/value observations and
overflow flag. It never resets the live count, and records prior live count
last so a previous invocation cannot hide a leak.

| Value slot | Observation |
| --- | --- |
| 0 | Dispatch result from Branch constructor body |
| 1 | Dispatch result while the complete Final object is alive |
| 2 | Dispatch result from Final destructor body |
| 3 | Dispatch result from Branch destructor body |
| 4 | Root id read through the stored complete-object Root view |
| 5 | Branch id |
| 6 | Final bonus converted to int |
| 7 | Live constructor units after ordinary destruction |
| 8 | Prior live constructor units before this invocation |

The fixture returns the sum of the four dispatch observations after the
automatic scope has ended. Each completed source constructor owns one matched
test live unit, released by its corresponding source destructor. That is
observation bookkeeping, not a proposed consumer ownership model. The complete
native values/events must be recorded before generated execution is claimed;
the source embeds no expected event array as an oracle.

## Required actual capture review

The public trace must retain complete and occupied-subobject types, all actual
base-subobject offsets and paths, virtual-root storage and every physical
vptr/vbase field. Inspect captured long-integer width/alignment and reused tail
storage before proposing a CLR/native representation. Complete Branch bodies
remain part of all-definitions output even though this fixture constructs only
Final; catalog and execution roles must be reported separately.

Record every actual C1/C2/D1/D2 body and its selected parameter order, roles and
types, including hidden VTT or other ABI parameters. Keep forwarding and
alternate-entry identities distinct. Follow constructor/destructor calls,
actual initialization/state occurrences and metadata cleanup recipes without
inferring lexical cleanup or treating a recipe as an executed call.

Follow generated application vtables, VTT objects and construction tables
through exact constant/address values. Preserve signed offset-to-top, virtual
base and vcall constants and actual byte address points. Any table/subarray
selection must remain a public captured relationship. The four helper calls
must expose their actual lowered receiver conversions, memory loads, method
entries and adjustment wrappers for their respective construction/destruction
states. No source-level override lookup, fixed virtual-root offset or guessed
VTT index may replace those facts.

Close any external ABI data dependencies through exact generated initializer
roles and provider descriptors. Existing address-only class/SI/VMI table
descriptors do not authorize native RTTI calls, arbitrary data access or a new
provider. Borrowed ABI metadata and generated application construction tables
remain separate. New lowering/runtime work requires review of this genuine
positive trace before implementation.

Virtual destructors, RTTI queries, member pointers, throwing constructors for
this new layout, native unwinding and wider inheritance forms remain separate
obligations. The following facts come from the actual unchanged input and
separate native observation, rather than source layout assumptions.

## Actual input and native evidence

All six capture/inspect/public-facts/native phases passed. Provenance is
`t5/vbase-capture-provenance-1.json`, SHA-256
`f1d50f1babb885dc675f73a265a8e8bda45acb74252f5a95d593a2a5f195ca7a`.
The input SHA-256 is
`4e2d8314ad97d749a5d8478e5b8b29cf7c4d84480cd213273856919dc028b191`.
Public facts are `t5/virtual-inheritance-facts-1.json`, SHA-256
`00be147cf49105eb14fc980833fa1857578833c71fcbb8d8284b25ff82ea7bab`,
with 1,745 entities, 28 bodies and 51 catalog functions. The probe confirms
unchanged input. Capture flags remain
`--g++ --c++17 --gnu_version 130300 --exceptions`, with RTTI enabled and the
same pinned EDG revision. Numbers below denote public probe locators, not an
execution schedule or internal storage identifiers.

The separate native binary SHA-256 is
`ee1860b5c693cddae83b776a8324463c43e938ec2bdbf219d8a8c7275d884efe`.
Its observations at `t5/native-virtual-inheritance-run-1/stdout.txt` have SHA-256
`e147a720e929e2fa2dbe4e1c9115d91a83e50c67cf47564a13d363e7bdde45ff`.
Both runs return 122 with values `15,36,40,31,5,7,11,0,0`, live/overflow 0 and
events `105,207,1000,807,311,1000,911,411,1000,911,507,1000,807,605`.
This establishes the native four-phase observations; it is not generated
execution evidence.

## Actual complete and occupied layouts

| Record | Complete size/alignment | Actual fields and occupied partner |
| --- | --- | --- |
| Root 1201 | 16/8 | Vptr at 0, id at 8; occupied partner 1452 is 12/8 |
| Branch 1210 | 32/8 | Primary vptr at 0, branch id at 8, virtual Root field 1498 at 16; occupied partner 1435 is 12/8 |
| Final 1220 | 40/8 | Branch field 1436 at 0, bonus field 1434 at 16, virtual Root field 1453 at 24; occupied partner 1557 is 24/8 |

Branch's direct virtual Root subobject 1208 has offset 16. Final's direct
nonvirtual Branch subobject 1217 has offset 0, and its indirect virtual Root
subobject 1218 has offset 24. Root storage therefore moves between complete
Branch and complete Final. The bonus type 1287 is signed integer storage 8/8
with 64 bits. Occupied partners are bounded subobject views;
their captured alignment does not establish standalone CLR layout or a native
by-value ABI for an unpadded 12-byte view.

The raw metadata records VirtualBaseOffsetIndex -3 for subobjects 1208 and
1218, ConstructionTableIndex 2 and 4 respectively, and
ConstructionSubarrayIndex 2 for subobject 1217. Preserve those public index
values. Actual call operands and VTT constants independently establish the
pointer arithmetic below; this note does not guess an index base convention.

## Exact tables, address points and VTTs

In this table, R/B/F denote generated RTTI variables 55/65/75 and M1/M3/M5
the generated Root/Branch/Final method entries. T4/T6 are the actual generated
virtual adjustment wrappers. Table words are captured const signed-64 values
or exact retained addresses; no signed prefix is normalized away.

| Generated variable | Actual ordinal contents | Captured address points |
| --- | --- | --- |
| Root table 57, 24/8 | `0, R, M1` | +16 |
| Branch table 67, 64/8 | `16, 0, B, M3, -16, -16, B, T4` | +24, +56 |
| Final table 80, 64/8 | `24, 0, F, M5, -24, -24, F, T6` | +24, +56 |
| Branch-in-Final table 85, 32/8 | `24, 0, B, M3` | +24 |
| Root-in-Branch-in-Final table 86, 32/8 | `-24, -24, B, T4` | +24 |

Branch VTT variable 74 is a two-element, 16/8 array of const pointer entries:
`[table 67 + 24, table 67 + 56]`. Final VTT variable 84 is a four-element,
32/8 array: `[table 80 + 24, table 85 + 24, table 86 + 24, table 80 + 56]`.
The arrays' element type 1558 is a const typeref over an 8-byte pointer. The
selected vtt_parameter type 1278 is an ordinary 8-byte pointer to that element.
All address values use exact variable bases, no subobject path, no one-past
flag and no reflection flag. Ownership of the VTTs comes from the actual
Branch/Final VirtualTableTableVariable relationships and their typed contents;
construction roles are not inferred from generated spellings.

Final C2/D2 pass their selected VTT parameter plus one element to Branch C2/D2
at actual calls 799 and 557. Final C2/D2 install the primary vptr from VTT[0]
and the virtual Root vptr from VTT[3]. Branch C2/D2 install their primary vptr
from the received VTT[0] and Root vptr from VTT[1]. In both cases the actual
lowered Root-address expression reads primary-vptr[-3]. Therefore the Branch
construction/destruction phase inside Final uses table 85's +24 Root delta
and table 86's Branch override/thunk. Complete Branch instead receives VTT 74
and its +16/-16 table data. A fixed complete-Branch Root offset would lose this
captured distinction.

## Selected entry roles and retained execution

| Actual entry | Selected roles after this | Defined forwarding/cleanup context |
| --- | --- | --- |
| Root C1 28 / D1 23 | C1 ordinary int / D1 none | Complete normal bodies; Root C2 27 / D2 22 forward with retained ABI parameter spelling |
| Branch C1 42 / D1 44 | C1 ABI int,int / D1 none | Complete bodies create VTT locals from variable 74; call C2 30 / D2 25 and retain virtual Root calls |
| Branch C2 30 / D2 25 | VTT, then C2 ordinary int,int | Real subobject bodies install received phase tables and perform the Branch helper calls |
| Final C1 26 / D1 21 | C1 ABI int,int,long long / D1 none | Complete bodies create VTT locals from variable 84; call C2 29 / D2 24 and retain virtual Root calls |
| Final C2 29 / D2 24 | VTT, then C2 ordinary int,int,long long | Real bodies pass VTT +1 to Branch and perform their own received-table writes |
| Branch D9 43 / Final D9 48 | VTT | Defined delegation bodies, with actual calls to subobject/complete destructor entries |

Complete Branch bodies remain required by all-definitions output even though
the source fixture constructs only Final. Declaration-only copy entries remain
catalog rows, not required executed copy calls. Root C1/D1 and Branch/Final
C1/C2/D1/D2 bodies have noexcept effective true. Forwarding Root C2/D2 and D9
entries retain null source noexcept facts and must be validated through their
actual selected closed calls. They are not aliases to discard.

Final/Branch complete C1 receiver types 1269/1337 are const typeref wrappers
over complete-class pointer types 1221/1211. Their C2 receivers also point to
the complete Final/Branch classes. The occupied SO views describe physical
fields; these routine facts require no pointer-to-SO receiver admission.

The complete entry's VTT local initialization, virtual Root call, subobject
call and later virtual Root destruction remain distinct actual occurrences.
Conditional Root initialization/cleanup in subobject bodies uses captured
integer flag locals 117/123/97/98. The body conditions are references to those
locals, not direct null-VTT tests. Decoded prologue/condition/VTT-location
contracts must govern them. Supplemental normal cleanup recipes and the
indirect VTT locations remain metadata dependencies until their actual
selected action/marker semantics are established; no extra destructor call is
created merely by visiting a recipe.

Virtual wrappers 4 and 6 each have ThisAdjustment 0, ReturnAdjustment 0 and
VirtualCallIndex -3, with selected this plus an ordinary integer argument.
Their actual bodies load a signed delta from the incoming receiver vptr at
index -3, add that delta to this, and then use
`enk_result_of_overriding_function` nodes 240/304. T4 selects Branch method 3;
T6 selects Final method 5. The same T4 identity appears with -16 in complete
Branch data and -24 in Branch-in-Final construction data. Projection must retain
the recorded load/add; a fixed receiver delta or a second metadata adjustment
would change behavior.

## Exact provider reuse and initializer closure

The only external data declarations are variables 52/53/54: class, VMI and SI
ABI typeinfo vtables. Each is `sc_extern`, static, `initk_none`, lowering-generated
and address-taken, with no initializer, alias, weak or local-static recipes.
Types 1373/1374/1375 are incomplete unbounded arrays with no total size or
alignment. Their common element is const typeref 1568 (8/8), over signed-64
integer 1426. The provider ELF size 88 is not a completed EDG declaration size.

| External declaration | Actual stored address | Generated RTTI initializer chain | Root's actual table association |
| --- | --- | --- | --- |
| Class table 52 | Address 1713, exact base +16 | Root variable 55: `1377 -> 1571 -> 1662 -> address 1713` | Record 1569 -> variable 52 |
| VMI table 53 | Address 1737, exact base +16 | Branch variable 65: `1384 -> 1580 -> 1674 -> 1720 -> address 1737` | Record 1578 -> variable 53 |
| SI table 54 | Address 1743, exact base +16 | Final variable 75: `1392 -> 1599 -> 1688 -> 1725 -> address 1743` | Record 1597 -> variable 54 |

Each address has OnePastEnd false, null SubobjectPath, no reflection flag and
no parameter number. All non-type data parents close to the listed typed
constant initializer paths. No body operation references these external
variables or generated RTTI roots. Branch VMI metadata is 40/8 with flags 0,
base_count 1 and generated Root typeinfo plus raw offset_flags -6141. Final
SI metadata is 24/8 and points to generated Branch typeinfo. These raw metadata
bytes are preserved without executing a native RTTI algorithm.

All three descriptors already exist for the same pinned provider
`/usr/lib/x86_64-linux-gnu/libstdc++.so.6.0.33`, SHA-256
`1fd75fe70354a416d75aef22bcae68c47bd25d20e2d0568c30b1a9838cf62f11`:
class RVA 0x26dd18, VMI RVA 0x26ec98 and SI RVA 0x26ebd8, each an audited
88-byte OBJECT/WEAK default export with CXXABI_1.3 metadata. The attributable
ELF/provider audits are `t5/virtual-provider-audit-1.json` and
`t5/multiple-provider-audit-1.json`. This input requires no new symbol, provider
file, version, size or runtime resolver behavior. Its own JIT/NativeAOT loaded-owner, read-only mapping and three generated
base-plus-16 stores now pass physical checks. No provider RTTI method slot is
called by this projection.

## Bounded projection and proof

Approved work retains these exact table constants, typed VTT pointer arguments,
complete/subobject/delegation bodies, flag semantics, explicit table writes and
dynamic vbase/vcall loads. Generated scalar method wrappers and known-address
dispatch remain closed and noexcept, with no unknown/native fallback. The
physical proof inspects complete/occupied layouts, exact VTT contents,
phase-specific primary/Root address points and signed prefixes, then executes
the actual canonical entries with raw receivers appropriate to their active
tables. In particular the same Branch thunk 4 executes with complete
Branch Root at +16 and construction-in-Final Root at +24, using the two actual
active vcall deltas. Native full-event comparison retains the four distinct phase
observations and balanced lifetimes over two fixture calls.

The VirtualInheritance checker uses the actual published type/member names.
Its approved prefix-only protocol first invokes the retained Root constructor
at +24 in aligned Final-sized storage, then Branch C2 with Final VTT 84 +1
element and arguments 5/7. It observes the actual resulting construction vptrs
and calls T4 with the raw Root address. Branch D2 receives the same VTT slice;
the Root complete destructor then releases the remaining constructed unit
exactly once. No Final destructor is called for that prefix. A separate complete
Branch C1/D1 lifecycle exercises T4 with Root at +16, without manual vptr writes.
Full Final and Root-initialized phases execute the other canonical entries.
These are physical lifecycle checks, separate from the full two-run native
fixture comparison; unexercised delegation variants remain explicitly so.

This is a concrete positive-input projection effort, not an implemented
general virtual-inheritance or VTT ABI. No native application body, native
unwind, virtual destructor, RTTI query, member pointer, covariant return or
unreviewed provider access is included. The following receipt closes only this
bounded execution slice.


## Verified production and physical proof

Parent checkpoint is `699dec425`. Immutable product source is `t5/vbase-source-2`
(227 files); standalone physical checker source is `t5/vbase-checker-source-1`
(nine files). The receipt SHA-256 is
`d7bf1ae5d14f0d659a1fb3167fbbd95c56476f7f305b487eb6d06b1c218fda76`.
All 55 expected outcomes pass: 49 ordinary successes, two prior expected
termination SIGABRTs and four intended unsupported-case rejections. There are
no unexpected outcomes or budget violations. All 14 input objects, both source
freezes, provider and completion hashes verify. Saved pristine source/blob/hash
excerpts are `t5/vbase-contract-source-1/provenance.json`; reused provider audits
and actual raw AOT dependency output are bound by the receipt.

Production output `t5/vbase-cli-generated-2` emits every actual body, including
all 14 constructor/destructor variants. Native, initial JIT and physical
JIT/NativeAOT full fixture runs each agree twice on result 122, all nine values,
14 events, live/overflow zero and prior-live zero. Repeat generated source,
project and DATA manifest are byte-identical; reports agree after output-path
normalization. Eight prior lifetime/SI/MI publications retain byte-identical
application sources and pass their affected JIT/NativeAOT regressions.

Physical checks prove complete sizes 16/32/40 and alignment 8, actual Root and
Branch occupied carriers of size 12, Root offsets 16/24 and Final bonus bytes
16 through 24. Captured Final occupied size 24 is metadata; no standalone CLR
carrier or `sizeof` claim is invented. Exact generated tables, VTT elements,
construction address points and signed prefixes are inspected. All five distinct
canonical method entries actually execute: Root returns 8/event 705 as a separate
captured-body ABI probe; Branch direct/thunk return 15/events 807; Final
direct/thunk return 36/events 911. The same Branch thunk uses active vcall delta
-16 in complete Branch and -24 in the retained Final prefix, with raw incoming
Root pointers. No receiver pre-adjustment or vptr patch occurs.

Prefix teardown calls Branch D2 then Root D1, observing live 1 then 0; it never
calls a Final destructor. Full Final teardown retains phase observations 40/31.
All three pinned provider exports pass independent owner/RVA/read-only mapping,
file hash and GetExport checks, and generated RTTI stores export BASE +16 once.
The extra physical lifecycles are separate from native-fixture activation.
D9 delegation bodies emit but are not claimed dynamically exercised. Current
source and physical paths supply nonnull VTT arguments; null-VTT conditional
branches remain emitted and validated without dynamic activation.

The first product built cleanly but rejected six required complete/delegation
entries under the older link policy (ROC1204); its unpublished attempt is
preserved. A separate single-unit admission now permits actual non-alias
complete/delegation bodies with same-class subobject primary, reciprocal entry
facts and selected this/VTT roles. Delegation is destructor-only; weak/GNU
alias/ifunc/unresolved binding guards remain. No identity canonicalization or
body substitution occurs, and later exact emitter capability checks still apply.

General diamonds/repeated virtual bases, virtual/deleting destructors,
covariant returns, RTTI queries/helpers, member pointers, throwing construction
of this layout, native C++ unwinding and broader ABI/scale remain open.
The existing SQLite publication/runtime evidence is unchanged and is not
reattributed to this C++ checkpoint.
