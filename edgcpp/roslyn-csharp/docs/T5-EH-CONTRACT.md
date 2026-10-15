This records interpretation and the agreed bounded adapter contract for the genuine
[exception-cleanup.cpp](../tests/Fixtures/T5/exception-cleanup.cpp) capture.
Normal lifetime milestone `f5c0f255a` is accepted. The bounded generated-only
integer EH adapter now passes production CLI publication and complete native /
JIT / NativeAOT observations; the [verified receipt](../acceptance/lifetimes/exception-report.json)
records the execution boundary and remaining unsupported forms.

The public report is
`/mnt/storage/roslynobj-csharp-20261015/t5/exception-cleanup-facts-3.json`,
SHA-256 `c73e44686e7c681da434f2b4ddc23e602d9a1ed392fa0044c43e888df55bafa3`.
Its input SHA-256 is
`5836f0588d1f168b367996c52a0c84cb681ae0cd953a2050acf9b6a49e08eee5`.
The locators below are probe-assigned public-equality labels, not stored IDs
or a new binding scheme. The object is immutable, format 2.0, native_lowered,
itanium / little-endian / pointer 64 bits with alignment 8. Captured capability
facts explicitly set DO_IL_LOWERING=1, DO_FULL_PORTABLE_EH_LOWERING=0,
GENERATE_EH_TABLES=1 and IA64_ABI=1. The profile follows these facts, not the
presence of a runtime-helper name.

## Exact retained throw and handler

| Fact | Actual public relationships |
| --- | --- |
| Throw | raise_selected body/statement[5]/operand[0], entity/000072, non-rethrow, no destructor or accessible-base list, no remaining Initialization |
| Thrown/caught type | The same entity/000438: signed_integer, 32 bits, size/alignment 4/4; handler entity/000512 uses that identical type |
| Throw operand | Typed ThrowOperation.Operand is entity/000073, although generic ExpressionOperation.Operands is empty on the throw itself |
| Lowered initialization | entity/000073 is right-to-left eok_assign: `*(int*)leck_thrown_object_address = value`; entity/000076 is the address placeholder and entity/000077 is the retained value reference |
| Handler initialization | The handler explicitly assigns its parameter from `*(int*)leck_caught_object_address`; entity/000140 carries handler entity/000512 |
| Catch transition | The explicit parameter copy is followed by leck_exception_caught, entity/000142 |
| Separate recipe | Handler.ParameterInitialization entity/000539 is metadata dik_none, with no expression, constructor, destructor or operands; it must not replay the explicit parameter copy |
| Type-info association | Handler.Typeinfo entity/000540 is the retained external _ZTIi declaration, not a required native import. Its spelling does not establish type matching; the actual thrown/caught semantic type does |

TryOperation entity/000131 has typed Body entity/000180 and ordered Handlers
containing entity/000512. Its structural list happens to show else[0] before
body[0], demonstrating why a structural inventory traversal is not execution.
The body and handler must be emitted from their typed relationships.

## Exact cleanup states and boundaries

Both functions with cleanup have actual initial/null state 65535, a region
table and no array table. All four selected cleanup actions are unconditional:
there are no conditional, array, indirect, base, VLA or deallocation flags.
Three object actions name direct variables at byte offset 0. The fourth is
the runtime exception-object action, with no_object storage and no argument.

| Function | Region | Action and next region |
| --- | --- | --- |
| raise_selected, entity/000014 | 0, entity/000084 | D1 on nested; next null |
| exceptional_path, entity/000017 | 0, entity/000255 | D1 on outer; next null |
| exceptional_path | 1, entity/000256 | D1 on inner; next 0 |
| exceptional_path | 2, entity/000257 | __destroy_exception_object; next 0 |

raise_selected retains prologue, constructor, state 0, note call, throw and
an unreachable state-65535 marker. The nested destructor is supplied by its
cleanup action when the throw leaves the function, not by a normal destructor
statement after throw.

