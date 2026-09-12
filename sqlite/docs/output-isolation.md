# Nested translation output

The product emission script uses `--nest-types --runtime=c`. The public API is
still `Managed.Database.Sqlite`; translated aggregates/enums, embedded libc,
layout constants and other generated helper types now live inside that class.
For example:

```csharp
using static Managed.Database.Sqlite;

sqlite3* database = null;
var free = SqliteFunctionPointers.sqlite3_free;
// Without using static: Managed.Database.Sqlite.sqlite3* database.
// Runtime surface: Managed.Database.Sqlite.Libc.
// Globals: Managed.Database.Sqlite.SqliteGlobals.
```

There is one `SqliteFunctionPointers` class containing all canonical address
fields. Each function still has exactly one cached address. `SqliteGlobals`
retains the original field and static initialization order.

Split files use file-local aliases. They do not add global aliases that can bind
another translation to this engine's types or helpers. This allows copied source
from differently named wrappers to compile in the same consumer namespace, with
separate embedded runtime state. The HostVfs partial sources import the nested
SQLite types where needed. HostVfs remains a separate platform adapter.

`--runtime=c` omits Zig-only runtime modules, including Slice, while retaining the
complete C runtime dependency closure. `--runtime=all` retains all modules (the
compatibility default); `auto` uses source/object language provenance and keeps
all modules for older objects with unknown provenance. A C-only profile rejects
Zig/unknown objects rather than silently dropping required runtime code. Apply
layout/runtime options at link time for object fragments.

The Roslyn postprocessor proves nested Cond/CBool helper implementations by their
symbols and bodies, using the same checks as namespace-level helpers.

## Cursor initialization audit

`sqlite3BtreeCursorZero` is intentionally a **partial** clear. In SQLite 3.53.4,
`sqlite3.c:78047` calls:

```c
memset(p, 0, offsetof(BtCursor, BTCURSOR_FIRST_UNINIT));
```

`BTCURSOR_FIRST_UNINIT` is `pBt` (`sqlite3.c:72729`). Upstream explains that the
large `apPage`/`aiIdx` arrays need not be zeroed, and trailing fields are initialized
later. Thus `__DotccOffset_4274437572736F722E704274.Value == 32` is correct;
`.Size == 296` describes the entire structure and is not the requested clear size.

The shared backend emits `.Value` for an IR `OffsetOf` expression. `sizeof`
uses the type's size instead. Flexible-array layout attributes legitimately use
`.Size` and `.Alignment`. Native product layout checks compare every emitted
offset request with C. ManagedConsumer additionally pre-fills a BtCursor with
`0xA5` and verifies that this function clears exactly the prefix and preserves
every trailing byte.


## Verification (2026-09-12)

- 2,096 compiler unit tests and 394 functional tests passed (969 opt-in skips).
  Seven isolation cases compile two libraries into one assembly across all split
  modes, source/object linking, and both C-only/full runtime profiles.
- 65 postprocessor tests passed, including nested helper symbol/body proof.
- Raw and postprocessed SQLite builds passed without warnings. ManagedConsumer
  passed under JIT and NativeAOT, including the 32-of-296-byte cursor regression,
  SQL/JSONB/FTS5, UTF-16, callbacks, WAL and cleanup.
- `test-copied-translations.sh` passed under JIT and NativeAOT without project
  references: two generated source trees share `Managed.Database`, keep globals
  and errno separate, and expose distinct cached function-pointer tables.
- The NativeAOT compiler published successfully and emitted the copied-source
  consumer using the new flags; that consumer built and ran successfully. Host
  VFS locking, mmap, WAL recovery and native interoperability also passed.
- All 41 emitted offset contracts matched native C. Tests also passed for 67
  field offsets, 48 aggregate size/alignment and 8 inline-array storage checks,
  under JIT and NativeAOT. The cursor `.Value` use is not an offset/size mix-up.

From the repository root:

```bash
dotnet build DotCC/DotCC.csproj -c Release
SQLITE_AOT=1 sqlite/scripts/test-managed-consumer.sh
SQLITE_AOT=1 sqlite/scripts/test-copied-translations.sh
SQLITE_AOT=1 sqlite/scripts/test-product-layout.sh
sqlite/scripts/test-host-vfs.sh
```

The copied-source test also accepts `DOTCC_EXECUTABLE=/absolute/path/to/dotcc`
for a NativeAOT-published compiler. Execution evidence is Linux x64. No native
SQLite or runtime source generator is required by the translated library.
