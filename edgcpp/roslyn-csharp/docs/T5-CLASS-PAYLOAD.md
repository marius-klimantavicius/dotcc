# Bounded exact class exception payload

Status: bounded production CLI, native/JIT/NativeAOT ownership and physical
checks now pass. [The compact receipt](../acceptance/lifetimes/class-payload-report.json)
records 60 expected outcomes. Broader T5 remains open; this slice does not
establish full DuckDB exception, string, inheritance or link closure.

[class-payload.cpp](../tests/Fixtures/T5/class-payload.cpp) defines one
nonpolymorphic `Payload` with a `long long canary`, an `int value`, and explicit
noexcept ordinary/copy constructors and destructor. The canary is
`-0x6123456789ABCDE0LL`: the positive signed literal is representable before
negation, avoiding an out-of-range unsigned-to-signed conversion. Its nonzero
upper bits and negative value expose truncation. The actual capture now records
Payload entity 529 as complete size/alignment 16/8, with signed-64-bit canary
field 604 at offset 0 and signed-32-bit value field 596 at offset 8. Its
nonvirtual occupied size is 12; that is not the complete allocation size.

`class_path` owns source value 7. `raise_selected(const Payload& source)` owns
guard value 3 and contains the only initial throw, `throw source`. The sole
typed handler catches `const Payload&`, so this first candidate requests
reference binding without a separate by-value catch object. There are no bases,
virtual methods, strings, native calls, rethrows, nested throws or source
allocation. Actual retained copy/cleanup operations remain authoritative.

The existing scalar native harness can use `VALUE_COUNT=9`, call `fixture`
twice, and collect all events, values, live count and overflow. The 64-event
buffer records constructor/copy/destructor and handler/after-handler events;
destruction also records a canary comparison. Live ownership is never reset.
The nine observations are:

| Index | Observation |
| --- | --- |
| 0 | Source value before the try |
| 1 | Caught value |
| 2 | Caught canary equals the full signed constant |
| 3 | Caught and source objects occupy distinct addresses, as a boolean |
| 4 | Live count inside the handler |
| 5 | Live count after the handler |
| 6 | Both source fields remain intact after the handler |
| 7 | Live count after the source-owning function returns |
| 8 | Prior live count at fixture entry |

The initial design encoded no predicted event sequence, copy count, result or
observation array as an oracle. The actual independent native observations are
recorded below. Raw addresses are not compared between engines.

## Frozen capture and native evidence

Artifacts are relative to `/mnt/storage/roslynobj-csharp-20261015/t5/`.

| Evidence | SHA-256 |
| --- | --- |
| Fixture source | `457a582a8261d7530bac5eca302487394192aae062dde1677bddb0efe746d950` |
| `capture/class-payload.roslynobj` | `220666266ef44c9469fe5abeba89c0776d680f5fa0055e84a0ae74e7f0b19f4a` |
| `capture/class-payload-inspect.json` | `c4f57645a3244a16cfe6781369e056119eb9cab75adc584cb3c16e62471f39e5` |
| `class-payload-facts-1.json` | `d6f6bc0468797d568a9a4e1ce6c4d9a72a5534cd25b7b1feb32d24e5abe4f743` |
| `class-payload-capture-provenance-1.json` | `4e05dd25dae145748bcc3e6127fa91ad2bf867e2e86d8fc57a25fbb04523516e` |
| `native-class-payload-run-1/stdout.txt` | `d6fe15d75db8e3a9bf24e67aad6f0cf5de6f068b1bb1693968cf3737d7c6b8cf` |

The provenance pins all six measured phases, frozen producer, probe, native
harness/compiler/binary and unchanged input/source. Capture flags remain
`--g++ --c++17 --gnu_version 130300 --exceptions`, with RTTI enabled. The
public report uses ProbeVersion 6 and records 736 entity identities, 15 bodies
and 399 structural relationship occurrences. Those counts are inventory, not
a schedule. Locators below are public semantic-equality labels in that report.

The separate native executable reports this complete observation twice:

```json
{"result":14,"overflow":0,"live":0,"values":[7,7,1,1,2,1,1,0,0],"events":[107,103,507,207,303,403,707,307,407,807,307,407]}
```

