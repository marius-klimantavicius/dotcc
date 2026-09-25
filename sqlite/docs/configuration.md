# Campaign configuration

`config/defines.txt` is the deterministic native/translated corpus profile: C17,
`SQLITE_OS_OTHER=1`, `SQLITE_THREADSAFE=0`, `SQLITE_TEMP_STORE=3`,
`SQLITE_MAX_MMAP_SIZE=0`, `SQLITE_ENABLE_FTS5`,
`SQLITE_ENABLE_MATH_FUNCTIONS`, `SQLITE_ENABLE_PERCENTILE`,
`SQLITE_ENABLE_COLUMN_METADATA`, `SQLITE_ENABLE_PREUPDATE_HOOK`, and
`SQLITE_OMIT_LOAD_EXTENSION`. Default SQLite core, JSON/JSONB, and statically
compiled FTS5 are enabled, together with SQL math functions, median/percentile
aggregates and windows, UTF-8/UTF-16 result-column origin metadata, and preupdate
callbacks. FTS3 and FTS4 remain deferred. There is no native SQLite interop or dynamic
extension loading. Translated C corpora use the same authored `src/MemoryVfs.cs` as the product,
with the memory VFS selected as default. Native reference builds alone compile
`tests/native/memory_vfs.c`. The
managed-library build applies `config/host-defines.txt` afterward, enabling
`SQLITE_THREADSAFE=1`, `SQLITE_MUTEX_APPDEF=1`, `DOTCC_HOST_VFS=1`,
`SQLITE_DEFAULT_MMAP_SIZE=67108864` and `SQLITE_MAX_MMAP_SIZE=268435456`. It compiles the
[real host VFS](host-vfs.md) sidecars; `dotcc-host` supplies its OS interface and
uses BCL file I/O plus explicitly permitted OS-level P/Invoke.

Initial host: Linux x64, little-endian LP64; .NET SDK 10.0.111, runtime 10.0.11,
GCC on Ubuntu 24.04/Zorin 18. dotcc targets 64-bit pointers/long/size_t.
Solution builds resolve NuGet `SharpAstro.LALR.CC` 4.7.0 (verified in
`DotCC.Lib/obj/project.assets.json`); campaign rebuilds explicitly set
`UseLocalLalrCc=false`. The product supports concurrent connections and serialized
connections by default, with BCL mutexes and read-only database mappings. See
[threading and mmap](threading-mmap.md) for lifecycle and ownership requirements.
The host VFS implements WAL shared-memory methods and OS byte locks;
WAL is selected per database with `PRAGMA journal_mode=WAL`. The managed host VFS now provides
real disk persistence, flushes and coordination between independent processes;
the memory VFS retains its original process-local contract.

`SQLITE_MAX_MMAP_SIZE=0` explicitly matches the memory VFS's lack of mapped
reads. Without it, native GCC/Linux selects a nonzero platform default even
with `SQLITE_OS_OTHER=1`, while dotcc's platform-neutral preprocessing selects
zero. That difference changes `PRAGMA mmap_size=1000000`: the zero-limit profile
returns one integer-zero row, whereas the native nonzero limit with this VFS
returns no row. Both corpus engines use the same explicit zero-limit profile;
the memory VFS advertises file methods version 1. The host product instead
advertises version 3 for WAL and database `xFetch`/`xUnfetch` mappings.
The unchanged upstream disabled-mmap stubs contain unused parameters. Native
builds retain those warnings in their diagnostic logs using
`-Wno-error=unused-parameter`; other enabled warnings remain errors in strict
harness builds. This warning policy does not change generated code or features.

## Bit-field layout selected for the native oracle

The native oracle uses GNU/System V bit-field placement with
`-mno-ms-bitfields` through `config/native-flags.txt`. The shared compiler now
places unpacked fields from a common bit-position map, including ordinary-byte
prefix reuse, mixed integer widths, zero-width boundaries and ordinary-member
tail reuse. This profile was selected by comparing actual native and translated
storage; LP64 scalar widths alone do not establish bit-field compatibility.