exceptional_path constructs outer, then explicitly enters state 0 before the
typed try. Its try body constructs inner and enters state 1 before calling
raise_selected. The catch body enters state 2, copies the parameter, marks
exception_caught and runs the handler. Its return path explicitly changes to
state 0, emits catch_epilogue and try_epilogue, changes to null state 65535,
calls outer's D1, emits function_epilogue and returns the saved scalar result.
The normal try return path explicitly changes to state 0, destroys inner,
emits try_epilogue, changes to null, destroys outer and returns. Unreachable
state markers also remain in the graph; they do not become executed fallthrough.

## Current stock contract basis

The pristine current revision is
`89e67e07c7f6d0fc38622f9778d8e79f40deaa0f` at
`/tmp/roslynobj-edg-upstream-20261012`. Its sparse working tree omits lib_src;
the actual revision's runtime files were read through git show and copied
without modification to
`/mnt/storage/roslynobj-csharp-20261015/t5/runtime-contract-source-1`.
The commands, byte counts and SHA-256 values are in provenance.json, SHA-256
`ce89615062702b146cf235a6b9894d273e45d68d0dd7f519e3b783e58df8c480`.
Nothing was compiled, linked or installed as a generated provider.

Concrete basis for this interpretation:

- src/lower_eh.c:6064–6271 keeps the nonportable throw, lowers its initialization
  against leck_thrown_object_address and clears dynamic_init. The operand is
  already the copy into runtime-owned storage; a second source-value copy
  would duplicate execution.
- src/lower_eh.c:5168–5267 supplies the caught-object address, adds the exception
  release cleanup for a new throw escaping a catch, lowers parameter copying
  and then marks exception_caught. src/lower_eh.c:5309–5340 emits an ordinary
  helper call in portable mode but catch_epilogue in this nonportable mode.
- src/lower_eh.c:6319–6370 and src/il_def.h:13773–13803 / 15531–15561 define the
  retained state, address, caught and epilogue operations. The state number is
  the point at which cleanup starts; no new state is inferred from source scopes.
- lib_src/throw.c:665–715 iterates region actions from current region until the
  stop region, exclusive, following the captured next-region chain. It handles
  __destroy_exception_object specially before object-storage processing and
  calls that helper with no arguments.
- lib_src/throw.c:1504–1524 cleans the destination function only up to the try's
  saved entry region, preserving surrounding objects. The portable lowering
  also explicitly saves the current region at try entry in
  src/lower_eh.c:5444–5458; this supports the boundary contract without changing
  the actual captured profile into the portable implementation.
- lib_src/throw.c:1300–1392 searches for an eligible handler before the cleanup
  pass. An already active catch is not its own new-throw destination. This
  candidate's exact integer match does not establish general type matching,
  noexcept violations or termination behavior.
- lib_src/throw.c:1221–1235 marks the exception caught after parameter copying.
  lib_src/throw.c:1143–1195 and 1875–1954 describe use/discard/destruction and
  storage release. __destroy_exception_object selects the nearest eligible
  in-handler throw entry; it is not a general free of an arbitrary current
  pointer. Nested/rethrown cases require more ownership state than this slice.

The exact runtime throw.c SHA-256 is
`27ed40c95c4d0f06df87d4e5969cec74c8adab774f108e2746c4cbfe64d83225`.
The C generator's lowered-EH printer is a debug printer and supplies no
implementation semantics here. The native oracle is an observation source;
the retained EDG execution and stock contracts remain authoritative.

## Agreed bounded generated-only adapter

The normal slice precedes this exceptional implementation. Admit
only the actual table-based profile, one exact integer thrown/caught type,
the observed unconditional direct-variable destructor actions and the specific
no-object release action. Reject unproved conditions/arrays/indirect storage,
class/pointer/base matching, function-try/rethrow/termination and native-boundary
unwinding. Keep all selected call roles and explicit normal destructor calls.

1. Give each admitted function its actual cleanup state, initialized from its
   captured null/initial state. Emit state writes at the actual retained
   occurrences. A generated cleanup dispatch follows its decoded actions and
   next links; it must not derive a destruction stack from C++ lexical scopes.
