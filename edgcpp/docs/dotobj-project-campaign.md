# Owned semantic object implementation campaign

The immediate user-requested checkpoint (2026-10-09) is complete DuckDB C#
generation through independent SQLite `.dotobj` files from original translation
units, including bundled dependencies. Generated DuckDB code need not compile or
execute for this checkpoint. Prioritize missing core semantic/type, initialization,
lifetime/EH, linker and emitter features; defer libc++ implementation and runtime
binding gaps. Existing headers remain useful evidence for general language
features, not a requirement to finish the standard-library runtime first.

The managed linker and C# emitter must work without original source files or EDG.
Never satisfy generation by silently dropping required startup, destruction,
handlers or bodies. Record generation, compilation and execution separately.
SQLite already has execution acceptance; broader cross-platform DuckDB execution
and NativeAOT remain later objectives. A small fixture supports a feature claim,
not complete-project acceptance.

## Coordination and ownership

The supervisor coordinates two implementation workers: native semantic capture
and managed linking/emission. Root owns the real-project manifests, restartable
translation driver and project acceptance probes. The supervisor owns schema and
packaging changes, contract integration, serialized native preparation/builds,
campaign evidence and milestone commits. File ownership is explicit; only one
compiler build and one Git mutation may run at a time. Retain intermediates.

The baseline is commit `d4d4a115e`, profile `scalar-control-v11`, with 527 checks
including NativeAOT. This baseline is deliberately bounded: it does not translate
either target project. Existing legacy direct-backend project outputs do not
satisfy the object-pipeline acceptance target.

## Implementation sequence

1. Establish restartable original-source project manifests and bounded-memory
   probes. Preserve all frontend commands, input hashes, logs and object results.
   SQLite uses the existing thread-safe Unix/musl source configuration. DuckDB
   includes its bundled dependencies and disables native extension loading.
2. Implement the complete C path driven by SQLite failures: global/static/TLS
   storage, constant data and symbolic relocations, literals, function types and
   indirect calls, record layouts, unions/bitfields/enums, expression sequencing,
   source control flow, varargs and selected runtime externs. Static initializers
   retain typed roots and field metadata so emission can use named aggregates and
   bulk constant spans rather than byte-by-byte assignments.
3. Link independently produced SQLite and runtime objects; bind the existing
   shared portable runtime. Verify SQL, allocation, threading and filesystem
   behavior against native SQLite, then run representative project tests and
   NativeAOT. Keep source names and structured control flow reviewable.
4. Implement the C++ path driven by independent DuckDB translation units:
   template/inline identity and COMDAT selection, references and object ABI,
   constructors/destructors, precise lifetime activation, exceptions/noexcept,
   virtual dispatch, RTTI/member pointers, static guards and library/runtime
   requirements. Consult stock EDG C lowering and the working direct backend;
   do not alter EDG frontend semantics to fit the emitter.
5. First link all required DuckDB objects and bundled dependencies using bounded
   body loading and emit complete reviewable C#. Account for every required unit
   and report any remaining semantic rejection explicitly. Compilation/execution
   and missing libc++/runtime bindings are not gates for this generation checkpoint.
6. Subsequently bind remaining runtime/library requirements, run SQL and DuckDB
   test selections against the native configuration, then NativeAOT. Keep this
   later execution milestone separate from complete source generation.

## Non-negotiable representation and output properties

- Objects contain owned typed semantic identities, expressions, statements,
  layouts, lifetime information, source snapshots/locations and captured macros.
  They do not contain opaque C# bodies or depend on the rejected old exporter.
- The executable graph may materialize external declarations only when used;
  all externally defined library APIs remain roots by default. Independently,
  the final owned source index must retain every parsed declaration and type
  with stable identities, locations and semantic type information. Raw header
  snapshots do not replace that index. Unsupported descriptions must carry an
  explicit incomplete-coverage record until implemented, even when their unused
  execution forms do not prevent translating the program.
- Renamable objects use identities throughout. Linking resolves cross-unit names
  and compatible definitions without retaining EDG addresses or frontend state.
- Internal preprocessing happens once per original source translation unit.
  Do not restore the memory-heavy whole-program merge or external `.ii` pipeline.
- C# preserves source names, named members and recognizable structured flow.
  Synthetic code is restricted to incompatible source semantics. Output remains
  splittable and namespace/class configurable; instance globals remain opt-in.
- Globals and TLS may use pinned arrays; heap and short-lived native storage use
  native allocation. Shared runtime sources support NativeAOT and cross-platform
  execution through BCL services or explicit runtime OS selection.
- Missing semantics fail explicitly. Emitting required extern declarations is
  distinct from providing implementations and passing execution tests.

## Evidence and completion

Each verified substantial milestone records commands, hashes, test results,
retained paths and remaining project blockers and is committed separately.
Acceptance requires independently packaged project/runtime objects, link without
source/EDG, compiled generated C#, native-result correspondence and NativeAOT
execution. Cross-platform design is not reported as cross-platform execution
unless that platform was actually tested. The broader C++ and libc language plan
remains explicit even where project acceptance does not exercise every feature.

## Verified C/runtime milestone (2026-10-08)

Native snapshot010 and managed snapshot006 pass 597 regression checks including
NativeAOT. Shared runtime binding independently passes 33 checks and cross-unit
storage passes 12, with native references, thread-local state, forced GC and
source-free relinking. Managed007 additionally fixes unsigned bitfield promotion
against signed operands (native/JIT/NativeAOT pass); managed008 accepts valid
narrow integer pointer displacement, verified against actual musl casemap.

