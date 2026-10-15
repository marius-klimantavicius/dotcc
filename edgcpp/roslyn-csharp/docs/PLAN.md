# Readable unsafe C# translation plan

Status: approved for implementation on 2026-10-15 (design commit `e40280bea`).
Implementation and evidence are tracked in [COVERAGE.md](COVERAGE.md) and
[RESTART.md](RESTART.md); approval does not close any acceptance gate.

## Decisions and boundaries

1. Build a separate translator library, CLI and generated-code runtime. Consume
   the public `RoslynObj` API. The model continues to load independent objects
   and knows nothing about C# translation or program linking.
2. Generate unsafe C# as the normal style. Prefer recognizable names, typed
   pointers, unmanaged records, explicit storage, ordinary expressions and
   structured statements. Preserve lowered execution when recovering source
   structure would require guessing or change behavior.
3. Small C/C++ fixtures establish the mappings; SQLite is the first runnable
   real project. Use the existing full-feature object unchanged. DuckDB follows
   only after the relevant C++ and multi-unit execution gates pass.
4. EDG alone decides types, overloads, template selection, layout, conversions
   and lifetime obligations. Program linking and C# implementation are consumer
   work, not permission to redo C++ semantic analysis.
5. Core EDG remains untouched. Consult the pristine stock C generator and the
   current Roslyn design when interpreting retained IL. Do not use the separate
   dotobj effort, its implementation, findings or acceptance evidence.
6. Source positions, macro associations and comments are optional presentation
   inputs. Use actual retained data, with no recovery or guessing campaign.
   C++/CLI and C++/CX remain excluded.

Recommended initial deployment is .NET 10/C# 14, matching the reader, with
`AllowUnsafeBlocks` enabled. Linux x64 using the captured GNU/Itanium and host C
ABI is the first execution profile. JIT and NativeAOT execution are acceptance
requirements. Cross-platform architecture remains a goal; a different RID alone
does not make Linux-captured layouts or OS structures portable.

The proposed runtime is hybrid: C# implementations/BCL facilities where they
faithfully implement the required contract, and explicit system ABI adapters
where needed. Initial Linux support may bind libc/POSIX/math/thread facilities.
Every import and any small native ABI shim must be reported. SQLite algorithms
and SQL execution must come from generated C#, never native SQLite fallback.

## Projects and dependency direction

Proposed layout under `edgcpp/roslyn-csharp/`:

| Project/directory | Responsibility |
| --- | --- |
| `src/RoslynObj.CSharp` | Public translation API, inventory, program assembly, representation planning and source emission; references `RoslynObj` |
| `src/RoslynObj.CSharp.Cli` | Manifest loading, inventory/translate commands, diagnostics and reproducible output |
| `src/RoslynObj.CSharp.Runtime` | Storage, target operations, initialization, function addresses and later C++ EH support |
| `src/RoslynObj.CSharp.Runtime.Host` | Explicit target/OS adapters and any narrowly scoped ABI shims |
| `tests/` | Genuine source fixtures, generated-code execution and readability checks |
| `acceptance/sqlite/` | Pinned workload, native reference build and feature evidence |
| `docs/` | Contract, capability ledger, acceptance reports and restart state |

Generated projects reference only the runtime components they use. They do not
load `.roslynobj` or depend on the reader's SQLite package. Start with a small
typed C# emission representation and deterministic text writer; no
`Microsoft.CodeAnalysis` dependency is proposed. Use the normal SDK compiler to
build emitted source. The translator is a build tool, not a runtime interpreter
of EDG nodes.

Public translator APIs use ordinary properties and object relationships. A
`TranslationRequest` contains `Inputs`, `Target`, `Runtime`, `Roots` and `Output`;
results expose `Diagnostics`, `GeneratedFiles` and a `ProgramReport`. Internal
index keys are implementation details, not user-facing semantic navigation.

## Input and output contract

A program manifest records ordered object paths and hashes/artifact identities,
capture requirements, source ABI profile, output RID, runtime providers,
executable/library mode, entry point or exports, name/file policy, resource
budgets and translator/runtime versions. It also records link policy, explicit
external bindings and initialization policy. A directory of objects alone does
not specify an executable program.

`TargetProfile` currently records ABI family, addressable-unit size, endianness
and pointer size/alignment; it is not a full OS/CPU/libc contract. Supplement it
with explicit pinned producer provenance and target configuration. Never infer
the source ABI from the machine running the translator.

