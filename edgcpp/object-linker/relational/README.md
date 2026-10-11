# Relational semantic program objects

This is the replacement for the broad EDG observation-graph path. EDG writes one
SQLite database directly per source translation unit. The database contains
program entities and resolved references in ordinary SQL rows; it contains no
JSON documents, encoded semantic field blocks, or frontend observation history.

The current implementation is an initial executable slice, not full C++ or
DuckDB support. The source-shaped bodies are captured before lowering, preserving
expression types/value categories and structured statements. Original source
names and EDG linkage names are separate. Instantiations reference their selected
template and actual arguments. The C# consumer resolves recorded link identities
and emits supported operations; it does not redo EDG's C++ semantic analysis.

`schema.sql` is the storage contract. `browse.sql` contains ordinary queries for
functions, expression types/selected symbols, children, record layout and template
arguments. A SQLite browser needs no custom decoder. The initial schema has no
BLOB columns at all. Future literal byte arrays may use BLOBs; semantic graphs may
not.

Build the native compiler through the existing `backend/prepare.py` and CMake
path. SQLite development headers/library are required by the native exporter.
Build the consumer:

```sh
dotnet build edgcpp/object-linker/relational/managed/DotObj.Relational.csproj \
  -c Release -o edgcpp/artifacts/relational-tool
```

Compile and link:

```sh
python3 edgcpp/object-linker/relational/compile.py example.cpp \
  --output /path/example.dotobj --work-dir /path/example-work
dotnet edgcpp/artifacts/relational-tool/dotobj-relational.dll link \
  --out /path/generated --namespace Example --class Core --entry main \
  /path/example.dotobj
```

The native compiler opens the SQLite transaction and inserts prepared rows.
There is no JSON wire or packager. On success the driver publishes the object;
on failure it preserves diagnostics and the unpublished partial file. Linking
and C# generation do not require the original source files.

The cross-unit test creates two objects, inspects SQL relations, hides original
sources, links and executes generated C#. It also compares native execution:

```sh
python3 edgcpp/object-linker/relational/tests/verify.py \
  --out /path/new-test-directory \
  --linker edgcpp/artifacts/relational-tool/dotobj-relational.dll --nativeaot
```

Explicit construction/destruction, lifetime/exception regions, macros, all type
variants, addressable globals/TLS storage and complex initialization are still required work.
Unsupported forms must produce diagnostics, not an opaque fallback or silently
incomplete object. The preserved legacy runtime/emitter contains algorithms to
port for those requirements. Full-project bounded loading also remains to be
implemented; the initial C# consumer materializes the small input units.