The tranche includes globals/TLS, relocations, function pointers, unions and
bitfields, complete POD record/array values, varargs, volatile operations,
intrinsics, macro stack history, supported goto entries and noexcept boundaries.
Static initialization retains readable names, aggregate rows and constant spans.
The BCL runtime binder checks signatures/layouts and reuses authoritative shared
runtime sources, including provider-owned standard-stream cells.

Project acceptance remains incomplete. SQLite reaches a function-designator
shape in fillInUnixFile; its next native fix is retained separately. Of 224 musl
provider units, 168 package; remaining categories include binary80, synthesized
local provenance, builtin formal conversions and constructor policy. DuckDB's
first unit reaches C++ inherited/vptr record representation. Full independent
source semantic indexing, C++ object/lifetime/EH semantics and project execution
remain required. All artifacts are retained beneath
`/mnt/storage/dotcc-edg-campaign-20261003`; the hashed milestone receipt links
exact tools and results.


Native012 and managed009 subsequently pass 40 focused native/JIT/NativeAOT
checks covering nested automatic aggregates, callback return/designator shapes,
source switch labels nested in lexical blocks/loops, bitfield arithmetic and
scope-entering C gotos. Ordinary source switches remain structured. SQLite now
passes the large VDBE switch and reaches a folded aggregate constant in
sqlite3ExprListAppend. [Hashed evidence](../evidence/dotobj-c-control-20261008.json).


## Verified provider profile v13 (2026-10-08)

Native015 and managed014 pass **730 checks including NativeAOT**. The profile
adds exact binary80 arithmetic/storage with explicit aligned addressable locals
and parameters, observable non-atomic volatile payload accesses, synthesized-local
provenance, original routine signatures (including bounded unspecified C forms),
folded record constants and source expression lifting with lazy branches.

The complete original SQLite translation unit now packages independently. Its
450,137-node object and 223 musl provider objects link through explicit provider
selection: all primary library APIs remain roots, 44 provider units contribute,
and 3,339 function definitions compile as C#. Shared BCL providers implement all
69 required external functions. The sole remaining environment storage belongs
to the GNU constructor-bearing environment_startup unit, which still rejects
until owned startup actions and explicit library initialization are implemented.
This is generation/compile evidence, not SQLite engine execution acceptance.

The packager now bulk-loads unpublished staging databases and performs complete
foreign-key validation before publication. Ten tests include byte-identical
repackaging of the full SQLite wire with fixed provenance and malformed-reference
rollback; the candidate packaging phase takes 6.02 seconds against the original
265.929-second attempt. Complete native014 SQLite translation/package takes
9.375 seconds. All earlier slow attempts remain retained.

[Hashed v13 evidence](../evidence/dotobj-provider-v13-20261008.json) records tools,
full regression and project probes. Five original DuckDB units parse in roughly
one second each, then stop explicitly on hidden-vptr record layout or C++ dynamic
global initialization. [The next C++ layout contract](dotobj-cpp-layout.md) and
native/stock-C lifetime oracles make those remaining gates concrete.

## SQLite engine execution through v14 objects (2026-10-08)

All **225 original translation units** now package independently: SQLite and all
224 musl provider candidates. Native016 captures GNU startup actions, including
the environment snapshot constructor. The managed linker selects 45 provider
units, emits 3,340 function definitions and binds 70 real shared runtime imports
with **zero unresolved requirements**. No fake runtime bodies or translated-source
patches were used for execution.

The complete prepared THREADSAFE=1 Unix/APPDEF SQLite engine passes the same eight
integration suites under **JIT and NativeAOT on Linux x64**, with identical output.
They cover concurrent cold initialization and restart, mutex identity/recursion/
contention, FULLMUTEX connection sharing and managed callback reentry, Unicode
real-file persistence, JSONB, FTS5, translated musl math, read-only errors,
independent NOMUTEX rollback/WAL connections, blob/backup/serialization and
deserialization, timezone/environment startup and forced GC.

Source startup runs through explicit `ObjectProgram.InitializeLibrary()` for
library hosts and automatically through executable `Run`. The implementation
permits same-thread reentry, waits for concurrent hosts, caches failures and runs
constructors outside CLR type-initialization/state locks. Independent JIT/AOT
tests cover constructor-created worker threads calling translated exports,
failure propagation and preserved duplicate registrations after body coalescing.

[Hashed engine evidence](../evidence/dotobj-sqlite-engine-20261008.json) records
all inputs, tools, provider selection, runtime bindings and execution output.
This establishes SQLite project integration, not exhaustive upstream conformance.
Instance-owned storage, other platform execution, complete source indexing,
general C++ layout/lifetime/EH and DuckDB execution remain required work.

The v14 full regression passes **771 checks including NativeAOT** with immutable
managed015. The expanded 70-check startup suite additionally verifies managed016
primary/provider coalescing and strong-definition root retention; 14 host startup
safety checks cover concurrency and failure. [Startup evidence](../evidence/dotobj-startup-20261008.json)
keeps these tool snapshots and coverage scopes explicit.

## C++ record origins and assignment order, v15 (2026-10-08)

Native020 and managed018 pass 144 focused record checks including NativeAOT.
Objects separate original record/member declarations from physical fields,
retain unnamed zero-width bitfields and mutable members, and identify hidden
vptr/ABI storage through read-only lowering observations. Originally nontrivial
records cannot acquire ordinary POD copies merely because their physical fields
are simple. Incomplete declarations do not fabricate completed-class lowering
proof. Existing C/C++ shared-header compatibility and readable member accesses
remain verified.

