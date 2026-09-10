# Standalone Roslyn condition inlining

`DotCC.PostProcess` is an explicit tool for an already emitted .NET 10 C# project.
Run it after dotcc finishes its normal emission/linking/build actions. It has no
compiler or SQLite build hook, and introduces no Roslyn dependency into
`DotCC.Lib`, the dotcc executable, or translated applications. The tool references
the Roslyn assemblies shipped with the .NET 10 SDK used to build it.

From the repository root:

```sh
dotnet build DotCC.PostProcess/DotCC.PostProcess.csproj -c Release
dotnet DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll \
  sqlite/generated/TranslatedSqlite/TranslatedSqlite.csproj \
  --output sqlite/generated/sqlite-optimized

dotnet build sqlite/generated/sqlite-optimized/Optimized/TranslatedSqlite.csproj -c Release
```

The input project's dependencies must already be restored/built. `--configuration`
defaults to `Release`. The output directory must be new and separate from the
input directory, including through symlinks. The original sources are preserved;
failed processing leaves no partial output directory.

The result contains an `Original/Original.csproj` comparison project, an
`Optimized/<assembly-name>.csproj` optimized project, copied metadata/runtime
references and resources, and `manifest.json`. Consumers can reference either
project normally, including for NativeAOT publishing. Both freeze the evaluated
input configuration: building the snapshot with another configuration does not
change its preprocessor symbols or checked/unsafe settings. The manifest records
input hashes, compiler arguments, output hashes, rewrite counts and skip reasons.
It is deterministic for identical evaluated inputs.

## Tree and semantic analysis

The tool evaluates MSBuild's compiler inputs with compiler execution disabled,
including linked sources, generated usings and SQLite's managed VFS sidecars.
Roslyn binds this complete compilation. The rewriter identifies the actual global
`Cond.B` method symbol and proves its helper body implements the known truth
conversion. A name filter only avoids binding unrelated calls; it never authorizes
a rewrite by itself. Helper type initializers, fields and changed implementations
prevent unsafe elimination of a call.

Replacements are syntax nodes, with explicit parentheses and retained operand
trivia. The resulting trees are serialized to source; there are no textual
search-and-replace substitutions or whole-file formatting passes. Original and
rewritten compilations must both pass semantic validation before output is saved.
The semantic rewrite is idempotent.

| Bound argument | Replacement behavior |
| --- | --- |
| `bool` | Use the boolean expression directly. |
| Signed/unsigned integers, including `nint`/`nuint` | Compare against zero. |
| `float`/`double` | Compare against zero, preserving NaN and signed-zero behavior. |
| Pointer/function pointer accepted by the `void*` overload | Preserve conversion and compare against null. |
| `CBool` value | Preserve its conversion to int and compare against zero. |
| Explicit `CBool` cast inside `Cond.B` | Remove the normalize/read pair only when its constructor and conversion bodies are proven safe. |

For example, `Cond.B((CBool)(a < b))` reduces to the comparison, while a CBool
assignment elsewhere keeps its 0/1 normalization. Both the namespaced library
runtime and dotcc's embedded global-namespace CBool are recognized structurally.
Selected argument conversions, target-typed expressions, checked contexts,
evaluation count and short-circuit behavior are preserved. An inserted explicit
cast is checked against the original selected user conversion; a different
operator causes that call to be retained.

The tool conservatively retains calls in expression trees/query expressions,
caller-argument text captures, discarded statement-expression positions, or
across preprocessor directives. Unknown helper implementations are retained too.
Skip diagnostics include the original source location. CBool stores, arithmetic,
API signatures and the helper definitions themselves are not rewritten.

## Supported project inputs

The first version supports a single `net10.0` target, ordinary executable/library
output, embedded resources, evaluated assembly references and copy-local managed
implementation assemblies. It preserves caller-file paths through directory
mapping, portable debug information, source embedding and compiler settings.
SDK analyzers are omitted from the snapshots and documented in the manifest;
SDK generator-trigger attributes and custom analyzers/configurations are rejected.

Inputs using explicit response files, signing, netmodules, linked resources,
Win32 resources or unsupported compiler switches are rejected with a diagnostic.
Snapshot projects use response files internally, so feed the original emitted
project to a new invocation instead of feeding a snapshot back to the CLI. The
pure tree rewriter itself can be applied repeatedly without additional edits.
This is not a general MSBuild project migration tool or a Rider analyzer.

## Verification

```sh
dotnet test DotCC.PostProcess.Tests/DotCC.PostProcess.Tests.csproj -c Release
python3 DotCC.PostProcess.Tests/cli-smoke.py \
  --tool DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll
python3 sqlite/scripts/test-postprocess.py \
  sqlite/generated/sqlite-optimized --aot --corpora
```

The SQLite gate requires the ordinary campaign's emitted core/API/JSONB/FTS5
projects and native VFS oracle to exist already. It compares original/optimized
managed consumers, threading and mmap/WAL contracts with native-process
interoperability, then compares the four executable corpora with committed native
baselines, under JIT and NativeAOT. A repeated prepared-query benchmark records
runtime and managed allocations; build timings and source/assembly sizes are
saved under `sqlite/artifacts/postprocess/`. Timings are observations, not pass
thresholds or a promised speedup: JIT/AOT may already inline the original helpers.
See [SQLite validation](../sqlite/docs/validation.md) for measured results.
