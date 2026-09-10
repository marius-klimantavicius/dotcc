# FTS5 coverage

The shared profile statically enables `SQLITE_ENABLE_FTS5` in unchanged SQLite
3.50.4. Core and JSON/JSONB remain enabled. FTS3/FTS4 remain deferred and are
explicitly checked as unavailable. `SQLITE_OMIT_LOAD_EXTENSION` remains enabled;
there is no native SQLite interop or dynamic extension loading.

`tests/fts5_native.c` runs unchanged against native SQLite and translated C#.
`scripts/test-fts5-native.sh` supplies the native oracle; `scripts/test-translated.sh
fts5` emits, builds, executes, and compares its exact transcript against
`tests/native-fts5.expected`. `scripts/verify.sh` includes both runs. Native
sanitizer execution is available with `SQLITE_SANITIZE=1`.

The corpus independently asserts:

- Phrase, NEAR, column-filter, boolean, prefix-index, and initial-token queries;
  malformed-query error handling; BM25 ordering and negative rank; highlight
  and snippet contents.
- unicode61 with accented Latin, Greek matching and diacritic removal; ascii;
  porter stemming; trigram MATCH and LIKE; fts5vocab document/instance counts.
- External-content tables with insert/update/delete triggers, rebuild, and
  external-content integrity checks; contentless token deletion and
  contentless-delete tables.
- Transactions, savepoints, rollback, commit, optimize, integrity checks,
  JSONB composition, reopening through the memory VFS, and final resource cleanup.
- Explicit retrieval of `fts5_api` through `sqlite3_bind_pointer`, auxiliary
  function registration and context destruction, `Fts5ExtensionApi` column,
  instance and rowid callbacks, tokenizer discovery/creation/tokenization/
  destruction, and explicit registration of a tokenizer alias.

Ranking checks assert ordering and sign rather than architecture-sensitive float
text. Result rows are ordered explicitly where order matters. The memory VFS
retains its single-process, serialized, non-durable contract; FTS5 does not add
WAL, mmap or host-file capabilities. The corpus is bounded coverage, not the
complete upstream Tcl FTS5 suite. Custom C# consumer registration belongs to the
separate managed-consumer contract.

Initial actual FTS5 emission reached the tokenizer's externally entered
`non_ascii_tokenchar` loop, where dotcc rejected an unsupported control-flow
shape. Evidence: `artifacts/fts5/engine-baseline-emission.log` and
`engine-baseline.time`. No upstream or emitted-source workaround is permitted;
translation is retried after the shared compiler regression is fixed.

The active native layout inventory grows from 30 to 39 offsetof requests. Nine
new FTS5 aggregates are included: Fts5DlidxIter, Fts5ExprNearset, Fts5ExprNode,
Fts5ExprPhrase, Fts5Iter, Fts5Sorter, Fts5Structure, Fts5TokenDataIter and
Fts5TombstoneArray. Every previous core layout row remains unchanged. The
profile-specific native transcript is `tests/layout-native.expected`; translated
layout comparison must cover this expanded inventory before completion.
