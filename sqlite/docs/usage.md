# Build and verify translated SQLite

Run commands from the repository's `sqlite/` directory. The supported initial
profile is Linux x64, LP64, little endian, .NET 10, concurrent connections with
serialized connections by default, and the real file-backed `dotcc-host` VFS
for the managed library. Deterministic C
corpora retain their process-local memory VFS. Windows/macOS host implementations
and CI are provided, but local execution evidence is Linux x64. See
[host VFS](host-vfs.md). Core SQLite, JSON/JSONB, FTS5, SQL math/percentile functions and column metadata
are enabled. Database mmap defaults to 64 MiB with a 256 MiB maximum per file.
FTS3/FTS4 and dynamic extensions remain excluded. WAL is supported
on the host VFS; select it with `PRAGMA journal_mode=WAL`.
See `configuration.md` for corpus and host-product definitions and
[threading and mmap](threading-mmap.md) for connection ownership and mapping controls.

Prerequisites are .NET SDK 10, Python 3, GCC, a POSIX shell and GNU coreutils.
NativeAOT also requires the .NET Linux native linker prerequisites (the CI recipe
installs Clang and zlib development headers). Initial fetch/restore requires
network access; fetched archives are pinned by SHA-256. The optional existing-port
checks also require Make, Node.js and `wat2wasm`.

The complete campaign command is:

```sh
scripts/verify.sh
```

This checks source hashes, builds dotcc with the NuGet LALR.CC dependency, runs the
unit and functional suites serially, regenerates native baselines for comparison,
then translates and executes layout, API, SQL, VFS, allocation, virtual-table,
public JSONB, FTS5 and database-image tests. It includes the separate C# consumer and
NativeAOT corpus/product-layout, consumer, API-corpus and threading checks,
span-varargs semantics/allocation benchmarks,
and real-file VFS locking, persistence and
database mapping lifetime, rollback/WAL recovery and checkpoints against native processes. It writes diagnostics under `artifacts/` and
fails on the first mismatch. `SQLITE_AOT=0 scripts/verify.sh` is an explicitly
smaller JIT-only run, not the completion gate. `scripts/verify.sh --with-ports`
adds the existing Lua, Chibi and WAT regressions; their upstream runners and
committed baselines remain the acceptance criteria.

To fetch the pinned inputs, rebuild dotcc, and regenerate/build
only the reusable SQLite assembly (no tests or consumer execution):

```sh
scripts/build.sh
```

The helper also works from another directory, for example
`sqlite/scripts/build.sh` from the repository root. To separately run the managed
consumer checks, use `scripts/test-managed-consumer.sh`.

Open `sqlite/ManagedConsumer.slnx` from the repository root (or
`ManagedConsumer.slnx` from this directory) in Rider to work on the consumer,
and TranslatedSqlite together. Select `ManagedConsumer` as the startup project.
The solution and SQLite design-time builds no longer include the postprocessor
analyzer/code-fix projects; emission already applies the transformations.

To regenerate only TranslatedSqlite with the already-built dotcc compiler,
and run the postprocessor in place, without compiling the generated C#:

```sh
scripts/emit-engine.sh
# To retain raw dotcc output instead:
scripts/emit-engine.sh --no-postprocess
```

SQLite emission defaults to one function per file, followed by in-place
postprocessing. All methods belong to `Managed.Database.Sqlite`; filenames use
`Sqlite.sqlite3_open.cs`, for example, with a numeric suffix only for collisions.
Shared runtime/types/globals stay in `Program.cs`. To change the source layout:

```sh
SQLITE_SOURCE_SPLIT=function scripts/emit-engine.sh
SQLITE_SOURCE_SPLIT=size SQLITE_SOURCE_SPLIT_SIZE=524288 scripts/emit-engine.sh
SQLITE_SOURCE_SPLIT=none scripts/emit-engine.sh
```

In size mode, filenames use `Sqlite.00000.cs`, `Sqlite.00001.cs`, and so on.
The byte target closes each function group after the first whole function takes
it over the target; large functions and shared declarations can exceed it.
Regeneration cleans obsolete files tracked in `Dotcc.SourceFiles.txt`. Keep that
manifest so switching layouts does not leave duplicate declarations. Add
`--no-postprocess` to any invocation when raw dotcc output is needed.

