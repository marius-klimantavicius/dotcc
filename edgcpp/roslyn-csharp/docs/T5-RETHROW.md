# One integer rethrow with shared payload ownership

Status: the bounded generated-only slice passes production CLI and native/JIT/
NativeAOT observations from immutable `t5/rethrow-source-1` (208 files). The
[compact receipt](../acceptance/lifetimes/rethrow-report.json) records all 31
expected outcomes: 27 successful phases and four intentional unsupported-case
rejections, with no budget violations. Broader T5 remains open. This does not
admit a new exception thrown from a handler or general C++ exception-stack behavior.

[integer-rethrow.cpp](../tests/Fixtures/T5/integer-rethrow.cpp) has one initial
integer throw and one operandless rethrow, with two typed catches in separate
functions. The inner handler changes its own copied parameter to 99. The outer
handler observes the original payload 7. Three tracked objects expose cleanup
before the inner catch, during rethrow and after the outer catch. Constructors,
destructors and the event helper are noexcept. There is no class payload,
allocation expression, array, native call boundary or new throw from a handler.

## Frozen input and observed execution

Artifact paths are relative to `/mnt/storage/roslynobj-csharp-20261015/t5/`.

| Evidence | SHA-256 |
| --- | --- |
| Fixture source | `a69c6fa45fbb5b58f92cfb952d962d2406f6448de2ecd0bd0749fa73455a5fcb` |
| `capture/integer-rethrow.roslynobj` | `ec81c83cf8473f1d5d0930472144b2b49667de652a8e56446a203f70786e15f5` |
| `integer-rethrow-facts-6.json` | `386bed0daf0a47dd79c178bc8d97826d4155224d86b88ddaafa2fbd08a211164` |
| `rethrow-capture-provenance-1.json` | `8e0d938504a9729f40f2530f2d60c9a9c3285b53fc017cb4996e4cd724864012` |
| `native-integer-rethrow-run-1/stdout.txt` | `99395ba23f2b41ab8fc880b8a151eecf0a67053b1b5cc4fd05f7361628282860` |

Provenance pins the producer, public probe, native compilers/harness/binary and
measured phases. The object remains unchanged after inspection. Probe locators
below use public semantic equality; they are not stored IDs or execution order.
The report records 13 bodies and 555 entities. Native observations repeat twice:

```json
{"result":7,"overflow":0,"live":0,"values":[7,99,7,0,0],"events":[101,102,507,302,699,103,303,707,301]}
```

The final value preserves prior live count rather than resetting it. Native
observations are comparison evidence; retained EDG execution and the exact
stock contracts remain authoritative. The preserved baseline CLI rejects this
input before publication with `CSHARP2502`, family `exception`, because the
earlier finite profile admits one integer throw and one typed try.

## Actual throws, handlers and regions

Initial Throw 121 belongs to inner_path 14. Its typed Operand 122 is the
right-to-left assignment of constant 7 through thrown-object address 125.
Thrown type 426 is signed integer 32, size/alignment 4/4. Initialization,
destructor and accessible bases are absent. Generic throw operands are empty.

Rethrow 102 occurs inside actual Handler 488.Body 57, at
`body/statement[1]/else[0]/statement[10]/operand[0]`. It records
`IsRethrow=true`; Operand, ThrownType, Initialization and Destructor are null,
with no accessible bases. No missing type is guessed from source text or the
absence of operands. The containing typed handler and actual initial/caught
type identities establish the admitted context.

Inner Try 56/descriptor 495 has handler 488, caught type 426 and parameter 134.
Outer Try 189/descriptor 508 has distinct handler 506 and parameter 270, with
the same caught type 426. Their explicit parameter copies are Assign 61 and
194, using caught-object addresses 65 and 198 carrying the respective handler
identities. The copies precede exception_caught 67 and 200. Separate metadata
parameter recipes 525 and 532 are `dik_none`; they do not replay those copies.
Both type-info associations name the retained declaration 526, `_ZTIi`, without
using its spelling as type matching.

