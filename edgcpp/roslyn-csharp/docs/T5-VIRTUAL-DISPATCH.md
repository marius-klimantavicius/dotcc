# Single inheritance and ordinary virtual dispatch

Status: bounded production CLI, native/JIT/NativeAOT execution, physical layout,
canonical method ABI and borrowed RTTI provider checks pass. Broader T5 remains
open. The [consolidated receipt](../acceptance/lifetimes/virtual-report.json)
records 45 expected outcomes: 39 ordinary successes, two expected termination
signals from prior regressions and four intended unsupported rejections.

The first candidate is
[single-inheritance.cpp](../tests/Fixtures/T5/single-inheritance.cpp), a
header-free C++17 input with one ordinary `Base` and one `Derived : Base`.
Its SHA-256 is
`4dde233a01eced0650812c7e8a3e2d06941302fe1c96ccf5adb9002c174d5c4f`.
Both constructors and both explicitly nonvirtual destructors are `noexcept`.
`Base::evaluate(int)` is virtual and noexcept; the corresponding derived method
overrides it. A separate `dispatch(Base*, int)` helper performs the virtual call
and is invoked with each complete local object. Distinct method events and
return expressions make the base implementation and override observable.

The complete derived object uses ordinary automatic storage and normal
destruction. There is no heap allocation, deleting destructor, virtual
inheritance, multiple inheritance, RTTI query, member pointer, application
native call, exception throw or custom lifetime inference. This adds one
capture-first activation within the broader T5 inheritance/dispatch obligation;
the other forms remain open.

## Finite observation protocol

The source uses the existing `fixture()` / `lifetime_*` scalar API, nine values
and 64 event slots, so the unchanged `tests/NormalLifetime` checker can invoke
it twice and compare the complete separate native JSON observations. Each
invocation resets only event/overflow/value observations. It does not reset
live ownership and records the prior live count in the last value slot.

| Value slot | Observation |
| --- | --- |
| 0 | Complete base object's id |
| 1 | Derived object's inherited id |
| 2 | Derived object's bonus |
| 3 | Live constructor units while both objects are complete |
| 4 | Result through `dispatch` for the base object |
| 5 | Result through `dispatch` for the derived object |
| 6 | Live constructor units after both dispatch calls |
| 7 | Live constructor units after the automatic scope ends |
| 8 | Prior live constructor units before the current invocation |

Each completed Base constructor owns one live unit and its destructor releases
it; the Derived constructor/destructor own another matched unit. This is test
observation bookkeeping, not a proposed object layout or runtime ownership
model. Event families distinguish base construction (100 + id), derived
construction (200 + id), derived destruction (400 + id), base destruction
(300 + id), helper entry (500), base method (600 + id) and override (700 + id).
The fixture returns the sum of its two dispatch results. No predicted event
array is embedded as an oracle; actual native observations establish the
finite comparison after capture.

## Facts required from the genuine public model

Capture review must identify the actual selected constructor/destructor entry
variants and parameter roles, nonthrowing facts, base-subobject layout,
complete record sizes/alignment and physical dispatch storage. Any vtable or
related constant/address initializers must be followed through their public
typed values and exact selected symbol identities. Metadata catalog rows alone
do not establish an executed allocation, initialization or call.

The helper's actual typed call occurrences must show whether dispatch remains
indirect/virtual, how its receiver is formed and adjusted, and the retained
callee/argument evaluation constraints. If the producer instead selects a
different lowered representation, the consumer must inspect that representation
and diagnose unimplemented forms. It must not recover virtual slots or targets
by reading source method names, rebuilding C++ overload lookup, or mapping the
declarations to C# nominal virtual dispatch.

Normal constructor, call, state and destructor occurrences govern execution.
Lifetime and cleanup recipes remain metadata unless the actual selected
profile makes them execution dependencies. Any required thunk, RTTI data,
alternate entry or nonzero adjustment is an observed captured dependency to
admit explicitly or reject; this initial note asserts none of those facts in
advance. Existing managed integer exception/rethrow/termination contracts do
not provide a native-unwinding or virtual-dispatch implementation.

