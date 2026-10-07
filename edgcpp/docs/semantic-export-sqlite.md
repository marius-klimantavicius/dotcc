# SQLite through the managed semantic-export backend

The C# handler now translates the complete configured SQLite 3.53.4 amalgamation
from EDG's unlowered semantic package. It emits a standalone .NET 10 project
containing 3,165 SQLite and memory-VFS routines, 224 globals, 361 record types,
and 348 array storage types. The engine and VFS are translated C#; execution
does not call a native SQLite library or consume the existing dot backend's
emitted code.

The native exporter and C# handler are separate programs. `emit-project` reads
validated package hashes, source snapshots and named semantic records. It does
not parse source C, IL display text, or the opaque ordering of `nativeEdges`.
The global `completeForEmission` flag deliberately remains false: this verified
C project does not establish complete C/C++ coverage.

## What the project exposed

The baseline SQLite graph contained 1,196,674 nodes and 7,906 adapter diagnostics.
Its referenced identities already closed, but named records were missing for
pointer-typed integer constants, switch cases/descriptors, floating constants,
source-sequence sublists, attributes, local static initialization, local
references, builtin operations and address subobjects. The native adapter now
exports these facts. Re-exporting the unchanged amalgamation produces zero
adapter diagnostics. Including the existing C memory VFS produces a package
with 1,205,132 nodes, 16 source-file records and zero adapter diagnostics.

The managed handler implements explicit record/union/array layouts, named
bitfield properties, aggregate/designated initialization, pinned global/string
storage, ordinary functions and managed function pointers. It maps C variadic
calls to explicit argument carriers and retains the existing software binary80
runtime for `long double`. Memory allocation uses `NativeMemory`; memory/string,
math and time imports have managed bindings. Undefined SQLite OS initialization
hooks remain required partial-method implementations when exporting the engine
alone. The test package supplies their actual C definitions in `memory_vfs.c`.

Generated functions retain source names, named field access, and structured
conditionals and loops. Locals are hoisted to accommodate C labels. A semantic
scope analysis lowers only controls entered by a jump to explicit labels;
ordinary controls remain structured. Switch dispatch preserves C fallthrough.
Function splitting keeps the amalgamation reviewable. Explicit conversions,
array storage wrappers and helper expressions remain visible, so this is not yet
source-fidelity parity with every presentation option of the existing backend.

## Reproduce

Run from the repository root with .NET 10, GCC and NativeAOT prerequisites:

```sh
python3 edgcpp/semantic-export/build.py \
  --work edgcpp/artifacts/campaign/persistent-continuation/semantic-sqlite-build -j 8

python3 edgcpp/semantic-export/tests/sqlite/run.py \
  --compiler edgcpp/artifacts/campaign/persistent-continuation/semantic-sqlite-build/build/bin/cpfe-semantic \
  --output edgcpp/artifacts/campaign/persistent-continuation/semantic-sqlite-run \
  --nativeaot
```

The driver preserves both the SQLite-only graph and the graph containing the
memory VFS. It takes the existing feature definitions from
`sqlite/config/defines.txt`, uses the portable C provider headers, and records
explicit C17/GNU13.3 mode and the missing predefined atomic-order constant.
It does not change SQLite's source or turn off its enabled FTS5, math,
percentile, column-metadata or preupdate-hook features.

To emit directly from an existing package:

```sh
dotnet run --project edgcpp/semantic-export/managed -c Release -- \
  emit-project /path/to/package --output-directory /fresh/generated/project
```

Public generated APIs are in `Edg.Semantic.Generated.SqliteCore`. Ordinary C API
parameters retain their pointer types; variadic calls accept
`ReadOnlySpan<SqliteCore.__CArg>`. The package's target ABI determines record
layouts. Function pointers use the managed calling convention, not a native
SQLite ABI entry point.

## Validation and limits

The retained campaign is
`artifacts/campaign/persistent-continuation/semantic-export-sqlite-001/`.
Native C is compiled from the package's original source snapshots. Its 43 output
observations match generated C# under JIT and NativeAOT on Linux x64. The corpus
covers transactions and rollback, triggers, indexed grouping, 2,000-row bulk
updates/deletes and vacuum, integrity checks, window functions, JSONB, FTS5 with
Unicode, math and percentile functions, UTF-8/blob/numeric bindings, variadic
formatting, and named memory-database close/reopen. These are project smoke and
differential tests, not SQLite's complete upstream test suite.

A further 67 focused expression observations match native C in both execution
modes, covering volatile reads/writes, bitfield promotion and assignment values,
callbacks and evaluation counts. The NativeAOT emitter produces the same 3,171
source/project files as the JIT emitter. The 63 bounded-emitter regression
checks and 20 reader checks per execution mode also pass. Exact hashes,
commands and result paths are indexed in
[`evidence-sqlite.json`](../semantic-export/evidence-sqlite.json); the final
fresh export/build/test run is retained in `final-pipeline/pipeline.json`.

The native oracle and generated project use the established `SQLITE_OS_OTHER=1`,
`SQLITE_THREADSAFE=0`, memory-VFS configuration. The VFS is the repository's
existing C test implementation, also translated by this handler. Filesystem
persistence, concurrent SQLite use, other target ABIs/platforms, and native
callback interoperability are not established by this result. The generated
class has process-static state. Unbound runtime functions and unsupported
semantic forms fail emission instead of producing placeholder implementations.

The original bounded `emit-csharp` profile remains separate. Its regression
checks continue to require identical previously verified output and controlled
rejections. This new handler does not loosen that profile. EDG's own C++ export
inventory and the remaining template/lifetime/class/lambda adapters are covered
in [the self audit](semantic-export-self-audit.md).
