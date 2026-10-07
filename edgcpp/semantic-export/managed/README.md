# Managed semantic graph consumer

`Edg.Semantic` is a reflection-free .NET 10 reader for `edg-semantic/1`
packages. It uses `JsonDocument` and explicit `Utf8JsonWriter` output, and
supports NativeAOT. Its separate bounded C emitter consumes the same named graph
facts and emits source-oriented unsafe C#.

```sh
dotnet run --project edgcpp/semantic-export/managed -c Release -- inspect PACKAGE_DIRECTORY
dotnet run --project edgcpp/semantic-export/managed -c Release -- assert-probe PACKAGE_DIRECTORY
dotnet run --project edgcpp/semantic-export/managed -c Release -- emit-csharp PACKAGE_DIRECTORY --output Generated.cs
```

`inspect` accepts a package directory, `package.json`, or a standalone graph.
Packages additionally verify graph and available source SHA256 hashes, byte
lengths, source identities, and relative member paths. Standalone graph input
cannot verify original source bytes. Graph validation checks node identity,
explicit reference fields, native edge kind agreement, source locations and
macro references. Unknown adapter fields remain accessible as JSON.

`--strict-emission` rejects incomplete graphs, diagnostic-bearing graphs and
packages, and unavailable source snapshots. The initial native exporter marks
all graphs incomplete; successful inspection is not a declaration of complete
semantic coverage.

`emit-csharp` applies the independent `c-integer-structured/1` profile. It requires
a full package, verified source/graph/producer hashes, explicit source availability,
zero diagnostics and supported semantic facts. It renders before writing the
output, preserving any existing file when validation rejects the input. The
generated class defaults to `SemanticModule`; use `--class NAME` to change it.
See the [managed C checkpoint](../../docs/semantic-export-c-emission.md) for tested
coverage, evidence and remaining work.

The public `SemanticGraph` API exposes ID-based `SemanticNode` objects, typed
common accessors, linked lists, source declaration ordering and resolved direct
call targets. IDs are local to one graph, not stable names or cross-unit keys.
Virtual calls retain their resolved declaration and virtual-call flag; the
runtime override is not statically resolved. `Raw` retains every exported field
while adapters evolve. Dispose the graph only after consumers finish using its
nodes and `JsonElement` values.

`assert-probe` verifies the exporter-owned C/C++ synthetic fixtures: source
statements and symbols, resolved overload/template selections, nested RAII
lifetimes, and macro provenance. The Python harness in
`../tests/managed-tests/verify.py` additionally tests malformed references,
integrity failures, unsupported schemas and strict-emission rejection. Run it
against both the managed DLL and published NativeAOT executable.
