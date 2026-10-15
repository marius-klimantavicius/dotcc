# Constructor failure after member completion

Status: this bounded constructor-body failure slice passes production CLI
publication and complete native/JIT/NativeAOT observations. The
[verified receipt](../acceptance/lifetimes/constructor-report.json) records the
limits, relevant regressions and expected unsupported admissions. Broader T5
constructor/array forms remain open.

The input is [constructor-failure.cpp](../tests/Fixtures/T5/constructor-failure.cpp).
It has one scalar prefix, one member, a containing constructor that optionally
throws `int`, and one typed handler. There are no arrays, heap allocation,
inheritance, nested throws, rethrow or native unwinding. Only the member changes
the application live count. The existing five-value/two-invocation protocol
preserves the prior live count rather than resetting it.

## Frozen evidence

All artifact paths below are relative to
`/mnt/storage/roslynobj-csharp-20261015/t5/`.

| Evidence | SHA-256 |
| --- | --- |
| Fixture source | `050d72d9a250daf7837076bce958a83ad80d07c805f07d70c31edb3de428b430` |
| `capture/constructor-failure.roslynobj` | `5c55f685b8f6650002d1c0e086eba814cfd0260998e7664692707c3dd633b428` |
| `constructor-failure-facts-1.json` | `bed3ea97ef7bf571711d7e4671829dc305b74d4c996af0c334ef1c2e9c286ad7` |
| `constructor-capture-provenance-1.json` | `9519753429f0a3c9779de2f64e64d34942ad4e44993ec15535105cdb5d0b93e3` |
| `native-constructor-failure-run-1/stdout.txt` | `0dd31221419e5b6b87e8a4416eb4bf153b287d0af8c39bb9a4dac6745f003833` |
| `constructor-preparation-1/baseline-report.json` | `ed2a1121126007ec253c65ceb1571b513a52dd4bc40909dbe9b5d363fe56be3e` |

Provenance pins the producer, driver, native compilers, native harness, binary
and measured phases. The capture uses the existing native-lowered C++17
table-based profile and is unchanged after the public probe. Probe locators
below are labels assigned through public semantic equality, not stored IDs or
a binding scheme. Its 653 entities and 16 bodies are inventory observations,
not an execution schedule.

## Selected entries and storage

Whole C1, `entity/000017`, has selected parameters `this_parameter` then
`ordinary_parameter` and retained `NoexceptEffective=null`. Its receiver is
Symbol 514, pointer type 503, size/alignment 8/8, pointing to Whole type 502.
Whole C2, entity 33, has reciprocal primary entry 17 and an independent body:
Call 443 forwards its `this_parameter` and `abi_parameter` to C1's receiver and
ordinary argument. Its noexcept fact is also null. No alias substitution or
deduplication replaces this retained body.

Member C1, entity 18, takes receiver plus ordinary integer and is noexcept.
Member C2, entity 28, independently forwards to it. Whole D1, entity 15, and
Member D1, entity 16, each take a receiver and are noexcept. Their D2 bodies,
entities 34 and 29, independently forward to the corresponding D1. Defined
C2/D2 entries retain alias-entry and COMDAT flags, without weak, GNU alias,
ifunc or GNU routine relationships. Nullable alternate-entry noexcept facts do
not invent a source noexcept boundary or erase the selected forwarding call.

Captured Whole layout is size/alignment 8/4: canary Field 594 has offset 0 and
Member Field 595 has offset 4. Member type 507 is 4/4, with its integer id at
offset 0. Execution Assign 224 writes canary constant 79225. These are captured
layout/value facts, not deductions from field declaration order.

Both the throwing Whole C1 and noexcept Whole D1 have one member cleanup action:

| Function | Region / action / storage | Captured storage |
| --- | --- | --- |
| Whole C1, 17 | 279 / 612 / 632 | receiver_relative, byte offset 4 |
| Whole D1, 15 | 205 / 607 / 631 | receiver_relative, byte offset 4 |

Both actions select Member D1, entity 16. Storage Variable and BaseEntity are
null; `UsesThisParameterOffset=true` and `IsIndirect=false`. Every action flag
is false, with no condition, array, ABI detail or supplement; each next-region
link is null. Receiver-relative storage must use the current selected receiver
value plus the captured offset. It must not invent a variable/base entity or
interpret the independent indirect flag as true.

Ctor recipe 611 and dtor recipe 605 are metadata-only `dik_none` recipes with
`IsConstructorInitialization=true`, destructor 16 and their own lifetimes 278
and 204. Target, Type, constructor and expression are null and operands empty.
The destructor scope explicitly associates recipe 605 with Field 595 through
a compiler-generated field initializer. The constructor scope's initializer
list is empty. These associations do not execute a second construction or
destruction. The actual body calls and state writes supply execution.

## Retained state and call boundaries

The sole Throw 259 occurs in Whole C1; its typed operand 260 assigns constant 7
through the thrown-object address 263. Generic throw operands are empty. The
thrown and caught type is the same signed 32-bit, naturally aligned 4-byte type
495. There is one Try 62 in `constructor_path`, entity 14. The two functions
with cleanup and no retained noexcept=true remain C1 and `constructor_path`;
Whole D1 has cleanup metadata but is noexcept.

