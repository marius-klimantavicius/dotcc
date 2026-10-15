This is interpretation and a bounded adapter proposal for the genuine
[exception-cleanup.cpp](../tests/Fixtures/T5/exception-cleanup.cpp) capture.
It changes no emitter/runtime behavior. Normal lifetime execution remains the
first gate; exceptional generated execution is not accepted by this document.

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

## Bounded generated-only adapter proposal

This proposal is for subsequent implementation after the normal slice. Admit
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

The next proof should compare the existing native event/value arrays in JIT and
NativeAOT, twice, with no live objects or event overflow. Also observe balanced
internal exception-payload ownership after each fixture invocation. No native
application or native C++ exception implementation may satisfy generated calls.
These observations close only this finite integer throw/catch slice; the broader
[T5 acceptance rows](ACCEPTANCE.md) remain separate.