Output comprises a compilable C# project, generated source, required runtime
configuration, import/link report, capability report and optional provenance
map. Successful publication requires complete representation of the selected
program. Unsupported required operations, unresolved symbols or unsupported
ABI boundaries fail generation with precise diagnostics; no silent stubs,
default-return replacements or omitted bodies.

Default library mode retains all executable definitions and global data in the
supplied units, respecting explicit EDG body classifications. Reachability-based
pruning is a later opt-in optimization. Metadata-only dependent prototypes,
discarded consteval bodies and source associations are not execution roots.
Callbacks, tables, static initializers and cleanup actions participate in the
execution/data dependency closure even when no direct call reaches them.

## Translation pipeline

1. **Admit and inventory.** Open through the reader, check supported contract
   and capture profile, enumerate definitions, storage, initialization roots,
   required kinds and imports. Publish a support ledger with actual occurrences.
2. **Assemble the program.** Preserve TU-local identities, bind actual external
   linkage identities and select definitions under an explicit link policy.
   Produce a deterministic report before emission.
3. **Plan representations.** Choose each type/storage/call representation from
   captured facts and the target contract. Plan global allocation and address
   fixups separately from runtime initialization.
4. **Plan one function at a time.** Interpret typed executable relationships,
   evaluation constraints, value categories and cleanup/control-flow edges.
   Produce a C# emission plan, not a reconstructed C++ syntax tree.
5. **Emit and publish.** Write names, declarations, bodies, initialization and
   adapters deterministically; split large source files without changing method
   semantics. Publish a completion manifest only after all required work succeeds.
6. **Build and execute acceptance.** Compilation alone is not a translation pass.

`DescendantOperations()` is useful for inventory, but deduplicates node identities
and excludes metadata. It is not an execution schedule. Executable occurrences,
recorded operand evaluation flags and explicit reuse determine evaluation. A
shared entity neither means “execute once globally” nor “replay every reference.”

## Required projection rules

| Area | Representation and correctness obligations |
| --- | --- |
| Integers, enums, Boolean | Map actual widths/signedness and storage; emit EDG-selected promotions, conversions, truth tests and narrowing. Use explicit unchecked operations where appropriate; C# shift masking, division and checked behavior require specific handling. Do not infer `long`/`char` from spelling. |
| Floating/complex values | Emit from exact captured components, preserving signed zero and supported special values. Respect source precision and conversions; unsupported extended precision needs an explicit runtime implementation or diagnostic, never silent `double` substitution. |
| Records, unions, bases | Prefer named unmanaged structs with verified explicit layout; preserve overlaps, base/vptr storage, sizes and alignment. Use native storage and named accessors when CLR layout cannot implement the captured representation. Do not rebuild inheritance as C# classes. |
| Bitfields | Implement masks/accessors using captured offsets, widths, signedness and target storage rules. Missing optional container metadata is not permission to guess a container. Validate against actual EDG layout fixtures. |
| Arrays, VLAs, strings | Preserve inline storage, element stride, exact string code units/terminators and captured evaluated bounds. Flexible/incomplete arrays retain that distinction. C strings remain target memory, not managed strings. |
| Pointers and references | Use typed pointers, address/dereference and byte/element offset operations with the recorded units. Lowered references/receivers remain explicit. Address-taken globals and TLS need stable storage; managed references must not leak into native records. |
| Expressions | Prefer ordinary operators and calls. Introduce explicit temporaries/casts for sequencing, comma/statement expressions, aggregate copies and value-category differences. Never duplicate effects or evaluate metadata backing expressions. |
| Control flow | Preserve loops, conditionals, switches and labels where legal. Handle switch fallthrough, goto restrictions and initialization/cleanup boundaries explicitly; use a local dispatcher only where required. Avoid whole-program interpreter-style output. |
| Calls and returns | Use current selected signatures, including hidden result/receiver/ABI parameters and return conventions. Direct calls, indirect calls, callbacks and external calls each need a representation contract. Source signatures are optional display facts. |
| Constants and globals | Separate value interning from storage/address identity. Allocate selected storage first, then resolve pointer-valued constants, then execute captured dynamic initialization. Preserve cyclic address initializers and actual string-storage sharing. |
| Initialization and TLS | Preserve per-TU order, explicit program-level cross-TU policy, local-static guards, thread storage and finalization. Initialization failure/retry and teardown order require runnable tests. Process-wide state is default; independent instances are a later explicit mode. |
| C++ lifetimes | Keep lowered normal destructor calls and implement actual exceptional cleanup actions, conditions, counts and storage. No reconstruction from lexical scopes and no replacement by GC, finalizers or blanket `using`. |
| C++ dispatch and templates | Emit concrete EDG instantiations, thunks, ctor/dtor variants, actual vtable/VTT data and lowered pointer-to-member operations. Do not generate C# generics or nominal virtual dispatch by reinterpreting C++ declarations. |
| EH and RTTI | Implement retained throw/catch/rethrow, type information, cleanup state, object ownership and termination contracts. Support table-based/tableless profiles deliberately; do not fabricate absent state numbers or match types by names. Native C++ unwinding across the host boundary is a separate capability. |
| Extensions | Inventory actual builtins, atomics/volatile operations, vectors, inline assembly, GNU/MS forms and residual coroutine operations. Implement required contracts or diagnose unsupported families. Successful EDG parsing is not a claim that every form already has a C# implementation. |