2. Represent this single-unit integer exception by a generated managed carrier
   and an explicitly owned, naturally aligned 4-byte payload. Bind a type token
   to the actual captured type identity; compare that token at the admitted
   handler, never _ZTIi/type display strings or only bit widths. A managed
   carrier does not imply CLR exceptions are interchangeable with C++ throws.
3. At ThrowOperation, establish runtime-owned destination storage, then evaluate
   the retained typed operand exactly once with its recorded right-to-left
   assignment. leck_thrown_object_address supplies that destination. Commit the
   initialized payload to the throw only after the operand completes; release
   a pending owner if transfer never occurs. Resolve the admitted destination
   before starting cleanup, preserving the runtime's handler-search/cleanup
   separation. Do not re-evaluate value or replay the discarded initialization
   recipe. Allocation/constructor failure variants remain outside this first
   admission.
4. Crossing raise_selected unwinds its current region 0 to null and destroys
   nested. The typed try in exceptional_path retains its entry boundary, state
   0 in this capture. Before entering its selected handler, unwind state 1 to
   that boundary, destroying inner while preserving outer. Only generated
   exception carriers take this path; arbitrary CLR errors are not caught as int.
5. Preserve the same payload owner through handler selection and the explicit
   catch-parameter copy. leck_caught_object_address is handler-specific and
   returns the selected owner's stable storage. Mark caught only at the
   retained exception_caught occurrence. The actual body then performs its
   normal state/epilogue/destructor sequence.
6. At the retained catch_epilogue, release the current handler's eligible owner
   once. Select the required __destroy_exception_object dependency as a precise
   generated semantic-runtime contract, validating its captured void()/no-role
   signature, declaration and special cleanup use. Do not bind it to libc or
   treat a same-spelled arbitrary import as that contract. The exceptional
   no-object action shares the same release ownership and must not double-free
   an owner already released by normal catch exit. Its new-throw-from-handler
   activation remains unexercised by this source.

This admitted source has no nested throw or class destructor for the payload;
its owner can use that explicitly validated restriction. A future broad helper
cannot replace the runtime's eligible-entry/use-count rules with free(current).
No GC/finalizer or blanket using performs application destruction. Automatic
objects continue to use their actual normal calls or exceptional region actions.

The verified proof compares the complete native event/value arrays in JIT and
NativeAOT twice, with no live objects or event overflow and balanced internal
payload ownership. No native application or native C++ exception implementation
satisfies generated calls. This closes only this finite integer throw/catch slice;
the broader [T5 acceptance rows](ACCEPTANCE.md) remain open.

## Runtime and checker integration

The agreed public surface in
[ManagedCppException.cs](../src/RoslynObj.CSharp.Runtime/ManagedCppException.cs)
is reader-independent and has these explicit states: pending, committed,
selected, caught, released. Ownership is entirely per carrier, with no ambient
current-owner registry or TLS lookup. Multiple-owner eligibility, nested/rethrow
patterns and native unwinding are not implemented by pretending this is a
general exception stack; the emitter's bounded admission rejects those forms.

| API | Bounded responsibility |
| --- | --- |
| CreateInt32(object exactTypeToken) → ManagedCppException | Allocate a naturally aligned 4-byte pending payload; the generated token represents actual captured type identity |
| ObjectAddress → void* | Pending-only destination for the retained throw-initialization expression |
| CommitThrow() → void | One pending-to-committed transition after the retained operand completes, followed by throwing the carrier |
| ReleaseUncommitted() → void | Preparation-finally release if transfer did not occur; never free an already committed owner |
| SelectHandler(object exactTypeToken, object handlerToken) → bool | CLR first-pass identity filter, choosing the one destination without cleanup or release |
| ObserveUnwind(ManagedCppException, ref ManagedCppException?) → bool | Store the actual carrier and return false, allowing handler search to continue before function finally cleanup |
| RequireSelectedHandler() → void | Reject exceptional cleanup before destination selection and record the first-cleanup ordering assertion |
| CaughtObjectAddress(object handlerToken) → void* | Stable payload address for the exact selected handler, including the explicit copy before MarkCaught |
| MarkCaught(object handlerToken) → void | Actual retained point after parameter copying |
| ReleaseCaught(object handlerToken) → void | Release the eligible selected owner once, with identity/state checks and no native pointer argument |

