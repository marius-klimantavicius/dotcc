# SQLite endian overrides

The translated engine uses `System.BitConverter.IsLittleEndian` for SQLite's
runtime byte-order probes. No SQLite-specific name or source pattern is built
into dotcc. The pinned upstream amalgamation remains unchanged.

[`../config/dotcc-overrides.json`](../config/dotcc-overrides.json) selects the two
original `sqlite3one` probe bodies by exact tokens:

| Macro | Original body | Replacement |
|---|---|---|
| `SQLITE_BIGENDIAN` | `(*(char *)(&sqlite3one)==0)` | `(!__dotcc_is_little_endian())` |
| `SQLITE_LITTLEENDIAN` | `(*(char *)(&sqlite3one)==1)` | `(__dotcc_is_little_endian())` |

Both rules require a match for this portable SQLite profile. Different numeric
upstream definitions remain unchanged; an invocation encountering only numeric
variants deliberately fails this profile's `requireMatch` assertions. `SQLITE_UTF16NATIVE` uses ordinary macro
rescanning; its definition is untouched. The original `sqlite3one` declaration
can remain unused. The intrinsic is `_Bool` in dotcc's typed IR, converted through
`CBool` when C arithmetic requires it. Roslyn postprocessing then inlines `Cond.B`
with the same rules as the rest of the engine.

`emit-engine.sh` passes the profile, writes `artifacts/engine-overrides.jsonl`, and
records source/header/profile dependencies in `artifacts/engine.d`. Reports show
one selected definition per endian macro at upstream lines 16056 and 16057,
25 BIGENDIAN expansions and one LITTLEENDIAN expansion. The generated engine has
26 `BitConverter.IsLittleEndian` reads and no address reads of `sqlite3one`.
Generated sources and build artifacts are ignored by Git; rerun the script to
reproduce them.

## Reproduce validation

From the repository root, after fetching the pinned SQLite reference:

```bash
dotnet build DotCC/DotCC.csproj -c Release
sqlite/scripts/emit-engine.sh --no-postprocess
dotnet run --project sqlite/tests/ManagedConsumer -c Release
SQLITE_AOT=1 sqlite/scripts/test-endian.sh
SQLITE_AOT=1 sqlite/scripts/test-host-vfs.sh
sqlite/scripts/test-product-layout.sh
```

`test-endian.sh` regenerates and postprocesses the engine. Its test-only native
oracle compiles the unmodified amalgamation with SQLite's native OS VFS. It
creates and exchanges UTF-16LE and UTF-16BE databases in both directions, checks
native/LE/BE text binding and retrieval, UTF-8 conversion, supplementary Unicode,
UTF-16 prepare/open APIs, WAL checkpoint/reopen and integrity. The same checks
also run inside normal ManagedConsumer. Native SQLite is only an independent
test oracle, never a dependency of the translated engine.

To test the generic CLI with a NativeAOT compiler and runtime-loaded regex rules:

```bash
dotnet publish DotCC/DotCC.csproj -c Release -r linux-x64 \
  -o sqlite/build/dotcc-overrides-aot
SQLITE_AOT=1 sqlite/scripts/test-macro-overrides-cli.sh
```

This compares an unchanged C fixture with gcc, checks generated JIT/NativeAOT
programs, source/object linking, JSON profile dependencies, preprocessing and
object-only override rejection.

## Validation record — 2026-09-11

- Full compiler suites passed: 2,094 unit tests and 387 functional tests; 969
  opt-in functional tests skipped. The unit suite includes 57 override/intrinsic
  cases, including typedef-name reservation and conditional-report checks.
- NativeAOT compiler published without warnings and loaded the runtime regex
  profile; its generated JIT and NativeAOT programs matched gcc.
- SQLite raw and postprocessed ManagedConsumer builds passed without warnings.
  Core SQL, JSONB, FTS5, optional functions, managed callbacks and cleanup passed.
- UTF-16LE/BE database exchange and all endian-specific C API checks passed
  against native SQLite under JIT and NativeAOT.
- Host VFS raw I/O, locking, mmap, SQL, WAL, native interoperability, forced-kill
  recovery, stale/missing WAL indexes and checkpoint stress passed under JIT and
  NativeAOT. Both independent-process campaigns passed.
- Product layout passed: 41 offsetof contracts, 67 field offsets, 48 aggregate
  size/alignment checks and 8 inline-array storage checks. `engine.d` includes
  the JSON profile.

Executed on Linux x64 (little-endian). Opposite-endian **data** and both Boolean
polarities are tested; this does not claim execution on a big-endian .NET host,
Windows, macOS or BSD. WAT deliberately rejects this runtime intrinsic. Future
structural detection and symbol overrides remain outside this implementation.

See the [generic contract](../../docs/macro-overrides.md) for rule ordering,
regex/capture/formal-template semantics, API options and diagnostics.
