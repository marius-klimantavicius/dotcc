# EDG semantic packages

This contains an **implemented, separately built exporter, managed reader and
C# emitters, including a verified SQLite project path**. It keeps EDG's
unlowered IL and source form available to managed passes. The existing `cpfe-dot` executable, preparation scripts, and
upstream source checkout are not changed by these tools.

## Build and export

Run from the repository root. Choose a fresh output package directory.

```sh
python3 edgcpp/semantic-export/build.py \
  --work edgcpp/artifacts/campaign/persistent-continuation/semantic-export-build -j 4

python3 edgcpp/semantic-export/export.py \
  --compiler edgcpp/artifacts/campaign/persistent-continuation/semantic-export-build/build/bin/cpfe-semantic \
  --output edgcpp/artifacts/campaign/persistent-continuation/semantic-export-probe \
  edgcpp/semantic-export/tests/probe.cpp -- --c++17

dotnet run --project edgcpp/semantic-export/managed -- \
  inspect edgcpp/artifacts/campaign/persistent-continuation/semantic-export-probe
```

`build.py` copies the pinned frontend into the requested work directory. It uses
EDG's supported C++ regeneration configuration, with lowering disabled,
`DOING_SOURCE_ANALYSIS=0`, all template information, source sequences, full source
positions, macro definitions, invocation trees, and normalized macro arguments.
The original C++ regeneration callback is renamed **in the isolated copy only**;
its utility functions remain available to EDG's diagnostic formatter. Our
`semantic_export_be.c` supplies the real `back_end` callback.

Macro recording exposes an upstream local-variable shadow warning. The isolated
target removes its promotion to an error; no parser/semantic source is fixed.
The first build profile is Linux x64/GCC. The managed reader uses a small NativeAOT-compatible binding to the platform
SQLite library (`libsqlite3` on Linux/macOS, `winsqlite3` on Windows). The native
writer embeds the pinned SQLite amalgamation in its isolated build. Linux x64
is verified; other native frontend and managed platform profiles remain to be tested.

The wrapper disables the default predefined-macro file, which is not present in
an isolated tree. For real projects, pass the explicit include/define/preinclude
and target options needed by their build. The package records the invocation;
this self-contained probe profile is not a portable libc/header configuration.
C++ exports use `--instantiate used`; C exports do not pass that C++-only switch.

## SQLite and the compiler self audit

`emit-project PACKAGE --output-directory DIRECTORY` emits the configured SQLite
amalgamation as a C# project. The complete engine and its C memory VFS compile
and execute under JIT and NativeAOT against a native-C oracle. Reproducible
commands, the feature profile and limits are in
[the SQLite checkpoint](../docs/semantic-export-sqlite.md).

`audit-self.py` exports the actual compiler translation units, including the dot
backend, and inventories missing named adapters and reference closure. See
[the compiler self audit](../docs/semantic-export-self-audit.md). Export success
is distinct from C++ managed code generation.

Exports now use the normalized [SQLite storage format](../docs/semantic-export-storage.md).
The managed reader uses indexed queries and bounded caches; it also accepts
legacy JSON packages. `inspect PACKAGE --summary` validates the entire package
and reports counts without enumerating direct calls.

## Package contract

The portable package is one `semantic.sqlite` database. Both a package directory
and the database path itself are accepted by the C# reader and emitters.
Invocation and native stdout/stderr logs remain alongside it for troubleshooting;
they are not needed to read or emit from the database.

- `nodes` contains unit-qualified identity, kind and common fields; typed
  `node_<kind>` tables contain kind-specific facts. IDs never encode native
  addresses. Zero denotes an absent semantic reference.
- `literals` shares text and nested payload bytes. A compact hash index finds
  candidates; exact byte comparisons decide sharing. Separate semantic objects
  retain separate IDs even when their payloads match.
- Ordered `native_edges`, `macros` and `diagnostics` tables retain audit and
  provenance facts. Integer payloads outside signed 64-bit use exact decimal
  BLOBs, never lossy SQLite REAL values.
- `source_files` records source/include occurrences; `source_blobs` holds original
  bytes deduplicated by content hash, with explicit unavailable-source records.
- `package_info` embeds the `edg-semantic-package/2` manifest and exact producer
  manifest, including invocation, capabilities, compiler/source hashes and a
  [logical-content checksum](storage/content-hash.md).

The [storage schema](storage/schema.sql) is `edg-semantic-sqlite/1`; reconstructed
semantic records retain `edg-semantic/1`. The writer inserts records directly into
SQLite, then the wrapper snapshots sources, validates and seals the database
before publishing it by rename. There is no intermediate whole-graph JSON file.
The current reader requires exactly one translation unit; unit-qualified IDs do
not imply cross-unit semantic linking.

