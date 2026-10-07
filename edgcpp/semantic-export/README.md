# EDG semantic package prototype

This is an **implemented, separately built exporter, managed reader and bounded
C# emitter prototype**. It keeps EDG's unlowered IL and source form available to
managed passes. The existing `cpfe-dot` executable, preparation scripts, and
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
The first build profile is Linux x64/GCC. The managed reader uses BCL APIs and is
NativeAOT compatible; other native frontend build profiles remain to be tested.

The wrapper disables the default predefined-macro file, which is not present in
an isolated tree. For real projects, pass the explicit include/define/preinclude
and target options needed by their build. The package records the invocation;
this self-contained probe profile is not a portable libc/header configuration.
C++ exports use `--instantiate used`; C exports do not pass that C++-only switch.

## Package contract

A package contains:

- `package.json`: `edg-semantic-package/1`, capabilities, invocation, graph/source
  hashes, source availability, and the exact producer manifest hash.
- `graph.json`: `edg-semantic/1`, a versioned semantic graph with numeric
  package-local identities. Zero denotes an absent edge. IDs are assigned by
  traversal and never encode native addresses.
- `producer.json`: executable SHA-256, compilation command/configuration, and
  hashes of every native adapter/frontend source/header in the isolated build.
- `sources/`: unchanged input/header bytes. File entries distinguish source-file
  occurrences from their physical paths. Missing/virtual input paths produce
  explicit availability diagnostics.
- Invocation and native stdout/stderr logs.

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
JSON. No pointer is rewritten to a different value. `nativeEdges` retains all
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
