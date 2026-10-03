# Experimental direct C# backend

`dot_gen_be.c` consumes EDG's structurally lowered IL directly. It does not emit
or parse C and does not use the dotcc compiler. This implements the scalar, storage,
plain-aggregate, managed-callable, RAII and integer/class/pointer/enum exception
slices of [the implementation plan](../docs/dot-gen-be-plan.md), not a complete
C++ implementation.

## Build and run

From the repository root, using the existing Linux/GCC EDG build prerequisites:

```sh
EDG_BUILD_TARGET=cpfe-dot edgcpp/build.sh
python3 edgcpp/dotnet.py edgcpp/backend/tests/scalars.cpp \
  --output-dir edgcpp/artifacts/example \
  --namespace My.App --class-name Engine --split function

dotnet run --project edgcpp/artifacts/example/Generated.csproj -c Release
dotnet publish edgcpp/artifacts/example/Generated.csproj -c Release \
  -r linux-x64 -p:PublishAot=true -o edgcpp/artifacts/example/aot
edgcpp/artifacts/example/aot/Generated

python3 edgcpp/backend/tests/verify.py --nativeaot
```

The expanded array tests also require Clang as a native oracle; set
`EDG_TEST_CLANGXX` if it is outside PATH and `/usr/lib/llvm-*/bin`.
Return-object tests additionally use `build/bin/cpfe` and `build/lib/libC.a`
from the full `edgcpp/build.sh` build as their executable reference.
Unordered/group tests use the standard `cpfe` C backend and compile its output
with both GCC and Clang. The older `cpfe-dot-oracle` target remains an optional
historical audit tool.

The managed/AOT fixture returns zero on success. `cpfe-dot` is a separate
executable alongside `cpfe`; each has its own object files and backend macro
configuration. The driver supplies validated output settings to the custom
backend through environment variables. Do not invoke the native executable
directly to configure output. EDG's existing language-extension gates remain
compiled in; this does not imply that the C# backend implements those features.

`backend/prepare.py` installs this directory's source and CMake overlay into the
embedded upstream build tree. Keep changes here, not only in `upstream/`.
After editing the emitter, the faster incremental build is:

```sh
python3 edgcpp/backend/prepare.py
EDG_BASE="$PWD/edgcpp/upstream/bases/docker/dev-env/gcc" \
  cmake --build edgcpp/build --target cpfe-dot --parallel 12
```

## Current contract: experimental-runtime-v3

- One or more C++17 translation units (default) or C11 with `--language c`, parsed in explicit GCC 13.3 compatibility
  mode. `-I` and `-D` supply explicit include roots and definitions; both are recorded in the manifest. The default runtime is `none`; `--runtime edg-exceptions` enables the
  pinned EDG exception/header bootstrap described below. `--emit exe` (default) accepts `int main()` or `int main(int,char**)`; `Run()` invokes it and
  EntryPoint supplies the managed entry point. Argument-bearing main also exposes
  `Run(string[])` with owned UTF-8 argv storage through shutdown; see
  [main arguments](../docs/main-arguments.md). `--emit managedlib` permits no
  main and exports public entry wrappers for defined routines. `dot-gen.json`
  maps EDG mangled symbols to their generated method names. Public C functions
  and record/opaque types retain source names: a managed SQLite consumer can
  call `core.sqlite3_open(...)` with `SqliteCore.sqlite3*` directly. Internal
  functions, parameters, locals and fields also retain source names; C# keywords
  are escaped and genuine identifier conflicts use deterministic suffixes.
- Multiple positional source paths use EDG's semantic merge, preserving
  internal linkage, shared inline/template definitions and program type identity.
  The exception bootstrap is supplied once for merged programs. Manifests record
  input paths and SHA-256 hashes. This is the first tested whole-program path;
  a separate object/linker format and full ODR diagnostic coverage remain open.
- `--libc dotcc` enables an audited standalone BCL bridge with explicit LP64
  headers, covering signature-checked memory/string/math/allocation/FILE/text imports and
  addressable per-owner/thread errno. Every imported routine has an exact
  lowered signature contract; incompatible declarations are rejected. External
  function addresses use the ordinary canonical owner-aware managed adapters.
  C allocation shares the C++ NativeMemory heap, including realloc preservation
  and aligned allocation. This subset has no host-libc dependency; full libc,
  fenv and arbitrary locales remain in progress. Selected musl formatting and
  numeric conversion sources now pass generated JIT/NativeAOT tests; source
  inclusion is still explicit. See [numeric conversion](../docs/libc-numeric-conversion.md)
  and [formatting](../docs/printf-cpp-and-stream-integration.md).
