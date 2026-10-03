# Plan for the EDG C# backend dot_gen_be.c

Status: implementation started 2026-10-01; continuous implementation campaign
active. The latest committed snapshot passes 439 checks, including Linux-x64
JIT/NativeAOT. Additional focused acceptance covers VLA lifetimes/alignment,
allocation-free exception capture, uncaught counts, wide-character libc,
atomic waits, clocks and lowered initializer details. See the
[campaign ledger](implementation-campaign.md) for exact accepted scopes and the
[implementation status and reproduction](../backend/README.md). Full milestone
0/1 acceptance is still open. Source inspection uses EDG 7.0 at revision
`89e67e07c7f6d0fc38622f9778d8e79f40deaa0f` and the current dotcc workspace.
Updated with an emitter and generated Valkey review on 2026-10-01. The review
inspected source and generated artifacts; it did not rerun Valkey's campaign.

Introduce **`src/dot_gen_be.c`**, compiled as C++ like EDG's other `.c` files,
to emit C# that builds and runs under .NET and NativeAOT. Generated code may
use unsafe pointers, explicit layouts, static functions and generated dispatch
tables throughout. Correct C++ behavior takes priority over readable or
idiomatic C#.

Reuse EDG's semantic analysis and most structural lowering. Implement C++
unwinding with CLR exceptions and object-lifetime `try/finally` regions. Reuse
dotcc's libc where its contracts fit, prefer musl over newlib for an upstream
libc port if a BCL-backed adaptation is straightforward, and translate an
upstream C++ standard library. Put platform differences behind a runtime-selected
C# platform layer. An optional Roslyn build tool performs final source processing.

This is an independent EDG backend. It does not translate through C or depend
on dotcc's C parser. Reusing audited runtime components and build-tool code
from dotcc is explicitly part of this plan.

## Required outcomes

- The same generated C# project and translated library sources run on Windows,
  Linux and macOS, with platform APIs selected at runtime.
- Generated code, the C++ semantic runtime, libc and libc++ adapters all pass
  NativeAOT publishing and execution tests. A successful JIT build alone is
  insufficient.
- Initial supported architectures are little-endian x64 and Arm64. Additional
  architectures require a versioned target profile and their own tests.
- Preserve explicit C++ storage, object lifetime, pointer identity, template
  instantiation, RTTI and exception behavior for the supported feature set.
- Port library implementations rather than recreating the standard library
  wholesale in C#. Supply platform and managed-runtime integration in C#.
- Provide configurable namespace, wrapper class, nested generated types and
  source-file splitting. Enable independent engine instances only through the
  explicit `--instance-methods` CLI option, matching dotcc. This option includes
  per-instance globals, libc/libc++ state and safe disposal; static output is
  the default.
- Emit an actionable diagnostic for unsupported semantics or unresolved
  symbols. Do not silently omit operations or substitute dummy results.

The portable unit is the generated project and managed assembly for the chosen
data model. **NativeAOT executables are built separately for each OS and
architecture.** Runtime platform selection remains in the source; AOT may
specialize a platform check for its target. Publishing per runtime identifier
does not authorize separate Windows/Linux translations or compile-time OS
branches in the portable library implementation. Microsoft documents the
[NativeAOT deployment model](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)
and [cross-compilation constraints](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/cross-compile).

## Architecture and proposed components

```text
C/C++ sources + portable target headers + pinned library configuration
    -> EDG semantic analysis, instantiation and selected IL lowering
    -> dot_gen_be.c adapter and backend-owned typed representation
    -> initial C# files + program manifest + source maps
    -> optional build-time Roslyn processing
    -> final C# + DotGen.Runtime + libc provider + translated libc++
    -> .NET build or NativeAOT publish for a selected runtime identifier
```

All paths below are proposed additions, not existing implementations.

| Component | Responsibility |
| --- | --- |
| `upstream/src/dot_gen_be.c`, `dot_gen_be.h` | EDG backend entry point, IL traversal and emission orchestration. |
| `upstream/src/dot_gen_ir.*`, `dot_gen_emit.*` | Owned types, expressions, control flow, lifetimes and C# emission; split further as needed. |
| `upstream/src/cmake/cpfe-dot/CMakeLists.txt` | Separate executable and configuration for the C# backend. |
| `config/dotnet/` | Explicit target ABI, predefined macros, system headers and library configuration. |
| `DotGen.Runtime/` | Storage, allocation, C++ type descriptors, exceptions, initialization, TLS and platform services. |
| `DotGen.Libc/` | Adapter around the selected libc provider, initially audited dotcc components. |
| `DotGen.PostProcess/` | Optional Roslyn finalization, including literal pooling; build-time dependency only. |
| `DotGen.Build/` | Program assembly, symbol resolution, manifest validation, namespace/wrapper layout, source splitting and SDK invocation. |
| `tests/dot-gen/`, `scripts/` | Differential fixtures, library suites, platform runs and reproducible generation. |

Keep implementation patches and new source files reproducible outside the
ignored upstream checkout, or commit them in a deliberate upstream fork.
Extend the existing build workflow to apply those changes to the pinned EDG
revision. Editing only `edgcpp/upstream/` is not a durable deliverable.

## EDG integration and the lowering boundary

Add a `cpfe-dot` executable that provides `back_end()`. Follow the existing
[driver integration](../upstream/src/cfe.c) and
[CMake target](../upstream/src/cmake/cpfe/CMakeLists.txt), without changing the
behavior of `cpfe` or `cpfe-cp`. Keep `dot_gen_be.c` as the required entry-point
filename even when implementation is split across helper files.

The current tested cleanup configuration is:

```c
BACK_END_IS_C_GEN_BE=0
BACK_END_IS_CP_GEN_BE=0
BACK_END_SHOULD_BE_CALLED=1
DO_IL_LOWERING=1
DO_FULL_PORTABLE_EH_LOWERING=0
GENERATE_EH_TABLES=1
DO_UNORDERED_EH_PROCESSING=1
KEEP_OBJECT_LIFETIME_INFO_IN_LOWERED_IL_WHEN_EH_ENABLED=1
IL_SHOULD_BE_WRITTEN_TO_FILE=0
```

These settings now underpin the tested lifetime/group-cleanup implementation;
they do not establish full C++ EH or complete backend support.
Review all dependent configuration checks and target macros. Input-language
exception support must remain enabled for exception-using programs even though
the native compiler executable itself is built without C++ exceptions.

Reuse template instantiation, overload resolution, constant evaluation, class
layout, member promotion, mangling, explicit `this`, reference lowering,
virtual-call adjustments, and constructor/copy/move operations. Retain the EH
nodes, type metadata and lifetime boundaries needed by the backend. EDG's
[exception lowering modes](../upstream/doc/source/lower_il.rst) explicitly
provide a backend-owned EH option.

**Cleanup ownership is explicit.** Keep EDG's ordinary destructor calls on normal
exits, and consume region-table actions for exceptional exits only. The current
C# implementation uses this division in a per-function protected body. It does
not identify or remove destructor calls by name or source-text matching; explicit
source destructor calls must survive.

Lifetime preservation and EH tables depend on `exceptions_enabled` in
[lower_il.c](../upstream/src/lower_il.c). The EH-disabled audit verifies the
normal destruction calls that EDG still emits. It does not promise cleanup for
foreign managed exceptions crossing EH-disabled code; extending that contract
would require a separate implementation and validation decision.

Read function memory regions through EDG's APIs, as the C backend does. Copy
needed information into the backend-owned representation before releasing a
region. Do not treat EDG's serialized IL or native pointer values as a stable
external format or deterministic symbol identity.

Build a small explicit representation containing typed operations, object
storage, symbol IDs, sequence points, lifetime transitions, normal and
exceptional control-flow edges, and source locations. A verifier rejects
unknown nodes, unbalanced lifetimes, illegal branches and missing layouts
before producing C#. This representation is also the proper place to legalize
C# control flow and evaluate side-effecting expressions exactly once.

## Portable target and native boundaries

Freeze a versioned **portable 64-bit ABI** before translating libraries.
Proposed baseline: 8-bit bytes, little endian, 64-bit data pointers, 32-bit
`int`, 64-bit `long` and `long long`, 64-bit `size_t`/`ptrdiff_t`, 32-bit
`wchar_t`, and explicitly specified scalar alignment. These are translated
program conventions even on Windows, whose native ABI has different sizes.

Do not inherit the translation host's GCC headers, `_WIN32`, `__linux__`,
native `FILE`, `pthread` layouts or `va_list`. Supply target headers and
predefined macros describing the portable runtime. An upstream platform branch
must either become part of the portable port or an explicit runtime adapter;
it cannot be selected once by the host and masquerade as portable output.