Unspecified source evaluation order permits a documented choice only where EDG
has not imposed an order; preserve all recorded constraints. The translator does
not reinterpret undefined source behavior to manufacture a passing comparison.

## Function addresses, varargs and host boundaries

These are early design/fixture gates, not deferred details discovered after
SQLite emission. A managed function pointer and a native ABI callback are not
interchangeable just because both occupy a pointer-sized field.

Before broad emission, establish direct/internal indirect/native callback
conventions, stable function-address identity, aliases, callbacks stored in
records, external function addresses and aggregate parameter/return adaptation.
Use ordinary methods for readable bodies and explicit wrappers/thunks where
required. Do not cast a mixed set of managed and unmanaged addresses and assume
one calling convention. C++ exceptions must remain on a supported generated
execution path; annotating every method as a native callback is not an EH design.

Translated variadic functions require an explicit argument-pack/`va_list`
implementation for the captured ABI, including promotions, `va_copy` and nested
forwarding. Native variadic imports need tested fixed-signature adapters or an
equivalent supported ABI mechanism. A C# `params object[]` rewrite does not
preserve native variadic storage. Test both translated and native boundaries.

Inventory every external function and variable with signature, calling
convention, address exposure, ownership, error/`errno`, thread and callback rules.
Memory/string/math helpers can be C# where equivalent. OS locking, files, mapped
memory, threads, clocks and dynamic loading require explicit adapters. Native
shims are limited to reviewed ABI/system integration; they cannot contain
translated application bodies or hide unsupported C++ execution.

Microsoft documents [unsafe pointer support](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/unsafe-code),
[native AOT interop](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/interop)
and [restrictions on native-callable methods](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.unmanagedcallersonlyattribute?view=net-10.0).
Use the pinned C# 14 toolchain, not preview syntax from evolving documentation.
NativeAOT exports need explicit entry wrappers in the published assembly.

## Multi-unit assembly

Maintain separate captured-entity, emitted-declaration and selected program
storage/function identities. Internal-linkage names remain TU-local. External
binding uses captured linkage, explicit assembly/mangled names and definition
facts, not source-name matching, overload resolution or newly generated mangling.
Detect repeated artifact identities before definition selection, including copies
of one immutable object. Diagnose duplicate inputs by default; an explicit
deduplication option may admit one copy. Independently compiled objects remain
distinct even when their bytes or source names are similar.

Start with strong definitions, declarations and TU-local storage. Extend with
fixture-proven common/weak/alias/inline/COMDAT policies and actual alternate-entry
relationships. Reject unsupported selection cases explicitly. Do not silently
choose the first strong definition or merge types because their names/layouts
match. Cross-unit declarations requiring one C# representation use verified
linkage/ABI adapters, not a new C++ ODR analyzer. Report every selection.

Variable definition classification must use recorded storage/definition facts;
an absent initializer does not imply an external declaration. `IsDefined` on a
function likewise does not guarantee an executable body. Distinguish discarded
or metadata-only bodies. Aliases share actual selected storage/address identity.
Archive extraction, dynamic interposition, ifunc/multiversion selection and
native C++ library interoperability are explicit later capabilities unless a
current target requires them sooner.

## Readability contract

Retain source names for functions, fields, arguments and locals when available;
escape C# keywords and resolve collisions deterministically. Qualified context,
specialization/variant information and a short stable suffix resolve ambiguity.
Keep generated ABI names honestly when no source name exists. Use named record
fields/accessors rather than repeated raw offsets wherever possible.