False observation filters do not catch/clean/rethrow. A true typed-try filter
selects the matching handler before CLR unwinding executes the observed
function's inline region cleanup. The handler prelude stops at its saved entry
state. Every encoded unwind site asserts selected ownership first. The runtime
does not map arbitrary CLR exceptions to C++ int exceptions.

The no-argument EDG helper remains a semantic action: its emitter projection
supplies validated carrier/handler context to ReleaseCaught. This consumer
context is not an invented native call signature or a libc binding. Normal
catch_epilogue uses the same eligible owner; a second release is rejected.
Application automatic objects still use explicit normal calls or decoded EH
actions, not GC/finalizers. Only the internal payload uses NativeMemory, with
ownership cleared before Free.

GetDiagnostics returns process-wide Interlocked counters and the most recently
released owner's selection/first-cleanup/release sequence. They observe resource
balance and selection-before-cleanup; they never select the owner. The
[ExceptionLifetime checker](../tests/ExceptionLifetime/README.md) preserves the
native two-run JSON and separately requires one created/committed/selected/
caught/released owner per fixture call, zero live owners/protocol rejections,
and ordered selection, first cleanup and release. The fixture does not activate
pending initialization failure, new throw from catch, rethrow or class-payload
failure; those remain unverified variants rather than inferred successful tests.


## Verified execution checkpoint

Immutable source snapshot `t5/eh-source-3` freezes 200 implementation, reader,
configuration and fixture files. Production output is
`/mnt/storage/roslynobj-csharp-20261015/t5/eh-cli-generated-3`:
13 required bodies and one record, with the exact required runtime helper
fulfilled only by the validated `managed-cpp-int32-exception-owner-v1` selection.
The CLI reconciles that selection against the actual public function identity;
an ordinary same-spelled declaration call is rejected before publication.
Generated dependencies are only Checks, Generated and RoslynObj.CSharp.Runtime;
NativeAOT ELF dependencies are libc, libm and the host loader. No native C++
application/provider or reader dependency supplies execution.

The [compact receipt](../acceptance/lifetimes/exception-report.json), SHA-256
`df4f593ffd5d9cdd7c4b19783f3b08a3f5df8b62239fb4516ebd6993b24e0960`,
records 17 accepted phase outcomes: 14 successful phases and three expected
exit-1 admission rejections. Both JIT and explicitly clang-18 NativeAOT produce
result 11, values `[3,0,8,0,0]` and all 14 ordered lifetime events on each of two
invocations. Each invocation creates, commits, selects, catches and releases one
payload, with zero live owners, pending releases or protocol rejections. Two
cleanup assertions occur after handler selection and before release. These
counters observe ownership; they do not choose the owner or schedule cleanup.

A genuine handler calling the sole throwing helper and a genuine noexcept
wrapper with no cleanup regions calling that helper both reject through the
closed generated no-throw capability guard. Their public traces retain one
throw/one try, so rejection does not depend on a second source throw. Normal
production CLI/JIT/AOT regression also passes, and its application source is
byte-identical to the accepted normal milestone. Repeat EH source/project bytes
are identical; program reports differ only by output directory. Every completion
file hash and input/source freeze is verified.

The initial direct emission, warning-bearing JIT snapshot and one coordinator
inspect-option typo remain preserved. The typo was retried with the correct
`--report` option against unchanged product source; it is an additional tooling
failure, excluded from the 17 accepted outcomes. Generated source 3 has zero
compiler warnings/errors. These small-fixture runs use frozen per-phase budgets;
RSS sampling and waited high-water measurements remain separately labeled.

Handler normal catch release is exercised. The generated no-object exceptional
release action is inspected but not activated: a new throw from the handler is
unsupported, as are pending initialization failure, rethrow, termination, class
payloads, partial construction/arrays and native unwinding. Existing const-record
reference limits remain. This checkpoint does not close broader T5 or T6.