Reference `generated/TranslatedSqlite/TranslatedSqlite.csproj` from a C# project,
or its built `TranslatedSqlite.dll`. `Managed.Database.Sqlite` (selected with `--class-name Sqlite --namespace Managed.Database` by `emit-engine.sh`) exposes the C API as unsafe managed
methods; public translated aggregate types and `delegate*` signatures preserve
SQLite's callback surface. Use `using Managed.Database;` for generated types and
`using static Managed.Database.Sqlite;` for the API methods. `tests/ManagedConsumer` demonstrates explicit C#
extension registration, ownership, callback re-entry and cleanup. No native SQLite
library or dynamic extension loader is part of that integration. The default VFS
uses `Managed.Database.HostVfs` for real files; OS-level P/Invoke supplies platform locking/durability alongside
BCL file I/O. The sample consumer creates and cleans up a temporary WAL database.

Pointer and function-pointer inline-array elements use unmanaged one-field
structs to support C# indexing and spans. Managed callers access the pointer as
`array[index].Value`; translated C retains its original pointer-array layout.

Generated engine source is never edited. dotcc emits offsetof constants directly
from its shared typed layout model; the emitted project needs no offset analyzer.
See [offset layout](offset-layout.md) for the model and validation contract.
Native SQLite is used only
in separate oracle processes. Database-image tests exchange closed files through
the deterministic harness; `scripts/test-host-vfs.sh` separately verifies ordinary
disk persistence and locking without image import/export.

Use `scripts/test-translated.sh core` (or `api`, `vfs`, `vtable`, `allocation`,
`upstream`, `fts5`) to isolate a corpus. Use `SQLITE_AOT=1 scripts/test-layout-translated.sh`
and `SQLITE_AOT=1 scripts/test-managed-consumer.sh` for the AOT gates.
`SQLITE_AOT=1 scripts/test-threading.sh` isolates concurrent engine contracts;
`SQLITE_AOT=1 scripts/test-product-layout.sh` checks the host profile ABI. Runtime
processes have a default 120-second bound, overridable with
`SQLITE_EXECUTION_TIMEOUT`; port suites use `SQLITE_PORT_TIMEOUT` (600 seconds).
Compiler and native build diagnostics are kept separate from expected SQL output.

The checked-in GitHub workflow runs the SQLite campaign on pull requests, main
pushes, a nightly schedule and manual dispatch. Adding it locally does not run
remote CI; local evidence and remaining limitations are recorded in `validation.md`.


## FTS5 corpus

The shared profile now statically enables FTS5 while FTS3/FTS4 remain deferred.
Run `scripts/test-fts5-native.sh` for the native oracle and
`scripts/test-translated.sh fts5` for the translated comparison. Both are included
in `scripts/verify.sh`. See [FTS5 coverage](fts5-inventory.md).

`scripts/test-function-identity-aot.sh` translates the repository's static-function
identity fixture through both source and separately emitted object routes. It
checks the native-proven expected output under JIT and Linux x64 NativeAOT,
including distinct addresses for identical static functions in separate C units.
The full campaign runs this gate when `SQLITE_AOT=1`; logs use the
`artifacts/function-identity-*` prefix.

## Source post-processing

`scripts/emit-engine.sh` runs the separate
[Roslyn post-processor](../../docs/postprocess.md) in place after dotcc emission,
inlining Cond.B calls and removing standalone empty blocks. `scripts/build.sh`
uses that script before compiling TranslatedSqlite. The compiler itself is unchanged.
Use `scripts/emit-engine.sh --no-postprocess` to produce a raw baseline, then invoke
the tool with `--output <new-directory>` to create original/optimized snapshots.
The explicit `scripts/test-postprocess.py <snapshot> --aot --corpora` gate compares
original and optimized SQL, threading, mmap/WAL and native corpus behavior.

The optional analyzer/code-fix tooling remains available for other projects; see
the [Rider instructions](../../docs/postprocess.md#rider-in-place-fixes).
