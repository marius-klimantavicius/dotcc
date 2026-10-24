# Npgsql embedded qualification mapping

The acceptance project is an ordinary external consumer of the copied-source
[Npgsql project](../src/Npgsql/Npgsql.csproj), through `NpgsqlDataSource`, commands,
readers, batches and transactions. Every embedded query reaches the actual
translated Managed.PGlite backend. The project contains no database replacement,
socket facade, JavaScript, WASM or native database product dependency.

## Inputs and attribution

Official Npgsql **v10.0.1**, commit
`12d792ac2163596385152dcea5c68ba99769731f`, is the test source. The tag was
resolved through the official GitHub ref API. The test-source archive is
<https://codeload.github.com/npgsql/npgsql/tar.gz/refs/tags/v10.0.1>.
The machine-readable [mapping](../tests/NpgsqlAcceptance/source-mapping.json)
records archive/source SHA-256 values, exact fixture methods and case IDs. The
[PostgreSQL license](../tests/NpgsqlAcceptance/LICENSE-NPGSQL.txt) is retained.
The core remains pinned to
`29b6424aa312ec3556247db069939006dd27f878` (PostgreSQL 18.6).

These are selected, adapted public assertions from the official suite plus
new embedding regressions. This is not a claim to run the complete upstream
NUnit suite. External server configuration, multiplexing fixtures, optional
extensions, authentication, EF/Dapper, replication and custom type ecosystems
are outside this harness. The original ManagedConsumer's 36+391 assertions are
not edited or replaced by this project.

## Assertion mapping

Upstream links below are relative to
<https://github.com/npgsql/npgsql/tree/v10.0.1/test/Npgsql.Tests>.
The JSON mapping identifies the exact source file hashes and methods for every
case. Additional input values strengthen the selected assertion; they do not
turn an unsupported outcome into a pass.

