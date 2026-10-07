# Generated C# whitespace formatter

This build-time tool uses the compiler assemblies shipped with the selected
.NET SDK. It adds no dependency to generated applications or NativeAOT outputs
and restores no external NuGet package. Its default mode only calls Roslyn's
`NormalizeWhitespace`; the explicit method-sharding mode below additionally
packages synthetic helpers to respect CLR type limits.

Build `Edg.SourceFormatter.csproj`, then invoke its DLL using `dotnet`. Standard
input is one JSON object mapping file names to complete C# source strings:

```json
{"Translated.cs":"unsafe class Program{static int Main(){return 0;}}"}
```

Standard output is one JSON response:

```json
{"files":{"Translated.cs":"unsafe class Program\n{\n    static int Main()\n    {\n        return 0;\n    }\n}\n"},"diagnostics":[]}
```

Formatting uses four spaces, LF newlines, and a final newline. Diagnostics have
`file`, `id`, `message`, one-based `line`/`column`, and `severity`. Locations refer
to input generated C# rather than remapped `#line` source locations. Syntax-error
files retain their original text. The process returns 0 for success, 2 for any
syntax errors in the batch, or 1 for malformed input. Malformed requests use the
`FORMAT_INPUT` diagnostic and zero line/column. Nothing is written to source
files by this tool. The driver owns persistence and must not ignore failures.

Run the formatter after parsing backend metadata and after splitting files; no
metadata extractor should depend on pre-format whitespace. It checks syntax
only, so ordinary C# compilation remains necessary for semantic verification.

Focused acceptance: `artifacts/campaign/persistent-continuation/source-formatter-001`
contains six checks for batching, idempotence, diagnostics and original/formatted
JIT/NativeAOT execution. `source-formatter-generated-001` retains a supplemental
15-file batch of actual generated C# (715900 input characters) with no syntax
diagnostics. [The component evidence index](../../evidence/source-expression-components.json)
records exact SHA-256 hashes and excludes this supplemental batch from the
52-check cross-component total. Execution evidence is Linux x64; no additional
platform execution claim is implied.

## Large translated owners

`--shard-methods` accepts `{ "source": "...", "owner": "Namespace.Class" }`.
`backend/source_format.py:shard_methods` invokes it before whitespace formatting
and source-file splitting. The tool budgets methods, property/indexer/event
accessors, possible implicit constructors, local functions and lambdas. Above
62,000 slots it moves enough private static expression, cleanup, initialization
and assignment helpers into nested `__EdgMethodShardN` classes to reach 61,000.
Each shard holds at most 10,000 methods. Smaller owners are returned unchanged.

Source function names, APIs, types, fields and state ownership remain in the
original class. References to moved helpers are qualified structurally; no global
imports can collide with another translated owner. Helpers with private types in
their signatures or references from nested/other types stay in place, preserving
accessibility and the original owner's initialization trigger. Instance methods
also stay in place. If too few safe static helpers exist, packaging fails with
the required counts rather than silently changing instance state or public APIs.
No runtime reflection, allocation or per-call dispatch is added.

One backend member delimiter surrounds each entire shard so every file-splitting
mode retains valid nested classes. The response includes `metadata` with before
and after budgets and shard sizes; `source` is transported as ordered string
chunks to support compiler-sized output. `method_shards_verify.py --output-dir
NEW_PATH --nativeaot` checks forced sharding, ordinary no-op behavior, two owners
in one assembly, static and instance state, function-pointer/API access, private
signature exclusions and split files under JIT and NativeAOT.