C++17 assignment sequencing independently passes 20 native O0/O2, JIT and
NativeAOT checks. Owned right-to-left evaluation order drives local expression
lifting for destination side effects, compound updates and lazy/loop contexts.
Constant static storage follows the stock C backend's initializer classification,
including constant payloads under a dynamic marker; actual dynamic actions and
destruction obligations continue to require explicit representation.

Actual DuckDB/provider probes advance beyond std::type_info: miniz, xxhash,
zstd_common and libc++ new_handler now package independently. Remaining observed
frontiers include 128-bit charconv tables, C++ reference/hidden-this ABI, packed
mbedtls records, base/subobject layouts and true dynamic initialization. These
are translation probes, not DuckDB execution acceptance.

The complete v15 regression passes **843 checks including NativeAOT** with
native020 and managed018. [Hashed v15 evidence](../evidence/dotobj-record-origin-20261008.json)
records the full run, focused record/assignment checks and exact tool snapshots.


## Native-width integer128 and thread lifecycle, v16 (2026-10-08)

Native022 and managed019 support signed and unsigned 128-bit source values,
including enum representations, arithmetic, exact constants, records, arrays,
function calls and shared variadic packing. Source-exposed storage retains
16-byte alignment through explicit local storage and pinned global/TLS arenas.
Numeric tables use embedded ulong-pair spans rather than allocated UInt128
collections. Pointer conversions preserve the observed LP64 target policy.
Unsupported wide switch, volatile, bitfield, intrinsic and address-relocation
forms remain explicit boundaries. The folded-address producer correction emits
a typed address followed by conversion; it does not change EDG semantics.

Focused native O0/O2, JIT and NativeAOT probes cover full-width extrema, float80
conversions, exact arrays, pointer round trips and alignment. Independent corrupt
objects test canonical range and alignment validation. Actual DuckDB/libc++
probes now pass the charconv integer128 tables and reach the next C++ layout and
initialization boundaries; this is not DuckDB execution acceptance.

The shared C11/libc++ thread runtime passes 94 native/JIT/NativeAOT checks and
86 process compatibility checks. It uses canonical BCL services for mutexes,
conditions, once, threads and TSS, including initial-thread exits, descendants,
last-thread process finalization and four-pass TSS cleanup. Managed019 supplies
the source-owned TLS release bridge, verified after C++ TLS, TSS and libc TLS
callbacks; the next source TLS access restores its initial values while globals
persist. [Thread evidence](../evidence/dotobj-runtime-threads-20261008.json)
records the exact tools and tests. Linux x64 is the executed platform.

The full v16 regression passes **871 checks including NativeAOT** with native022
and managed019. [Hashed integer128 evidence](../evidence/dotobj-integer128-20261008.json)
records that run, focused integer/enum and malformed-object checks, current
DuckDB probes and the exact immutable tools.

## Original references and instance receiver ABI, v17 (2026-10-08)

The object now distinguishes original lvalue/rvalue references from their
pointer execution representations. Instance routines retain original receiver
record identity, CV/ref qualifiers and written parameter ordinals; the hidden
receiver has an explicit synthetic binding with no invented source declaration.
Static members remain ordinary functions. Default member initializer presence
is preserved independently of physical layout and trivial-copyability facts.

Managed source signature matching is recursive: equal pointer ABIs cannot hide
reference/pointer mismatches, including callback signatures. Legal top-level
parameter CV differences, function decay, array references and reference
collapsing remain compatible. The focused suite passes 45 checks including
NativeAOT. A real libc++ typeinfo reduction now packages and verifies the
`std::type_info::name() const noexcept` call; external RTTI storage means this
reduction is not yet an execution claim.

Normal-exit destructor calls already materialized by EDG with source exceptions
disabled can now execute through the receiver ABI. The regression distinguishes
that case from active unwinding obligations: a throwing call within the same
RAII scope with exceptions enabled still rejects explicitly. Generated deleting,
complete and subobject ctor/dtor entries also remain explicit origin boundaries;
copying a late lowered signature is not original source provenance.

All 225 original SQLite/musl units repackage with native023 and link through
managed020 to 3340 functions and 70 real runtime imports, with zero unresolved
bindings. The same eight engine suites pass under JIT and NativeAOT.
[SQLite v17 evidence](../evidence/dotobj-sqlite-engine-v17-20261008.json) records the
fresh run. DuckDB probes move past the reference and default member initializer
gates to aggregate copy ABI, inheritance and generated ctor/dtor origins.

The full v17 regression passes **909 checks including NativeAOT** with native025
and managed020. [Hashed reference evidence](../evidence/dotobj-reference-20261008.json)
includes the focused source-signature checks, fresh SQLite execution, real
libc++/DuckDB probes and immutable tool snapshots. The initial v17 run is retained
with its stale negative expectation; the stock C backend confirmed that EDG had
already materialized normal-exit destruction when source exceptions are disabled.


## Generated entries and nonvirtual subobjects, v18 (2026-10-08)

The owned graph distinguishes original constructors/destructors from observed
complete, subobject and deleting entries. Generated parameters and bodies retain
explicit origins without inventing source declarations. Original direct base
relationships connect complete nominal classes to exact physical subobject
views; optimized empty bases have logical relationships and no zero-size CLR
record. Complete-object copies retain original trivial-copyability constraints.
Partial base copies preserve reused derived tail bytes instead of copying the
complete base sizeof. Retained block-copy nodes use their exact source extent.