All selected actions are unconditional, with no arrays, conditions or ABI
details. Object storage is direct local, offset 0, without indirection or
receiver-relative addressing. Initial/null state is 65535.

| Function | Actual region chain and action |
| --- | --- |
| inner_path 14 | Region 140/state 0: D1 15 on before variable 132; next null |
| inner_path | Region 141/state 1: no-object helper 17, `__destroy_exception_object`; next null |
| inner_path | Region 142/state 2: D1 15 on in_handler variable 133; next 141 |
| outer_path 18 | Region 275/state 0: D1 15 on outer variable 268; next null |
| outer_path | Region 276/state 1: no-object helper 17; next 275 |

Inner prologue 55 precedes Try 56, whose saved entry state is null. Construction
of before at Call 109 precedes State 115=0 and initial Throw 121. Inner catch
entry State 59=1 precedes parameter copy/mark-caught, local mutation and note
699. Call 94 constructs in_handler, then State 100=2 precedes Rethrow 102.
There is no retained inner catch_epilogue or function epilogue. Exceptional
cleanup follows region 2 to region 1 to null, rather than fabricating a normal
epilogue. Handler Scope 489/Lifetime 138 owns metadata recipe 498 for in_handler,
linked to freeing-exception recipe 499. Before has separate recipe 500 in
Scope 494/Lifetime 139.

Outer construction Call 182 and State 188=0 precede Try 189. Its saved boundary
is therefore region 0, preserving outer during propagation to its handler.
Catch entry State 192=1, copy 194 and caught 200 precede the observed value 7.
The return changes state to 0, executes catch_epilogue 222 for handler 506 and
try_epilogue 224, clears state, explicitly calls outer's D1 at 228 and emits
function_epilogue 234. Outer metadata recipe 512 is the try lifetime's parent
checkpoint; its handler lifetime owns freeing recipe 513. Metadata recipes
describe dependencies and do not add evaluated cleanup occurrences.

## Exact stock ownership contract

The pristine revision is `89e67e07c7f6d0fc38622f9778d8e79f40deaa0f`.
The already recorded read-only runtime extraction provenance is
`runtime-contract-source-1/provenance.json`, SHA-256
`ce89615062702b146cf235a6b9894d273e45d68d0dd7f519e3b783e58df8c480`.
No native C++ runtime is compiled or linked as a generated provider.

- `lower_eh.c:6084–6093` leaves the operandless throw in this nonportable
  lowered profile; portable lowering would replace it with `__rethrow`.
- `throw.c:1699–1726` finds the currently handled primary entry, preserves its
  payload address/type/destructor, pushes a rethrow entry and starts propagation.
  `push_throw_stack:1677–1685` increments the primary entry's use count for a
  rethrow rather than allocating or copying another payload.
- First-pass search at `throw.c:1300–1380` skips already active catches and
  selects the destination before cleanup. The active inner handling entry
  still exists when the outer destination is selected.
- `destroy_thrown_object:1143–1178` discards each entry once and decrements its
  primary use count once. Payload destruction waits for zero primary uses.
  `__destroy_exception_object:1935–1953` chooses the nearest eligible active
  handler entry, not an arbitrary current payload pointer. Final stack release
  at 1895–1915 frees payload storage only for a primary entry.

The bounded projection must preserve that separation: outer destination
selection cannot overwrite the still-active inner handler's release context.
Inner no-object cleanup retires its handling reference without freeing the
payload retained by rethrow. The outer explicit copy and mark-caught then use
the same stable owner, and final catch exit performs the sole payload free.

## Agreed additive runtime and checker

[ManagedCppException](../src/RoslynObj.CSharp.Runtime/ManagedCppException.cs)
keeps its existing public methods and GetDiagnostics positional DTO. The new
`CreateRethrow(object activeHandlerToken)` returns a distinct managed propagation
wrapper sharing one private primary owner: exact type token, payload pointer,
creator thread and reference count. It starts committed/unselected without
calling CommitThrow or incrementing the primary Committed counter again. No
NativeMemory allocation or payload copy occurs on this path.

