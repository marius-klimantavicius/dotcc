# Roslyn source post-processing

`DotCC.PostProcess` is an explicit tool for an already emitted .NET 10 C# project.
Run it after dotcc finishes its normal emission/linking/build actions. SQLite’s `emit-engine.sh` invokes it as a separate step after emission, using
`--in-place` by default. The compiler itself does not invoke it, and it introduces
no Roslyn dependency into
`DotCC.Lib`, the dotcc executable, or translated applications. The tool references
the Roslyn assemblies shipped with the .NET 10 SDK used to build it. It inlines
proven `Cond.B` calls, then removes standalone empty blocks for readability.
The optional [Rider analyzer and code fix](#rider-in-place-fixes) applies the same
rewrites directly to documents through IDE quick-fixes.

## In-place processing

From the repository root:

```sh
dotnet run --project DotCC.PostProcess -c Release -- \
  sqlite/generated/TranslatedSqlite/TranslatedSqlite.csproj --in-place
```

Choose exactly one of `--in-place` or `--output directory`. Both modes use the
same semantic tree transformations and require restored/built dependencies.
In-place mode changes only evaluated C# source files whose text changes, including
linked files outside the project directory. It preserves encoding, BOM, trivia
and file permissions; unchanged files retain their timestamps. It does not modify
the project file or create a comparison snapshot.

Before replacement, serialized source is parsed and semantically validated again.
Source hashes detect concurrent edits. Replacements are staged beside each source
and applied atomically per file, with rollback on caught failures or cancellation.
This is not a crash-safe transaction across multiple files. If a concurrent edit
prevents rollback, that edit is preserved and the error identifies the retained
backup containing the original source. Repeating the command makes no further edits.

`sqlite/scripts/emit-engine.sh` builds the tool, restores the emitted project, then
processes it in place. `sqlite/scripts/build.sh` subsequently compiles that source.
Use `sqlite/scripts/emit-engine.sh --no-postprocess` for raw dotcc output, including
when creating a meaningful original/optimized comparison or trying Rider fixes.

## Separate comparison snapshots

From the repository root (first emit with `--no-postprocess` for an unoptimized
SQLite baseline):

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
`RemovedEmptyBlocks` counts the empty blocks removed by the cleanup pass.
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

## Empty block cleanup

After condition inlining, a syntax-tree pass removes empty block statements from
method/block statement lists, switch sections and top-level code. It also removes
blocks that become empty after their nested empty blocks are removed. This pass
works even when the input has no `Cond` helper.

Bodies belonging to statements or declarations stay intact: `if (x) {}`,
loops, labeled statements, methods, lambdas, accessors and `try`/`catch`/`finally`
keep their braces. Nonempty blocks retain their scopes. An otherwise empty
top-level program retains one block to preserve its implicit entry point.

Comments, whitespace and line breaks from removed blocks are retained in order,
preserving caller line numbers. Blocks containing preprocessor directives or
disabled source are retained, as are blocks inside captured caller-argument
expressions. The pass does not reformat the whole file or remove blank lines.
Serialized output is compiled again before either output mode saves changes.

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
The standalone command is not a general MSBuild project migration tool. Rider
integration is provided by the separate analyzer/code-fix assemblies below.

## Rider in-place fixes

Build the IDE tooling from the repository root:

```sh
dotnet build DotCC.PostProcess.CodeFixes/DotCC.PostProcess.CodeFixes.csproj -c Release
```

Reload `sqlite/generated/TranslatedSqlite/TranslatedSqlite.csproj` in Rider.
`sqlite/Directory.Build.targets` references the two built assemblies during
design-time compilation, so suggestions appear on the original emitted C#.
Those references are omitted from ordinary builds and standalone postprocessor
evaluation. Set `DotCCDisablePostProcessAnalyzer=true` to disable SQLite's IDE
integration. Build the tooling again after changing it; reload Rider's project
or restart the IDE if it still has the previous assemblies loaded.

Select a suggestion and use **Alt+Enter** to preview/apply its quick-fix:

| Diagnostic | Action |
| --- | --- |
| `DCCPP001` | Inline the proven `Cond.B` call, including supported CBool normalization/read pairs. |
| `DCCPP002` | Remove the standalone empty block, including nested blocks that become empty. |

Each action offers **Fix All in document, project or solution**. Fix All handles
each affected document in one tree pass per rule instead of compiling once per
diagnostic. Run it once for each rule to apply both transformations. A single
fix includes eligible nested expressions/blocks within the selected diagnostic,
while leaving unrelated siblings alone. Analysis and builds only report
suggestions; applying a code action is what edits the source. Regenerating with
dotcc will replace edits to generated files.

Both diagnostics have default severity `suggestion` and intentionally analyze
generated files. They honor standard diagnostic suppression. For example:

```ini
[*.cs]
dotnet_diagnostic.DCCPP001.severity = suggestion
dotnet_diagnostic.DCCPP002.severity = suggestion
```

Rider's **Editor | Inspection Settings | Roslyn Analyzers** setting must be
enabled. JetBrains documents [analyzer references and quick-fix support](https://www.jetbrains.com/help/rider/Using_NET_Compiler_Analyzers.html)
and [file/project/solution scoped Roslyn fixes](https://blog.jetbrains.com/dotnet/2025/02/24/rider-2025-1-eap-5/).
Use these quick-fixes rather than assuming Rider's general Reformat action runs
the analyzer fixes.

For another project, reference both assemblies from the same directory:

```xml
<ItemGroup>
  <Analyzer Include="path/to/DotCC.PostProcess.Analyzers.dll" />
  <Analyzer Include="path/to/DotCC.PostProcess.CodeFixes.dll" />
</ItemGroup>
```

Alternatively, build the local NuGet package and install `DotCC.PostProcess.Rider`
from that directory using Rider's NuGet client. Nothing is published by this command:

```sh
dotnet pack DotCC.PostProcess.CodeFixes/DotCC.PostProcess.CodeFixes.csproj \
  -c Release -o sqlite/artifacts/packages
```

Use `PrivateAssets="all"` on a package reference. The package contains only the
two assemblies in `analyzers/dotnet/cs`, with no application runtime assets or
dependencies. The analyzer targets .NET Standard 2.0 and Roslyn 4.14; the code fix
also uses the IDE's Roslyn Workspaces/MEF services. Neither assembly is a
dependency of dotcc, its compiler library, or translated applications. For
projects with explicit analyzer/package references, remove those references
before feeding the project to the standalone command, whose input contract
still rejects custom analyzers. SQLite's design-time-only wiring avoids this.

The IDE and standalone tool compile the same source files for rewrite proofs,
observable-context checks and empty-block cleanup. Documents with compiler
errors are skipped until bindings are valid. Edits retain trivia and are
serialized/reparsed and semantically checked before being returned to the IDE;
no whole-document formatter is invoked. Source-generator outputs are not editable
workspace documents, so apply these fixes to dotcc's on-disk emitted C#.

## Verification

```sh
dotnet test DotCC.PostProcess.Tests/DotCC.PostProcess.Tests.csproj -c Release
dotnet test DotCC.PostProcess.Analyzers.Tests/DotCC.PostProcess.Analyzers.Tests.csproj -c Release
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

The IDE tests run real diagnostic and code-action APIs, including MEF discovery,
single fixes, all three Fix All scopes, generated files and suppressions. To
check packaging and compare the IDE Fix All output against a standalone SQLite
snapshot (absolute path required for the test environment variable):

```sh
python3 DotCC.PostProcess.Analyzers.Tests/package-smoke.py \
  --package sqlite/artifacts/packages/DotCC.PostProcess.Rider.0.1.0.nupkg \
  --sqlite-project sqlite/generated/TranslatedSqlite/TranslatedSqlite.csproj
DOTCC_POSTPROCESS_SQLITE_SNAPSHOT="$PWD/sqlite/generated/postprocess-empty-blocks" \
  dotnet test DotCC.PostProcess.Analyzers.Tests/DotCC.PostProcess.Analyzers.Tests.csproj \
  -c Release --filter FullyQualifiedName~Sqlite_fix_all_matches_standalone_output
```