Whole C1 explicitly calls Member C1 at Call 230, then State 238 arms region 279
at state 0. Its failure branch throws; its success path clears state to null
65535 before its function epilogue. No completed-initializer association
replaces those state operations.

The caller enters Try 62 from initial/null state 65535. Call 104 constructs
Whole; only the following State 110 arms Whole's destructor in region 166.
Thus the actual constructor-failure path has no armed Whole cleanup in the
caller. The selected handler boundary is the try's saved null state, not a
new source-derived scope state. Region 166 selects Whole D1 on direct local
157; independent region 167 selects the no-object exception-release helper.
Both next links are null.

On normal return, the caller clears state before explicit Whole D1 Call 139.
Whole D1 itself arms its member region, executes its user body, clears state
and explicitly calls Member D1 at Call 192. Normal omission of its EH markers
requires its actual validated noexcept call graph and matching receiver-relative
metadata; it must retain that explicit destructor call. Exceptional constructor
cleanup follows the retained region action instead. No lexical destruction
stack or metadata-recipe replay supplies either path.

Pristine revision `89e67e07c7f6d0fc38622f9778d8e79f40deaa0f`,
`src/lower_eh.c:3208–3248`, describes handles relative to the current receiver
and modifier byte offsets. The exact runtime contract is documented in
[T5-EH-CONTRACT.md](T5-EH-CONTRACT.md); `lib_src/throw.c:772–787` distinguishes
object-table storage from explicit indirection. These stock contracts explain
the public facts without manufacturing a different capture profile.

## Observed comparison baseline and verified execution

The independent native oracle reports twice:

```json
{"result":79233,"overflow":0,"live":0,"values":[79226,0,7,0,0],"events":[101,200,79225,201,500,79227,400,79226,301,900,101,200,79225,507,301,707]}
```

The successful path includes the containing destructor's distinct event and
the member destructor. The failure path records the member destructor before
the handler, without a containing-destructor event. Canary observations and
the nonzero member offset distinguish whole-object storage from member storage.
Native observations provide comparison evidence; retained EDG execution and
the stock contracts remain the semantic authority.

The preserved baseline production CLI exits 1 with inventory complete and
translation incomplete: `CSHARP2502`, family `exception-cleanup`, rejects the
metadata recipe's newly activated constructor-initialization role. This is
expected unsupported admission, not generated execution evidence. The later bounded extension admits this exact structural form; the old rejection
is preserved as baseline evidence. Partial arrays, failed-member construction,
native unwinding and all other broader T5 rows remain separate obligations.


Immutable `t5/constructor-source-1` freezes 201 source/configuration/fixture files.
The final [receipt](../acceptance/lifetimes/constructor-report.json), SHA-256
`e720a0d6826db048f6645e999df287ae356e50b26a4e2d809fb225c4467d3ff7`,
records all 20 expected phase outcomes: 17 successful phases and three intended
exit-1 configuration/capability rejections. There are no budget violations or
tooling retries. Production output `t5/constructor-cli-generated-1` represents
all 16 required bodies and the actual exception-owner semantic selection.
All completion hashes and six pinned input hashes pass verification; repeat
source/project bytes are identical and reports differ only by output directory.

The implementation changes only the two emitter lifetime/EH files. Runtime and
checker sources are unchanged from `0628a20d2`. A member cleanup is admitted only
through the selected receiver role, complete ordinary layout, unique exact field
at the captured displacement, selected no-throw member constructor/destructor,
matching scope-bound metadata recipe and actual following state write. The
constructor-initialization recipe flag is accepted only for that validated
association. Whole C1 joins the supported throwing constructors only after its
EH plan validates; C2 remains its explicit independent forwarding body. The
normal noexcept Whole D1 retains its member destructor call exactly once.

JIT and explicitly clang-18 NativeAOT each repeat the full native observation
above. Per fixture invocation one payload is created, committed, selected,
caught and released, with zero live owners or protocol rejections; selection
precedes cleanup and release. Two cleanup assertions are recorded, including the
handler's empty walk to its null entry boundary; this is not a claim of two
exceptional destructor calls. Actual failure destroys the member before catch
and never invokes Whole D1. Normal success destroys Whole and then its member.

The preceding normal and integer-EH production CLI/JIT/AOT regressions also pass.
Both generated application sources are byte-identical to their accepted prior
publications. The ordinary runtime-name, handler-new-throw and noexcept-wrapper
negative objects still reject before output publication. Generated dependency
manifests contain only Checks, Generated and RoslynObj.CSharp.Runtime; NativeAOT
ELF dependencies are host libc/libm/loader, with no reader or native application
implementation. Sampling/waited memory measurements remain separately labeled.

No failed-member constructor, multiple completed members, bases, arrays,
conditions, vector helper/count protocol, heap construction, rethrow, termination
or native unwinding is enabled by this proof. Catch normal release is exercised;
new-throw no-object release and pending initialization failure remain inactive.


The later [automatic partial-array proof](T5-PARTIAL-ARRAY.md) closes its own
bounded success/zero/one/two-prefix cases with generated-only vector helpers.
This does not extend the member-layout proof to failed-member construction,
heap arrays, inheritance or native unwinding.