Focused native O0/O2, JIT and NativeAOT tests execute tail reuse, empty and
multiple bases, complete derived copies, constructor entries and actual block
copies. COMDAT data from two independent objects retains shared addresses and
mutations, and relinking after removing source inputs produces identical C#.
Unknown external-array address displacements defer extent checks to the selected
definition or to an explicit minimum-extent runtime provider contract.

The full regression passes **925 checks including NativeAOT** with native029 and
managed021. Native030 additionally makes implicit fixed data-pointer-to-void
argument conversions explicit in the graph, and managed022 improves mismatch
diagnostics. A real cross-TU nonvirtual fixture then verifies and links without
its source files; it truthfully awaits the shared vector-construction runtime.
[Hashed subobject evidence](../evidence/dotobj-subobjects-20261008.json) records
these distinct tool snapshots and acceptance scopes.

A full virtual/delete fixture now verifies and emits exact 5/12-byte views and
real deleting bodies. Its two external RTTI vtable providers remain unresolved,
so this is generation evidence, not execution acceptance. Default-argument
constructor adapters are a distinct next origin contract: their default
expressions, selected target and exception policy must remain owned. Virtual
bases, hidden-result/copy ABIs, unwinding and full DuckDB execution remain open.


## Defaults, nominal identity and program arguments, v19 (2026-10-08)

Observed constructor default adapters now retain their private identity, original
parameter defaults, exact selected target and independent exception policy.
Their real bodies evaluate defaults once per constructed element. The unchanged
independent-TU nonvirtual fixture now runs under JIT and NativeAOT with the shared
vector runtime. Genuine EDG/LLVM typeinfo source providers additionally enable
virtual dispatch and deleting destructors without fabricated vtable storage.

Template specializations preserve finalized frontend names. Cross-unit tests
prove shared external-inline local types and COMDAT state remain shared, while
anonymous-namespace and private-function local types remain distinct even when
header locations and layouts match. The exporter only reads finalized names;
it does not remangle lowered IL or change frontend semantics. Writable UTF-8
argc/argv storage now remains alive through source shutdown and atexit callbacks.

The complete regression passes **1001 checks including NativeAOT** with native036
and managed024. Focused defaults, nominal identity and argument suites pass 35,
57 and 30 checks respectively. [Hashed v19 evidence](../evidence/dotobj-default-adapters-20261008.json)
records the exact tools, source-free relinking and project frontier. All failed
intermediate reductions remain retained.

The actual DuckDB frontier has advanced to packed records, hidden result and
indirect-value ABI, dynamic initialization and lifetime/exception obligations.
No full DuckDB execution is claimed. The next implementation contract owns source
routine ABI mappings rather than inferring original semantics from lowered
pointer signatures.


## Original and physical routine ABI, v20 (2026-10-08)

Every retained original routine type now owns its lowering map, including
callback-only signatures. Original class returns and by-value parameters remain
class values in the semantic source index; hidden result, receiver and indirect
execution slots have explicit identities and order. Generated runtime imports
and callback prototypes use exact creation proofs and an explicit generated
declaration origin rather than pretending to be original user functions.

Native O0/O2, JIT and NativeAOT tests execute result buffers, same-class
receiver/result pointers, nontrivial copies, callback fields, nested prvalues
and default-argument array adapters. Two independent objects relink and run after
their source directory is hidden. Compatible C/C++ callback declarations link
despite different observation provenance. Malformed source/ABI proof variants
are rejected. Generated aliases retain their own exception policy and real body;
only their physical parameter/return contract must match the original entry.

The complete regression passes **1044 checks including NativeAOT** with native038
and managed026. A fresh end-to-end SQLite run packages all 225 original units,
resolves 70 real runtime imports and passes the same eight engine suites under
JIT and NativeAOT. [Hashed v20 evidence](../evidence/dotobj-indirect-values-20261008.json)
records focused, cross-unit, project and full-regression receipts. The first
regression's obsolete corruption fixture is retained: it lacked the newly
required ABI metadata and was rejected earlier than the intended mismatch.

Actual DuckDB allocator and libc++ thread units now reach destruction obligations
instead of the indirect-value ABI boundary. The latest 15-unit frontier still
packages four units; packed libc++ string layouts, dynamic initialization,
assembly barriers and lifetime/exception operations remain explicit blockers.
Indirect passing alone does not discharge those obligations.


## Observed packed records and readable accesses, v21 (2026-10-09)

Original record/field packing directives and accepted field alignment are now
captured at EDG's actual layout observation points. The exporter does not rerun
layout or normalize frontend decisions. C++ unnamed classes with a real typedef
linkage name retain their public nominal identity; anonymous-namespace and local
scope ownership remains distinct across independent objects.

Named packed fields and record copies retain ordinary C# member expressions and
correct explicit offsets/packing. Absence of an IL `unaligned.` prefix was not
proof of unsafe named access; the earlier blanket-helper proposal was corrected.
Only raw typed accesses that lose field packing context use explicit unaligned
operations. Tests cover offsets 1 through 15, nested arrays, pointer/callback and
double fields, copies, static initialization and exact-once mutations. A separate
naming regression preserves actual defined/extern function names when synthetic
storage getters would collide.

Native040/managed030 passes **1093 full regression checks including NativeAOT**,
61 focused packed checks and 12 property-name checks. The fresh 225-unit SQLite
pipeline passes all eight JIT/NativeAOT engine suites; final managed030 emits all 237
translator C# files byte-identically to the executed managed028 output. Packed code also
passes Windows .NET JIT under Wine, without claiming Windows NativeAOT or ARM64
execution. [Hashed v21 evidence](../evidence/dotobj-packed-layout-20261009.json)
records tools, failed intermediate tests, source-hidden relinking and provenance.

