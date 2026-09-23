# CLI surface (clang-shaped)

> Extracted from CLAUDE.md (2026-07-07) — the full flag reference. CLAUDE.md keeps a
> one-line summary and points here.

| Flag | Meaning |
|---|---|
| `dotcc <a.c> <b.c>` | Compile translation units (whole-program). Default: write `DotCcProgram.cs + dotcc-out.csproj` to `./a.out-cs/`. **`.cs` inputs are object fragments → linked** (see `--emit=obj`). |
| `dotcc zig test <f.zig>` | **Subcommand** (zig-CLI-shaped). Compile the input's `test "…" {}` blocks and **run** them (like `zig test`): each block lowers to an `anyerror!void` function, and a generated entry point runs each, printing `OK`/`FAIL` per test + a summary, exiting non-zero if any fails. `main` is ignored. Assertions: curated `std.testing.expect`/`expectEqual`. Emits a self-contained file-based program and runs it via `dotnet run` (CLI-tool territory, like `--emit=build`). The harness for running real `std` tests from source (road-to-zig-std). Other `zig` subcommands (`build`/`cc`/`build-exe`/`run`) are future scope. |
| `-o <path>` | Output: a directory for csproj/build, a file for `file`/`obj`. **Inferred when omitted** (`obj` → `<src>.cs`, csproj/build → `./a.out-cs/`, file → stdout). |
| `--emit=file` | Single .NET 10 file-based program (`#:property AllowUnsafeBlocks=true`). To `-o <file>` if given, else stdout. |
| `--emit=csproj` | Default — `DotCcProgram.cs` + paired csproj to `-o` dir. |
| `--emit=build` | As `csproj`, then run `dotnet build -c Release` in the output dir. |
| `--emit=managedlib` | Emit a reusable managed library with public functions and aggregate types. Add `-c` to compile it. |
| `--instance-methods` | Opt-in instance methods, owner storage and explicit-owner function pointers. Select at object emission and managed-library link (`--runtime=c`); incompatible object conventions are rejected. See [instance translation](instance-translation.md). |
| `--overrides-file <path>` | Load a strict version-1 translation profile: macro selectors/templates and `fieldTypeNames` for stable anonymous struct/union names selected through fields. See [translation overrides](macro-overrides.md). |
| `--override-macro NAME=BODY` | Replace each active definition of NAME, preserving its signature. Repeat for distinct names; replaces same-name profile rules and conflicts with `-DNAME`. |
| `--override-report <path>` | Write JSONL selection/expansion/provenance diagnostics for macro overrides, field type names and runtime intrinsics. |
| `--class-name <name>` | Set the generated API class for `--emit=managedlib` or `-shared` (default `DotCcLib`). Applies to whole-program emission and object linking; specify it at link time, not with `--emit=obj`. Accepts a single ASCII identifier, optionally `@`-escaped; keywords are escaped automatically. Executable, preprocessing and WAT modes reject this option. |
| `--namespace <name>` | Place generated functions, aggregate types and embedded runtime in a dotted C# namespace. Default: global namespace. Supported by C# executable/library output and object linking; rejected with preprocessing, WAT and object emission. Specify at link time for objects. |
| `--nest-types` | Place translated types, globals/cache helpers and embedded runtime inside the library wrapper. Supports managed/shared libraries and object linking. Split output uses file-local aliases so differently named translations can share a consumer namespace. |
| `--runtime=all\|c\|auto` | Select embedded runtime modules. `all` (default) retains everything; `c` excludes Zig-only modules and requires known C-only inputs; `auto` uses source/object language provenance (unknown old objects retain all). Set at link time for objects. |
| `--deduplicate-inline` | Merge provably equivalent static inline definitions and redirect their bound calls. Set at link time for objects. C function addresses and distinct static dependencies prevent merging. |
| `--export-inline <pattern>` | Expose selected inline definitions under their original C names. Repeatable case-sensitive exact names or `*` / `?` globs; quote globs. Missing matches, ambiguous definitions and name collisions are errors. Set at link time for objects. |
| `--split=none\|function\|size` | C# project source layout: one file (default), one translated function per file, or groups of whole functions. Supported by csproj/build/managedlib/shared emission and object linking. File/stdout, preprocessing, WAT and object emission reject splitting. |
| `--split-size <bytes>` | Positive UTF-8 byte target with `--split=size`, default 262144 (256 KiB). Append a complete function, then close the file on its first crossing of the target. |
| `--emit=obj` | **Separate compilation.** Compile ONE `.c` to a `.cs` object fragment (functions + its type decls + globals, no shell/runtime). Link by passing `.cs` objects back: `dotcc a.cs b.cs -o app` merges (deduping shared types) and wraps in the shell. Drives CMake/make per file (`examples/cmake-demo/`). |
| **`-o` ⇄ `--emit` inference** | When one is omitted it's inferred: `-o foo.cs` ⇒ `file`; `-o <dir>` ⇒ `csproj`; `--emit=obj` with no `-o` ⇒ `<src>.cs`. Explicit `--emit` wins; `obj` is never inferred. |
| `-E` | Preprocess only — dump the post-`#include`/`#define` token stream to stdout. No parsing. |
| `-I <dir>` | Add header search dir. Repeatable. Auto-includes each `<input>.c`'s directory. |
| `-D NAME[=VALUE]` | Predefine a macro. Repeatable. `=VALUE` is lexed through the same byte lexer as the parser; bare `NAME` is a defined-as-marker (empty body). |
| `-MD` / `-MMD` | **Header-dependency file** (`Compiler.EmitDependencyRule`): a Make rule listing the TU + every transitively-`#include`d header, so CMake/Ninja/Make recompile on header change. `-MMD` drops angle headers; synthetic system headers (no disk path) are always omitted. The scan honors `#if`/`#ifdef`. Paths normalized to `/`, make-special chars escaped. |
| `-MF <file>` | Dependency-file output path (defaults to the object/source name with `.d`). |
| `-MT <target>` | Dependency rule target name(s). Repeatable. CMake passes `-MT <OBJECT>`. |
| `-c` | Compile to .NET assembly (no native publish). Clang-shaped alias for `--emit=build`. |
| `-shared` | Shared library: csproj with `<NativeLib>Shared</NativeLib>` + `<PublishAot>true</PublishAot>`; non-static C functions exported via `[UnmanagedCallersOnly(...CallConvCdecl...)]`. `main` not required. Run `dotnet publish -c Release -r <RID>` for the native artifact. |
| `-l<name>` / `-L<dir>` | **Import mode** — implicit, linker-style consumption of a prebuilt native library. A *called* prototype with no body in any TU and **not** from a synthetic system header is bound at startup to the first `-l` library exporting it. GOT-style (NOT `[DllImport]`/`[LibraryImport]`): emits a `DotCcImports` table of `delegate* unmanaged[Cdecl]<…>` fields named like the C functions (`using static` → call sites unchanged), bound by `__BindAll()` (before `main`; static cctor under `-shared`) via `NativeImports.LoadLibrary`/`TryResolveExport` over `NativeLibrary` — `-L` dirs probed with platform name variants, ld.so first-wins, `DllNotFoundException` on a miss. Works under `dotnet run` AND NativeAOT. Discriminator = the **synthetic line band** (`SrcPos.SyntheticLineBase = 1<<20`): a synthetic-header proto lexes in the band → `Symbol.FromSystemHeader` → runtime-provided, never imported. Both `-l name`/`-lname` and `-L dir`/`-Ldir` forms parse. V1 cuts (warn + skip): variadic, `extern` data, global-name collision, address-of of an import. Static `.a`/`.lib` archives + separate-compilation `-l` are not built yet. |
| `-std=<dialect>` | `c90`/`c99`/`c11`/`c17`/`c18`/`c23`. Default `c17`. Sets `__STDC_VERSION__` (omitted for `c90`) and drives rule-2 keyword promotion. **Alone it stays permissive** — the parser is dialect-agnostic. `c89` is omitted for the canonical `c90`; no `gnu*` variants (dotcc implements no GNU extensions). |
| `-pedantic` | Opt into **dialect rejection**: features newer than the selected `-std=` warn to stderr but still compile. Maps `CDialect.Version` against a per-feature introduced-year table (`DialectGate`, emit pass only). Per-feature gate list lives in `C-SUPPORT.md`. `//` comments are intentionally NOT gated. |
| `-pedantic-errors` | Like `-pedantic` but violations are **errors** — collect-all, exit non-zero with every violation listed. |
| `-Wconversion` | Opt-in (off by default; like gcc/clang `-Wconversion` / MSVC C4244): warn on an implicit integer conversion that **narrows** (wider → narrower) at init/assignment/return. dotcc inserts the cast regardless; the flag only controls the warning (`ConversionGate`). A constant that fits is neither cast nor flagged. Same-width sign changes are NOT flagged (that's `-Wsign-conversion`, out of scope). |
| `-Wno-discarded-qualifiers` | Suppress the **on-by-default** warning for an implicit conversion that discards a pointee `const` (passing/assigning/initializing/returning a `const T*` where a `T*` is expected; gcc `-Wdiscarded-qualifiers`). An explicit cast is already exempt. Does **not** affect the write-to-const **error** (a constraint violation, not suppressible). The check lives in `IrBuilder` (`CheckQualifierDiscard`, gated by `WarningFlags.DiscardedQualifiers`); const-correctness is on by default with no `-std=` gate (`const` is C89). |
| `-Wimplicit-fallthrough` | Opt-in (off by default; like gcc/clang, which need `-Wextra` or an explicit opt-in): warn on a **non-empty** `switch` case that falls through to the next label without a C23 `[[fallthrough]];` marker — gcc-verbatim `this statement may fall through`. This is what gives `[[fallthrough]]` a job: dotcc's switch lowering already synthesizes the `goto case`, so the attribute carries no codegen — it only suppresses this warning. The check lives in `IrBuilder` (`CheckImplicitFallthrough`, gated by `WarningFlags.ImplicitFallthrough`); it fires exactly when the lowering would synthesize an implicit `goto case` and no marker excused it, and a `[[fallthrough]];` leaves a `FallthroughMarker` IR node so it can tell intentional from accidental. A `/* fall through */` comment does NOT suppress it (dotcc strips comments in the preprocessor). |
| `-fsanitize=address` | Opt-in checked **debug heap** (a heap-only subset of clang's ASan): routes the emitted program's `malloc`/`calloc`/`realloc`/`free` through a `[magic\|size]`-header + trailing-redzone block layout, so `free` flags a bad/double free or a write-past-end with a managed stack trace at the call site. The shell calls `Libc.EnableDebugHeap()` once before `main`. `DOTCC_DEBUG_HEAP=1` is the no-recompile runtime override; `DOTCC_DEBUG_HEAP_SCAN=1` adds a full live-block redzone sweep on every alloc/free (catches an overflow into a never-freed block, e.g. a guest GC arena). Other `-fsanitize=` kinds warn and are ignored. Off by default — one inert branch in malloc/free. |
| (no inputs) | Error and exit non-zero — same as clang. |

**Predefined macros** (seeded every compile, plus any `-D`): `__STDC__`=`1`, `__STDC_HOSTED__`=`1`, `__STDC_VERSION__`=per-`-std=` value (undefined under `c90`), `__dotcc__`=`1` (compiler id, like `__clang__`), and the **LP64 data-model trio** `__LP64__`=`1`, `__SIZEOF_POINTER__`=`8`, `__SIZEOF_LONG__`=`8` — dotcc IS an LP64 compiler (`long` → C# `long`, 8-byte pointers), and portable C (chibi-scheme's `SEXP_64_BIT`) decides pointer-tagging strategy from exactly these macros; without them it would mis-configure for 32-bit and miscompute at runtime.

**Library mode (`-shared`) emit shape:** user functions land in `internal static class DotCcLib` so inter-function calls resolve as direct C# invocations (`[UnmanagedCallersOnly]` prohibits managed call sites). Each non-static C function gets a `public static` wrapper in `public static class DotCcExports` annotated `[UnmanagedCallersOnly]`; NativeAOT inlines the trampoline. C `static` functions stay internal (no wrapper). Varargs functions are skipped from exports (`params object[]` isn't a valid `UnmanagedCallersOnly` signature).

## Generated namespaces

```sh
dotcc engine.c --emit=managedlib --class-name Sqlite --namespace Managed.Database --split=function -o TranslatedSqlite
```

The API is `Managed.Database.Sqlite`; translated types such as `sqlite3`, `CBool`,
and `DotCcFunctionPointers` live in the same namespace. All split function files
share it, while filenames remain `Sqlite.<function>.cs`. Namespace components
must be ASCII C# identifiers; keywords are escaped automatically. Without this
option, types remain in the global namespace. The project’s `RootNamespace`
reflects the supplied namespace too.

Executable output uses an explicit namespaced entry class when needed, retaining
its stack/thread and argv behavior. Object fragments remain independent of the
final namespace: symbolic aliases bind at link time. Older objects must be
regenerated before namespaced linking; ordinary global-namespace linking remains
supported. This does not rewrite C strings or require Roslyn in dotcc.

Use `namespaceName: "Managed.Database"` on `EmitCSharp`, `EmitCSharpFiles`,
`LinkObjects`, `LinkObjectFiles`, and `BuildGeneratedCsproj`. Consumers can import
`using Managed.Database;` and `using static Managed.Database.Sqlite;`.

## Splitting generated C#

```sh
dotcc engine.c --emit=managedlib --class-name Sqlite --split=function -o TranslatedSqlite
dotcc engine.c --emit=managedlib --class-name Sqlite --split=size --split-size=262144 -o TranslatedSqlite
```

All translated methods belong to the same partial class (`DotCcProgram` for
executables or the configured library class). `{class_name}.cs` retains entry-point
wiring, runtime, types, globals and initialization so field initialization order
is preserved. Canonical pointer aliases are emitted once in
`{class_name}.GlobalUsings.g.cs`. Function files use the owning class name:
`Sqlite.00000.cs` in size mode, or `Sqlite.sqlite3_open.cs` in function mode.
Only filename collisions receive a numeric suffix, such as `Sqlite.open.2.cs`;
comparison is case-insensitive for portability. C# identifier escapes (`@`) are
omitted from filenames. For the examples above, the shared files are `Sqlite.cs`
and `Sqlite.GlobalUsings.g.cs`; unsplit output also uses `Sqlite.cs`.

Size mode counts UTF-8 bytes including the file header and closing braces. The
threshold is a grouping target, not a hard limit: a whole function can exceed it,
and the final group can be smaller. The shared `{class_name}.cs` and alias file are
not subject to this target. No C# parsing or Roslyn dependency is added to dotcc;
the backend supplies function boundaries. Newly emitted objects preserve those
boundaries; old objects still link normally but must be regenerated for splitting.

`Dotcc.SourceFiles.txt` records generated files. Re-emission removes obsolete
listed files, including when returning to `--split=none`; unlisted sidecars are
preserved. Keep this manifest with the output directory.

Library callers can use `Compiler.EmitCSharpFiles` or `Compiler.LinkObjectFiles`
with `split: SourceSplit.Function` or `SourceSplit.Size` and `splitSize: 262144`.
They return filename/source dictionaries; `Compiler.WriteCSharpFiles` writes them
and handles obsolete files. Existing string-returning APIs retain single-file
behavior. The separate postprocessor evaluates all generated Compile inputs, so
its `--in-place` mode also works with split output.

Generated C# files disable warnings `CS0162`, `CS8909`, `CS1717`, `CS0164`,
`CS0642`, and `CS0675` with a file-scoped `#pragma warning disable` directive.
The primary source file containing types additionally suppresses `CS8981`
(lowercase type names). Split function and alias files retain the common list.

## Isolating copied translations

```bash
dotcc engine.c --emit=managedlib --class-name Sqlite --namespace Managed.Database \
  --nest-types --runtime=c --split=size --split-size=102400 -o TranslatedSqlite
```

Types become `Managed.Database.Sqlite.sqlite3`, `Sqlite.Libc`,
`Sqlite.SqliteGlobals`, and `Sqlite.SqliteFunctionPointers`. Import nested API
types with `using static Managed.Database.Sqlite;`. Different wrappers may be
copied into the same consumer project/namespace. Each owns its runtime state.
Nesting applies to generated helpers too, including layout and inline-array types.
The platform sidecars supplied by a translation campaign are maintained separately.

With nesting, aliases are file-local; no `{class_name}.GlobalUsings.g.cs` is needed.
Function-owner aliases appear only in the shared source; function files carry
just the aliases they need. Without nesting, the existing global-using layout
remains. Compiler APIs accept `outputOptions: new CSharpOutputOptions(NestTypes:
true, Runtime: RuntimeProfile.C)` on `EmitCSharp`, `EmitCSharpFiles`, `LinkObjects`
and `LinkObjectFiles`.

C mode omits `Zig*.cs` and `Slice.cs`; compilation tests verify that the complete
remaining C runtime has no dependency on those modules. Auto mode retains Zig
support if any input/object needs it. C mode rejects unknown-provenance older
objects; regenerate them or use all/auto. Preprocessor/WAT modes reject these C#
output flags. The postprocessor also supports nested Cond/CBool helpers.

## Inline function deduplication and managed exports

```sh
dotcc --emit=managedlib --deduplicate-inline \
  --export-inline CxPlatEwma --export-inline 'QuicAddrGet*' \
  --nest-types --class-name MsQuic --runtime=c objects/*.cs -o TranslatedMsQuic
```

Both options also work when translating C sources together. Object compilation
always records inline metadata; pass these options at the final link, and
regenerate objects produced by older compilers. The library API exposes the same
controls through `CSharpOutputOptions(DeduplicateInline: true,
ExportInline: new[] { "CxPlatEwma", "QuicAddrGet*" })`.

`--deduplicate-inline` compares typed IR, including signatures, constants,
aggregate type identities, local bindings, control flow and bound function
dependencies. Complete aggregate layouts are checked separately by the linker;
an unrelated forward declaration becoming complete does not change a function.
It merges equivalent definitions of the same original name, including recursive
groups. Calls are redirected through explicit symbol relocations. Differing
macro expansions, mutable static storage, unmerged static callees, C address uses, and
IR nodes without an equivalence proof keep their definitions separate. This is
conservative code sharing, not an attempt to prove arbitrary programs equivalent.
Value reads from `static const` scalars and pointer-free aggregates can also
match: their typed initializers participate in the proof. This allows readers
of an already shared constant to share a body. Volatile/atomic data, thread-local
objects, pointer-bearing constants, address uses and array decay remain excluded.
This does not repair the existing per-translation-unit mutable-global storage
limitation; retaining separate methods does not establish separate global storage.

Externally linked definitions and non-inline functions are not deduplicated.

`--export-inline` gives one selected definition its original name, for example
`MsQuic.QuicAddrGetPort(...)`. With deduplication enabled, it is the shared body;
without it, other per-unit bodies remain. Selection requires either a single
definition or one proven equivalent group. Ambiguous selections fail with a
suggestion to provide a C wrapper in the desired translation unit. Exporting a
single address-used function preserves its existing canonical pointer cache.
Managed exports do not add native entry points to a shared library.

When either option is enabled, inline functions receive pointer-cache properties only
for actual C address uses (including global initializers and uses from other
objects). Managed callers can invoke the methods directly. Defaults remain
unchanged when neither option is selected. Unambiguous, proven static inline groups receive their original names
automatically. Conflicting names and unproven groups retain their existing names.
`--export-inline` pins a requested managed name: it diagnoses ambiguity instead of
falling back to unit names when new source units are added. Visibility is unchanged.

## Extending copied translated structs

Generated structs are `partial`, including union representations and anonymous
aggregate/inline-array helper structs. With `--nest-types`, the enclosing API
class is partial even when output is not split. Additional partial declarations
can supply methods such as `Equals`, `GetHashCode` and `ToString` in the same
namespace and containing type, compiled together with the generated sources.
Methods add no storage; the generated C layout remains authoritative.

Promoted mutable struct/union members use `[UnscopedRef]` ref-returning
properties. For example, `settings.IsSet.PeerUnidiStreamCount = 1` updates the
embedded flags without a read-modify-write temporary. Const/volatile/atomic
restrictions and bit-field value accessors remain in effect. Rebuild object
fragments and regenerate source to pick up the new declarations.

## Optional source post-processing

After dotcc finishes, the separate [Roslyn post-processor](postprocess.md) can
rewrite source files with `--in-place` or produce original/optimized project snapshots. It uses semantic syntax-tree
rewrites for Cond.B followed by standalone empty-block cleanup. The SQLite emission
script runs it in place by default (`--no-postprocess` skips it). The compiler
itself does not invoke it, and no Roslyn runtime dependency is added.

For in-place IDE edits, the separate [Rider analyzer/code fix](postprocess.md#rider-in-place-fixes)
offers the same transformations as quick-fixes with Fix All. This does not add
a dotcc CLI flag or automatically edit emitted sources.

For example, `dotcc --emit=managedlib --class-name Sqlite engine.c -o TranslatedSqlite`
emits `public static class Sqlite`. The corresponding APIs are
`Compiler.EmitCSharp(..., emit: EmitMode.ManagedLib, className: "Sqlite")` and
`Compiler.LinkObjects(..., emit: EmitMode.ManagedLib, className: "Sqlite")`.
The selected name is used in declarations, static imports, function-owner aliases
and native export wrapper calls. Globals and canonical pointer containers are named `{class_name}Globals` and
`{class_name}FunctionPointers`. The pointer properties are consolidated into one
class. Each getter lazily caches its method address using the C# `field` keyword,
with a null check and assignment and no synchronization.
If names such as `foo` and `get_foo` would collide with C#'s generated getter
names, the container uses abstract base classes to keep both static properties
accessible through the same container under their original names.
Functions whose declarator names come from function-like macro expansion get
pointer properties only when their addresses are used by translated C code. Ordinary
functions retain automatic public pointer properties; macro expansion in a return
type or function body alone does not suppress a property.
Aggregate names, assembly names and native export entry-point names stay unchanged. Choose a name that does not conflict with translated symbols or runtime
helper types; infrastructure and translated-declaration collisions are diagnosed.

## Public macro constants

C# output now includes `public const` fields on the generated API class for
object-like string and numeric macros from source/user headers and explicit
`-D` options. For example, SQLite exports `Sqlite.SQLITE_CHECKPOINT_TRUNCATE`
and `Sqlite.SQLITE_VERSION`. Function bodies still use preprocessed values.

The frontend fully expands object/function macro wrappers, aliases, argument
prescan/rescan, stringification, token pasting and variadic replacements before
using the C parser and typed constant evaluator. The evaluator resolves the
translation unit's typedefs, enum constants, `sizeof`, `_Alignof`, and `offsetof`,
as well as arithmetic, bitwise, comparison and conditional expressions. Integer
widths/signedness and float suffixes are retained. Numeric/string/enum values
become `public const`; integer-cast pointer sentinels and C `_Bool` (`CBool`)
values become `public static readonly`. For example, MsQuic's `QUIC_STATUS_*`
values wrapped in `QUIC_STATUS_DEF(...)` are discovered automatically.
Ordinary UTF-8 C string literals become C# strings, without an implicit terminal
NUL; explicit embedded NULs remain. Synthetic system-header macros and seeded
compiler built-ins are not exported unless explicitly defined or selected by the caller.

Function-like, empty, undefined, contextual (`__LINE__`/`__FILE__`), nonconstant,
non-UTF-8 string and unsupported replacement lists are omitted. Discovery uses
a separate evaluator so unused macros cannot add imports, diagnostics, or change
the generated program's storage. Calls are never executed; unevaluated operands
such as `sizeof(declared_function())` can still yield constants. Expansion is
bounded to 8,192 visited tokens, 1 MiB of visited token text, and 128 nested disabled macros; replacement
lists over 256 tokens and declaration/statement bodies are omitted.
Macro names colliding with emitted members/types
or the API class are omitted too. Across translation units/objects, identical
fields coalesce and conflicting definitions are omitted rather than picking one
translation unit's value. Constants remain in `{class_name}.cs` for every split mode.

Use repeatable `--emit-define` selectors to export additional object-like macros:

```bash
dotcc --emit=managedlib --emit-define 'QUIC_STATUS_*' --emit-define MY_HANDLE_INVALID source.c -o generated/
```

Each selector matches the entire, case-sensitive macro name: an exact name,
`*` for any sequence, or `?` for one character. Quote patterns to prevent shell
expansion. Selection adds to automatic discovery and may include system-header
macros. Active definitions at the end of each translation unit are used, after
macro overrides; undefined macros and function-like definitions themselves are
ignored, so `QUIC_STATUS_*` can include helper names such as `QUIC_STATUS_DEF`.

Selected replacement lists use the same expansion and typed evaluation as
automatic discovery. Selection is useful for system-header macros and for
requiring a named definition to be exportable: an unsupported,
contextual, empty or nonconstant selected definition is a compile error naming
the macro. Collision and cross-unit conflict rules above still apply.

Pass selectors when compiling C sources, including `--emit=obj`; they cannot be
applied to existing objects at link time. The library API accepts the same list:

```csharp
var preprocessing = new CPreprocessingOptions(
    Array.Empty<MacroOverride>(), emitDefines: new[] { "QUIC_STATUS_*", "MY_HANDLE_INVALID" });
var source = Compiler.EmitCSharp(inputs, emit: EmitMode.ManagedLib, preprocessing: preprocessing);
```
