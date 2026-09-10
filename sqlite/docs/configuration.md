# Campaign configuration

`config/defines.txt` is the shared native/translated profile: C17,
`SQLITE_OS_OTHER=1`, `SQLITE_THREADSAFE=0`, `SQLITE_TEMP_STORE=3`,
`SQLITE_MAX_MMAP_SIZE=0`, `SQLITE_ENABLE_FTS5`, and
`SQLITE_OMIT_LOAD_EXTENSION`. Default SQLite core, JSON/JSONB, and statically
compiled FTS5 are enabled; FTS3 and FTS4 remain deferred. There is no native SQLite interop or dynamic
extension loading. Deterministic C corpora use the portable memory VFS. The
managed-library build additionally defines `DOTCC_HOST_VFS=1` and compiles the
[real host VFS](host-vfs.md) sidecars; `dotcc-host` supplies its OS interface and
uses BCL file I/O plus explicitly permitted OS-level P/Invoke.

Initial host: Linux x64, little-endian LP64; .NET SDK 10.0.111, runtime 10.0.11,
GCC on Ubuntu 24.04/Zorin 18. dotcc targets 64-bit pointers/long/size_t.
Solution builds resolve NuGet `SharpAstro.LALR.CC` 4.7.0 (verified in
`DotCC.Lib/obj/project.assets.json`); campaign rebuilds explicitly set
`UseLocalLalrCc=false`. Calls are serialized; database mmap and multithreaded SQLite hosting remain
unsupported. The host VFS implements WAL shared-memory methods and OS byte locks;
WAL is selected per database with `PRAGMA journal_mode=WAL`. The managed host VFS now provides
real disk persistence, flushes and coordination between independent processes;
the memory VFS retains its original process-local contract.

`SQLITE_MAX_MMAP_SIZE=0` explicitly matches the memory VFS's lack of mapped
reads. Without it, native GCC/Linux selects a nonzero platform default even
with `SQLITE_OS_OTHER=1`, while dotcc's platform-neutral preprocessing selects
zero. That difference changes `PRAGMA mmap_size=1000000`: the zero-limit profile
returns one integer-zero row, whereas the native nonzero limit with this VFS
returns no row. Both engines now use the same explicit zero-limit profile;
the memory VFS advertises file methods version 1, while the host VFS advertises
version 2 for WAL. Neither advertises `xFetch` capability.
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
introduces no native interop. The SQL engine is still the unchanged upstream C.

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