The original DuckDB SHA-256 unit now packages, advancing the 15-unit frontier
from four to five packaged units. Full dependency execution still needs the
original memory-assembly barrier provider. Zero-bound arrays in libc++ string,
dynamic initialization, destruction/EH and other project gates remain open.


## Owned memory assembly and project generation, v22 (2026-10-09)

Empty GNU memory assembly now retains original operand direction, names, source
locations, leaf types and dimensions in dedicated semantic tables. Address and
byte-extent expressions remain owned graph nodes. Observers capture array shape
before stock lowering erases it and retain the existing evaluated VLA-total
binding afterwards. Emission evaluates addresses and extents once, then uses a
BCL memory barrier without fabricated operand reads or writes. Unsupported
assembly remains rejected; ignored `cc` clobbers follow EDG's explicit diagnostic.

Pointer alignment analysis now distinguishes opaque unknown origins from actual
packed storage and preserves typed-pointer information through byte/void erasure
and conditional joins. Native041/managed033 passes **1095 full regression checks
including NativeAOT**, plus 63 focused independent-TU memory-assembly checks.
[Hashed v22 core evidence](../evidence/dotobj-memory-assembly-core-20261009.json)
records immutable tools, source-hidden linking and malformed-object rejection.

Original DuckDB MbedTLS and yyjson units now package, verify, link and emit C#.
MbedTLS additionally passes native/JIT/NativeAOT acceptance on managed031; final033
emission is recorded separately. yyjson reaches a runtime-binding gap after C#
generation. The user's next checkpoint is complete DuckDB C# generation, without
requiring compilation/execution yet; work now prioritizes missing dotobj type,
initialization and lifetime/EH features instead of filling libc++ runtime gaps.

The next general type feature distinguishes fixed zero-bound arrays from truly
unspecified/flexible arrays. Existing exact stock-C and native oracles preserve
zero byte extent and zero pointer stride without dropping side effects. No
complete DuckDB generation claim is made at this milestone.


## Fixed zero-bound array identity and execution, v23 (2026-10-09)

Fixed `[0]` arrays now remain distinct from unspecified `[]` arrays in both the
source index and execution graph. Original field extents, offsets and alignment
are retained without phantom CLR elements. Zero-stride pointer arithmetic and
mutations preserve operand evaluation and side effects, including nested arrays
and supported volatile/unaligned places. True flexible fields retain their
separate layout restrictions. Unsupported standalone empty storage and undefined
zero-stride pointer difference remain explicit gates.

Native042/managed034 passes **1139 full regression checks including NativeAOT**.
The focused acceptance passes 54 checks, including independent native O0/O2,
stock C lowering, source-hidden linking and malformed-object rejection; the full
run additionally checks final generated source shape. [Hashed v23 evidence](../evidence/dotobj-zero-bound-arrays-20261009.json)
records the immutable tools and receipts.

The retained 15-unit DuckDB frontier packages six units. Actual `exception.cpp`
and `string_util.cpp` now reach destruction obligations after clearing zero-array
layout, while `date.cpp` and `hugeint.cpp` still require owned dynamic startup.
This is a selected frontier, not complete generation of the 2635-unit manifest.

The new [dynamic-startup oracle](../evidence/dotobj-dynamic-startup-20261009.json)
is explicitly pending implementation: scalar and constructor-array global startup
pass native/stock-C references but are rejected by the owned exporter. Local
static generation succeeds with unresolved externs, without proving concurrent
guard publication. It establishes the next core-feature acceptance baseline;
libc++ implementation and runtime provider work remain deferred.


## Owned C++ file-scope startup, v24 (2026-10-09)

Global scalar initialization, constructor-initialized constant arrays and
constructor temporaries now own their generated startup routines and original
per-storage statement intervals. Observed local-declaration scaffolding is kept
separate from original initializer work. Their union must explain every direct
body statement exactly once. Original source ordinals remain distinct from
priority-partitioned action order; native GNU init_priority tests verify reversed
source order. Source removal does not affect managed linking or scheduling.

The stock GNU frontend may leave consumed `dik_none` entries on the file-scope
list. Exact creation/insertion observations now distinguish those from unowned
initialization; no frontend semantics or generated-name guesses were introduced.
Zero and constant storage initialization precedes all selected dynamic actions.
C++ startup exception boundaries use the existing FailFast termination policy;
custom std::set_terminate dispatch remains runtime work.

Native043/managed036 passes **1151 full regression checks including NativeAOT**.
Independent scalar/array acceptance passes 34 checks, priority acceptance 17,
and temporary scaffolding 19 (including executed stock C). Resealed malformed
objects produce 41 semantic rejections across 164 metadata checks.
[Hashed v24 evidence](../evidence/dotobj-file-scope-initialization-20261009.json)
records final tools and focused/full receipts. Local static generation separately
passes 12 checks with unresolved guard imports, without claiming concurrency or
execution. The zero-initialization scaffold branch remains explicitly unexercised;
a partial-aggregate attempt exposed a compiler-injected memset origin gap.

Both original DuckDB `date.cpp` and `hugeint.cpp` now clear startup and reach
unowned destruction in `FromCString` and `ToString`. Complete unit generation is
still blocked. The next core slice owns typed cleanup states, destructor actions
and constructor rollback, followed by source exception constructs; libc++ runtime
implementation remains deferred under the generation-first checkpoint.

## Owned scalar C++ cleanup, v25 (2026-10-09)