The observed fields and full canary agree, the caught object is distinct from
the source, live count is 2 in the handler and 1 after it, and final/prior live
counts are zero. Native events include destruction/canary checks after the
handler observation and before the after-handler observation. These are
comparison evidence; retained EDG operations and the selected runtime contract
govern projection. There is no generated execution claim yet.

## Actual reference binding and handler state

The sole typed Try operation 228 has descriptor 634, body 291 and handler 620
with body 229. It is not a function-try block. Handler 620 has caught type 526
and parameter `caught`, variable 51, of that identical type. Type 526 is an
8/8 ordinary lvalue-reference pointer, not an object value: its pointee is
const typeref 597, whose underlying type is Payload 529. Throw 105 records
ThrownType 597. Thus the caught reference type itself does not equal the
thrown type; its actual reference/const edges establish the relationship.
Handler typeinfo variable 33 is the same variable associated with Payload 529.
No name, display string, layout equality or RTTI query supplies this identity.

The retained handler block contains these actual ordered statement positions:

1. State marker 231 selects region 367, state 1.
2. Assignment 233, `eok_assign` with `EvalRightToLeft=true`, writes Reference
   234 targeting `caught` 51 from cast 235. The cast has ordinary pointer type
   595, an 8/8 nonreference pointer to the same const type 597, and its operand
   is caught-address marker 236. That marker names handler 620 explicitly.
3. Marker 238 is `leck_exception_caught` after the reference assignment.
4. The explicit scalar observation and `note` statements follow.
5. State marker 288 restores region 366, state 0, before catch-epilogue marker
   290, which explicitly names handler 620.

ParameterInitialization 687 is a metadata-only `dik_none` association targeting
variable 51/type 526. Constructor, destructor, expression, operands, constant,
object lifetime and initialization lifetime are absent; all recorded recipe
flags are false. It does not request a class copy or independently owned handler
object. The body assignment is the actual reference binding; replaying the
metadata would invent extra initialization.

## Actual regions and release ownership

`raise_selected` 14 has initial/null state 65535, prologue 86, one region 117
and no epilogue. Region 117 has ordinal 0, no next region or condition, and
action 601 selecting complete destructor 15. Storage 683 names direct variable
`guard` 43, offset 0, with no base, indirection or this-relative addressing.
Metadata recipe 600 names that same variable/type 529/destructor 15 and lifetime
116; it has no executable expression, operands, master or destruction link.

`class_path` 18 has initial/null state 65535, prologue 209, epilogue 359 and
two unconditional nonsupplement regions:

| Region | Next | Selected action and storage |
| --- | --- | --- |
| 366, ordinal/state 0 | null | Action 647 selects destructor 15; storage 690 is direct source variable 50, offset 0 |
| 367, ordinal/state 1 | 366 | Action 648 selects declaration-only `__destroy_exception_object` 19; storage 691 is `no_object`, no variable/base, offset 0 |

All action array/indirect/conditional/base/VLA/deallocation/local-guard flags are
false, with no array or ABI detail. Table variables 44 and 52 have reciprocal
cleanup projections to their actual functions and one empty local static
descriptor each; neither has an executable or constant initializer.

State marker 218 arms source region 366 before Try 228. Try lifetime 364 names
source recipe 645 as its ParentCheckpoint; recipe 645 targets source 50/type
529/destructor 15 in lifetime 363. These are explicit state and association
facts. The current state at typed-try entry supplies the stop boundary; lexical
scope is not an alternative cleanup algorithm. Handler scope 621 has lifetime
365 and metadata recipe 646, with `IsFreeingOfExceptionObject=true` and null
target/type/destructor/expression/operands. It corresponds to the eligible
no-object action, not a second payload destructor call.

After normal handler exit restores state 0, catch epilogue 290 is the actual
handler-specific release point. Later, state marker 351 selects null state
65535 before explicit Call 353 destroys source 50 through destructor 15.
Function epilogue 359 follows. Guard/source destruction and runtime payload
destruction are distinct ownership roles, even though all select the same
class destructor identity. The captured Throw 105 supplies Destructor 15;
the no-argument release helper does not take an application object pointer.
Its semantic adapter must retire the exact eligible owner and call that
owner's selected destructor before freeing its payload.

No additional public facet is currently missing for this reference binding and
region/recipe audit. The selected throw-copy call, constructor/destructor roles,
layout and import closure are reviewed separately before consumer changes.

