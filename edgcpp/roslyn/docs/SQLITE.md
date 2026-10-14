# SQLite amalgamation RoslynObj export

The untouched official SQLite 3.54.0 amalgamation exports successfully as one
format 2.0 object, including JSON/JSONB, FTS3/4/5, WAL and the additional features
listed below. Managed reader admission and bounded property navigation pass.
SQLite and EDG core sources were not modified, and the existing frozen native
compiler was reused without rebuilding.

The object is
`/mnt/storage/roslynobj-sqlite-20261014/export/sqlite3.roslynobj`:
297,556,992 bytes (283.77 MiB), with 3,887,908 recorded entities and 43,228 atoms.
Its SHA256 is
`e26d8937c94f5ccc9e3fca7915f53c4a31a1bb63432419a847d99394ab06bb1c`.
The complete snapshot uses 1 KiB pages and the exact format 2.0 contract described
in [FORMAT.md](FORMAT.md). Foreign keys were checked by the producer before
publication.

## Source and configuration

The [official download](https://sqlite.org/2026/sqlite-amalgamation-3540000.zip)
was acquired from the [SQLite download page](https://sqlite.org/download.html).
The 2,990,479-byte archive matches its published SHA3-256:
`7b670a62fdfbd672b75fef004cb703c8a3e87d3a5cc7d675b4a08337004a2d93`.
The unchanged `sqlite3.c` SHA256 is
`2da161e310ef0aba3465f22a4d12b8a43723236f0942fa76e8aac523c3317243`.
The [source provenance](sqlite-source-provenance.json) records hashes of all four
extracted files; this is an official release archive, not a Git checkout.

Only `sqlite3.c` was compiled. The command uses GNU C17, actual GCC 13 host
predefines/system includes, a 64 MiB process stack and these 19 definitions:

```text
SQLITE_THREADSAFE=1
SQLITE_ENABLE_FTS4=1             SQLITE_ENABLE_FTS5=1
SQLITE_ENABLE_RTREE=1            SQLITE_ENABLE_GEOPOLY=1
SQLITE_ENABLE_SESSION=1          SQLITE_ENABLE_PREUPDATE_HOOK=1
SQLITE_ENABLE_RBU=1              SQLITE_ENABLE_DBSTAT_VTAB=1
SQLITE_ENABLE_DBPAGE_VTAB=1       SQLITE_ENABLE_STMTVTAB=1
SQLITE_ENABLE_BYTECODE_VTAB=1     SQLITE_ENABLE_MATH_FUNCTIONS=1
SQLITE_ENABLE_COLUMN_METADATA=1  SQLITE_ENABLE_STAT4=1
SQLITE_ENABLE_SNAPSHOT=1          SQLITE_ENABLE_CARRAY=1
SQLITE_ENABLE_NORMALIZE=1         SQLITE_ENABLE_UNLOCK_NOTIFY=1
```

JSON/JSONB and WAL use their release defaults. No JSON, WAL, virtual-table or
other omission flags were added; no obsolete `SQLITE_ENABLE_JSON1` was used.
FTS4 includes FTS3, and session support includes its required preupdate hook.
The [actual source feature matrix](sqlite-feature-matrix.json) confirms the
requested definitions and absence of
the omission guards. The configuration follows SQLite's
[compile-time options](https://sqlite.org/compile.html). ICU and other external
integrations, shell/tests and runtime SQL execution are outside this export.

The [producer provenance](sqlite-producer-provenance.json) identifies the immutable
compiler and pinned EDG source. The [actual recipe](sqlite-recipe.json) freezes
all forwarded arguments, host probes and driver hash. The first attempt published
without retries: [result](sqlite-result.json). Combined invocation wall time was
27.670 seconds, with stock frontend CPU 0.54 seconds and backend CPU 23.28 seconds;
peak process RSS was 604,384 KiB. These are different clocks, not an exporter time
derived by subtraction.

## Bounded reader and feature evidence

Reader admission passes for the actual object. The existing project-input helper
samples four routine roots: `sqlite3_open_v2`, `jsonExtractFunc`, `sqlite3Fts5Init`
and `sqlite3WalCheckpoint`, plus the recorded `sqlite3` C type/alias descriptors.
Cold and warm navigation cover 526 operations and 32 calls; two roots reach the
256-operation cap. This sample passes 2,749 assertions. Nineteen further exact-name
symbol checks pass 153 assertions with zero additional routine roots or operation
traversal. The navigation checks pass 2,902 assertions, with admission verified
separately.

The [feature-definition proof](sqlite-feature-symbols.json) records 23 unique
defined routines with retained types and bodies, including JSONB, FTS3/5, RTree,
GeoPoly, session, preupdate, RBU, dbstat/dbpage/statement/bytecode virtual tables,
math, column metadata, STAT4, snapshot, carray, normalization and unlock notify.
The [reader report](sqlite-reader-acceptance.json) and
[managed inventory](sqlite-managed-ready.json) link exact artifact identity,
logs and sample limits; the [native inventory](sqlite-native-ready.json) records
source/configuration/publication evidence.

Exact-name queries and public navigation have bounded results and materialization;
their SQL may inspect many rows. This is retained-definition and consumer proof,
not full-graph validation or proof of SQLite runtime behavior. No SQLite
SQL-engine execution tests, CLI-shell run or upstream test suite were performed.

## Reproducing the export

The [owned driver](../build/sqlite_export.py) accepts a package root containing
the untouched extracted source, `source-provenance.json` and
`producer-provenance.json`. Those inputs are preserved under the dedicated root
above; the committed provenance files show their exact schema and hashes. It
checks source/compiler identity, probes the actual host environment and refuses
to overwrite an existing attempt directory.

From the repository root, a new attempt can be generated with:

```sh
python3 edgcpp/roslyn/build/sqlite_export.py \
  --root /mnt/storage/roslynobj-sqlite-20261014 \
  --attempt-root /mnt/storage/roslynobj-sqlite-20261014/reproduction-1
```

The original successful attempt and its ignored driver remain unchanged. The
owned driver adds configurable paths and release identity records; it was checked
with help/syntax/whitespace validation without repeating generation. No prior
SQLite or dotobj campaign evidence was used.