## Verified concrete projection

Public ProbeVersion 6 now records 882 entities/19 bodies in
`t5/single-inheritance-facts-7.json`. Native two-run observations are result 28,
values `[2,5,11,3,5,23,3,0,0]`, events
`[102,105,205,500,602,500,705,405,305,302]`, live/overflow zero. They are observations,
not a source-language dispatch recipe.

Base complete storage is 16/8; its explicit subobject partner is 12/8. Derived
is 16/8 with generated base field at 0 and bonus at 12, reusing tail padding.
The emitted CLR view is exactly 12 bytes with deliberately restricted field/
pointer use. Its packed CLR alignment is not claimed as EDG alignment. Complete
carriers remain 16 bytes/aligned 8; any overlapping alignment anchor uses existing
vptr bytes and is not modeled as an EDG field. No standalone, by-value or native
subobject view is admitted. Actual copy lengths remain execution facts.

Both application vtables are captured signed64[3] constants: integer 0, generated
typeinfo address, exact selected generated routine address. Actual constructor/
destructor stores use the recorded +16-byte address point. The helper already
loads a function pointer from slot 0 and calls it with explicit receiver and int
argument in retained LTR order. TargetMethod and RetainedVirtualCall are absent/
false; no virtual name lookup is reconstructed. Known-address dispatch compares
the actual loaded pointer with canonical generated wrappers. Unknown member
addresses fail closed. Separate JIT and NativeAOT physical checks execute both actual loaded UCO slots,
returning 5/23 with events 602/705, and establish complete sizes 16/16, view size
12, complete alignment 8 and bonus offset 12 with balanced lifetimes.

Two required native data declarations are incomplete arrays of const signed64;
they have no captured extent/alignment. Each appears only as an exact +16 address
in the corresponding generated typeinfo-vptr initializer. Bindings preserve both
exact exported identities through `itanium-rtti-vtable-address-only-v1`. The pinned
libstdc++ content, CXXABI_1.3 default-export metadata, ELF RVAs and 88-byte RELRO
extents are provider evidence, never EDG sizes. Runtime retains the provider
handle and verifies hash, resolved owner/RVA and full read-only mapping; both JIT
and NativeAOT independently exercise these checks. No native
RTTI query/helper/method is invoked. Generated application tables/typeinfos and
method bodies stay generated. Provider audit is `t5/virtual-provider-audit-1.json`;
capture provenance is `t5/virtual-capture-provenance-1.json`.

The final product snapshot is `t5/virtual-source-3` (217 files); the separately
frozen physical checker is `t5/virtual-checker-source-1` (nine files). All 12
retained inputs, the provider hash and published completion hashes verify.
Repeat source/project/data-import output is byte-identical. Six earlier C++
publications retain byte-identical application sources and pass their relevant
JIT/NativeAOT regressions. Generated dependencies contain only the checker,
generated project and Runtime. Initial compile-only nullable-fact failures and
the earlier unsupported-data CLI baseline remain preserved separately.

The compact receipt SHA-256 is
`8eea4235fc923bd0164898c327debd10b154f45b8286b87e45752f3dbe390a4b`.
Full artifacts remain under `/mnt/storage/roslynobj-csharp-20261015/t5`.
The `__layout_alignment` field is a CLR-only alias of existing vptr bytes, not
an EDG member. No standalone, by-value or native use of the packed subobject
view is admitted. The provider's 88-byte extent is not a captured EDG type size;
CXXABI_1.3 is audited default-export metadata, not a `dlvsym` lookup.

This proves exactly two selected methods and one captured indirect site.
Multiple/virtual inheritance, nonzero thunks, virtual destructors, RTTI queries,
member pointers, native unwinding and general data providers remain separate
capabilities. The next capture-first target is multiple inheritance with an
actual nonzero receiver adjustment; any additional provider scope requires its
own public closure and ABI audit.