## Starting limitation and pinned ownership contract

Before this slice, `ManagedCppException` allocated only four naturally aligned
bytes and final reference retirement freed storage without invoking a payload
destructor. The starting emitter validated a signed-32-bit assignment throw
and an identical by-value integer handler parameter, rejecting a retained throw
destructor, payload constructor and noninteger type. Those are consumer limits,
not an alleged capture gap. The existing `CreateInt32` path remains unchanged;
the additive record draft below is not execution acceptance.

Pristine revision `89e67e07c7f6d0fc38622f9778d8e79f40deaa0f` supplies the
contract to inspect against the new capture:

- `src/lower_eh.c:6050–6059,6250–6268` lowers initialization against
  `leck_thrown_object_address`; the nonportable lowering retains the resulting
  expression and clears the original dynamic initializer.
- `src/lower_eh.c:6107–6110` assigns destruction responsibility to the runtime,
  rather than the initialization recipe. `6180–6189` selects a complete
  destructor entry for the portable Itanium setup when that branch applies;
  the actual nonportable capture must supply its own selected identity.
- `src/lower_eh.c:2674–2707,2783–2786` obtains effective typeinfo through the
  recorded reference and qualifier structure. Exact public type relationships,
  rather than names or sizes, must connect this handler to the thrown type.
- `src/lower_eh.c:5168–5189,5226–5264` associates the caught address with its
  exact handler and initializes the parameter before `exception_caught`.
  `5304–5336` places the eligible release at the actual catch epilogue.
- `lib_src/throw.c:1261–1268` marks the copy complete when throw processing
  starts. `1143–1198` retires a handling reference and invokes the recorded
  destructor once only when the primary use count reaches zero and the copy is
  complete. `1875–1914` frees primary storage after destruction;
  `1925–1954` locates the eligible active handling entry.

The implementation review uses actual `ThrowOperation` operand, initialization,
destructor, accessible bases and type identity; selected constructor/destructor
roles and noexcept facts; the full record layout; reference-parameter binding;
caught-address and caught-marker associations; region/state chains, metadata
recipes and the no-object release action. Metadata is not a second copy or
destruction schedule. Public identity inventory is not execution order.

A bounded owner must preserve first-pass selection before cleanup,
stable handler-specific payload storage, completed construction before transfer,
the selected generated destructor before final free, and storage-only cleanup
if initialization never completes. This fixture activates successful noexcept
copying and destruction only. Copy failure, by-value catch copies, class rethrow,
polymorphic matching and native unwinding remain separate work.

## Bounded runtime and checker

The additive runtime API is `CreateRecord(object exactTypeToken, nuint size,
nuint alignment, Action<nint> destroy)` and instance
`MarkRecordEvaluationComplete()`. Runtime admission is exact complete 16/8
storage on a little-endian 64-bit host. Allocation checks actual natural
8-byte address alignment. The generated token still denotes captured semantic
identity; the destructor callback is an ordinary managed selected D1 method
adapter, never an unmanaged callback or a lookup by class name.

The record evaluation marker is pending-only, record-only and once-only. It
maps actual marker 107 before copy Call 108. The pending payload address is
available after this marker; successful copy must return before `CommitThrow`
marks the copy complete. The existing handler filter, caught-address binding,
caught marker and eligible release methods are reused. Record rethrow and
noexcept termination destinations are explicitly rejected in this slice.

Final completed handling retirement invokes the supplied destructor exactly
once with stable valid bytes, then clears the owner pointer and frees storage.
A callback that unexpectedly throws reports a runtime protocol failure and
does not report successful destructor completion; it is not an implementation
of C++ throwing-destructor semantics. Generated admission must prove the
selected destructor closed and noexcept. Pending retirement does not invoke
the destructor. Failed pending initialization remains unactivated evidence.

Existing `GetDiagnostics` and `GetRethrowDiagnostics` DTOs are preserved.
Additive `GetRecordDiagnostics` reports creation, evaluation completion, commit,
destructor start/completion, free and live-owner counters; actual size/alignment
and alignment check; an opaque primary diagnostic identity; and shared-global
sequence snapshots for creation, evaluation, commit, selection, first cleanup,
destructor start/end and free. These diagnostics do not resolve ownership.