| Local case | Official fixture/method | Assertions retained or added |
| --- | --- | --- |
| `protocol.server-metadata` | `ConnectionTests.PostgreSqlVersion_ServerVersion`, `Client_encoding_is_UTF8_by_default` | Server/core version 18.6, both UTF8 settings, UTC, connection state and embedded zero timeout. |
| `protocol.live-session-gucs` | `CommandTests.Non_standards_conforming_strings`, `SqlQueryParserTests.Non_conforming_string` | Live application_name (including PostgreSQL ASCII sanitization) and standard_conforming_strings ParameterStatus match actual SET/SHOW; named parameters respect backslash-escaped quotes; reset and recovery. |
| `protocol.tracing-all-data` | `TracingTests.CommandExecute`, `CommandExecute_error` | Actual ActivityListener requests AllData; successful/error/recovery queries, source/kind/status/SQL/database tags, first-response event and SQLSTATE tags. |
| `protocol.rejected-reload-preserves-codecs` | `CommandTests.Use_after_reload_types_invalidates_cached_infos` plus new rejection regressions | Live/pre-cancelled reloads and sync/async busy-reader reloads reject without poisoning int[] codecs or the reader; successful sync/async reloads retain working codecs. |
| `protocol.reader-metadata` | `ReaderTests.FieldCount`, `GetFieldType`, `GetDataTypeName`, `GetDataTypeOID`, `GetName`, `GetOrdinal_case_insensitive`, `Get_value_by_name` | Column count/aliases, case insensitive ordinals, exact CLR types/type names/OIDs, schema and named values. |
| `protocol.prepared-reuse` | `PrepareTests.Basic`, `Async`, `Unprepare`, `Reuse_prepared_statement` | Prepare state, changed parameter values, sync/async prepare/unprepare, actual `pg_prepared_statements` creation/deallocation. |
| `protocol.multiple-results` | `CommandTests.Multiple_statements_with_parameters` | All six original row/value/NextResult assertions, prepared and unprepared. |
| `protocol.batch-parameters` | `BatchTests.Named_parameters`, `Positional_parameters` | All eleven original assertions per variant; default and sequential readers. |
| `protocol.batch-records-affected` | `BatchTests.RecordsAffected_and_Rows` | Every per-command affected/returned count plus aggregate count. |
| `protocol.batch-error-recovery` | `BatchTests.Batch_with_result_sets_and_error`, `NpgsqlException_references_BatchCommand_with_multiple_commands` | Exact upstream PL/pgSQL RAISE fixture, NextResult exception and failing command identity, no recycling of exception-referenced batch, SQLSTATE/message and recovery. Constant division planning errors are retained separately in `protocol.batch-constant-error-identity`. |
| `protocol.transactions-commit-rollback` | `TransactionTests.Commit`, `CommitAsync`, `Rollback`, `RollbackAsync` | Own-write count, completed transaction rejects reuse, Connection getter before/after disposal, committed/rolled-back count, each prepared/unprepared. |
| `protocol.transaction-dispose-rollback` | `TransactionTests.Rollback_on_Dispose`, `Empty_rollback` | Transaction disposal rolls back the inserted row; empty rollback completes. |
| `protocol.sqlstate-recovery` | `CommandTests.ExecuteNonQuery_Throws_PostgresException`, `TransactionTests.Rollback_failed` | Syntax/division/missing-relation SQLSTATE, severity, usable open connection after each error; aborted transaction SQLSTATE, rollback and absence of inserted row. |
| `protocol.early-reader-dispose` | `ReaderTests.Cleans_up_ok_with_dispose_calls`, `GetBytes` | Dispose unread rows/results or partial fields and run another command successfully, sync/async/default/sequential. |
| `protocol.same-connection-misuse` | `ReaderTests.Reader_is_still_open` | All four original operation-in-progress rejections; first reader and later commands remain usable. |
| `protocol.parameter-reuse` | `CommandTests.Same_command_different_param_values`, `Same_command_different_param_instances` | Replacing parameter value and instance changes actual result. |
| `codecs.null` | `ReaderTests.Null`, selected `MiscTypeTests.Null` | All seven ReaderTests NULL assertions for each column; non-generic CLR null rejects. |
| `codecs.text-unicode` | `TextTests.Text_as_string`, `Truncate` | Literal/parameter equality, type names, empty/Unicode/combining/quoted text; all Size/truncation assertions, including the original second `Value` assignment before increasing Size. |
| `codecs.bytea` | `ByteaTests.Bytea` | Original five-byte and empty vectors, all 256 byte values, exact literal/parameter values and SQL equality. |
| `codecs.integer-float-bool` | `NumericTypeTests.Int16`, `Int32`, `Int64`, `Float`, `Double`; `MiscTypeTests.Boolean` | Signed integer limits, float/double values, both bools and bool array; literal and binary-parameter equality/type metadata. |
| `codecs.numeric-all-upstream-vectors` | `NumericTests.ReadWriteCases`, `Read`, `Write`, `NumericZero_WithScale` | **All 60 original SQL/decimal vectors**, in original order. Exact `decimal.GetBits` for both read and write, SQL equality for every write, zero scale. No normalization or numeric tolerance. |
| `codecs.numeric-overflow-recovery` | `NumericTests.Read_overflow_is_safe` | Both original overflow rows; exact exception class/message, following field, public FullState/State, subsequent query. Upstream internal `ReaderState` is inaccessible to an ordinary consumer; public state/field progress remain asserted. |
| `codecs.arrays` | `ArrayTests.Arrays`, `NullableInts`, `Read_null_as_non_nullable_array_throws`, `Strings_with_null` | Integer/empty/multidimensional/text-with-NULL arrays; dimensions, bounds and every element; both original nullable vectors, typed values and InvalidCastException with exact original message for nonnullable reads. Uses explicit typed read rather than changing source-wide array nullability configuration. |
| `codecs.date-time-uuid` | `DateTimeTests.Date_as_DateOnly`, `Time_as_TimeOnly`, `Timestamp_as_DateTime`, `Timestamptz_as_DateTime`, `Interval_as_TimeSpan`; `MiscTypeTests.Uuid` | Exact date/time/timestamp/UTC/interval/UUID literal and parameter values, server equality and types. Includes pre-2000 values. |
| `codecs.large-fragmented-values` | `TextTests.Text_long`, `ByteaTests.Bytea_long` | Byte/text sizes around 8192 and above 256 KiB, split UTF8 characters, exact values and byte lengths; 2049 rows with exact ordered values. This tests buffered transport, not bounded-memory streaming. |
| `codecs.sequential-getbytes` | `ReaderTests.GetBytes` | Original public sequential-access assertions with 32771 bytes, partial reads, no seek-back, length, clipping, buffer bounds, column skipping and close mid-column. |
| `protocol.same-pin-oracle` | New common SQL inventory | All 26 [native oracle vectors](../tests/NpgsqlAcceptance/oracle-cases.json), exact rows/NULLs and SQLSTATE through actual Npgsql. Native independently asserts backend readiness and completion tags. |

`Roundtrip<T>` is a bounded adaptation of upstream `TestBase.AssertType`: it
checks literal reads and explicitly typed binary parameters, exact typed values,
server-side equality and type names. It does not claim all of AssertType's
inference permutations, alternate CLR mappings, list forms or implicit
array-of-every-scalar expansion. Every selected vector has concrete expected
values rather than accepting whichever value the implementation produces.

## Embedded profile regressions

- All ten sync/async binary/text/raw streaming COPY APIs reject with
  `NotSupportedException`, before binding or sending the command.
- Direct/prepared client COPY SQL, named NpgsqlBatch commands (prepared and
  unprepared), leading comments, semicolon batches, dynamically
  constructed DO bodies and a stored PL/pgSQL function reject with PostgreSQL
  SQLSTATE `0A000`. Each error is followed by a successful query. The engine
  utility hook performs this guard; ordinary literals/comments containing COPY
  remain legal. Ordinary UTF8 file COPY TO/FROM remains supported and round trips.