- `void` returns and signed/unsigned 8-, 16-, 32-, 64- and 128-bit integers
  (including plain `char`, using the EDG target signedness). Integer casts
  and arithmetic use unchecked C# expressions; logical/comparison results use
  C-style 0/1 values, including runtime short-circuiting.
- Automatic scalar locals, direct assignments, structured if/else, while/do/for,
  switch (including GNU ranges and fallthrough), break/continue and conditional
  expressions. Initializers stay at their execution point; legal source scopes
  retain declarations. Duff dispatch, jumps into nested regions, setjmp and
  lifetime constraints retain necessary labels or hoisted storage.
- Scalar and pointer constant/zero-initialized globals, including relocations to
  other globals. Static class/methods are the default. **`--instance-methods`
  explicitly enables a sealed owner class, instance methods and a separate
  pinned global arena for each owner.** No output-layout option enables it.
- Pointers to supported types, incomplete opaque record handles and void; lowered C++ references;
  address-of/dereference, pointer arithmetic/difference, subscripting, casts,
  null tests and indirect assignment. Const is erased after C++ validation;
  ordinary volatile reads/writes use memory barriers around typed accesses; read helpers cannot inline, so discarded volatile loads remain observable. Volatile bitfields retain their exact occupied-byte access protocol. See [atomic runtime](../docs/atomic-runtime.md) for the separate accepted atomic operations and limits.
- Fixed-size arrays, nested plain POD records and unions use explicit-layout
  unmanaged structs with EDG sizes and field offsets. Arrays use `InlineArray`
  storage, preserving their full extent in value copies with compact source.
  Pointer elements use one-field cells to satisfy C# generic-element constraints. Supported brace initializers recursively write fields/elements;
  omitted elements are zeroed. Plain records support value passing, returns,
  copies, nested arrays and pointers to members/subobjects. C flexible array tails are address-only views at the EDG offset; they add no CLR storage field and preserve the containing record size.
- Nonvirtual single/multiple inheritance uses EDG's base fields and offsets.
  Empty bases and reused tail padding are covered. Truncated base-subobject
  types retain their EDG size with `Pack=1`; compiler-generated byte-counted
  copy/zero helpers preserve tail padding occupied by derived members.
- Virtual calls retain EDG's primary/secondary vtables, construction vtables,
  VTTs and covariant-return/`this` adjustments. Slots hold managed function
  pointers; instance mode uses explicit-owner adapters. Virtual-base cleanup
  consumes EDG's hidden VTT supplements. Pure virtual calls terminate with
  `Environment.FailFast`.
- Ordinary record access uses C# `value.member` / `pointer->member`, not
  per-access byte arithmetic. `[FieldOffset]` stores the EDG layout. Global and
  frame base projections and folded address constants still use byte offsets.
- Aggregate locals occupy a single aligned frame allocated once per
  function invocation, including declarations inside loops. Aggregate and explicit
  scalar-local alignment up to 4096 bytes is supported. Parameters requiring alignment above eight bytes are copied to aligned frame slots. Globals use aligned offsets
  in the pinned arena.
- Resource limits are 64 MiB per global/TLS arena, aggregate type and local
  frame, with a 64 KiB stack-allocation budget
  including alignment slack. Larger frames use NativeMemory allocation and
  a `finally` that releases storage on returns and managed exception exits;
  frames above 64 MiB are rejected. Named types stay in the shared file when
  methods split. This storage cleanup does not implement C++ destructors.
- C++17 assignment evaluates the RHS before the LHS. Generated typed helpers
  enforce that order, including compound assignment, without duplicating
  evaluation. Subscripting preserves the original operand order even for
  `index[pointer]`. Helpers contain no managed references or generic pointers.