Inputs must remain immutable during export. The current wrapper copies source
bytes after frontend completion and states this snapshot policy in the manifest.
Source-file entries, include-child chains, sequence numbers, line numbers,
expansion/spelling positions, and macro context IDs are preserved. Columns are
**EDG columns, not promised byte offsets**; presumed `#line` identities are
separate from physical file names. Native text/string fields use a reversible byte mapping to JSON
U+0000–U+00FF characters (also explicitly named `bytesLatin1` for raw string
payloads). ASCII names therefore read normally. Decoding non-ASCII native names
according to the frontend input encoding is a future managed adapter; this
milestone does not claim Unicode spelling fidelity through the name accessor.
Original source bytes are authoritative for comments, literal spelling, Unicode,
and inactive branches.

## Typed facts implemented

The native adapter currently exports:

- Symbol names, linkage, scope and declaration occurrences; declared versus
  canonical types, scalar/array/pointer/reference/record/routine relationships,
  integer kinds, qualifiers, sizes, alignment and field/bitfield offsets.
- Source-sequence order, structured blocks/loops/conditionals/switch links,
  labels and original `break`/`continue` markers. Statements retain parents,
  source positions and compiler-generated flags.
- Typed expressions, value categories, source ranges, implicit member-naming and
  evaluation-order flags, operations, operand order and selected routine/member
  identities. Runtime virtual dispatch remains a separate fact from the
  statically selected declaration.
- Template definitions/text, declaration links, concrete function/class
  instances, type/value argument links, prototype/nonreal markers and selected
  partial-specialization identity.
- Dynamic initialization/constructor/destructor links; lifetime parents,
  ordered destruction lists, parent cleanup cursors, full-expression lifetime
  links, scope-exit common lifetime, conditional/temporary/partial-construction,
  overlapping-temporary, unordered cleanup and exception-deallocation facts.
- Macro definition/undefinition/redefinition records, command-line/predefined
  origin, replacement ranges/text, invocation nesting, original invocation
  positions and normalized argument strings.

The EDG file/routine walkers do not follow every reference into another region.
After collecting their nodes, the adapter follows the supported identity
remapper to a fixed point, retaining additional referenced nodes before writing
the database. No pointer is rewritten to a different value. `nativeEdges` retains all
visited pointer references for auditing graph closure. Its ordinal traversal
order is **not** the stable semantic schema; managed lowering must use named
fields and reject missing adapters.

## Explicit limits

Every package currently declares `completeForEmission: false`. This flag is
intentional even for simple probes. `--strict-emission` in the managed reader
rejects such packages. A successfully parsed/validated package does not establish
complete language coverage or mean it can already drive a C# emitter.

The separate `emit-csharp` command validates the bounded
`c-integer-structured/1` profile before producing C#. It requires a package with
verified hashes and available source snapshots, zero adapter diagnostics, and
only supported C semantics. It does not override the graph-wide completeness
flag. See the [managed emission checkpoint](../docs/semantic-export-c-emission.md)
for commands, accepted coverage and remaining work.

Unimplemented node/variant adapters produce node-ID diagnostics (`SEM001`) and
retain generic edges; they are not silently discarded. Current probes still
report metadata adapters including exception specifications, dependent
template supplements and hidden-name records. Integer
constant values outside signed/unsigned 64-bit and unimplemented expression/statement/
constant forms likewise require explicit adapters. Basic type records do not
claim complete callable ABI or class-layout semantics on their own.

The exported cleanup data are **primitive EDG facts**, not yet a managed
exception/RAII lowering or a proof of all normal/exceptional execution paths.
There is no semantic linker, C++ code generator, macro-to-managed-API conversion,
lossless syntax tree, or complete module/vendor-extension serialization in this
milestone. Dependent template prototypes remain dependent; concrete instance
identity is relative to the invocation and its instantiation policy.

## Verification

`tests/plain.c` proves that a C# consumer can navigate the named `sum` routine,
its typed parameters and field references, source sequence, and structured
`for`/`if`/`continue` without reverse-engineering C# or parsing IL display text.

`tests/probe.cpp` proves actual overload binding to `choose(int)`, instantiated
`twice<int>`, the selected `Box<T*>` specialization, nested `Guard` destruction
records and structured exception handlers. `tests/api.h` exercises public macro
constants, nested function macros and spelling/expansion provenance; the C++
source also undefines and redefines a public macro.

The managed verifier additionally rejects malformed package/schema/edge/hash/
macro/source records and tests fail-closed emission gating. Its test script
supports the same graph assertions under JIT and NativeAOT. See the implementation
checkpoint document for exact accepted artifacts and test counts.
