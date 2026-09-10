# Campaign configuration

`config/defines.txt` is the deterministic native/translated corpus profile: C17,
`SQLITE_OS_OTHER=1`, `SQLITE_THREADSAFE=0`, `SQLITE_TEMP_STORE=3`,
`SQLITE_MAX_MMAP_SIZE=0`, `SQLITE_ENABLE_FTS5`,
`SQLITE_ENABLE_MATH_FUNCTIONS`, `SQLITE_ENABLE_PERCENTILE`,
`SQLITE_ENABLE_COLUMN_METADATA`, and `SQLITE_OMIT_LOAD_EXTENSION`. Default SQLite core, JSON/JSONB, and statically
compiled FTS5 are enabled, together with SQL math functions, median/percentile
aggregates and windows, and UTF-8/UTF-16 result-column origin metadata. FTS3 and
FTS4 remain deferred. There is no native SQLite interop or dynamic
extension loading. Deterministic C corpora use the portable memory VFS. The
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

The compiler already packs adjacent same-sized bit-fields into complete storage
units. LP64 specifies scalar sizes but does not uniquely specify bit-field packing.
The campaign therefore explicitly uses GCC `-mms-bitfields` through
`config/native-flags.txt` for the native oracle, matching dotcc's existing storage
unit layout on this x64 LP64 host. This changes no SQLite feature definitions and
introduces no native interop. The SQL algorithms remain upstream C. The host
product uses one hash-checked mutex-selection guard adaptation in a generated
input copy, documented in [threading and mmap](threading-mmap.md); reference
downloads remain unchanged. `scripts/test-product-layout.sh` independently
compares the actual threaded product layout with GCC using these same ABI flags.

Actual upstream probes show why the flag is required: `ExprList_item` has size32
and union offset24 in this profile, versus size24/offset20 with default GCC;
`VdbeCursor` has size120, seekHit offset12 and aType offset120, versus112/6/112.
The expanded probe of all active offsetof requests also finds differences in
`Parse` (size424 versus416), `WhereInfo` (size864 versus856), and `WhereLoop`
(size112 versus104), including their fields following bit-field storage.
`scripts/layout-native.sh` prints sizeof/alignment/offsetof alongside actual
address differences for all 39 active requests in the FTS5 profile and additional aggregates; set
`SQLITE_MS_BITFIELDS=0` for the separate default-GCC comparison. Expected matching
layout is `tests/layout-native.expected`; `layout-native-sysv.reference` is an
explicit comparison, not the oracle's ABI.

This is a verified profile for the observed SQLite layouts, not a claim that every
MS ABI corner case is implemented. Zero-width and mixed/union bit-field cases
remain subject to the shared compiler's layout audit. The actual translated layout
probe passes under both JIT and NativeAOT; see `layout-storage.md`.

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