The old `-mms-bitfields` flag was an explicit workaround for the compiler's earlier
whole-unit packing. Its measured transcript is preserved unchanged in
`tests/layout-native-ms.reference`. It is no longer the product oracle. Fresh GNU
measurements, including all 39 active FTS5-profile offsetof requests and actual
storage, now supply `tests/layout-native.expected`. The older
`layout-native-sysv.reference` is historical comparison data from an earlier
feature profile; it must not replace a current measurement. Set
`SQLITE_MS_BITFIELDS=1` on `scripts/layout-native.sh` to reproduce the historical
MS packing comparison; the default and explicit `0` use the current GNU profile.

The measured GNU profile has `ExprList_item` size24/union offset20,
`VdbeCursor` size112/seekHit offset6/aType offset112, `WhereInfo` size856 and
`WhereLoop` size104. The current FTS5 feature configuration has `Parse` size424;
earlier non-FTS comparison values must not be mixed with this oracle.
`scripts/test-product-layout.sh` independently checks the threaded product using
the same configured native flags. No SQLite feature definitions or upstream
sources changed for this profile migration. Reference downloads remain unchanged;
the existing hash-checked mutex-selection adaptation is documented in
[threading and mmap](threading-mmap.md).

This is an executed profile for the observed SQLite layouts, not a promise of
every compiler/target bit-field ABI. Packed aggregates retain their separately
documented compiler behavior. Current commands and execution evidence are in
[layout-storage.md](layout-storage.md) and [validation.md](validation.md).

## Plain char in the C# target

The native oracle also uses `-funsigned-char`. Generated C# stores plain `char`
as `byte`, and dotcc's `limits.h` defines `CHAR_MIN=0`, `CHAR_MAX=255`. An
actual generated executable prints `255 0 0 255` for `(char)255`, its comparison
with zero, and those limits; GCC with `-funsigned-char` prints the same values.
Default x64 GCC instead prints `-1 1 -128 127`. Evidence is under `artifacts/abi/`.
This selects the native oracle's plain-char behavior without changing SQLite
feature definitions. Explicit `signed char` remains signed.

## Additional SQL functions and result metadata

The shared profile enables `SQLITE_ENABLE_MATH_FUNCTIONS`,
`SQLITE_ENABLE_PERCENTILE`, and `SQLITE_ENABLE_COLUMN_METADATA` for both the
native comparison builds and translated engine. Math uses dotcc's BCL-backed
libc, including inverse hyperbolic functions used in SQLite's callback tables.
Percentiles use SQLite's unchanged aggregate/window implementation. The optional
ordered-set `WITHIN GROUP` syntax is not enabled; use SQLite's function-call
syntax such as `percentile_cont(value, 0.5)`.

The metadata APIs expose original database/table/column names even when a query
uses aliases and views. Computed expressions have no source column metadata.
These APIs retain SQLite's borrowed-string lifetime rules; copy names before
reset/reprepare/finalize or other calls that invalidate their storage.
`tests/optional_features.h` runs in the native/translated API corpus, and the
separate managed consumer calls all six UTF-8/UTF-16 origin APIs directly.

## Preupdate callbacks

`SQLITE_ENABLE_PREUPDATE_HOOK` enables `sqlite3_preupdate_hook` and the
`sqlite3_preupdate_old`, `sqlite3_preupdate_new`, `sqlite3_preupdate_count`,
`sqlite3_preupdate_depth`, and `sqlite3_preupdate_blobwrite` APIs in both profiles.
Register a cached managed function pointer and an optional context pointer;
native interop is not required. Registering a null callback disables the hook
and returns the previous context pointer.

Read old values only for UPDATE/DELETE and new values only for INSERT/UPDATE,
inside the callback on the supplied connection. Copy any values needed afterward:
SQLite owns them and invalidates them when the callback returns. Rowid arguments
are defined only for rowid tables: the old rowid on UPDATE/DELETE and the new rowid
on INSERT/UPDATE. Hooks cover real tables, including changes made by triggers;
they do not report virtual-table changes directly.

`tests/ManagedConsumer/PreupdateHook.cs` demonstrates registration and cleanup,
captures INSERT/UPDATE/DELETE old/new values, checks trigger depth and rowid
changes, and verifies that unregistering stops delivery.