Emit conventional expressions and statement formatting, minimal necessary
temporaries, and small named helpers for unavoidable semantic differences.
Retain control flow where possible; avoid numbered variables everywhere,
per-operation runtime dispatch and broad method-wide state machines. Split
amalgamation output by declarations/size; source-file grouping is optional.
Large-method compiler limits need a separate semantics-preserving strategy;
splitting a text file alone does not solve them.

Readability acceptance reviews representative emitted functions alongside the
native fixture or available source. Source input is for comparison, never a
required translation input or semantic fallback. Missing original signatures,
virtual-call declarations/defaults, macro spelling and comments stay missing.

## Scale, diagnostics and restartability

Open a bounded number of units, stream declaration summaries to a translator-owned
disk index, and emit one body/chunk at a time. Avoid whole-workspace graph loads.
Reader caches do not bound translator IR, public materialized lists or the
identity set used by descendant traversal. Measure index, body, cache and output
costs separately; include C# build/publish resource use in acceptance.
Before scale acceptance, fix numerical memory, output-size and concurrency budgets
in the manifest and compare measured use with them. Exceeding a budget leaves an
incomplete attempt or an explicit failed resource gate, not a bounded-scale pass.

Public entity equality survives reopen, but the current API has no public opaque
reopen handle. Initially use translator-owned declaration ordinals and sequential
remapping while each unit is open. If scale requires random reopening, propose a
narrow opaque-handle reader addition with focused tests; do not use reflection,
internal access or raw SQL against `.roslynobj` as an alternate semantic API.

Persist phase completion, per-unit input/version/configuration keys, selected
definitions, output hashes and diagnostics. Cancellation leaves an explicitly
incomplete resumable attempt, never a falsely complete generated library.
Keep original artifacts immutable. A local index is disposable build state.

Diagnostics distinguish unsupported translator/runtime features, unresolved
program bindings, unsupported target profiles, and proven exporter/model gaps.
Include input identity, retained function/name, operation family and a stable
translator-owned locator; add source positions only when available. Ordinary
unsupported features do not imply EDG is wrong. For an actual missing live EDG
fact, isolate a source fixture and correct only our backend/model under the
versioning contract. Irrecoverable stock-lowering losses stay documented limits.

## Work sequence and ownership

The detailed gates are in [ACCEPTANCE.md](ACCEPTANCE.md). Implement in order:
contract/inventory; small runnable unsafe C/C++; storage, calls and multi-unit
assembly; SQLite complete emission and real SQL; full configured SQLite feature
execution; broader C++ runtime and additional platform/scale support.

After approval, use a `gpt-6.1-sol` **high** coordinator and `gpt-6.1-sol`
**xhigh** implementers. Planning reviews use `gpt-6-astra` **high**. The
coordinator owns shared contracts, integration, build scheduling, coverage ledger,
restart state and commits. Assign disjoint program/indexing, projection/emission,
runtime/interop and acceptance work, in waves within available concurrency.

Commit each verified milestone using exact owned paths. Preserve unrelated work.
Each gate requires implementation plus execution/readability evidence and an
explicit remaining-feature ledger. Documented deferrals never count as completed
features. SQLite success does not complete all C++ or libc support. Changes to
the accepted representation or native-shim scope return to design review; routine
implementation choices within this contract do not require repeated permission.

## Local design evidence

- [Reader contract](../../roslyn/managed/README.md) and
  [actual workspace](../../roslyn/managed/RoslynObj/RoslynWorkspace.cs): separate
  immutable TUs, selected signatures, metadata/execution separation.
- [Operations](../../roslyn/managed/RoslynObj/Operations.cs),
  [layout](../../roslyn/managed/RoslynObj/LayoutMetadata.cs),
  [lifetimes](../../roslyn/managed/RoslynObj/Lifetimes.cs) and
  [routine ABI metadata](../../roslyn/managed/RoslynObj/RoutineMetadata.cs).
- [Target profile](../../roslyn/managed/RoslynObj/ExactValues.cs) and
  [entity ownership/identity](../../roslyn/managed/RoslynObj/RoslynObject.Foundation.cs).
- [Approved lowering boundary](../../roslyn-design/lowering-and-integration.md),
  [retained-data limits](../../roslyn/docs/LIMITS.md),
  [SQLite input](../../roslyn/docs/SQLITE.md) and
  [DuckDB core corpus](../../roslyn/docs/DUCKDB.md).