Source lifetime trees, ordered destruction obligations and exact normal destructor
calls now survive independently of EDG. Exceptional cleanup actions and their
state transitions refer to typed target expressions and observed callbacks.
Original destructor calls stay in normal control flow; CLR unwinding follows the
verified cleanup chain for locals, temporaries, ordinary bases and members.
Generated methods use named locals and a cleanup cursor rather than native stack
frames. Optimized empty bases have an address-only view of the observed base
subobject, with no fabricated storage field. Expression lifetime wrappers retain
source identity through observed IL copies.

The complete-object and subobject destructor distinction remains explicit. A
delegating-constructor reduction exposed an observer timing error: capturing the
destructor signature before EDG finalized its out-of-line declaration. The
adapter now records identity first and observes the finalized signature at the
existing routine hook; frontend semantics are unchanged. No-exceptions normal
destruction remains supported and is checked against original obligations.

Native048/managed042 passes **1154 full regression checks including NativeAOT**.
Twenty focused native O0/O2 versus JIT/NativeAOT scenarios pass, including nested
locals, constructor rollback, data-bearing and empty bases, member rollback,
temporary return boundaries and delegating construction. Metadata acceptance
passes 133 checks with 32 resealed malformed objects rejected; the separate
normal-lifetime compatibility run passes 81 checks.
[Hashed v25 evidence](../evidence/dotobj-scalar-cleanup-20261009.json) links all
retained receipts and tools.

Five original DuckDB units advance to the next explicit gates: conditional scalar
cleanup in `exception.cpp`, virtual-base/VTT layout in `string_util.cpp`, source
throw in `allocator.cpp` and `date.cpp`, and integer128 intrinsic operands in
`hugeint.cpp`. None of these complete units generates yet. Conditional/array/VTT
cleanup, source C++ throw/catch matching and a separate comma-place/prvalue member
access remain open; this is not general C++ exception support. The generation-first
checkpoint and deferral of libc++ runtime work remain unchanged.

## Conditional scalar cleanup, v26 (2026-10-09)

Conditional temporary destruction now owns its exact synthetic integer flag.
The native adapter validates EDG's supplementary region row without treating it
as an action, preserving primary state indices and successor links. Original
flag initialization and body set/reset stores remain intact. Unwinding advances
the cursor, tests the flag and evaluates the target only for an active action.
Stock EDG does not reset this flag while unwinding; the generated code follows it.

Native049/managed044 passes **1154 full regression checks including NativeAOT**.
The expanded 43-case fixture agrees with native C++ and stock EDG-generated C at
O0/O2, and translated C# under JIT/NativeAOT. It exercises multiple conditional
temporaries, false branches, constructor/consumer failure and repeated loop
activation/reset. Metadata verification passes 31 checks, including six resealed
SQL-valid semantic rejections and three separately identified schema rejections.
[Hashed v26 evidence](../evidence/dotobj-conditional-cleanup-20261009.json) links
the retained receipts and tools.

Original DuckDB `exception.cpp` passes its conditional cleanup gate and now stops
at member selection on a record-valued expression in `unordered_map::operator[]`.
The next core work follows stock C's distinction between a sequence ending in
existing storage and a genuine record value requiring materialization. Array/VTT
cleanup and source throw/catch remain open. No complete DuckDB generation or
libc++ runtime implementation is claimed.

## Addressable sequences and record materialization, v27 (2026-10-09)

Comma expressions now preserve the exact final item's execution location while
evaluating earlier items once. Member selection on a genuine record value gets
an explicit materialization node, following stock C emission. This remains
bounded by existing safe-copy rules and does not invent a C++ object lifetime.
Named fields, alignment, array decay, volatile operations and bitfield semantics
remain intact. Overaligned execution storage is reserved once per activation.

Native050/managed045 passes **1154 full regression checks including NativeAOT**.
The expanded 47-case fixture agrees with native C++ and stock EDG-generated C
at O0/O2, and C# under JIT/NativeAOT. It covers temporary member reads and
destruction, sequence mutation/addressing, returned records/unions/bitfields,
volatile accesses and a 300000-iteration aligned array-decay stress case.
Metadata acceptance passes 39 checks with eight resealed semantic rejections.
[Hashed v27 evidence](../evidence/dotobj-record-materialization-20261009.json)
links the retained receipts and tools.

Original DuckDB `exception.cpp` advances to the next copyability gate. A reduced
type demonstrates the distinction: trivial copy construction can coexist with a
user-provided assignment operator. Stock C preserves that distinction; future
owned source facts must do the same rather than permit raw copying globally.
The next arithmetic gate in `hugeint.cpp` is unsigned128 multiply-with-overflow.
Full DuckDB generation remains incomplete, and libc++ runtime work stays deferred.

## Mathematical 128-bit overflow intrinsics, v28 (2026-10-09)

Add/subtract/multiply overflow nodes now support independent signed/unsigned
operand and destination widths through 128 bits. They preserve the mathematical
range test and always store the truncated result. Fixed UInt128 limbs and
UInt128.BigMul avoid allocation, BigInteger and exception-based overflow tests.
The unsigned multiplication case has a direct full-product path; ordinary narrow
helpers are unchanged. Volatile 128 destinations remain explicitly rejected.

Native051/managed046 passes 34 focused acceptance checks, including **12469
mathematical cases** over 488 type triples and 1488 builtin wrappers, side-effect
and alias checks, source-hidden linking, JIT and NativeAOT. Existing narrow
intrinsics also pass JIT/NativeAOT. An independent emitter prototype passes
20000 mathematical cases in both modes. Metadata verification passes 39 checks
with eight resealed semantic rejections, and both signed/unsigned volatile 128
source destinations reject before publishing objects.
[Hashed v28 evidence](../evidence/dotobj-wide-overflow-20261009.json) links all
receipts. Validation is targeted to arithmetic; the preceding profile 27 full
1154-check regression remains the latest full run. Future full runs include the
new wide fixture.