- `--namespace` and `--class-name` accept ASCII identifiers (C# keywords work).
  An empty namespace puts output in the global namespace. `--nest-types` is
  the default; `--no-nest-types` emits public top-level types prefixed with the
  wrapper name. Public record types make managed-library signatures accessible
  to a separate consumer assembly.
  `Run`, `Shutdown`, `ReleaseThread`, `Dispose` and names beginning with `__edg_` are reserved class names.
  The wrapper is partial. `--split none|function|size|source` defaults to none;
  `--split-size N` is a positive UTF-8 byte target, default 262144. Whole
  functions are indivisible and may exceed the target. Storage declarations and
  initialization remain in `Translated.cs`; source mode groups movable members
  by their original file. Amalgamations can still use function/size splitting.
  `dot-gen.sources.json` and `source-index.md` map original symbols to exact
  generated declaration lines in every mode.
- `--format roslyn` (default) runs the SDK's Roslyn parser as a whitespace-only
  build tool before splitting; `--format none` disables it. This formatting step
  does not change expressions or add a dependency to generated applications.
- `dot-gen.json` records mode, ABI version, sources, bound imports and generated files. The project
  explicitly lists its source files. Regeneration archives superseded owned
  sources/manifests under `.dot-gen-history` and retires stale split files from
  the current project, preserving unrelated files and campaign intermediates. Translation rejection
  leaves a previous successful output intact. File replacement is atomic per
  file; the entire project update is not a filesystem transaction.
- The generated project targets .NET 10 and is AOT-compatible. There are no
  external dependencies, runtime code generation, reflection or P/Invoke in
  the generated source. Globals use a rooted, pinned byte array and source-named
  ref accessors, including pointer-valued slots. Explicit address operations
  retain the stable storage pointer. All storage is allocated and
  zeroed before constant values and pointer relocations are assigned.
- Entry wrappers and instance function-pointer adapters use `GC.KeepAlive` in
  `finally` so the owner survives the
  entire translated call tree. Lifecycle-enabled entry wrappers and canonical function-pointer adapters also hold active-call leases. `Shutdown()` waits for active calls and rejects new entries. Instance classes implement `IDisposable`. A managed export can return a raw pointer; the host
  must retain its owner for the entire use of that pointer. Retained native callbacks and their boundary adapters remain pending. Raw pointers must not be used after shutdown.

- Ordinary function pointers use `delegate* managed<...>`. Instance mode adds
  an explicit owner parameter and static adapters; a pointer obtained from A
  may be invoked with B. A Volatile/Interlocked cache publishes one canonical
  address per routine. Calls, null tests, equality, pointer arrays and address
  roundtrips are supported. This is a managed internal ABI; it is not a native
  callback ABI, and no libc RuntimeContext binding is implemented yet.
- Narrow, UTF-16 and UTF-32 string literals are pooled by bytes into one static
  pinned array, aligned to eight bytes. Embedded NULs and interior addresses
  are retained. Named mutable character arrays have independent storage.
  The pool is capped at 4 MiB. No Roslyn pass is needed for literal storage.
- Constant and dynamically initialized TLS uses aligned pinned byte arenas,
  lazily allocated through tracked thread-state objects. Static output owns one arena
  per thread; instance output owns one per owner/thread pair. `ReleaseThread()` runs TLS destructors on the calling thread in reverse registration order and releases its arena. The host must call it at translated thread exit: CLR thread exit has no automatic hook here. Shutdown rejects pending destructors belonging to another thread. GCC mode avoids unnecessary wrappers for constant TLS.

- Bitfields up to 64 bits use properties over EDG-positioned backing bytes; signed extension, truncation, packed crossing-byte fields and mutation sequencing are preserved. Ordinary control flow uses labels so C/C++ jumps into ordinary blocks, Duff's device and switch fallthrough remain representable.
- Float/double constants preserve exact IEEE bits. The current target's 16-byte `long double` uses the unmanaged software `Edg.Runtime.Float80` representation (64-bit significand), with ties-to-even arithmetic and conversions. It uses BigInteger temporaries, remains NativeAOT-compatible and does not narrow long double to double. Floating-environment rounding modes and traps are not implemented. Binary128 remains rejected.
- EDG-generated startup bodies run after constant initialization/relocations, in initialization-priority order. Local-static guards synchronize concurrent initialization, retry after failure, and terminate on recursive initialization. TLS initializer aliases resolve to EDG's generated initializer. Global/TLS destructor registrations retain reverse order.
- `Shutdown()` drains the current thread's TLS destructors, then process/owner destructors, and releases outstanding bootstrap allocations and storage. It is idempotent, waits for active calls, and rejects disposal from an active call. Destructors that throw at shutdown terminate. `ReleaseThread()` coordinates with concurrent shutdown. Generated executables invoke shutdown from their entry point; library hosts own this obligation. Instance ownership remains opt-in with `--instance-methods`; there is no implicit instancification.

**The full portable C++ ABI is not frozen or implemented.** The default
`--target portable-lp64-binary80-v1` publishes an explicit little-endian 64-bit
LP64 data model with software binary80 long double. Its custom EDG system-macro
hook suppresses host OS/CPU macros and the host predefined-macro file;
`NO_USR_INCLUDE=1` disables implicit native header paths. Only explicit include
roots are searched. Target layout is validated before translation; profile and
macro-table hashes are recorded in the manifest. `--target legacy-linux-x64`
remains an explicit diagnostic compatibility profile. Complete portable library
headers and binary128 remain pending. Actual execution on Windows, macOS and
Arm64 is still unverified.

Virtual operations on RTTI metadata objects, member-pointer values, variable-length arrays,
binary128 floating point, variadic calls and complete libc/libc++ are not
implemented. General external calls require a declared provider binding. EDG-generated vector construction/destruction/allocation and guard/atexit
calls are mapped to managed helpers; unknown external calls remain rejected. The covered negative
fixtures reject unsupported constructs with backend errors.
Designated/flattened multidimensional initializer forms outside the implemented
initializer walker and partial/cross-type block copies are also rejected.
Unused/folded-away source constructs may disappear before emission. Other IL
outside the implemented cases is rejected; this small emitter is not yet the
plan's independent owned-IR verifier. C# legality for arbitrary goto graphs
and expression combinations is not established.

The EH configuration retains EDG region tables while leaving full portable
setjmp-based lowering disabled. The [lifetime implementation](../docs/lifetime-cleanup-prototype.md)
consumes those tables at translation time and emits direct C# cleanup switches
inside exceptional-exit `finally` blocks. EDG's flags track conditional and
unordered group liveness, including temporary receivers with temporary
arguments. Cloned table entries handle return completion and overlapping
temporary lifetimes; the earlier backend completion-bit buffer is gone.
Normal destructor calls remain EDG-owned. Managed exception identity survives
cleanup; noexcept violations and secondary destructor exceptions terminate via
`Environment.FailFast`. Fixed arrays, NRVO and copy/move/elided returns retain
their existing coverage. The limit is 4096 EH table entries per function,
including supplements. Constructor, destructor and ordinary function-try-blocks now reuse EDG's subobject cleanup and implicit rethrow. First-pass CLR filters preserve EDG termination before secondary-exception cleanup. One cold cleanup decoder per routine avoids duplicating region switches at every unwind site. Unsupported region actions (VLA) and the uncaught exported-boundary policy remain outside this slice. The manifest records `cleanupModel: edg-region-tables`.
The C++ exception slice supports built-in integer payloads (including bool
and character types), enums, object/void pointers, nullptr and
classes/structs/unions, including polymorphic classes and virtual bases. It handles
typed catches by value/reference, catch-all and rethrow from called functions. Payloads live in aligned pinned managed storage;
matching preserves C++ distinctions such as long versus long long. A thread-local
catch stack is shared across instances of the generated program class. EDG's
try/catch epilogues mark exits before subsequent return/goto cleanup. Catch-all
accepts foreign managed exceptions and rethrow preserves their identity.
EDG initializes class payloads and copies catch parameters. Each throw/rethrow
owns a counted use; the last handler releases the payload and calls its
destructor exactly once using the throwing owner's state. Throw/catch copy
failure termination is checked during CLR exception search, before extra
unwinding. Class payloads support the existing 64 KiB size and 4096-byte alignment
limits. Compiler-generated constant aggregate templates and user local statics get stable arena storage.
Pointer matching supports const qualification conversions at multiple levels,
conversion to void pointers and nullptr-to-pointer catches. A pinned catch buffer
keeps pointer-reference mutation separate from the original rethrow payload,
following EDG's C runtime. Enums retain nominal identity even when their C#
storage types coincide. Function pointers, member pointers, pointers to arrays,
remain outside the exception subset; volatile object-pointer catches are supported.
Class and single-level pointer catches accept public, unambiguous bases, with
the EDG byte adjustment applied to the caught address. Null pointers remain null;
rethrow retains the original most-derived type and address. Base catches by value
use EDG's copy construction, while final destruction uses the original owner and
complete-object destructor.
Vtable RTTI slots use stable opaque pinned tokens shared across owners. This
supports table identity and pointer `dynamic_cast`. The RTTI helper reads EDG's
class descriptor initializers into an immutable managed inheritance graph. It
follows the C runtime's downcast and cross-cast traversals, including access,
ambiguity, repeated bases and virtual-base offsets from the active vtable.
`dynamic_cast<void*>` retains EDG's direct offset-to-top lowering. No reflection,
native RTTI runtime or runtime code generation is used. Graphs are limited to
65536 type IDs, 128 inheritance edges in depth and 65536 node visits per root
traversal.
Reference casts use `std::bad_cast` with `--runtime edg-exceptions`; without
that provider the failure entry remains rejected. The same option supplies
`type_info` equality/inequality, `before()`, `name()` and `bad_typeid` (see below).
External declarations allocate no storage; actual external variable
reads/addresses still reject, while unused cast-only vtables can be omitted.
Virtual destructors and scalar new/delete work with translated providers, including
sized-delete failure cleanup. The optional exception bootstrap supplies basic
scalar/array, sized, aligned, placement and nothrow new/delete, plus per-owner new handlers. A NativeMemory allocation ledger tracks originating owners, permits cross-owner frees, and releases remaining allocations at shutdown. libc remains pending.

Exception_ptr and uncaught-exception counts remain pending. This is a single-translation-unit
exception runtime; separately emitted assembly type identity and native EH interop are not implemented; semantically merged translation units share identities.
Only translated handlers participate in payload ownership; retaining a carrier
in a managed host and transferring/rethrowing it later is not an ownership API.
Ownership of future libc/native resources and native callbacks remains a separate integration task.

## Validation

`tests/verify.py --nativeaot` compares scalar, pointer and aggregate fixtures
against native GCC,
builds/runs managed and NativeAOT static/instance variants, checks two independent
owners, keyword identifiers, split output, deterministic regeneration, file
ownership and unsupported-feature rejection. Pointer tests cover local/parameter
addresses, indirect mutation, references, one-past arithmetic, null pointers,
global relocations, alignment and C++17 sequencing. Aggregate tests cover nested
and packed records, multidimensional arrays, partial zero initialization, union
storage, by-value records containing arrays, self/interior pointers and bounded
loop-local allocation. A background thread forces
GC while translated calls use pointers, and collections between calls verify
persistent global addresses and two-owner isolation. Results are saved in
`../artifacts/backend-tests/results.json`; generated C# and native executables
remain alongside it for inspection. Linux x64 is the only execution platform
currently checked. Full milestone 0 and milestone 1 acceptance gates remain open.

`runtime_verify.py` additionally builds a separate consumer assembly for static
and instance managed libraries, with nested and top-level public types and an
empty namespace. It checks concurrent first address publication, explicit-owner
indirect calls, per-owner/per-thread TLS, literal addresses under GC, UTF-16/32,
large-frame normal/exception exits and aggregate signature accessibility.
A compact record of the latest passing checks and emitter/driver/test hashes is saved in
[evidence/dot-gen-backend.json](../evidence/dot-gen-backend.json).

The lifetime increment additionally compares 14 exact native destruction traces
in static/instance JIT and NativeAOT modes, checks termination in subprocesses,
and audits normal destruction with EDG exceptions disabled. The evidence JSON
records the latest full-suite results. Full milestone acceptance remains open.

The array/temporary increment adds shared native/managed case-table tests,
including destruction traces, return values and expected termination during
partial/full array cleanup. GCC and Clang traces are retained, with two exact
GCC 13.3 nested-brace cleanup discrepancies recorded explicitly; see the
[lifetime findings](../docs/lifetime-cleanup-prototype.md). Current totals are
recorded in the evidence JSON.

The return-object increment adds 114 cases using executable EDG-generated C
linked with EDG's native `libC.a` as the primary oracle. GCC/Clang differences
are saved for inspection. Each returned object checks its own address; tests
include construction failure, throwing cleanup, nested returns, loop/goto
reentry and simultaneous results. The C# output uses CLR unwinding and has no
dependency on that native oracle library.

Generated reference C is in `artifacts/backend-tests/return-objects-oracle.c`;
managed output and traces are in `artifacts/backend-tests/return-objects-{static,instance}/`.
All 114 return-object cases run in each of the four modes. The evidence JSON
records source hashes, compiler versions and observed differences.


The group-liveness increment covers 317 cases, including temporary receivers,
conditional/nested receivers, partially constructed receiver members, aggregate
receivers, overlapping returned results and loop/goto reentry. It replaces the
canonical-order eligibility restriction with EDG's standard group flags and
region links. EDG lowering sources are unchanged.

Generated reference C and compiler evidence are saved as
`artifacts/backend-tests/unordered-portable.c` and `unordered-native-oracles.json`.
Generated C#, retained IL and execution traces are under
`artifacts/backend-tests/unordered-{static,instance}/`. Current full-suite
validation totals and source hashes are recorded in the evidence JSON.


The current full suite passed **164 checks**, with **46 successful Linux x64
NativeAOT executions** and **104 expected-termination subprocess checks** (52 JIT,
52 NativeAOT), plus 20 native C-oracle termination checks. All 317 unordered/group
cases, 32 scalar-exception cases, 39 class-exception cases and 48 pointer/enum
exception cases run
in each of the four modes. Exception cases also check forced GC, concurrent
catch stacks, rethrow through another owner, foreign identity and failed operand
evaluation. C++ traces agree with EDG C compiled by GCC and Clang. Inspect
`artifacts/backend-tests/exceptions-oracle.c`, `exceptions-oracle-{gcc,clang}.txt`
and the split C# projects in `exceptions-{static,instance}/`.
Class evidence is in `class-exceptions-oracle-{gcc,clang}.txt`,
`class-exceptions-termination-traces.json` and `class-exceptions-{static,instance}/`.
The C runtime has fixed exception-allocation alignment, so its over-alignment
observations are recorded separately in `class-exceptions-oracle-alignment.json`;
all managed variants independently require the declared payload alignment.
Pointer/enum evidence is in `pointer-exceptions-oracle.c`,
`pointer-exceptions-oracle-{gcc,clang}.txt` and `pointer-exceptions-{static,instance}/`.
It covers const additions/removals, multilevel qualifications, void/nullptr
conversions, nominal enum identity, separate pointer catch buffers, nested
rethrows, owner crossings, thread isolation and forced GC. Regenerating each
split project must produce identical C# files.
A separate catch-all-only translation unit verifies RTTI metadata traversal
without typed handlers supplying the metadata roots incidentally.
Nonvirtual inheritance adds 48 layout/value/lifetime traces per mode, matching
EDG C under GCC and Clang. They cover multiple and repeated bases, public/private/
protected paths, ambiguity, empty bases, tail-padding reuse, constructor failure,
base-value/reference/pointer catches, null adjustment, rethrows and owner-specific
destruction under forced GC. See `inheritance-oracle-{gcc,clang}.txt` and
`inheritance-{static,instance}/` in `artifacts/backend-tests/`.
The former temporary-receiver rejection test is now covered by positive cases.

Virtual dispatch adds 32 result/lifetime traces and virtual inheritance adds 24,
each compared with EDG C compiled by GCC and Clang in all four managed modes.
Cases cover secondary vtables, null/non-null covariant returns, construction and
destruction dispatch, shared virtual bases, hidden VTT cleanup, virtual base
catches, deleting destructors and failed scalar construction. Forced GC checks
stable type tokens and origin-owner destruction. Pure-virtual termination is
checked in four managed subprocesses and both native oracles. Inspect
`virtual-oracle.c`, `virtual-inheritance-oracle.c`, their `*-{gcc,clang}.txt`
traces, and `virtual-{static,instance}/` / `virtual-inheritance-{static,instance}/`
under `artifacts/backend-tests/`. Regenerating all four split projects produces
identical C# files (`virtual-determinism.txt`). No EDG lowering changes were needed.

Pointer RTTI adds 42 result/lifetime traces per managed mode, compared with EDG C
under GCC and Clang. The cases exercise successful/failed/null downcasts,
secondary-base and nonpolymorphic cross-casts, repeated/ambiguous and inaccessible
destinations, shared virtual bases, construction/destruction vtables, const and
void pointer casts, operand evaluation once, local class identity and exception
payloads. Host checks cover forced GC, casting another owner's live object and
foreign exception cleanup. Both split layouts regenerate deterministically.
Inspect `artifacts/backend-tests/rtti-oracle.c`, `rtti-oracle-{gcc,clang}.txt` and
`rtti-{static,instance}/` for generated code and traces. The RTTI runtime uses the
existing translation-unit token identity; cross-module/native object casts are
not an interop contract.

## Optional EDG exception bootstrap

Use `--runtime edg-exceptions` to translate reference casts and the initial
standard exception classes:

```sh
python3 edgcpp/dotnet.py edgcpp/backend/tests/reference_cast.cpp \
  --runtime edg-exceptions --emit managedlib --instance-methods \
  --namespace My.App --class-name Engine --split size \
  --output-dir edgcpp/artifacts/reference-example
```

The driver adds only `backend/runtime/include` to the include path and
preincludes `backend/runtime/exceptions.cpp`. The header wrappers reuse
`exception.stdh`, `typeinfo.stdh`, `new.stdh` and `edg_internal_defines.stdh` from
the pinned EDG checkout. The small `stddef.h` shim serves only this current
LP64 bootstrap; it is not a general C header package. No host include discovery
is performed. The manifest records `runtime: edg-exceptions` or `runtime: none`.

The provider defines `std::exception`, `std::bad_cast`, `std::bad_typeid`, `std::bad_alloc` and `std::bad_array_new_length`
constructors, copies, assignments, virtual destructors and `what()`. Empty
`what()` strings match the EDG runtime. A compiler-generated `__cxa_bad_cast`
call bridges to a translated C++ function containing `throw std::bad_cast()`.
Its payload, vptr, catch matching/copying, rethrows and final destruction use the
same EDG IL and managed exception ownership as source-written throws. Catches by
`std::bad_cast&`, `std::exception&`, value and ellipsis work, including across
owners of the same generated class. The provider is explicitly selected; it is
not a choice of the final libc++ implementation.

Scalar `operator new` requests at least one byte and throws `std::bad_alloc`
when allocation fails. The storage bridge uses `NativeMemory.Alloc/Free`;
.NET selects its platform implementation at runtime and the calls are AOT
compatible. CLR allocation failure becomes a null result to the C++ provider.
Scalar/array, sized, aligned, placement and nothrow new/delete and new handlers are supplied. These definitions cannot be combined with application replacements of the same global operators. An owner heap ledger supports explicit C++ delete, cross-owner free and shutdown release. Virtual RTTI metadata operations and remaining exception-library APIs are unsupported. Execution remains verified only on Linux x64.

`reference_cast.cpp` adds 24 value/lifetime traces per managed mode, including
successful/failed/const casts, exception copying and slicing, nested/cross-owner
rethrows, foreign cleanup, allocation failure, zero-size allocation, virtual
delete and failed-constructor cleanup. Original EDG C compiled by GCC and Clang
is the oracle; its failures use the original libC `__cxa_bad_cast`, not the new
bridge. Two fatal scenarios per mode check noexcept and secondary exceptions.
The harness places a handler outside the operation to force unwinding (an
unhandled NativeAOT exception may terminate during search). Split output is
deterministic. Former array/aligned/new-handler negative cases are now positive allocation tests. Generated code and traces are under
`artifacts/backend-tests/reference-{static,instance}/` and `reference-oracle*`.

## Public type information

With `--runtime edg-exceptions`, static and dynamic `typeid` share the same
stable pinned tokens used by vtables and `dynamic_cast`. The backend reads
EDG's type lists and descriptor initializers; built-in descriptors normally
supplied by libC use their Itanium type-name encoding. `std::type_info::name()`
returns a stable, null-terminated pointer into a pinned name pool. Equality
uses token/name identity, and `before()` compares name-string addresses, as in
EDG `lib_src/typeinfo.c`. The ordering is implementation dependent; it need not
match the native linker's address order. Metadata is immutable, shared across
owners of a generated class and rooted independently of owner lifetimes.

EDG supplies polymorphic operand evaluation, active construction/destruction
vtables and null checks. `__cxa_bad_typeid` calls an ordinary translated C++
function throwing `std::bad_typeid()`. Typed/base/value catches, slicing,
rethrow, cleanup and noexcept therefore use the existing exception machinery.
No EDG lowering changes were needed.

`type_info.cpp` checks 34 result/lifetime cases and 24 names against original
EDG C/libC under GCC and Clang, then static/instance JIT and NativeAOT. Names
cover classes, enums, unions, built-ins, qualified pointers, arrays, function
and member-pointer types, incomplete pointees and distinct local types. These
metadata queries do not enable executing otherwise unsupported values, such as
floating-point or member-pointer operations. Tests cover global metadata
relocations, forced GC, concurrent access, owner sharing and one noexcept
termination per mode. A focused JIT/NativeAOT rerun after the full suite adds
case 33 and verifies that EDG already lowers discarded `typeid` to a pointer expression,
preserving effects without copying an opaque metadata object. Explicit-cast case 16 intentionally preserves the original
C backend's static L/R descriptors (result 0); case 32 exercises dynamic
secondary-base identity through pointer variables (result 1).

Opaque tokens do not implement the native metadata class hierarchy. Virtual
operations on `std::type_info` objects, including nested `typeid(typeid(x))`,
casts that inspect their vptr and destruction, reject before publishing output.
Native/cross-module RTTI identity remains unsupported. Generated C, C#, manifests
and traces are in `artifacts/backend-tests/typeinfo-oracle*` and
`typeinfo-{static,instance}/`.

## Basic language, initialization and allocation validation

`c_language.c` additionally checks C11 mode, explicit include/define options, opaque handles and flexible array tails under static/instance JIT and NativeAOT.

`basic_language.cpp` has 64 native-differential cases covering switch/ranges/fallthrough, Duff's device, jumps into ordinary blocks, packed/signed bitfields, exact float/double/binary80 values, 512 randomized extended arithmetic comparisons, 128-bit operations/conversions, ordinary volatile access and volatile-pointer catches. Both GCC and Clang compile the original EDG-generated C oracle.

`initialization.cpp` covers startup priorities, dynamic global arrays, failed local-static retry, once guards under eight concurrent callers, dynamic and local TLS, destructor order, owner isolation, callable entry rejection after disposal, shutdown waiting for active calls and reentry during that wait. Hosts explicitly release foreign TLS on its owning thread.

`allocation.cpp` adds 18 cases for array cookies, partial-construction cleanup, throwing destruction, zero-sized allocation, aligned scalar/arrays, placement/nothrow forms, new handlers and bad array lengths. The host also frees through a different owner and leaves an allocation for shutdown. Both suites compare static/function-split and instance/size-split output under JIT and NativeAOT, with forced GC and deterministic regeneration.

The allocation oracle supplies one C++ aligned-nothrow forwarding definition because the pinned native archive does not export that symbol; it still uses the original EDG C lowering, EH runtime and aligned allocator. Clang uses `-femit-all-decls` to retain the C backend's assembly-referenced TLS initializer. EDG's invalid-length nothrow array expression throws `bad_array_new_length` before allocation; this behavior is preserved, not changed in EDG.

## SQLite acceptance attempt (2026-10-02)

Run `python3 edgcpp/sqlite_probe.py`. The script uses the pinned SQLite 3.53.4 amalgamation and the existing deterministic definitions in `sqlite/config/defines.txt` (including `SQLITE_OS_OTHER=1`, `SQLITE_THREADSAFE=0`, FTS5 and math functions). Explicit local GCC LP64 headers are a diagnostic input profile, not the final cross-platform header contract.

The original EDG C backend succeeds. Its 4,840,704-byte [generated C](../artifacts/sqlite-edg/sqlite3.edg.c) passes GCC syntax checking and object compilation. The native object has 39 libc/math imports and `sqlite3_os_init`/`sqlite3_os_end`, plus two compiler/linker symbols. See the [import inventory](../artifacts/sqlite-edg/native-imports.json).

The C# run first exposed opaque `sqlite3_mutex` and flexible `ExprList.a` layouts; both now have emitter support and C11 native/JIT/NativeAOT tests. The current first rejection is `address of external function: trunc`, referenced by SQLite's math-function table. No C# project was published, so no SQLite C# build, query execution or NativeAOT claim is made. External managed adapters, the libc binding layer, internal variadic functions and the SQLite OS/VFS provider are the next integration work.

[Detailed results and exact commands](../artifacts/sqlite-edg/results.json), [C# diagnostics](../artifacts/sqlite-edg/csharp.stderr.log) and [compact saved evidence](../evidence/sqlite-edg.json) remain available. Latest backend validation: 175 full-suite checks and a final 10-check focused rerun, across 54 distinct successful Linux x64 NativeAOT fixture configurations.
