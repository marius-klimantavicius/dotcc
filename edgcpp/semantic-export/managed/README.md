# Managed semantic graph consumer

`Edg.Semantic` is a reflection-free .NET 10 reader for `edg-semantic/1`
packages. It reads normalized SQLite through indexed queries and bounded
record caches, retains legacy JSON input support, and supports NativeAOT.
The SQLite path requires the platform SQLite native library. Its separate bounded C emitter consumes the same named graph
facts and emits source-oriented unsafe C#.

```sh
dotnet run --project edgcpp/semantic-export/managed -c Release -- inspect PACKAGE_DIRECTORY
dotnet run --project edgcpp/semantic-export/managed -c Release -- assert-probe PACKAGE_DIRECTORY
dotnet run --project edgcpp/semantic-export/managed -c Release -- emit-csharp PACKAGE_DIRECTORY --output Generated.cs
```

`inspect` accepts a package directory, `semantic.sqlite`, legacy `package.json`,
or a standalone JSON graph. `--summary` performs full validation and reports
counts without enumerating direct calls. SQLite packages embed their source
snapshots and producer metadata; no companion files are required.
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
runtime override is not statically resolved. Each node's `Raw` retains its exported fields while adapters evolve. For SQLite,
`SemanticGraph.Raw` contains graph metadata only; use `Nodes`, `OfKind`,
`MacroInvocations` and `Diagnostics` for lazy record access. Decoded node values
survive cache eviction. Dispose the graph after all traversal finishes; one
read transaction keeps integrity validation and later lookups on the same snapshot.

`assert-probe` verifies the exporter-owned C/C++ synthetic fixtures: source
statements and symbols, resolved overload/template selections, nested RAII
lifetimes, and macro provenance. The Python harness in
`../tests/managed-tests/verify.py` additionally tests malformed references,
integrity failures, unsupported schemas and strict-emission rejection. Run it
against both the managed DLL and published NativeAOT executable.

The SQLite contract and tests are in [storage/schema.sql](../storage/schema.sql)
and [tests/storage/verify.py](../tests/storage/verify.py). `emit-project` accepts
the same package inputs and emits the configured SQLite C engine as C#.
