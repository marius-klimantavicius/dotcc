# Campaign configuration

`config/defines.txt` is the shared native/translated profile: C17,
`SQLITE_OS_OTHER=1`, `SQLITE_THREADSAFE=0`, `SQLITE_TEMP_STORE=3`, and
`SQLITE_OMIT_LOAD_EXTENSION`. Default SQLite core and JSON/JSONB remain enabled;
all FTS opt-in defines remain absent. There is no native interop or dynamic
loading. A portable memory VFS supplies the OS interface.

Initial host: Linux x64, little-endian LP64; .NET SDK 10.0.111, runtime 10.0.11,
GCC on Ubuntu 24.04/Zorin 18. dotcc targets 64-bit pointers/long/size_t.
Initial solution build resolves NuGet `SharpAstro.LALR.CC` 4.7.0 (verified in
`DotCC.Lib/obj/project.assets.json`); a forced NuGet rebuild remains a validation item. Calls are serialized; WAL/shared memory, mmap,
process durability and concurrent hosting are unsupported platform capabilities.

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
All other selected aggregates match in both modes. `scripts/layout-native.sh`
prints sizeof/alignment/offsetof alongside actual address differences; set
`SQLITE_MS_BITFIELDS=0` for the separate default-GCC comparison. Expected matching
layout is `tests/layout-native.expected`; `layout-native-sysv.reference` is an
explicit comparison, not the oracle's ABI.

This is a verified profile for the observed SQLite layouts, not a claim that every
MS ABI corner case is implemented. Zero-width and mixed/union bit-field cases
remain subject to the shared compiler's layout audit. Translated SQLite layout
comparison is still required once full emission succeeds.