Choose floating formats explicitly. The proposed portable `long double` and
`__float128` representation is 16-byte IEEE binary128, with software operations
where needed; support explicit binary80 as a separate type if a workload
requires it. Audit dotcc's [Float128](../../DotCC.Libc/Float128.cs) for reuse,
including layout, rounding, formatting and NativeAOT behavior. Configure EDG's
constant evaluator, target headers and runtime to agree. Never silently map an
extended float to `double`. The final target table, including alignment and
floating formats, is a milestone-zero acceptance artifact.

Keep EDG's object-model rules where reusable, but do not claim native C++ ABI
compatibility for the resulting managed calling convention. Native calls go
through adapters converting integers, structures, strings, error codes and
ownership. A Windows `long`, native `wchar_t`, `stat` structure or `va_list`
cannot be passed by reinterpreting the portable representation.

For EDG self-translation, distinguish the ABI of the translated compiler
executable from the ABI of the C/C++ programs that compiler processes. Host
implementation helpers such as extended arithmetic must still support EDG's
chosen compilation targets.

## Storage and generated operations

| Area | Implementation decision and required evidence |
| --- | --- |
| Scalars and expressions | Explicit promotions, signedness and casts; `unchecked` where appropriate; byte-sized C++ bool storage. Preserve defined sequencing and short-circuit evaluation. |
| Objects and unions | Unmanaged explicit-layout structs or byte-backed storage with generated accessors. Preserve base offsets, bitfields, overlap, empty bases, tail padding and array stride. |
| Alignment | Compute from the target profile. Overallocate and align storage where CLR local/array alignment is insufficient; verify every required alignment. |
| Locals and temporaries | Stable addressable storage with lifetime markers. Use bounded stack allocation or owned unmanaged frames; release frames in `finally`. Stack allocation inside loops must not accumulate without bounds. |
| Allocation | One ownership protocol for `malloc/free`, `new/delete`, aligned allocation and array cookies. Implement overflow checks, partial array destruction and allocation-failure behavior. |
| Functions and vtables | Internal generated calls remain managed, including typed `delegate* managed` calls where valid. Generate adjustment thunks and stable symbol identities. Validate function-pointer comparisons and round trips. |
| Callbacks | Separate native ABI wrappers from internal functions. Statically generate/root required callbacks; catch exceptions at native boundaries and translate them according to the adapter contract. |
| Variadic functions | Define a portable argument-pack representation shared by generated callers and libc. Preserve promotions and `va_copy`/`va_end`; native varargs need explicit signature-specific adapters. |
| Globals | Program manifest owns storage, identity, zero initialization, relocations and dynamic initializer order. Avoid depending on incidental CLR type-initializer order. |
| Static locals and TLS | Thread-safe once guards, retry after failed initialization, recursion handling and per-thread storage. Run TLS destruction at translated thread exit; define explicit attach/detach behavior for foreign threads. |
| Atomics and volatile | Map C++ operations to tested Interlocked/Volatile primitives or locks, including memory orders, widths and alignment. A pointer load alone is not an atomicity guarantee. |
| RTTI | Generated descriptors and inheritance graphs operate on unmanaged objects; share them with exception matching and `dynamic_cast`. |

Unmanaged object memory must not contain untracked CLR object references.
Represent managed services with opaque handles backed by explicitly rooted
tables and release protocols. A stable pointer to a pinned array also requires
a strong root for as long as the pointer can be used.

## C# emission lessons from dotcc and Valkey

The following are inspected implementations, not claims that all their behavior
has already been validated for C++, every OS or this new backend.

| Evidence | Behavior to use as a design reference |
| --- | --- |
| [Compiler.StateContext.cs](../../DotCC.Lib/Compiler.StateContext.cs) | Separate ambient state-context and instance-method modes; owner-held pinned global storage and per-owner thread state. |
| [Compiler.Shell.cs](../../DotCC.Lib/Compiler.Shell.cs) | Static global containers, ref accessors, lazy pinned TLS, wrapper and namespace emission. |
| [CSharpBackend.FunctionPointers.cs](../../DotCC.Lib/Backends/CSharpBackend.FunctionPointers.cs) | Canonical address caches and static adapters that take an explicit program owner. |
| [CSharpBackend.cs](../../DotCC.Lib/Backends/CSharpBackend.cs) | Addressing globals through their storage root, pointer globals stored as `nint`, and C# lvalue/legalization rules. |
| [RuntimeContextLib.cs](../../DotCC.Libc/RuntimeContextLib.cs) | Thread-affine runtime bindings, callback/thread leases, resource ownership and refusal to dispose live state. |
| [Compiler.SourceFiles.cs](../../DotCC.Lib/Compiler.SourceFiles.cs) | Partial-class splitting at complete-function boundaries, portable filenames and removal of stale generated files. |
| [ValkeyCore.cs](../../valkey/generated/TranslatedValkey/ValkeyCore.cs) | Actual generated `Managed.Database.ValkeyCore`: sealed partial owner, nested types, pinned globals, per-owner TLS and static callback adapters. |
| [ValkeyCore.00000.cs](../../valkey/generated/TranslatedValkey/ValkeyCore.00000.cs) | Actual split method file with the same partial owner and file-local aliases. |
| [ValkeyServer.cs](../../valkey/src/Managed.Valkey/ValkeyServer.cs) | Authored host creates a core, enters its runtime on a dedicated thread, and coordinates shutdown. |
| [Valkey campaign](../../valkey/scripts/campaign.py) | Instance convention selected for every object and at link time; configurable wrapper/namespace and 256 KiB splitting. |

The generated Valkey files are local artifacts and may be regenerated; the
emitter, campaign and [instance-translation contract](../../docs/instance-translation.md)
are durable implementation references. Do not copy its generated source as a
new backend's implementation.

### Addressable globals and initialization

C# has no namespace-level variables. Put storage in generated containers and
emit methods on a wrapper class. A C++ global is a stable storage object,
not a C# property value: any facade accessor must preserve lvalue identity,
including `&global`, `&global.member` and pointers into arrays. Return `ref`
when exposing mutable storage; do not copy a globals struct on access.

Use zeroed, rooted pinned byte storage or explicitly owned unmanaged arenas
for addressable globals. Emit typed projections using verified offsets and
ref/pointer helpers. Verify the projected types contain no managed references.
An ordinary field in a managed owner object is movable; moving C++ globals
into that class without a storage strategy is incorrect. Dotcc's static mode
uses an unmanaged static globals struct; its instance mode uses pinned bytes.
Adopt the stronger explicit-storage contract uniformly in the new backend.

Taking an address via `Unsafe.AsPointer(ref value)` does not pin or root its
backing. Use it only after storage ownership proves that the pointer remains
valid. C# also rejects pointers/function pointers as generic type arguments,
which affects `Unsafe.AsPointer<T>`, generic arrays and generic atomic helpers.
For pointer-valued global slots, use pointer-width integer storage with typed
read/write/address operations, or project from an enclosing unmanaged struct.
Do not instantiate `Unsafe.AsPointer<Foo*>`; do not take the address of a
temporary cast. Preserve one storage slot across all translation units.

Separate storage allocation, zero/constant initialization, pointer relocations
and dynamic construction. Allocate every global before resolving references
between them. Run dynamic construction through an explicit ordered initializer
driver and record completed destructors for teardown. Avoid type-initializer
cycles caused by callback caches, constants and global addresses. Source-file
splitting must not change initialization order.

### TLS and pinning

TLS belongs to a physical thread. In instance mode its key is **program owner
plus thread**, not just thread. An owner-held `ThreadLocal<ThreadState>` or an
equivalent registry provides that separation. A single `[ThreadStatic]` field
cannot distinguish two engines entered on the same thread, and placing that
attribute on an instance field does not solve the problem.

