# Managed memory VFS

`src/MemoryVfs.cs` implements the named `dotcc-memory` VFS in authored C#.
It is compiled beside the translated engine, not generated from C. Product
builds register it as an additional VFS and keep `dotcc-host` as the default.
Translated test executables register the same implementation as their default.

Files use managed byte arrays and handles reference managed state through a
locked dictionary keyed by SQLite's file address. A BCL lock serializes shared
state; the VFS never calls SQLite's allocator or mutex API while holding it.
This permits concurrent connections without lock inversion during initialization,
failure recovery or restart. SQLite owns each `sqlite3_file` allocation. Closing
a handle removes its dictionary entry; unlinking frees file state after its last
handle closes. Named files otherwise remain until deletion or an explicit reset.
Reset returns `SQLITE_BUSY` while handles remain open.

Read/write, zero-filled growth and short reads, truncate, path normalization,
shared/reserved/pending/exclusive locks, delete-on-close, file controls, image
import/export, deterministic time/randomness and one-shot I/O failure injection
retain the reference contracts. Storage is limited to `Array.MaxLength` bytes
per file: requests beyond it return `SQLITE_FULL`; allocation failures return
`SQLITE_NOMEM`. There is no disk persistence, cross-process locking, WAL, mmap,
dynamic loading or physical durability. File methods advertise version 1.
Path keys preserve raw bytes, including invalid UTF-8.

Callback addresses are captured once in static tables. Unmanaged copies of the
small VFS/method/name tables are retained for process lifetime, including across
SQLite shutdown/reinitialization. File data is managed and remains valid across
compacting GC. `src/Sqlite.MemoryVfs.cs` keeps the existing managed
`dotcc_memory_vfs_*` convenience APIs and test constants available.

`config/dotcc-overrides.json` binds the product's OS-init/end and mutex/barrier
functions to authored managed methods. `config/corpus-overrides.json` binds C
harness declarations to the same memory VFS. `Directory.Build.targets` imports
the adapter for product and corpus projects. The source defaults to the product's
nested `Sqlite` types and host registration, including when copied to a consumer.
Only corpus projects define `DOTCC_SQLITE_CORPUS` to select their executable
wrapper and memory-default registration.

The original C VFS lives only in `tests/native/memory_vfs.c`. Native GCC oracle
builds compile it; dotcc never does. Both implementations run the identical
`tests/vfs_native.c` contract harness and SQL/API/image corpora. The translated
harness and amalgamation are independent dotcc input files. The layout probe
intentionally includes the amalgamation so its tests can inspect private types.

Useful checks:

```sh
scripts/test-vfs-native.sh
SQLITE_AOT=1 scripts/test-translated.sh vfs
SQLITE_AOT=1 scripts/test-managed-consumer.sh
SQLITE_AOT=1 scripts/test-threading.sh
scripts/test-image-exchange.sh
```
