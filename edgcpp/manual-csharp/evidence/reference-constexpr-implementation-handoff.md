# Scalar reference constant storage implementation handoff

Authored implementation by `/root/manual_edg_supervisor/implementation_m3`.
This is a source handoff for supervisor-owned execution and independent review;
no implementation-agent builds, tests, native runs or Git operations occurred.

## Representation and execution

`ConstantStorage.cs` separates runtime integer values from locations. Every
known scalar object has an evaluation-local cell identity and immutable retained
observation. Reference bindings transport locations. Only an explicit typed
lvalue-to-rvalue conversion loads storage. Reference parameters preserve the
caller location; value parameters allocate fresh callee cells after all caller
arguments have finished evaluation. Recursive activations therefore never
replace an outer activation's parameter symbols or object values.

Explicit work, call, block cleanup and full-expression work delimit storage.
Return values/locations are installed before cleanup. Caller argument
materializations survive the enclosing full expression; callee return-created,
local and by-value parameter objects expire before subsequent caller loads.
Discarding a glvalue call executes it while bare discarded object designators
do not load. Resource failures and early returns clean up automatic storage;
reusing an interpreter clears every mutable owner collection and budget.

Unknown reference-parameter addresses can travel through supported reference
calls without an integer read. Their retained facts have Unknown lifetime,
null identity/liveness/initialization and no object Symbol. UnknownReference
names the parameter responsible for that symbolic address. Unknown loads and
persistent required reference results fail. Known initializing scalar object
addresses remain transportable; loading an in-progress initializer fails.

Optional declaration-time observations are historical. Required reads of
potentially constant scalar objects replay the original typed initializer in
fresh storage, including already-selected function identities. Failed optional
attempts do not bar a later visible function definition, and successful cached
folds never replace real storage reads. Cycles use explicit in-progress cells.
Mandatory constexpr initializers still diagnose failure at their source point.

Scalar logical &&/|| contextually convert both semantically analyzed operands
to bool. Interpreter work schedules the second operand only when needed.

## EDG correspondence

All routines were read in the pinned upstream source before authoring code.
This is a handwritten bounded algorithm, not a mechanical source translation.

- interpret.c 70–241: constexpr storage, temporary/static allocation/live sets.
- interpret.c 27322ff and 33874ff: address_of/reference_to/ref_indirect and
  explicit glvalue loads versus transported reference results.
- interpret.c 22952–23090: process_call_work caller argument evaluation before
  callee parameter mapping and allocation lifetime ownership.
- interpret.c 9356ff: install return expression before unwinding storage.
- interpret.c 32234ff/32262ff: existing constant variable import and symbolic
  unknown reference objects; 6587ff address/runtime marker distinction.
- exprutil.c 17519ff/17681ff and il.c 17431ff/17344ff: potentially constant
  variables, initializer_constant and original dynamic initializer replay.
- expr.c 35086ff and interpret.c 31659ff: logical operand checking/conversion
  and short-circuit evaluation work.

Pinned SHA-256 source hashes:

- interpret.c: d3f34d737a6690e482942a52a463e406b767a0d5fc06b3609c8cb9c79b27cb4d
- exprutil.c: 1b01609700d2a72f64f94e218a3f210e7e9503394bc3cf16168e8a1a50e50812
- il.c: 535768a37a2bd0f90dd18c41bd867398074289a305c3ff88589da67153514cd0
- expr.c: 056f8e7416d6f948846abbe17c3401e9eb4ca8ed1ce0ae7673cd573b314df3c2

## Authored execution gate

Seven new ConstantReference groups exercise locations/load separation,
reference/value argument transport, recursion using identical declaration
symbols, temporary/local lifetime failures, unknown and initializing address
transport versus reads, original selected-entity initializer replay, short
circuiting with semantic RHS checks, cleanup/reuse/collection and limits.
Native-contract inputs are copied exactly into owned fixtures. The combined
fixture uses expanded RET provenance, distinct char16_t/long template instances,
frozen earlier reference overloads, later void overload/caller hiding, local
aliases and required evaluation. Its separate native-only witness checks result
and reference types over those unchanged source bytes. Existing tests change
only where actual storage interpretation replaces the earlier Unsupported bound
or an earlier optional-import policy disproved by pinned native results.

## Bounds

This does not implement general pointer arithmetic, assignment, branch/loop
statements, classes, dependent/template reference declarations, or constexpr
void. Such source forms retain explicit Unsupported diagnostics. Scalar object
storage and known/symbolic addresses are internal semantic locations, not a
claim of those remaining language constructs. No session-global storage/cache
retains translation units; immutable results deliberately retain their graphs.

The wider campaign and remaining M1–M6 inventory remain active after this gate.
