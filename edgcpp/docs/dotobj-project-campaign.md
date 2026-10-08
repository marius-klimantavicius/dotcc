# Owned semantic object implementation campaign

The acceptance target is translation and execution of SQLite and DuckDB through
independent SQLite `.dotobj` files produced from original translation units.
The managed linker and C# emitter must work without the original source files or
EDG. A successful small language fixture is supporting evidence, not completion.

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
5. Link all required DuckDB objects and bundled dependencies using bounded body
   loading. Run SQL and DuckDB test selections against the native configuration,
   then NativeAOT. Resolve actual failures before broadening acceptance claims.

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