Original DuckDB `hugeint.cpp` advances past multiplication to a source throw in
`Convert`. The remaining `exception.cpp` copy-transport gate is being addressed
with separate observed construction and assignment permissions. Full DuckDB
generation and source C++ exception support remain incomplete; libc++ runtime
implementation stays deferred.

## Original record construction and transport, v29 (2026-10-09)

Complete original C++ records now own separate bitwise construction, assignment
and caller-result-storage facts. Constructor-safe transfers retain exact EDG
provenance, including generated argument temporaries moved through shallow IL
copies. User-defined assignment remains an explicit call. Deleted copy
constructors and explicit result buffers are respected independently of the
whole-type trivial-copyability trait.

Native054/managed051 passes **1210 full regression checks including
NativeAOT**. The focused single/cross-unit fixture passes54 checks with JIT and
NativeAOT; native C++ and stock EDG C agree at O0/O2. Final metadata acceptance
passes52 checks with11 resealed semantic rejections. The broad run additionally
found and fixed mixed C/C++ fact comparison and flexible-tail zero-initialization
regressions; no source frontend semantics changed.
[Hashed v29 evidence](../evidence/dotobj-record-transport-20261009.json) records
final tools, passing receipts and retained failed attempts.

Original DuckDB exception.cpp advances past copy transport to source throw.
Date, allocator and hugeint also stop at source throws; string_util requires
the live stringstream hierarchy's virtual-base/VTT layout. Full DuckDB generation
remains incomplete. The next bounded core work owns source exception types,
payload construction and destruction, start/completion ordering and rethrow.
Broader exception semantics have a separate nine-check native/stock-C
[oracle baseline](../evidence/dotobj-source-eh-oracle-20261009.json); this is
not an owned source exception implementation or execution claim.

## Bounded source throw/rethrow generation, v30 (2026-10-09)

Original exception identity, payload layout and destruction requirements now
survive independently of EDG. Owned initializer addresses and start markers
retain exact throw ownership; generated C# preserves selected construction,
start/completion order, complete-object destruction and dynamic rethrow.
Conditional throw arms emit the real terminal operation and omit unreachable
stock placeholders. Source handlers and initializer cleanup boundaries remain
explicitly unsupported.

Native055/managed052 passes **56 generation checks**, including cross-unit
descriptor identity, six resealed semantic rejections and four unsupported-source
rejections. Source-hidden linking succeeds with --allow-externs. The C# compiler
reports only six deliberately unbound runtime partial methods, not other C#
errors. The required Edg.Runtime.CppExceptions contract has no invented provider
bodies; this is not source-exception execution acceptance. Existing scalar
cleanup/materialization and record-transport tests pass75 checks under JIT and
NativeAOT. The last full regression remains profile29's1210 checks.
[Hashed v30 evidence](../evidence/dotobj-source-throw-20261009.json) separates
these claims and retains exact tools/receipts.

The selected original DuckDB units now expose throw-initializer cleanup as the
next common gate. GetIndex constructs a std::string argument temporary before
constructing its exception; failure must clean that temporary before releasing
payload storage, then preserve the outer function's remaining cleanup. Hugeint
advances from Convert to NegateInPlace. string_util still requires the live
stringstream hierarchy's virtual-base/VTT layout. Full DuckDB C# generation
remains incomplete; the next work owns these initialization boundaries.

## Throw initializer cleanup boundaries, v31 (2026-10-09)

Implemented by the root agent alone, as requested. Each typed throw now owns an
initialization boundary and every enclosed cleanup transition names its exact
innermost throw. Generated C# saves the entry cleanup cursor before allocation.
Accepted initialization failures unwind initializer-local actions to that cursor,
release unfinished payload storage, then propagate to the enclosing function.
The existing verified destructor chain is reused, with state advancement before
callbacks and conditional flag checks intact. Nested throws retain independent
boundaries; no native stack-frame offsets or frontend semantic changes are added.

Native056/managed054 passes 60 focused scenarios against native C++ and stock EDG
C at O0/O2, and translated C# under JIT/NativeAOT. A fixture-only provider checks
that local temporaries are destroyed before payload release, enclosing locals
remain live at that boundary, incomplete payload destructors are not called and
all payload allocations are released. Four schema-valid, resealed malformed
objects reject missing or incorrect boundary/transition ownership. Prior typed
throw generation passes 56 checks; existing cleanup acceptance passes 21 checks
covering its 47 scenarios under JIT/NativeAOT. These tests do not implement a
production exception provider or source catch matching.

The full native056/managed054 regression passes **1210 checks including
NativeAOT**. [Hashed v31 evidence](../evidence/dotobj-throw-initializer-20261009.json)
records the immutable tools, focused oracles, malformed metadata rejections,
project probe and generated-source hashes.

Original DuckDB `hugeint.cpp` now exports, verifies and generates C# (280 emitted
function definitions including required header bodies). The review alias is
`artifacts/review/duckdb-dotobj-hugeint`. Its externs remain unbound; no compilation
or execution of that unit is claimed. The other selected units retain explicit
gates: unsupported source declaration types in `exception.cpp`/`date.cpp`, an
initialization carrying a destructor in `allocator.cpp`, and virtual-base/VTT
layout in `string_util.cpp`. These are recorded, not taken into this phase.