The new [ClassPayloadLifetime checker](../tests/ClassPayloadLifetime/README.md)
compares the full native nine-value/event observations twice, checks one
balanced record allocation/destructor/free per invocation, zero owner/reference
leaks and unchanged pending/protocol-rejection counters, and requires:

```text
creation < evaluation107 < commit < selection < first cleanup
         < destructor start < destructor end < free
```

Every observed encoded cleanup assertion has prior selection. The affected
integer/rethrow/noexcept and other lifetime/inheritance regressions now pass with
byte-identical application source; the final evidence is recorded below.

The first immutable production CLI publication and native-equal generic JIT
checks have passed at `t5/class-payload-cli-generated-1`. The focused checker now
uses its actual emitted `Payload__3` name to measure complete size 16, canary
offset 0, value offset 8 and natural alignment 8 with a sequential byte-prefix
probe. It does not claim a standalone carrier for occupied size 12.
An independent third JSON line checks the sole observed class RTTI descriptor
against the existing pinned libstdc++ provider contract and verifies actual
`Program.storage->__class_type_info.@base.__vptr` equals that export BASE plus
the captured 16-byte displacement once. ELF extent 88 is provider metadata,
not the captured class size; `CXXABI_1.3` is pinned default-export metadata,
not a runtime versioned lookup. No RTTI slot is executed. These physical
assertions and the owner checks pass in both JIT and NativeAOT.


## Verified milestone

Parent checkpoint is `504bf7641`. Immutable product snapshot
`t5/class-payload-source-1` contains 232 files; standalone checker snapshot
`t5/class-payload-checker-source-1` contains nine. Receipt SHA-256:
`12e75111547bb45415c5da20001257b80a5c82a3b2d399de6c54ca1a6d8404bb`.
All 60 expected outcomes pass: 54 ordinary successes, two prior expected
termination SIGABRTs and four intended unsupported-case rejections. There are
no unexpected outcomes or resource violations. All 15 input objects, both source
freezes, provider and completion hashes verify. The first product attempt passes;
no failed class implementation attempt is relabeled as evidence.

Production `t5/class-payload-cli-generated-1` publishes all 15 required bodies.
The helper uses actual-identity contract `managed-cpp-exact-record-owner-v1`;
the only native dependency is the existing class RTTI DATA descriptor. Catalog
new/delete declarations remain unrequired. Native, first JIT and the focused
JIT/NativeAOT checker agree twice on result 14, values [7,7,1,1,2,1,1,0,0]
and all 12 events. Each run has one evaluation marker, commit, selected/caught
entry, destructor start/completion and payload free; pending release, protocol
rejection and live owner/reference counts are zero. Shared ordering sequences
are 1..8 then 9..16; primary diagnostic identities are distinct per invocation.
Opaque IDs are observations, not keys for choosing or releasing ownership.

The nine prior normal/int-EH/member-failure/array/rethrow/termination/SI/MI/VTT
publications retain byte-identical application sources and pass affected
JIT/NativeAOT behavior. Ordinary runtime/vector names and unadmitted
handler-new-throw/noexcept paths reject without publication. Repeat class source,
project and DATA manifest are byte-identical; program reports agree after
output-directory normalization. Generated JIT dependencies contain only checker,
generated code and Runtime. Saved raw AOT dependency output excludes native
SQLite; no native class/application implementation services generated calls.

Attributable exact-revision raw blobs, Git-blob/SHA hashes and numbered excerpts
are in `t5/class-payload-contract-source-1`; the separate actual-marker context
and source-evaluation-before-copy excerpts are in
`t5/class-payload-marker-contract-source-1`. Existing libstdc++ provider evidence
is reused with the new actual stored-vptr check. No provider descriptor/resolver
or native unwind scope expands.

Only successful noexcept class copy and normal catch-epilogue payload release
are activated. Pending-copy failure is designed storage-only but unexercised;
throwing copy/destructor, class rethrow/termination, base matching, by-value
class catches, exception_ptr and native unwinding remain unsupported/unexercised.
The generated no-object exceptional release action is retained but not activated
because this handler cannot throw. C2/D2 forwarding definitions emit separately;
not every alternate entry is claimed dynamically executed. Complete 16/8 is the
allocation/storage contract; occupied 12/8 is not a standalone CLR carrier.
The original SQLite source/runtime evidence remains separately attributed.
