# Generated C# whitespace formatter

This build-time tool uses the compiler assemblies shipped with the selected
.NET SDK. It adds no dependency to generated applications or NativeAOT outputs
and restores no external NuGet package. It only calls Roslyn's
`NormalizeWhitespace`; names, expressions, tokens and control flow are the
backend's responsibility.

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