Partial payload subobject rollback, source handlers and the shared production
exception runtime remain unfinished. A direct discarded nontrivial record comma
operand also remains gated; the focused fixture calls an ordinary observer for
that operand while preserving the same temporary lifetime. Earlier failed probes
and all generated artifacts are retained. Stop after this phase's validation and
commit; do not start the next phase automatically.

## Payload ownership and source catch-all regions, v32–v33 (2026-10-09)

Implemented by root alone in successive bounded phases. Profile v32 added scalar
member/nonvirtual-base rollback inside exception payloads, with complete-payload
ownership taking over after successful initialization. Its [acceptance evidence](../evidence/dotobj-throw-payload-20261009.json)
records 40 execution scenarios and eight malformed-object rejections, including
the preserved differences among independent native reference implementations.

Profile v33 adds ordinary structured `try`/`catch (...)`, original try lifetime
identity, caught/exit markers and exact handler-release states. Emission preserves
normal source exits and destructor order, including an outer destructor throwing
after the source try has ended. Nested handlers, rethrows through called functions,
replacement exceptions and handler-local construction failures retain payload
ownership until the last handler relinquishes it. No frontend semantic fixes or
native stack-offset frames were introduced.

Native063/managed061 passes 105 focused scenarios against native C++ and stock
EDG C at O0/O2, under JIT and NativeAOT. Five SQL-valid, digest-resealed malformed
objects are rejected. A separate release-only module also builds and runs in
both modes. Generated review source is at `artifacts/review/dotobj-catch-all`.
The [v33 evidence](../evidence/dotobj-catch-all-20261009.json) records the final
regression totals, immutable tools and retained artifacts.

The same five original DuckDB units were probed. `hugeint.cpp` still exports,
verifies and generates 280 function definitions; its C# has not been compiled
or executed. The other four units retain their previous type, initialization
and virtual-base/VTT gates. Typed catches, function-try blocks, array/VTT cleanup
and the production exception provider remain open. Stop at this phase boundary.

## Ordered typed handlers and catch parameters, v34 (2026-10-09)

Implemented by root alone. Each try now owns its ordered typed/ellipsis handlers;
typed entries retain the original parameter type, matching type and body-local
binding. Caught-object-address expressions name their exact handler. The managed
emitter keeps EDG's selected parameter initialization, copy-constructor calls and
destructions in the structured handler body. Provider-owned matching and adjusted
addresses use original source identities, including pointer qualifications and
the existing public/unambiguous base-conversion facts.

Native064/managed064 passes 58 focused checks: 14 traces match native C++ and
stock EDG C at O0/O2 under JIT and NativeAOT, and separate child processes verify
termination when a catch-parameter copy constructor throws. Five SQL-valid,
digest-resealed malformed objects reject missing/foreign caught addresses,
wrong matching types, foreign parameter bindings and invalid handler order.
A separate catch-only module with no throw expressions or object-destructor
actions compiles and runs in both managed modes. The existing catch-all suite
passes its 105 scenarios and 57 checks, and source-throw generation retains 56
checks. Constructor/destructor function-try blocks replace typed catches as the
negative handler fixture; EDG lowers ordinary function-try blocks as ordinary try.

The [v34 evidence](../evidence/dotobj-typed-catch-20261009.json) records the full
regression result and retained tools. Review source is at
`artifacts/review/dotobj-typed-catch`. The five-unit DuckDB probe is unchanged:
`hugeint.cpp` exports, verifies and generates 280 function definitions, while
the other sampled type/initialization/VTT gates remain. No compilation or
execution of DuckDB is claimed. The shared production exception provider,
constructor/destructor function-try blocks and array/VTT cleanup remain open.
Stop after this phase's commit and await user direction.

## Constructor/destructor function-try regions, v35 (2026-10-10)

Implemented by root alone. The writer now admits EDG's observed constructor and
destructor function-try blocks and records their original routine kind alongside
the try lifetime. The reader verifies that role through ABI provenance. Existing
protected bodies already contain selected member/base initialization, destruction
and implicit rethrows, so the emitter reuses the verified cleanup/handler model
without reconstructing wrappers or changing EDG semantics.

Native065/managed067 passes 47 focused checks with 34 execution traces matching
native C++ and stock EDG C at O0/O2, under JIT and NativeAOT. These include partial
construction, base-subobject use, delegating construction failure before/after
target completion, destructor body and member/base failures, replacement throws,
implicit rethrow, explicit destructor-handler return and suppression inside a
`noexcept` destructor. Four SQL-valid resealed objects reject incorrect routine
kinds or foreign original lifetimes. A temporary-expression fixture also verifies
that ordinary expression-lifetime rows and their source map remain unchanged.

The first DuckDB probe exposed an accidental `tryKind` addition to expression-
lifetime packaging/source-map columns. That edit was corrected, the incomplete
broad run retained and stopped, and the broad regression restarted under JIT.
Focused exception suites provide NativeAOT coverage; no completed full-suite
NativeAOT pass is claimed for this phase. The [v35 evidence](../evidence/dotobj-function-try-20261010.json)
records exact tool versions, final totals and retained failed/interrupted attempts.

The corrected DuckDB probe returns to the previous result: `hugeint.cpp` exports,
verifies and generates 280 function definitions; the other four sampled units
retain their type/initialization/VTT gates. Review source is available at
`artifacts/review/dotobj-function-try`. The shared production exception provider,
array/VTT cleanup, unsupported exception identities and full DuckDB generation
remain unfinished. Stop after this phase's commit and await user direction.
