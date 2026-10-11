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

Explicit construction/destruction and exception execution now have a verified
cross-unit slice. `initializations`, `initialization_arguments`, `lifetimes`,
`lifetime_destructions`, `constructor_initializers`, `try_handlers` and `throws`
store EDG-selected semantic facts. Destruction ordinals are the actual EDG order;
initialization nodes and cleanup actions reference the same initialization row.
The generator preserves executable initialization positions, arms cleanup after
success, and reuses the existing shared C++ exception runtime. Run:

```sh
python3 edgcpp/object-linker/relational/tests/verify_lifetime.py \
  --out /path/new-lifetime-directory \
  --linker edgcpp/artifacts/relational-tool/dotobj-relational.dll --nativeaot
```

Nonvirtual base layout, selected pointer/reference conversions and explicit base
construction/destruction are also supported. `base_subobjects` and
`class_conversions` retain EDG-selected identities and adjustments, including null
preservation. Named pointer projections respect reused base tail padding. Run
`tests/verify_bases.py` with the same arguments to compare native, source-hidden
C# and NativeAOT execution, including second-base construction failure.

Named enums retain EDG-selected underlying types, ordered exact values and selected
enumerator references in `enums`, `enum_values` and `nodes.enum_value_id`. Generated
C# uses named enum declarations and values; folded constants retain their recorded
numeric value. `tests/verify_enums.py` exercises cross-unit storage, promotions,
unsigned 64-bit values and enum exception identity.

This covers lexical return cleanup, partial member/base construction, reference catches,
rethrow and exception-object destruction. Arrays/virtual-base construction, nontrivial
temporaries, function-try blocks, complex catch conversions, macros, all type
variants, addressable globals/TLS storage and complex initialization remain work.
Unsupported forms must produce diagnostics, not an opaque fallback or silently
incomplete object. The preserved legacy runtime/emitter contains algorithms to
port for those requirements. Full-project bounded loading also remains to be
implemented; the initial C# consumer materializes the small input units.
