# Validation evidence

Campaign commands run from `sqlite/`; scripts resolve their own absolute roots.

- `dotnet build ../dotcc.sln -c Release`: passed, zero warnings/errors, 9 seconds.
- Baseline unit suite: **1,723 passed, zero failed/skipped**, 35 seconds, with isolated TMPDIR.
  The first default `/tmp` run was stopped after eight minutes without completion;
  compiler include discovery repeatedly traversed an unrelated 18 GB temporary tree.
  `scripts/test-repository.sh` isolates TMPDIR for repeatable test timing.
- Baseline functional suite: **224 passed, zero failed, 819 opt-in skips**, 1 minute
  16 seconds, with isolated TMPDIR. Skips include external GCC/Zig/WAT oracle tests;
  they are not counted as executed translated tests.
- `scripts/native.sh`: 36-case deterministic core/JSON/JSONB native corpus completes;
  expected transcript checked in at `tests/native-corpus.expected`. Translated comparison pending.
- VFS worker strict GCC native suite passes; ASan+UBSan suite passes. Reproduce with
  `scripts/test-vfs-native.sh` and `SQLITE_SANITIZE=1 scripts/test-vfs-native.sh`.
  Covers direct I/O/locking/failure contracts, journal cleanup, transaction recovery,
  reopen/read-only, WAL fallback, JSONB, image export/import and resource cleanup.
- `python3 scripts/fetch.py`: archive SHA-256 verified; upstream source unchanged.
- `scripts/test-api-native.sh`: native API corpus passes; matching full-source
  ASan+UBSan execution passes. Transcript at `tests/native-api.expected`.
  Covers prepare/bind/reset/finalize, embedded NULs, destructor ownership, UTF16,
  scalar/aggregate/collation callbacks, busy/progress/transaction hooks, backup,
  incremental blobs, extended errors and 256 deterministic random mutations
  compared with an independent C model (seed `0x51a17e`).
- `scripts/preprocess.sh > artifacts/sqlite3.i`: passed.
- `scripts/translate.sh`: failed at nested callback declarator; see B001.
- GCC C17 reduced nested-callback fixture: expected output `7` confirmed.

Generated artifacts/logs are ignored under `generated/`, `build/`, `artifacts/`.
No translated SQLite execution or corpus pass is claimed yet.

## First integrated compiler increment

Commits `4db77b5`, `c679ee4`, `0071282`:

- Release solution build: zero warnings/errors, 8 seconds.
- Full unit suite: 1,740 passed, zero failed, 41 seconds.
- Full functional suite: 239 passed, zero failed, 833 opt-in skips, 75 seconds.
  The not-yet-implemented managed-library regression was deliberately excluded;
  it remains uncommitted until its API implementation lands.
- All new parser/macro/layout fixtures have native GCC expected-output evidence.
- Full SQLite retry advances to the nested flexible array B005. Preprocessing now
  registers exactly one `->` and one `->>` with no corrupted spaced operator.
- Raw logs: `artifacts/stringify-*-integrated.log`, earlier offset logs under
  `artifacts/offsetof/`. Generator actual storage/native fixture is documented
  in `generators/README.md`. Full SQLite execution remains pending.

## Public JSONB tests

`scripts/test-upstream-native.sh` passes all 37 adapted SQL assertions and setup
from the matching public `jsonb01.test`, retaining upstream expected bytes and
error behavior. `tests/upstream-jsonb.expected` records the native transcript;
`docs/upstream-tests.md` documents the adaptation and coverage boundary.
Translated execution remains pending.