Allocate and initialize each TLS block lazily, keeping a strong reference to
its pinned backing. Do not put a nonzero initializer on a `[ThreadStatic]`
field and expect it on every thread: its type initializer executes only once.
See [ThreadStatic documentation](https://learn.microsoft.com/en-us/dotnet/api/system.threadstaticattribute?view=net-10.0).
Preserve a stable `&thread_local_object` until that thread's translated lifetime
ends. Dynamic TLS constructors, destruction order and failures need generated
guards and thread-exit hooks, not just `ThreadLocal<T>.Dispose()`.

Distinguish storage lifetimes: stack/frame locals, synchronous borrowed pinned
host buffers, owner globals, owner/thread TLS, and immutable literal pools.
`fixed` only protects its lexical lifetime; it cannot authorize a pointer that
escapes beyond it. Root pinned pools, retain owners for outstanding pointer
uses, and explicitly own/free unmanaged allocations. Support empty storage,
over-alignment and overflow in allocation arithmetic. The C#
[unsafe-code rules](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/unsafe-code)
explain why an unmanaged pointer cannot serve as a GC root.

Dotcc currently rejects initialized TLS arrays and some over-aligned TLS
objects in its emitter. Record these as coverage gaps to implement/test in
`dot_gen_be`, not restrictions to silently inherit. Byte-backed aligned TLS
can avoid pointer-element generic constraints while preserving the target ABI.

### Instantiated programs and runtime state

Support static output by default. The **`--instance-methods` CLI option** enables
the independent-engine mode modeled on Valkey, including per-owner globals,
libc/libc++ state, TLS ownership, callback conventions and safe disposal.
Select it for whole-program translation, or consistently for every object and
the final managed-library link, as in dotcc. Supplying `--class-name`,
`--namespace`, `--nest-types` or a split option must not implicitly enable it.

With this option, emit a sealed unsafe partial owner class. Each instance
represents an independent translated program with its own storage, startup and
shutdown.
This wrapper is a hosting construct; it does not turn C++ classes into CLR
classes or add owner fields to their object layouts.

The ownership inventory must include namespace/file globals, function-local
statics, static data members, variable-template/inline storage, once guards,
dynamic initializer/destructor lists, TLS, C++ runtime handlers and libc/libc++
state. Include `errno`, streams/descriptors, logical current directory,
environment policy, locale, RNG state, synchronization objects, thread registry,
atexit registrations and mutable caches. Changing only the application's
global fields is insufficient when its libraries still share mutable state.

Generated code and truly immutable metadata can be shared. Identical literal
bytes may share storage where C++ permits it; named addressable objects still
follow the instance's identity contract. C++ type metadata can be shared only
when it contains no owner-relative addresses. Resolve mutable vtable/runtime
data and global relocations against the proper owner.

Define an explicit synchronous entry scope that binds the owner's libc/runtime
context and restores the previous binding on return or exception. It must nest
correctly for A -> B -> A reentry on the same thread. Direct global/TLS access
uses the receiver, not whichever owner happens to be ambient. Public facade
entries establish/check the binding; internal calls may rely on an established
matching scope. Never silently fall back to a process singleton in instance
mode. Prefer explicit owner arguments for new shared runtime APIs; any ambient
binding is an adapter mechanism with a tested contract.

Bindings are thread-affine and cannot cross `await`. Do not use `AsyncLocal`
as a replacement for C++ TLS. Asynchronous hosts should queue work to an engine
thread or explicitly enter/leave on each synchronous segment. Translated thread
creation captures and retains the originating owner before launch, binds it on
the new thread and performs TLS cleanup before releasing it.

Give owners explicit `Dispose`/shutdown behavior. First stop accepting work,
drain retained callbacks and join threads; then run required TLS/global
destructors while runtime services remain available; finally release resources,
roots and storage. Reject or defer disposal with active bindings or leases.
A retained CLR reference alone does not prevent explicit disposal. Failed
shutdown must keep live storage intact rather than manufacture dangling
pointers. Initialization failure must unwind only completed initializations.
Define process-exit/abort handling separately for embedding; per-instance state
is isolation of program resources, not an OS security sandbox.

### Function pointers with program owners

A C++ function pointer identifies code; it cannot silently become a managed
delegate containing an owner without changing its size and representation.
Adopt dotcc's explicit-owner managed convention for instance mode:

```csharp
// Illustrative target signatures, not a native callback ABI.
delegate* managed<Engine, int, int> callback;
int result = callback(engine, argument);

static int ReadAdapter(Engine engine, int argument)
{
    using var binding = engine.Enter();
    return engine.Read(argument);
}
```

The stored pointer remains one pointer wide and refers to a static adapter;
the owner is passed on each invocation and remains a GC-tracked managed
parameter. Do not put a CLR owner reference inside C++ unmanaged memory.
An instance method cannot itself be the target of a C# `&method` function
pointer; the adapter provides that entry point. C++ member calls additionally
pass the C++ object address, distinct from the program owner. Apply this
convention through vtables, thunks, callback arrays, lambda call operators,
function returns/parameters, variadics and legal casts, not only direct calls.

Taking a function address in instance A does not bind that pointer to A:
invoking it with B executes against B. A deferred registration must retain
both its code pointer and intended owner separately, plus a lifetime lease.
Adapters enter the supplied owner on the invoking thread and restore prior
bindings. C++ objects/raw pointers exchanged between owners need an explicit
ownership contract; a code pointer does not make another owner's storage safe.

Use canonical generated address accessors per function and test address
identity under JIT tiering, repeated address-taking and NativeAOT. Dotcc caches
addresses on demand and avoids one large eager cache initializer. Verify any
lazy publication strategy; suppressing a pointer-comparison warning is not
proof of C++ identity semantics.

Record static versus instance convention and its version in every object and
runtime ABI manifest. Reject mixed conventions before emission/linking. A
namespace or filename change must not change symbol identity, whereas adding
the owner argument is an ABI change selected before translating objects.

Native function pointers require distinct `delegate* unmanaged`/P/Invoke
adapters; a signature with a CLR owner reference cannot cross that boundary.
Where a native API provides a user-data pointer, use a rooted owner handle and
registration lease. Other native callback protocols need a statically generated
adapter/registration design compatible with AOT, or a precise rejection. Never
cast a native callback to the instance convention. Shared-library export mode
must define this boundary explicitly rather than reuse the managed API.

### Output controls and source partitioning

The following table describes the target driver/build API. The implemented
subset and its current defaults are recorded in `backend/README.md`; lifecycle
and final-program assembly contracts below are not all implemented yet.

| Option | Contract |
| --- | --- |
| `--emit=exe|managedlib` | Explicit executable entry or reusable managed project. |
| `--runtime none|edg-exceptions` | Implemented bootstrap selection, default `none`. Explicitly include the pinned EDG exception headers/provider and basic scalar allocator; does not choose the final libc/libc++ provider. |
| `--class-name <identifier>` | Name the partial wrapper, default `DotProgram`; validate/escape C# identifiers. |
| `--namespace <dotted-name>` | Place generated program types in a namespace; default global namespace. Bind at final program assembly. |
| `--nest-types` | Put translated types and program-specific helpers inside the wrapper, isolating separately generated engines. Reusable runtime assemblies keep their own namespace. |
| `--instance-methods` | Explicit opt-in to independent engine instances: owner methods/globals, libc/libc++ state, owner/thread TLS, owner-aware callbacks and safe disposal. Default off; selected for whole-program translation or every object and the final link. |
| `--state-context` | Optional secondary mode retaining static methods with an explicitly bound context. Mutually exclusive with instance methods; implement only with its own callback/binding contract. |
| `--split=none|function|size` | One output unit, one whole translated function per file, or groups of whole functions. All are one semantic program. |
| `--split-size <bytes>` | Positive UTF-8 grouping target in size mode, initially 262144. Whole functions may exceed it. |

Emit static unsafe partial wrappers in static mode and sealed unsafe partial
wrappers in instance mode. Preserve partial extension points for authored host
code. C# partial declarations must agree and compile in the same assembly;
file splitting does not create separately loadable components.
[Partial type documentation](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/partial-type)

Qualify references with `global::` where needed, escape keywords and resolve
C++ identifiers that collide with wrapper members, types or C# keywords.
Propagate accessibility to all types appearing in public signatures. Reserve
a generated-member prefix. With nested types, emit file-local aliases so two
translations copied into one consumer project do not fight over global aliases.
Test two wrappers in the same namespace as well as namespace changes.

Keep an authoritative initialization driver independent of partial declaration
order. Dotcc currently keeps fields/initializers in the main shell file; use
that as the initial strategy, with separately partitioned data/initializer
methods only when their order is explicit. Splitting methods cannot fix an
oversized individual method; that remains an IR-level transformation.

Use compiler-provided method boundaries, deterministic filenames and
case-insensitive collision checks. Emit a file manifest and delete only stale
files from that manifest, preserving authored partial classes and sidecars.
Every file receives the appropriate namespace, unsafe context, aliases,
source mapping and narrow warning policy. The Roslyn postprocessor must load
all evaluated project inputs and preserve this source-layout contract.

### Other C# legality requirements

Retain dotcc's separation of a storage lvalue from the expression used to read
it. Pointer globals stored as integers still require typed arithmetic and
address semantics. Flattened pointers-to-arrays require explicit row strides;
ordinary C# scalar-pointer arithmetic is not sufficient. Scalar conditions,
enum conversions, bitfields, aggregate initialization and expression-valued
assignments need deliberate lowering, not text-level C-to-C# substitution.

Generated inline/fixed-array storage must meet C# element-type restrictions;
pointer elements may need pointer-width slots and typed accessors. Distinguish
unmanaged values from CLR references, spans and other restricted ref-like
types. Do not store a span as an ordinary global or carry it across an async
boundary. Keep large frames/arrays from silently becoming unbounded stack use.
Unaligned fields require validated accesses, and C# `volatile` is not a blanket
implementation of arbitrary C++ volatile or atomic objects.

## Acceptance tests for output and instance ownership

Add these gates to the normal semantic/JIT/NativeAOT suites. They are requirements
for this backend, not tests claimed to have run during the documentation review.

1. **Output combinations:** static/instance modes, global/dotted/escaped
   namespaces, renamed wrappers, nested types and every split mode. Compile
   two generated wrappers plus authored partial sidecars in one project.
   Regenerate between split modes and verify only stale generated files vanish.
   Verify static mode remains the default and layout-only options never enable
   independent instances; require `--instance-methods` explicitly and reject
   incompatible object/link conventions.
2. **Global addresses:** repeated and cross-TU `&global`, pointer-to-pointer
   slots, function-pointer slots, nested fields, aligned objects and global
   initializer cycles. Force GC and verify identity and stored pointer values.
3. **Instance isolation:** run A and B concurrently and interleave them on one
   thread. Verify distinct globals, function statics, inline/template variables,
   libc/libc++ state, current directories, streams, locale and destructors.
4. **TLS matrix:** at least two owners on two threads; same-owner/thread reuse
   is stable, all distinct pairs are isolated. Include initialized arrays,
   over-alignment, foreign thread attach/detach and throwing TLS construction.
5. **Callback ownership:** A -> B -> A reentry; a pointer obtained in A invoked
   with B; deferred callbacks on another thread; owner-specific qsort/thread
   adapters; native wrappers; exceptions while bindings are nested.
6. **Lifetime safety:** disposal with active bindings/threads/leases fails
   safely; normal disposal drains them; construction/shutdown failure does not
   release live storage. Borrowed host pointers cannot outlive their lease.
7. **ABI enforcement:** reject mixed static/instance objects and incompatible
   callback/vararg/runtime signatures. Verify C++ object and pointer sizes are
   unchanged by the managed program-owner convention.

Use a small deterministic two-engine fixture first, then a Valkey-style
integration workload. Source inspection of Valkey demonstrates a useful design;
it does not certify a new backend's multi-instance behavior or cross-platform
support.

## Exceptions and RAII through CLR unwinding

Implementation update: [lifetime/group cleanup](lifetime-cleanup-prototype.md)
now consumes EDG partial-EH region tables with `GENERATE_EH_TABLES=1` and
`DO_UNORDERED_EH_PROCESSING=1`, while keeping full portable EH lowering disabled.
It preserves EDG normal cleanup and dispatches table-derived destructor actions
only on exceptional exits. EDG conditional flags handle unordered groups,
including temporary receivers; cloned entries handle return completion without
backend completion bits. Numeric region transitions and symbolic local/this
handles become direct C# operations, not native EH tables or setjmp.
The previous canonical-order group restriction is removed. Ordinary objects,
members, arrays, temporaries and returns retain their regression coverage.
The C++ exception slice now supports aligned native built-in integer and class payloads,
exact typed catches by value/reference, catch-all and rethrow through a
thread-local active-catch stack shared across owners. Try boundaries drain EDG
regions before entering handlers; EDG try/catch epilogues control return/goto
exit timing. Class payloads retain the throwing owner, count original/rethrow
uses and run the destructor on the final release. EDG supplies construction and
catch copying. Copy failure filters enforce the native termination point before
additional CLR unwinding. All 32 integer and 39 class value/lifetime traces agree
with the C backend under GCC and Clang, in static/instance JIT/NativeAOT modes;
over-alignment is checked separately from the C runtime's fixed-alignment allocator.
Nonvirtual single/multiple inheritance now includes adjusted base catches,
public/ambiguous path checks, partial base cleanup, empty bases and tail-padding
reuse. All 48 layout/value/lifetime cases agree with EDG C under GCC and Clang
in static/instance JIT/NativeAOT, including origin-owner destruction under GC.
Object/void pointer and nullptr catches now use C++ type/qualification matching
and separate native catch buffers, preserving EDG's original-pointer rethrow
behavior. Enums retain nominal identity. All 48 pointer/enum traces match the C
backend under GCC and Clang in static/instance JIT/NativeAOT modes, including GC
and concurrent catches. The later implementation campaign added function/member/array-pointer
exception descriptors; volatile exception forms still require their own coverage. Virtual dispatch and inheritance now cover vtables, covariant thunks, hidden
VTT cleanup, complete exception destruction and adjusted virtual-base catches.
The 32 dispatch and 24 virtual-inheritance cases agree with EDG C under GCC/Clang
in all four managed modes. RTTI slots have pinned identities, and pointer
`dynamic_cast` now walks a managed graph extracted from EDG class descriptors.
Its 42 result/lifetime cases cover downcasts, cross-casts, access/ambiguity and
active construction vtables. Reference casts now use an opt-in translated EDG standard-exception provider
(`--runtime edg-exceptions`), with 24 differential cases and noexcept/secondary
exception termination tests. Public `type_info` equality/inequality, name, ordering and `std::bad_typeid` now have 34 differential cases and 24 name comparisons; virtual metadata-object operations and cross-module identity remain open. Scalar new/delete accepts translated providers; the optional exception
bootstrap supplies a basic NativeMemory-backed scalar provider. The later translated libc++ exception_ptr adapter provides originating-owner
reference counts, nested exceptions and managed escape/reinjection. Complete
foreign/native EH integration remains a separate boundary; see the current
status table and implementation documents.

The implementation campaign replaces short-lived pinned exception arrays with
NativeMemory storage. Escaped managed carriers support explicit disposal, which
runs the C++ destructor on the originating owner and releases storage once.
Finalizers release native bytes only. Outstanding carriers hold owner leases;
shutdown rejects premature disposal and preserves storage if a destructor's
host callback retains a new carrier. Deferred shutdown rejects ordinary calls,
permits carrier disposal (including cross-thread callback reentry), and can be
retried after release. These mechanisms have focused Linux x64 JIT/NativeAOT
coverage and underpin the subsequently implemented exception_ptr adapter. They
do not settle every foreign/native exception boundary.

Generate `try/finally` around actual destruction lifetimes: function scopes,
inner scopes, expression temporaries and construction phases. Functions with
no cleanup require no protected region. Nested lifetimes naturally establish
reverse destruction order; conditional lifetimes need initialized flags or
equivalent explicit state.

For a simple local object the intended shape is:

```csharp
Widget storage = default;
Widget_ctor(&storage);
try
{
    TranslatedBody(&storage);
}
finally
{
    Widget_dtor(&storage);
}
```

This is illustrative. Constructor-generated code owns cleanup of members and
bases if the constructor fails. Return objects, NRVO, placement construction,
explicit destruction and loop reentry need their own lifetime transitions.
Setting storage to `default` is not a substitute for C++ initialization.

Use a `CppException : Exception` carrier with stable storage for the thrown
object, C++ type identity, destruction operation and ownership state. Generate
catch selection against the C++ type graph, not the CLR inheritance of the
payload. Catch-by-value construction, base adjustments, `throw;`,
`exception_ptr`, nested exceptions and exception-object release must have
explicit runtime contracts. Do not rely on CLR finalizers to destroy thrown
C++ objects.

Catch filters may inspect type metadata before unwinding; keep that matching
free of user-visible side effects. Construct catch parameters only when the
handler is entered. Track active C++ unwinding so a destructor exception cannot
silently replace the original exception. Enforce `noexcept`, failed handler
parameter construction and termination behavior, and implement
`uncaught_exceptions()` coherently.

C# restricts branches into/out of protected regions. Normalize each function's
control flow against its lifetime tree. Use structured regions where possible
and an explicit state-machine transform for irreducible cases, preserving
scope entry/exit cleanup. A flat `switch` that ignores lifetimes is not an
acceptable fallback.

All cleanup tests must establish **exactly one** destruction, including normal
returns, `break`, `continue`, `goto`, exceptions and failed construction.
Do not also run EDG's portable EH cleanup tables for the same objects.

C++ exception support uses no `setjmp`. Explicit source `setjmp/longjmp` is
a separate feature: adapt the compiler-assisted tagged-jump approach in
[dotcc](../../DotCC.Libc/SetJmpLib.cs), preserving surviving locals and target
identity. CLR `finally` behavior differs from C longjmp; support only documented
well-defined cases and diagnose unsupported control flow. Neither native
longjmp nor a native exception may unwind through managed frames.

## libc strategy

**Musl is the preferred upstream libc candidate, ahead of newlib, conditional
on a reasonably small BCL-backed port.** Audited reuse of dotcc's libc remains
the initial integration path and fallback, behind a DotGen ABI adapter, while
that feasibility is established. Its [project](../../DotCC.Libc/DotCC.Libc.csproj) declares AOT/trim
compatibility, but that is evidence of intent rather than proof that every
function satisfies this backend's ABI or all platforms.

Inventory every imported C function, intrinsic and data symbol used by the
test programs, libc++ and EDG itself. Classify each as reusable, adaptable,
missing, or intentionally unsupported. For each reusable operation record
integer widths, pointer ownership, error behavior, thread state, callbacks and
platform dependencies. Link or extract a versioned runtime subset; do not
copy emitted helper text into unrelated files without provenance and tests.

High-priority areas are allocation/alignment, memory/string routines, printf
and portable varargs, `FILE` and descriptor ownership, `errno`, environment,
locale/encoding, wide characters, time, filesystem, synchronization, math and
floating-environment behavior. C++ streams will expose gaps in the C I/O and
locale layers. Audit compiler builtins and atomic/math support separately from
ordinary libc names.

Keep one provider for each shared state domain. In instance mode, libc and
libc++ mutable state must participate in the same program-owner model as the
translated application; reuse through an adapter cannot leave hidden singleton
state. Keep the runtime entry binding and owner-aware callback ABI explicit.
Do not mix two independent
`FILE` implementations, allocators, locale objects or `errno` stores across a
library boundary. Opaque portable handles map to managed objects or native OS
resources through adapters; they are not native libc pointers.

**Preferred upstream route:** translate musl's reusable C implementation with
`cpfe-dot` and implement its platform services in C#, primarily through the
.NET BCL. Musl targets Linux, so a BCL-backed port needs an explicit adaptation
audit; a small set of portable OS hooks must not be assumed to suffice.
[Musl project overview](https://wiki.musl-libc.org/)

Evaluate a pinned revision before committing to a full port. Inventory syscall,
assembly, atomic, thread/TLS, allocator, stdio/descriptor, locale and startup
dependencies. Build a vertical slice covering memory/string/math operations,
allocation, formatting and file I/O. Measure how much upstream code can remain
unchanged and how much Linux behavior would require emulation. Prefer existing
dotcc BCL implementations where they fit the chosen state and ABI contracts.

Adopt musl if this slice needs a bounded, maintainable C# platform layer and
passes the same libc conformance and libc++ integration probes in JIT and
NativeAOT. The same translated sources must retain runtime platform selection
for Windows, Linux and macOS; platform-specific APIs are allowed behind that
layer. Verify per-owner globals, errno/TLS, resource ownership and disposal in
the opt-in instance mode. Loading native musl is not the proposed BCL port.

If musl requires extensive Linux emulation or pervasive upstream rewrites,
continue with audited dotcc libc and selectively reuse suitable musl algorithms.
Newlib is a fallback candidate only if that evaluation warrants reconsideration;
it is no longer the preferred upstream option. Record the measured porting cost,
pinned revisions, licenses, patches and symbol ownership. This preference does
not assert that a musl port is already easy, and no source revision is selected
or vendored by this plan update.

## C++ standard library and ABI support

Use **LLVM libc++ as the first upstream candidate**, pinning a revision after
EDG compatibility probes. Translate both its headers and required out-of-line
library sources with `cpfe-dot`. The target program must not depend on the
host's native libc++ or libstdc++ for the portable core.

Treat this as a new libc++ platform port. Audit compiler builtins, attributes,
feature checks, generated configuration, visibility assumptions and target
types. Establish an explicit portable configuration used by both library and
application translation. Keep an auditable patch set instead of defining host
OS macros until the headers happen to compile. LLVM documents its
[vendor configuration mechanisms](https://libcxx.llvm.org/VendorDocumentation.html).

Implement a portable threading adaptation with C# synchronization and thread
services. Libc++ exposes an external threading interface, which is a concrete
integration point; its exact macros and signatures must be checked against the
pinned revision. [Threading support API](https://libcxx.llvm.org/DesignDocs/ThreadingSupportAPI.html)

The port needs more than OS syscall wrappers. Inventory the ABI support
symbols expected by both EDG and libc++, including allocation, exception
creation/catching/rethrow, RTTI, guard variables, termination, atexit and TLS
destruction. Provide exactly one owner for each operation, with generated
adapters where EDG and libc++ naming/contracts differ. The
[libc++abi specification](https://libcxxabi.llvm.org/spec.html) is a contract
reference. Native libc++abi/libunwind cannot be plugged in wholesale to unwind
CLR frames. Reuse suitable algorithms or metadata handling where helpful,
but implement exception transport and lifetime integration in DotGen.Runtime.

Bring up library families in this order:

1. Traits, utility, iterators, algorithms, array, span and basic containers.
2. String, vector/map, allocators, smart pointers, function objects, optional
   and variant, including exceptions and failed construction.
3. Exception facilities, synchronization, threads, atomics and TLS.
4. Streams, locale, filesystem, chrono/time zones, regex and formatting.
5. Remaining facilities admitted by the published support matrix.

Every stage translates required library definitions as well as application
headers. A template-heavy header compiling does not prove that the library's
out-of-line dependencies are implemented. Preserve feature coverage goals;
temporarily disabling a library family is a recorded milestone limitation,
not the final portability strategy.

## Runtime platform selection

Define a narrow Platform API consumed by libc and C++ library ports. Prefer
portable .NET APIs when their behavior matches the contract. For genuinely
platform-specific operations, implement Windows, Linux and macOS adapters and
select them with runtime `OperatingSystem` checks and, where necessary,
architecture/capability probes.

Keep platform-native layouts inside their adapter. Convert portable flags,
errno values, paths, encodings, handles, times and structures explicitly.
Unsupported facilities such as unavailable process primitives return the
documented library error or a translation diagnostic when fundamental; they
must not fake success.

All adapter code ships in the same source project. Do not use generated
`#if WINDOWS` or per-OS libc++ translations to choose runtime behavior.
Architecture intrinsics need a guarded portable fallback. Platform-specific
imports must remain uninvoked until their platform is selected; avoid static
initializers that eagerly load every OS library.

Use statically described P/Invoke signatures or typed native exports where
needed, with explicit ownership and calling conventions. Default NativeAOT
P/Invoke is lazily bound; direct P/Invoke can require a library at process
startup, so do not enable it indiscriminately for optional platform adapters.
[NativeAOT interop documentation](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/interop)

Internal calls and C++ unwinding stay on the managed side. Native callbacks
use generated wrappers with a documented error/deferred-exception protocol;
no exception escapes an unmanaged callback. Test reentrant callbacks and
resource cleanup, not just successful one-way calls.

## Optional Roslyn second stage

Adopt an explicit optional two-stage source pipeline. `dot_gen_be.c` emits
semantically complete initial C# with generated metadata/markers. A Roslyn
tool then performs transformations requiring whole-program C# knowledge.
It runs during the build and is not referenced by the translated application.
Roslyn is never loaded at application runtime or used for dynamic code emission.

Initial use: literal and constant-data pooling. Emit a stable literal ID,
element type, exact bytes, length, terminator, alignment and identity policy.
The initial implementation can use correctly rooted per-literal storage so
the unprocessed project already works. The second stage consolidates eligible
storage into a generated pinned pool and replaces references with base-plus-
offset operations.

For `u8` data, a generated static constructor can copy compile-time byte data
into `GC.AllocateUninitializedArray<byte>(..., pinned: true)`, retain the array
in a static field and publish its address only after initialization. Preserve
embedded NULs, required trailing NULs, arbitrary byte strings, wide literals,
alignment and program-lifetime roots. A temporary span or a pointer alone does
not provide those guarantees. Split pools when needed for size limits.

Pool only storage whose language identity permits pooling. Mutable arrays,
independent globals and allocations retain distinct storage even when bytes
match. Static pointer relocations and address-observable constants must remain
correct after pooling. Do not use text matching to discover addresses.

Reuse the relevant ideas from dotcc's
[literal pool](../../DotCC.Lib/LiteralPool.cs),
[global storage](../../DotCC.Libc/GlobalArrayLib.cs), and
[Roslyn post-processing workflow](../../docs/postprocess.md). At the inspected
revision, the main pool generation is in the compiler's `LiteralPool.cs`; the
standalone postprocessor is a useful workflow/reference rather than an
assumption that the requested pooling pass already exists there.

Additional passes may canonicalize generated declarations, split large source
containers, simplify proven helper operations and validate NativeAOT roots.
Semantic EH, lifetime and native-ABI decisions remain in the compiler/runtime,
not inferred from arbitrary emitted C#. Large-method splitting requires an
IR-level correctness transform before source emission.

Each pass must bind symbols, preserve evaluation/exception behavior, produce
deterministic output and source maps, and be idempotent. Verify the original
and rewritten compilations, then run behavioral parity tests. Keep unprocessed
output available. No Roslyn references may appear in the shipped program's
dependency graph.

## NativeAOT rules

- Start with .NET 10 and pin the SDK/toolchain used for acceptance. Enable
  unsafe code, AOT compatibility and trim analysis for runtime libraries;
  publish actual executables with `PublishAot=true` from the first milestone.
- Resolve templates, types, callable signatures and dispatch targets at build
  time. Generate tables and adapters instead of Reflection.Emit, runtime
  compilation, expression-tree compilation or dynamic generic construction.
- Root all generated indirect-call targets and callbacks through explicit
  references/metadata understood by the toolchain. Treat unexpected trim/AOT
  warnings as gate failures; document any narrowly justified suppression.
- Keep internal callable entry points managed so CLR exceptions can propagate.
  Use native entry-point attributes only for explicit native wrappers.
- Keep pointers valid under moving GC, concurrent collection and allocation
  pressure. Validate roots, alignment, pinning, frame lifetime and deallocation
  in both JIT and NativeAOT builds.
- Runtime loading of an allowed native OS library is distinct from loading a
  managed plugin assembly. The latter is outside the initial closed-world
  NativeAOT contract. Enumerate every native dependency per published target.
- Treat COM, dynamic marshalling, native varargs, signals and process-control
  APIs as explicit adapter work, never implied by successful C# compilation.

NativeAOT's restrictions on dynamic code and managed assembly loading are
documented in its [limitations](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/#limitations-of-native-aot-deployment).

## Program assembly and large inputs

Use EDG's semantic multi-translation-unit merge as the first whole-program
path, with one program manifest and multiple emitted `.cs` files. Physical
source splitting is independent of semantic merging. Do not require a single
enormous C# file or lose internal-linkage identities when splitting.

Verify this path before depending on it for libc++. Our full EDG self-merge
produced [110.1 MiB of C](../artifacts/self-lowered-combined/README.md), but GCC
found missing generated type/helper names; the 75 separate C outputs passed
syntax checks. Determine whether each defect is in merged IL, lowering or C
emission. A new emitter does not automatically fix upstream merge defects.

Add a per-translation-unit object-manifest mode if needed for scalability or
to isolate such defects. Its linker must resolve externals, forward types,
templates, inline/COMDAT identities, static storage, relocations and startup
order. It must verify compatible complete definitions and preserve independent
internal symbols. Never merge types by spelling alone or silently select an
incompatible definition. Unknown external symbols stop the build unless an
explicit native adapter is supplied.

Record EDG revision, target-profile version, headers/library hashes, flags,
runtime version, source hashes and postprocessor version in build manifests.
Also record static/instance calling-convention version, state-ownership mode,
namespace, wrapper, nesting and split policy. Calling convention participates
in semantic compatibility; presentation options bind at final assembly.
Measure translation time, peak memory, emitted size, Roslyn compile time and
NativeAOT publish size/time on increasing workloads.

## Implementation milestones and acceptance gates

The full implementation campaign is now coordinated in
[the persistent ledger](implementation-campaign.md), with disjoint worker
ownership, serialized builds/commits and preserved per-run artifacts. During
this campaign, use only synthetic sources and EDG tests for verification;
real-world acceptance, including SQLite, is deferred until the entire plan is
implemented. Preserve intermediate C/C#, IL, projects, compiler outputs and
failure evidence. Commit each significant verified milestone.

Prefer NativeMemory for short-lived storage and C/C++ heap allocation. Pinned
arrays remain appropriate for globals and owner/thread TLS. Exception payload
and catch-buffer conversion, including final release and host ownership, is a
tracked runtime task. Correctness takes precedence over allocation shortcuts.

The first campaign integration increment adds exact-signature libc imports,
callable import addresses, per-owner/thread errno, a portable LP64 header subset,
and a shared C/C++ NativeMemory allocator under `--libc dotcc`. EDG semantic
merging now accepts multiple positional input files; synthetic C/C++ fixtures
cover internal linkage, inline/template identities, initialization and cross-TU
RTTI/EH. Function-try-blocks reuse EDG's implicit rethrow/subobject lowering;
cleanup bookkeeping preserves CLR first-pass exception search. Focused JIT/AOT
evidence is in the campaign ledger; this does not complete libc, full EH or
the portable target/platform matrix.

Milestones 0 and 1 remain in progress; the first storage portion of milestone
2 is also implemented. Remaining milestone 2 work and later milestones are
planned. The initial slices implement the custom target and direct scalar IL emission, static and
opt-in instance globals, namespace/class controls, whole-function/size splitting,
and project/ownership manifests. Aggregate ABI v2 includes pointers/references to
supported scalars, addressable locals, aligned pinned global arenas, zero and
constant initialization followed by global pointer relocations, and owner
rooting through Run. Fixed arrays and plain POD records/unions now use unmanaged
explicit-layout C# structs. Ordinary record members retain `.`/`->` access;
EDG offsets are recorded in FieldOffset attributes rather than expanded at each
access. Aggregate locals use an aligned frame per invocation; array
and record initialization, nested members and value copies/returns have native
oracles. Array elements are real CLR fields, not nominal padding. Bitfields up to 64 bits now use EDG-positioned byte backing and named properties; nonvirtual/virtual bases, virtual calls and ordinary lifetimes are now implemented for the tested subset. Frames above the 64 KiB
stack budget now use heap allocation with finally-based release; aligned scalar
locals share the frame. The total frame limit is 64 MiB. Typed assignment helpers preserve C++17 RHS-before-LHS
sequencing. Linux x64 tests cover aliasing, alignment, concurrent forced GC and
independent owners under JIT and NativeAOT. This ABI is still experimental, not
the final portable target. The retained-EH configuration and ordinary cleanup
ownership are implemented; complete EH coverage, owned IR, platform headers,
complete runtime storage and libc++ probes
remain open. Existing source audits and C-output probes are inputs, not completed
backend milestones. See the backend README for the precise supported subset.

The runtime ABI v3 increment implements ordinary managed function pointers,
canonical Interlocked-published routine addresses and explicit-owner static
adapters. It also adds pinned narrow/UTF-16/UTF-32 literal pooling and lazy
constant/zero-initialized TLS arenas per thread or owner/thread. Output controls
now include `--emit exe|managedlib`, public managed entry wrappers and mangled
symbol mappings, `--nest-types`/`--no-nest-types`, and an empty namespace.
A separate consumer project exercises public record signatures and owner-aware
calls under JIT and NativeAOT. These are implemented portions of milestones
2, 4 and 7; they do not complete those milestones.

The basic-language and initialization/allocation increment adds flattened ordinary control flow, bitfield access, exact float/double constants, software binary80 arithmetic, 128-bit integers and volatile scalar/pointer access. EDG startup bodies and TLS initializer aliases are executed directly; local-static guard cleanup follows the C runtime's region flag 0x80. Vector allocation and expression-level internal-try cleanup preserve EDG cookies, constructor failure and reverse destruction. No EDG lowering changes were made for these cases.

The optional bootstrap now provides all ordinary scalar/array, sized, aligned, placement and nothrow allocation signatures. Each owner has its own new handler and heap ledger. Generated library hosts call `ReleaseThread()` at translated thread exit and `Shutdown()`/`Dispose()` at owner end. Executable wrappers automatically shut down. Native callbacks, libc resources, dynamic libraries and cross-module lifetime still require their own contracts. Instance ownership remains selected solely by `--instance-methods`.

The next runtime increment adds explicit portable variadic packs and opt-in
byte-stream libc imports. Variadic signatures receive a hidden VaList pointer;
callers evaluate arguments once, preserve EDG's default promotions and write
aligned values to caller-owned storage. va_copy copies cursors, and packs above
64 KiB use NativeMemory with finally-based release. The FILE bridge uses opaque
native tokens, owner-specific pinned standard-stream cells and BCL streams,
with runtime-selected append/path adapters where required for C semantics.
Wide-oriented FILE behavior now has captured encoding and a strict version2
fpos ABI with native/JIT/AOT evidence. Native varargs and complete libc remain open.

### Remaining work after the autonomous runtime increment

| Area | Status and next dependency |
| --- | --- |
| Ordinary function pointers, literal addresses, constant TLS, output controls, large frames | Implemented and covered by Linux x64 JIT/AOT tests; see backend README for exact limits. No Roslyn runtime dependency. |
| Owner/native callback leases, RuntimeContext binding and disposal | Managed exports and canonical function pointers hold active-call leases; shutdown waits, rejects new entries, drains destructors and releases bootstrap allocations. Foreign TLS requires ReleaseThread on its original thread. Complete lifecycle integration still depends on the libc/native boundary and its resource ownership contract; current exports require the host to retain owners while using returned pointers. |
| RAII, construction/destruction, EH, dynamic initialization/TLS | Ordinary local/member cleanup, fixed arrays, ordered/conditional temporaries and return ownership/NRVO/copy elision are differential-tested. EDG region tables supply ordered/unordered liveness, including temporary receivers and overlapping results. Integer, enum, object/void pointer, nullptr and class exceptions (including virtual bases) match the EDG C oracle in JIT/NativeAOT. Class ownership counts rethrows and calls the destructor once with the originating owner; throw/catch copy-failure termination traces also match. Pointer catch buffers preserve the original pointer for rethrow. Function-try-blocks now have differential traces, including secondary-throw termination before cleanup. Function/member/array pointer exceptions preserve semantic descriptor identity. Translated libc++ exception_ptr and nested-exception adapters now cover originating-owner destruction, global/TLS/heap/foreign-region handles, concurrency and explicit host disposal. Warmed capture/copy/assignment loops allocate zero managed bytes; uncaught counts, catch-parameter current_exception and managed escape/reinjection have focused native/JIT/AOT evidence. Configured owner-specific termination now passes 78 focused checks plus 50 targeted regressions. Dynamic exception specifications pass six positive and 28 fatal native/JIT/AOT checks, including replacement, owner handlers and destructor boundaries; persistent original/replacement payload handles pass five further lifetime checks. Dynamic globals (including priorities), local-static retry/concurrency guards, dynamic TLS, reverse destructor registration and explicit shutdown/thread release are now implemented. |
| Inheritance, virtual dispatch, member pointers | Nonvirtual single/multiple inheritance, empty bases, tail-padding reuse and adjusted public/unambiguous base catches are implemented; 48 EDG C traces agree in static/instance JIT/NativeAOT. Virtual dispatch (32 cases) and virtual inheritance (24 cases) now cover primary/secondary and construction vtables, covariant thunks, hidden VTT cleanup and adjusted virtual-base catches. Virtual destructors require translated delete providers. Pointer dynamic_cast now follows EDG runtime traversal over an immutable managed descriptor graph, with 42 differential cases including construction layouts and cross-owner casts. Reference casts/std::bad_cast now use an opt-in translated C++ provider, with 24 differential cases; public type_info equality/inequality/name/ordering and bad_typeid now have 34 differential cases and 24 name comparisons. Opaque metadata virtual operations and cross-module identity remain open. Member pointers use explicit descriptor tags and adjustment metadata, including virtual functions, null/equality and copy/round-trip tests. Managed pointer low bits are not used as native ABI tags. |
| Portable headers, libc and libc++ | The default portable LP64/binary80 target has versioned macro/header inputs and blocks host-header leakage. The audited BCL libc bridge supplies owner-scoped resources; --libc musl packages 173 pinned formatting/scanning/numeric/wide/multibyte/calendar/timezone/environment/standard-utility and PRNG/math source units, including the authored C11 thread facade, with retained hashes. Wide classification/case conversion matches all Unicode values; byte classification, clocks, locale, atomic waits and selected filesystem/stdio APIs have focused evidence. libc++18.1.8 is pinned; traits and exception/thread/atomic components pass synthetic JIT/AOT fixtures. Explicit single-TU synthetic array, vector, shared ownership, allocation, algorithm and associative-container consumers build and run JIT/AOT with the original core/locale/thread/future/chrono source closure. A two-owner fixture verifies independent containers, new handlers and TLS. Complete libc API coverage, additional libc++ consumers and separate-TU library assembly remain open. The EDG allocation bootstrap remains explicit, not a claim of complete libc++. |
| Basic language coverage | Switch/ranges/fallthrough and nested jumps, signed/packed bitfields, float/double, software binary80 long double, 128-bit integers and ordinary volatile access now have 64 differential cases in static/instance JIT/NativeAOT. EDG VLA lowering now preserves scope/exception cleanup, partial class-array destruction and explicit alignment through native storage; qualifiers and nonfirst-union constant initialization are covered. Binary128 and binary16 storage/arithmetic/conversions now pass ten native/JIT/AOT checks, including 2,048 IEEE rows and all half encodings. C11 atomic-qualified accesses and public atomic macros now use shared seq-cst load/store/CAS adapters with pointer scaling and bitwise floating/aggregate semantics; contained GNU statement expressions preserve caller storage and have differential proof. Binary80 comparisons are exact and allocation-free, with native/JIT/AOT evidence; other arithmetic still needs its allocation/performance audit. Nonlocal/lifetime-bearing GNU expressions, binary128 arithmetic allocation/performance, floating-environment modes, volatile bitfields and unsupported vendor extensions remain open. |
| Source setjmp/longjmp | Compiler-assisted activation/site dispatch skips translated C++ exception handling while managed finally scopes preserve host resource leases. ABI v2 adds VLA allocation checkpoints in a32-byte buffer; 50 focused runtime/source/native/JIT/AOT checks cover surviving allocations, repeated calls and expired allocated-scope rejection. Protected-region targets and complete diagnosis of undefined VM-typedef/pointer scope exits remain outside the accepted slice. |
| Coroutines | Heap-frame suspend/resume/destroy, selected promise allocation, protocol/creation-failure traces and a licensed libc++ no-op handle adaptation are accepted under static/instance JIT/NativeAOT. Initial suspension-dependent scalar values, saved lvalue addresses, short-circuit/conditional expressions and loop conditions use frame storage; ten focused checks plus 38 protocol regressions pass. Awaiter construction/failure and suspended destruction now have 74 native/JIT/AOT checks plus 48 regressions. Exact region/expression provenance distinguishes retained descriptive operands from executable await triples, including reused temporary storage, without changing EDG IL. Allocation-failure return hooks, exact scalar/pointer/explicit-reference placement arguments and standard global nothrow allocation now have focused evidence. Discarded hook defaults, implicit-object placement, reference-result configuration, protected expression suspension and other allocator variants remain in progress. Native GCC/Clang provide differential traces because the EDG C backend retains co_await; a documented GCC saved-value divergence uses the independent Clang oracle. |
| C11 threads and process execution | Worker lifecycle, mutexes, conditions, once/TSS and explicit worker exit have native/JIT/AOT coverage. Normal, quick and immediate process exit preserve callback/stream semantics without freeing storage used by live workers. Executable initial-thread exit supports logical join and last-source-thread normal termination; managed Run keeps explicit host control. Initial C++ cleanup policy and host-libc differences are documented. Foreign-host exit, owner-local process cancellation, signals and additional process APIs remain open. |
| Initialization and allocation | Implemented for supported semantic multi-TU programs: startup/priority order, local-static guards, dynamic TLS, destruction, scalar/array/aligned/nothrow/placement allocation, new handlers and explicit thread/owner shutdown. Tested against original EDG C and with managed concurrency/GC probes. This does not complete libc or cross-module ownership. |
| Owned IR/verifier, source diagnostics and multi-TU assembly | EDG semantic merge now accepts multiple positional sources; synthetic C/C++ fixtures cover internal linkage, inline/template state and cross-TU RTTI/EH. Source diagnostics carry source location/context. The opt-in --type-ir physical graph stores pointer-free identities/layouts and validates edges, extents, alignment and embedded cycles; six focused checks establish raw-emission parity and rejection behavior. Compatible fixed-bound array storage wrappers are canonicalized without merging distinct record identities. Owned operations/lifetimes, the full verifier and separate object/linker mode remain unfinished engineering; this graph is not a linkable program IR. |
| Windows/macOS/Arm64 execution | Environment/validation gap; current evidence is Linux x64 only. |

This table distinguishes design dependencies from unfinished engineering. The
increment does not establish that every task with a settled design is finished.
The ordinary-object cleanup/lifetime prototype now passes native trace comparisons.
Complete lifetime coverage and runtime/library integration still gate the
C++ library phases; milestone 3 has started but is not complete.

| Milestone | Deliverables | Acceptance gate |
| --- | --- | --- |
| 0. Target and IL contract | Versioned target table/headers; static/instance callable ABI and ownership table; backend configuration; lifetime/cleanup ownership prototype; libc import inventory; pinned libc++ candidate and compatibility report. | Prove layout/constant agreement, retained EH/lifetimes with and without input exceptions, and a minimal libc++ header/source probe. Resolve storage, callback and cleanup ownership before implementing bulk emission. |
| 1. Backend skeleton | Reproducible `cpfe-dot` build, `dot_gen_be.c`, owned IR verifier, scalar/control-flow emitter, minimal runtime and project/manifest output; configurable namespace/partial wrapper and initial split mode. | Same generated program runs under JIT and NativeAOT on Linux x64, Windows x64 and macOS Arm64. A two-owner counter fixture proves basic global separation. No C translation stage or runtime Roslyn dependency. |
| 2. Storage and object operations | Layout, pointers, aligned frames, templates, lambdas, construction, copy/move, references, globals and literal storage. | Differential tests for offsets/bytes, aliasing, address identity and sequencing; forced-GC and AOT runs. Source-location diagnostics remain usable. |
| 3. Lifetimes and CLR EH | Protected-region construction, partial cleanup, exception payload/matching, rethrow, exception_ptr, noexcept and termination. | Exact destruction traces across normal and exceptional exits, failed constructors/arrays, nested scopes, return objects, explicit destruction, throwing destructors and complex branches; no duplicate cleanup. |
| 4. Runtime and platform ABI | libc adapter, variadic packs, owner-aware callbacks, initialization/TLS, atomics, native adapters, runtime bindings and retained-owner leases. | Same generated project exercises selected Windows/Linux/macOS implementations at runtime; verify two-owner/two-thread isolation, reentry, disposal, errors, resource ownership and foreign-thread attachment under AOT. |
| 5. Object-model completion | Multiple/virtual inheritance, virtual calls, member pointers, RTTI, TLS guards and audited intrinsic mappings. | Layout/call/RTTI oracle suite, concurrency stress and callbacks on x64 and Arm64. Unsupported extensions fail explicitly. |
| 6. libc provider and libc++ core | Chosen libc provider, translated libc++ core and ABI adapter, pinned patches/licenses, unresolved-symbol report. | Selected upstream libc/libc++ tests pass in JIT and AOT; allocation failure, container exceptions and smart-pointer lifetime tests included. No accidental dependency on native libstdc++/libc++ or native unwinding. |
| 7. Library breadth and source finalization | Streams/locale/filesystem/threads/chrono families with owner-scoped state, optional Roslyn pooling, all output-layout options and reproducibility checks. | Raw/finalized parity; output/instance acceptance matrix; embedded-NUL/wide/aligned literal tests; file/encoding/locale/resource tests across OSes; complete declared library coverage report. |
| 8. Real programs and EDG self-translation | Representative applications plus the compiler implementation translated through `cpfe-dot`; reproducible publish scripts and support matrix. | NativeAOT EDG executable processes a defined C/C++ corpus with matching diagnostics/output behavior against native EDG; translated-program outputs are built and run. Performance and memory budgets are reported. |

Milestones 3 and 4 precede exception-using libc++ acceptance. The optional
postprocessor can begin after stable manifests exist, but is not a dependency
for basic correctness. Extend the platform matrix to Linux Arm64, Windows
Arm64 and macOS x64 before claiming the complete initial architecture set.
Use native runners or equivalent supported environments; unexecuted matrix
cells remain explicitly unverified.

## Validation and release criteria

Maintain focused semantic fixtures and run suitable upstream library tests.
Compare well-defined observable behavior with native EDG/GCC/Clang where the
target contracts agree. For the portable ABI's intentional differences, use
explicit expected layouts and a compatible reference model; an LP64-vs-LLP64
size difference is not itself a compiler bug. Record undefined or
implementation-defined cases rather than comparing arbitrary results.

The runtime suite must cover floating edge cases, allocation failure, GC
pressure, atomic contention, TLS destruction, static-init failure/retry,
exception copying/rethrow, constructor failure, native callback reentry and
cleanup during termination paths. Run abort/terminate tests as child processes.
Verify exception filters have no user-visible side effects before cleanup.

For every claimed platform, build the same generated project under both JIT
and NativeAOT, run it, inspect native dependencies and retain evidence.
Roslyn success, `IsAotCompatible=true`, or a publish without execution does
not establish runtime correctness. Native calls must never use an import from
an unselected platform. Repeated translations and finalization must be
deterministic apart from explicitly excluded diagnostic timestamps.

Publish separate support tables for C++ language features, EDG extensions,
libc functions, libc++ facilities and native adapters. Frontend feature gates
being enabled do not certify backend execution support. Coroutines, inline
assembly, unusual address spaces, vectors and managed C++ extensions each
need an implementation contract and tests or a precise diagnostic.

## Risks and decisions to close first

1. **Lifetime integration:** ordinary local/member cleanup ownership is now
   established: retain EDG normal calls and dispatch remaining actions on
   exceptional exits. Fixed-array and ordered/conditional temporary cleanup
   and return objects now have trace coverage. Ordered and unordered groups now
   use EDG region tables, conditional flags and cloned return-completion entries,
   with standard C-backend oracle coverage including temporary receivers. Inspect the C backend for suspicious cases and do
   not patch EDG lowering. EH-disabled normal exits have a separate audit fixture.
2. **Target consistency:** freeze extended floats, alignment and native-boundary
   conversion before library compilation caches incompatible constants/layouts.
3. **Merged IL correctness:** reduce the observed full-merge failures to tests;
   isolate emitter defects from invalid or incomplete IL.
4. **libc reuse:** measure actual dotcc runtime compatibility with the portable
   ABI and libc++ workload; decide whether a translated libc lowers total cost.
5. **libc++ compatibility:** pin a tested revision, audit required compiler
   intrinsics and ABI hooks, and define the port's configuration/patch contract.
6. **CLR control flow:** prove C++ goto/cleanup and throwing-destructor behavior
   before large-scale source generation makes failures difficult to diagnose.
7. **AOT and scale:** test real indirect calls, roots, callbacks, large methods
   and source volume early. Do not defer AOT until after the runtime is written.
8. **Instance completeness:** audit all mutable application, libc and libc++
   state and every callback boundary. Valkey's C instance convention is a useful
   reference; extending it through C++ vtables, exceptions and static objects
   requires explicit implementation and tests.
9. **Member pointers (review finding):** do not encode virtual/nonvirtual tags
   in assumed unused CLR function-pointer bits. Prototype an explicit tagged
   descriptor/index representation before accepting member-pointer IL.
10. **Foreign CLR exceptions (review finding):** freeze whether/how foreign
    exceptions participate in C++ catch-all, noexcept, active-unwind counting
    and native boundary termination before implementing EH emission.
11. **Function-try-blocks (review finding):** add constructor/destructor handler
    entry, subobject cleanup and implicit rethrow/fallthrough trace fixtures
    before claiming complete RAII/EH support.

The first implementation increment is milestones 0 and 1: a durable native
backend target, an explicit target/runtime contract, a lifetime prototype and
a scalar program that actually runs as NativeAOT on the first three platforms.
This plan authorizes no claim of completed implementation or library support.


### SQLite acceptance result after the language/allocation increment

SQLite 3.53.4 now exercises the new C11 driver mode, incomplete opaque handle pointers and flexible array tails. Original EDG C generation and native object compilation pass; generated C is 4.84 MB. Direct C# emission reaches the external address of `trunc` in the math-function table and deliberately stops. The complete commands/import inventory are in `artifacts/sqlite-edg/results.json` and `native-imports.json`, with a durable snapshot in `evidence/sqlite-edg.json`.

The next work is runtime integration: bind the 39 observed libc/math imports and their managed callable addresses, lower SQLite's internal variadic calls to the planned argument-pack ABI, and supply the SQLite OS/VFS hooks. The diagnostic run used explicit host GCC headers; it does not freeze the final portable header profile. No SQLite C# executable has been produced yet. The first two requested workstreams are implemented for the documented basic single-TU subset; broader language extensions, full EH APIs, cross-module assembly and platform validation remain open.

Coroutine selected-allocation failure hooks now return noncopyable or plain-value
objects without constructing a promise/frame on the failure path. Throwing hooks
preserve ordinary exception semantics. A hook requiring a discarded default
argument remains diagnosed pending backend-owned prelower capture; this is not
complete coroutine allocation coverage. Evidence: `evidence/milestone-coroutine-allocation-failure.json`.

Managed-library coroutine factories now use the ordinary export lifecycle and EH
wrappers, including owner admission and disposal checks. Five native/JIT/NativeAOT
checks and four byte-identical ordinary-wrapper comparisons are recorded in
`evidence/milestone-coroutine-library.json`. Remaining coroutine forms stay open.

GNU `__builtin_expect` now preserves the first long operand and evaluates both
arguments once through an inlinable helper. Original EDG C/C++ oracles and all
eight generated JIT/NativeAOT configurations pass; see
`evidence/milestone-branch-expect.json`. Probability variants remain separate.

Coroutine source function-try blocks now fail closed with a precise diagnostic:
the retained outer source handler needs a backend-owned structural view around
the body-only region, and the previous output could bypass it. Selected
allocation/deallocation routines lacking source definitions also fail before
C# publication. Eleven focused checks are indexed in
`evidence/milestone-coroutine-provider-boundaries.json`; this is a correctness
boundary, not completed function-try coroutine support.