Only the original caught entry can create one rethrow. Wrong token/state,
repeated restart, rethrow-of-rethrow and selection of its already active source
handler are rejected. Original handling stays caught while the new wrapper is
selected by the outer filter. Its nonfinal ReleaseCaught requires the exact
child already selected, retiring reference 2 to 1. Child MarkCaught requires
the original handling entry already retired. Final ReleaseCaught retires 1 to
0, clears the owner pointer and performs the sole native free. No ambient
handler registry, TLS, GC or finalizer supplies application cleanup.

`GetRethrowDiagnostics()` adds a separate readonly
`ManagedCppExceptionRethrowDiagnostics` snapshot with fields Rethrown,
HandlingEntriesReleased, LiveReferences, LastCreatedPrimaryOwner,
LastRethrownPrimaryOwner and LastReleasedPrimaryOwner. IDs are opaque diagnostic
labels, never ownership lookup keys. Existing Created/Committed/Released keep
their primary allocation/commit/free meanings; pending cleanup retains its
existing behavior. Process-wide snapshots are checked serially here.

The new [RethrowLifetime checker](../tests/RethrowLifetime/README.md) compares the
entire native two-run JSON and requires main counter deltas 1 created, 1
committed, 2 selected, 2 caught and 1 released; additive deltas 1 rethrow and 2
handling-entry releases; zero live owners/references, pending releases or
protocol rejections; and the same positive primary identity at creation,
rethrow and final free. Every cleanup assertion must observe prior selection.
Latest sequence fields describe the last propagation only; assertion counts
are not destructor counts or complete histories of both entries.

## Verified publication and execution

Production output is `t5/rethrow-cli-generated-1`. All 13 required bodies publish
with one exact public-identity selection of `__destroy_exception_object` under
`managed-cpp-int32-rethrow-owner-v1`; no native import or native C++ runtime is
used. Ordinary same-spelled runtime/vector calls, handler-new-throw and noexcept
boundary candidates still reject before publication. Candidate spelling alone
cannot fulfill a required semantic runtime entry.

JIT and NativeAOT each reproduce the complete two-run native JSON above. Per
invocation the checker observes 1 allocation/commit/free, 2 selected/caught
entries, 1 rethrow, 2 handling-reference retirements, zero live references/owners
and no protocol rejection. Shared primary IDs agree at creation/rethrow/free;
these are observations, not lookup keys. All three cleanup assertions observe
prior selection. Final propagation sequence is selection 5, cleanup 6, free 7
on the first invocation and 12, 13, 14 on the second.

The inner finally destroys the handler-local object before retiring the old
caught entry. The outer catch copies the original primary value 7, not the
modified local 99. No absent inner epilogue is synthesized. The new wrapper
shares the primary storage; it does not allocate another native payload.

Normal, original int EH, member-constructor failure and partial-array CLI/JIT/
NativeAOT regressions pass. Their generated application sources are byte-identical
to their prior accepted publications. Repeated rethrow source/project bytes and
program reports (after output directory normalization) agree. All completion
hashes, 10 retained inputs and the source freeze verify. Generated JIT dependencies
are Checks, Generated and Runtime only; NativeAOT has ordinary system dependencies.

Receipt SHA-256:
`f18edb73404faa0ff31e74c590bd5385d21f5ebd87b170cc485bb6d9c47b5296`.
A coordinator-only preliminary runtime snapshot omitted Directory.Build.props
and failed unsafe compilation; the corrected complete snapshot passed without
product changes. Both attempts and the earlier baseline rejection are preserved
outside the final 31-phase proof.

Further rethrow chains, new exceptions from handlers, class/pointer payloads,
multiple eligibility stacks, termination, cross-thread transfer and native
unwinding remain separate unsupported forms. Earlier SQLite receipts retain
their immutable publication/runtime attribution; this C++ extension does not
relabel them.