- Cancel and nonzero command/batch timeout reject. A live cancellable token
  rejects before entry; a pre-cancelled token yields `OperationCanceledException`;
  `CancellationToken.None` succeeds. Reader token rejection must leave the reader
  positioned before its first row. Running cancellation is not claimed.
- Pooling and multiplexing are disabled; default command timeout is zero.
- Notification Wait/WaitAsync overloads, ChangeDatabase, CloneWith/CloneWithAsync
  and ConnectionString mutation on a closed embedded clone reject. Plain Clone
  retains the exclusive embedded data source and cannot escape to socket mode.
  A subsequent query proves these rejections preserve the active connection.
- A second connection from either the same data source or another data source
  cannot acquire the active session. Failed acquisition preserves the first
  connection's results and usability.
- An AllData ActivityListener observes actual physical open and query spans.
- A managed extension close callback failure propagates its original exception,
  leaves the connection Closed, releases the lease, preserves committed data and
  permits the same data source to reopen and persist additional writes.
- Closing an active transaction rolls back. Reopening on the same data source,
  then a new data source, preserves committed Unicode data while resetting temp
  tables, prepared statements, application_name and transaction state. New writes
  succeed after reopen.

## Reproduction

The project has ordinary no-argument Debug/IDE defaults and references the
existing paired processed projects. It reuses `BootstrapAssets.targets` and
`NativeAotNotices.targets`; bootstrap assets and notices follow the selected
products into build/publish outputs. Both project-selection properties can
select paired raw forms. No shared obj/bin output is needed for qualification.

From any working directory, use the absolute script path:

```bash
bash /path/to/dotcc/pglite/scripts/npgsql-test.sh \
  --form all --mode all --rid linux-x64 \
  --work-root /private/path/npgsql-qualification
```

The default specialist row is processed/JIT; the default private output root is
created through the host temporary-directory API. `--configuration Debug` also
exercises ordinary default project settings. Every process runs from an unrelated
working directory and relies on copied bootstrap assets. `--skip-build` reruns
previously built outputs with new database/work directories. Independently run
rows can be combined using `--collect-only --form all --mode all` with the same
work root; this revalidates current source snapshots, executable/report hashes
and matching assertion counts without rerunning passing processes. `--no-restore` is
available after matching restores. Every row writes command/build/run logs,
SHA-256 hashes, semantic assertion/case counts and a JSON receipt. Attempt-specific
logs and reports preserve earlier failures. The executed case inventory must
match the mapping exactly, and assertion counts must agree across matrix rows.
The runner records authored/generated source input hashes before and after each
build and rejects a changed source snapshot before qualifying its output. A failing case
or missing semantic result fails the process and matrix; a partial passing count
is not acceptance. The summary writer uses `Utf8JsonWriter` and the case registry
is explicit, so neither requires reflection or dynamic code under NativeAOT.

The matching native oracle can reuse existing native build caches:

```bash
bash /path/to/dotcc/pglite/scripts/npgsql-native-oracle.sh \
  --native-cache /path/to/dotcc/pglite/build/native-control/14abb02f16cfb8013f7c \
  --control /path/to/dotcc/pglite/build/native-control/runs/20261022-163448-222180a4/native-control/embedding-control \
  --work-root /private/path/npgsql-native-oracle
```

It validates both recorded core pins and the control's configured installation
path, uses private clusters/transcripts and never rebuilds or mutates the cache.
It checks 26 cases × four observations plus one deliberate wrong-row rejection:
**105 native oracle assertions**. This native callback executable is used only as
an oracle. It is not loaded by the managed acceptance program or product.
Native readiness/command-tag comparisons are additional native observations;
the Npgsql row/SQLSTATE checks are shared through the same JSON inputs.

To verify the official source mapping and unchanged numeric inputs separately:

```bash
python3 /path/to/dotcc/pglite/scripts/npgsql-source-check.py \
  --source-root /private/path/npgsql-10.0.1 \
  --report /private/path/source-check.json
```

The embedded profile enables `IncludeFailedBatchedCommand`, matching the upstream
identity fixture prerequisites. `AssertTypeUnsupportedRead<T>` has a CLR target
type parameter; its default exception is `InvalidCastException`, as shown in
`Support/TestBase.cs:395`. The typed `int[]` rejection maps directly to
`ArrayTests.Read_null_as_non_nullable_array_throws`.

Upstream `NpgsqlTransaction.IsCompleted` is internal. An ordinary consumer
asserts the same completed-state predicate through the public `IsolationLevel`
getter, which must throw `InvalidOperationException` once the transaction has
completed; disposed Connection access is checked separately.

Runtime results, counts and known limitations belong to the current validation
receipt. A source/provenance pass alone does not qualify runtime behavior.
