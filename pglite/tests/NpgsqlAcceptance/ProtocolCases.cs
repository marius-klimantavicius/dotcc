using System.Data;
using System.Diagnostics;
using System.Collections.Concurrent;
using Npgsql;
using NpgsqlTypes;

namespace NpgsqlAcceptance;

internal sealed partial class Suite
{
    internal IEnumerable<AcceptanceCase> ProtocolCases()
    {
        yield return new("protocol.server-metadata", ServerMetadata);
        yield return new("protocol.live-session-gucs", LiveGucs);
        yield return new("protocol.tracing-all-data", Tracing);
        yield return new("protocol.rejected-reload-preserves-codecs", ReloadPreservesCodecs);
        yield return new("protocol.same-pin-oracle", SamePinOracle);
        yield return new("protocol.reader-metadata", ReaderMetadata);
        yield return new("protocol.prepared-reuse", PreparedReuse);
        yield return new("protocol.multiple-results", MultipleResults);
        yield return new("protocol.batch-parameters", BatchParameters);
        yield return new("protocol.batch-records-affected", BatchRecords);
        yield return new("protocol.batch-error-recovery", BatchRuntimeError);
        yield return new("protocol.batch-constant-error-identity", BatchError);
        yield return new("protocol.transactions-commit-rollback", Transactions);
        yield return new("protocol.transaction-dispose-rollback", TransactionDispose);
        yield return new("protocol.sqlstate-recovery", SqlStateRecovery);
        yield return new("protocol.early-reader-dispose", EarlyDispose);
        yield return new("protocol.same-connection-misuse", SameConnectionMisuse);
        yield return new("protocol.parameter-reuse", ParameterReuse);
    }

    async Task ServerMetadata()
    {
        check.Equal(ConnectionState.Open, connection.State, "open State");
        check.Equal(18, connection.PostgreSqlVersion.Major, "pinned PostgreSQL major");
        check.Equal<object?>("18.6", await Scalar("SHOW server_version"), "actual core version");
        check.Equal<object?>("UTF8", await Scalar("SHOW client_encoding"), "actual client encoding");
        check.Equal<object?>("UTF8", await Scalar("SHOW server_encoding"), "actual server encoding");
        check.Equal<object?>("UTC", await Scalar("SHOW TimeZone"), "UTC fixture");
        check.True(connection.ServerVersion.StartsWith("18.6", StringComparison.Ordinal), "Npgsql ServerVersion from core");
        check.True(connection.PostgresParameters.TryGetValue("is_superuser", out string? superuser), "real startup metadata includes is_superuser");
        check.Equal("on", superuser, "trusted bootstrap role is superuser");
        check.Equal<object?>(superuser, await Scalar("SELECT current_setting('is_superuser')::text"), "Npgsql superuser metadata matches actual SQL setting");
        check.Equal<object?>(connection.UserName, await Scalar("SELECT session_user::text"), "Npgsql username matches actual trusted session role");
        using var cmd = connection.CreateCommand();
        check.Equal(0, cmd.CommandTimeout, "embedded command default timeout");
    }

    async Task LiveGucs()
    {
        string previousApplication = (string)(await Scalar("SHOW application_name"))!;
        try
        {
            await Execute("SET application_name = 'npgsql-live-Žąsis'");
            check.Equal(@"npgsql-live-\xc5\xbd\xc4\x85sis", connection.PostgresParameters["application_name"], "actual live application_name ParameterStatus");
            check.Equal<object?>(@"npgsql-live-\xc5\xbd\xc4\x85sis", await Scalar("SHOW application_name"), "application_name engine setting");
            await Execute("SET standard_conforming_strings = off");
            check.Equal("off", connection.PostgresParameters["standard_conforming_strings"], "actual live standard_conforming_strings ParameterStatus");
            check.Equal<object?>("off", await Scalar("SHOW standard_conforming_strings"), "standard_conforming_strings engine setting");
            using var cmd = Command("SELECT 'a\\'@missing', @p::text");
            cmd.Parameters.AddWithValue("p", "Žąsis");
            await using var reader = await cmd.ExecuteReaderAsync();
            check.True(await reader.ReadAsync(), "named parser with non-conforming strings row");
            check.Equal(2, reader.FieldCount, "named parser keeps escaped string and real parameter separate");
            check.Equal("a'@missing", reader.GetString(0), "backslash quote preserves placeholder-looking string data");
            check.Equal("Žąsis", reader.GetString(1), "named parameter parsed using actual live GUC");
            check.False(await reader.ReadAsync(), "named parser non-conforming string end");
        }
        finally
        {
            await Execute("SET standard_conforming_strings = on");
            await Execute("SET application_name = '" + previousApplication.Replace("'", "''") + "'");
        }
        check.Equal("on", connection.PostgresParameters["standard_conforming_strings"], "standard_conforming_strings live reset");
        check.Equal(previousApplication, connection.PostgresParameters["application_name"], "application_name live reset");
        check.Equal<object?>(42, await Scalar("SELECT 42"), "query after live GUC reset");
    }

    async Task Tracing()
    {
        var activities = new ConcurrentQueue<Activity>();
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Npgsql",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            SampleUsingParentId = (ref ActivityCreationOptions<string> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity => activities.Enqueue(activity)
        };
        ActivitySource.AddActivityListener(listener);
        check.Equal<object?>(42, await Scalar("SELECT 42"), "AllData tracing query succeeds without a socket endpoint");
        var completed = activities.ToArray();
        check.Equal(1, completed.Length, "one stopped activity for successful query");
        var success = completed[0];
        check.True(success.IsAllDataRequested, "AllData tracing was actually requested");
        check.Equal("Npgsql", success.Source.Name, "original Npgsql activity source");
        check.Equal(ActivityKind.Client, success.Kind, "Npgsql command activity kind");
        check.Equal(ActivityStatusCode.Unset, success.Status, "successful activity status");
        check.Equal<object?>("SELECT 42", success.GetTagItem("db.query.text"), "trace actual command SQL");
        check.Equal<object?>("postgresql", success.GetTagItem("db.system.name"), "trace database system");
        check.Equal<object?>(connection.Database, success.GetTagItem("db.namespace"), "trace actual database name");
        check.True(success.Events.Any(item => item.Name == "received-first-response"), "trace actual first-response event");
        var error = await check.ThrowsAsync<PostgresException>(async () => { await Execute("SELECT 1 / 0"); }, "SQL error with AllData tracing");
        check.Equal(PostgresErrorCodes.DivisionByZero, error.SqlState, "traced SQLSTATE");
        completed = activities.ToArray();
        check.Equal(2, completed.Length, "error command activity stopped");
        var failed = completed[1];
        check.Equal(ActivityStatusCode.Error, failed.Status, "failed activity status");
        check.Equal<object?>(PostgresErrorCodes.DivisionByZero, failed.GetTagItem("db.response.status_code"), "trace error SQLSTATE");
        check.Equal<object?>(PostgresErrorCodes.DivisionByZero, failed.GetTagItem("error.type"), "trace error type");
        check.Equal<object?>(42, await Scalar("SELECT 42"), "query recovers with AllData listener still attached");
        check.Equal(3, activities.Count, "recovery activity stopped");
    }

    async Task ReloadPreservesCodecs()
    {
        using var live = new CancellationTokenSource();
        await check.ThrowsAsync<NotSupportedException>(async () => { await connection.ReloadTypesAsync(live.Token); }, "live ReloadTypes token rejection");
        await AssertReloadArray("after live token rejection");
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        await check.ThrowsAsync<OperationCanceledException>(async () => { await connection.ReloadTypesAsync(cancelled.Token); }, "pre-cancelled ReloadTypes token");
        await AssertReloadArray("after pre-cancelled reload");
        using (var cmd = Command("SELECT 17"))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            var syncBusy = check.Throws<InvalidOperationException>(() => connection.ReloadTypes(), "busy reader sync ReloadTypes rejection");
            check.Equal("Connection is busy", syncBusy.Message, "upstream sync busy-connection message");
            var asyncBusy = await check.ThrowsAsync<InvalidOperationException>(async () => { await connection.ReloadTypesAsync(); }, "busy reader async ReloadTypes rejection");
            check.Equal("Connection is busy", asyncBusy.Message, "upstream async busy-connection message");
            check.True(await reader.ReadAsync(), "reader remains positioned after rejected reload");
            check.Equal(17, reader.GetInt32(0), "reader result survives rejected reload");
        }
        await AssertReloadArray("after busy reader rejection");
        await connection.ReloadTypesAsync();
        await AssertReloadArray("after successful async ReloadTypes");
        connection.ReloadTypes();
        await AssertReloadArray("after successful sync ReloadTypes");
    }

    async Task AssertReloadArray(string stage)
    {
        using var cmd = Command("SELECT @p::integer[], cardinality(@p::integer[])");
        int[] expected = [1, 5, 9];
        cmd.Parameters.AddWithValue("p", NpgsqlDbType.Array | NpgsqlDbType.Integer, expected);
        await using var reader = await cmd.ExecuteReaderAsync();
        check.True(await reader.ReadAsync(), "array codec row " + stage);
        check.Equal(expected, reader.GetFieldValue<int[]>(0), "array codec values " + stage);
        check.Equal("integer[]", reader.GetDataTypeName(0), "array codec metadata " + stage);
        check.Equal(3, reader.GetInt32(1), "array codec server cardinality " + stage);
        check.False(await reader.ReadAsync(), "array codec result end " + stage);
    }

    async Task ReaderMetadata()
    {
        await Execute("CREATE TABLE npgsql_meta(id integer PRIMARY KEY, label text NOT NULL, flag boolean)");
        await Execute("INSERT INTO npgsql_meta VALUES (7, 'Žąsis', true)");
        using var cmd = Command("SELECT id AS identifier, label, flag FROM npgsql_meta");
        await using var reader = await cmd.ExecuteReaderAsync();
        check.Equal(3, reader.FieldCount, "metadata FieldCount");
        check.Equal("identifier", reader.GetName(0), "metadata alias");
        check.Equal(0, reader.GetOrdinal("identifier"), "metadata ordinal");
        check.Equal(1, reader.GetOrdinal("LABEL"), "metadata case insensitive ordinal");
        check.Equal(typeof(int), reader.GetFieldType(0), "integer CLR type");
        check.Equal(typeof(string), reader.GetFieldType(1), "text CLR type");
        check.Equal(typeof(bool), reader.GetFieldType(2), "boolean CLR type");
        check.Equal("integer", reader.GetDataTypeName(0), "integer type name");
        check.Equal("text", reader.GetDataTypeName(1), "text type name");
        check.Equal(23u, reader.GetDataTypeOID(0), "integer OID");
        check.Equal(25u, reader.GetDataTypeOID(1), "text OID");
        check.Equal(16u, reader.GetDataTypeOID(2), "bool OID");
        var schema = reader.GetColumnSchema();
        check.Equal(3, schema.Count, "schema column count");
        check.Equal("identifier", schema[0].ColumnName, "schema alias");
        check.Equal(typeof(int), schema[0].DataType, "schema CLR type");
        check.Equal("integer", schema[0].DataTypeName, "schema type name");
        check.True(await reader.ReadAsync(), "metadata row");
        check.Equal<object>(7, reader["identifier"], "named integer access");
        check.Equal<object>("Žąsis", reader["label"], "named Unicode access");
        check.Equal<object>(true, reader["flag"], "named bool access");
        check.False(await reader.ReadAsync(), "metadata row end");
    }

    async Task PreparedReuse()
    {
        using var cmd = Command("SELECT $1::integer + 1");
        var parameter = new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Integer, Value = 7 };
        cmd.Parameters.Add(parameter);
        check.False(cmd.IsPrepared, "initial IsPrepared");
        await cmd.PrepareAsync();
        check.True(cmd.IsPrepared, "PrepareAsync IsPrepared");
        foreach (int value in new[] { 7, -1, 41, 1024 })
        {
            parameter.Value = value;
            check.Equal<object?>(value + 1, await cmd.ExecuteScalarAsync(), "prepared changed parameter " + value);
            check.True(cmd.IsPrepared, "prepared persists");
        }
        check.Equal<object?>(1L, await Scalar("SELECT count(*) FROM pg_prepared_statements WHERE statement = 'SELECT $1::integer + 1'"), "real backend prepared statement");
        await cmd.UnprepareAsync();
        check.False(cmd.IsPrepared, "UnprepareAsync IsPrepared");
        check.Equal<object?>(0L, await Scalar("SELECT count(*) FROM pg_prepared_statements WHERE statement = 'SELECT $1::integer + 1'"), "real backend deallocation");
        cmd.Prepare();
        check.True(cmd.IsPrepared, "synchronous Prepare IsPrepared");
        check.Equal<object?>(1025, cmd.ExecuteScalar(), "synchronous prepared ExecuteScalar");
        cmd.Unprepare();
    }

    async Task MultipleResults()
    {
        foreach (bool prepare in new[] { false, true })
        {
            using var cmd = Command("SELECT @p1; SELECT @p2");
            cmd.Parameters.Add(new NpgsqlParameter("p1", NpgsqlDbType.Integer) { Value = 8 });
            cmd.Parameters.Add(new NpgsqlParameter("p2", NpgsqlDbType.Text) { Value = "foo" });
            if (prepare) cmd.Prepare();
            await using var reader = await cmd.ExecuteReaderAsync();
            check.True(await reader.ReadAsync(), "multiple result first Read");
            check.Equal(8, reader.GetInt32(0), "multiple result first value");
            check.True(await reader.NextResultAsync(), "multiple result NextResult");
            check.True(await reader.ReadAsync(), "multiple result second Read");
            check.Equal("foo", reader.GetString(0), "multiple result second value");
            check.False(await reader.NextResultAsync(), "multiple result end");
        }
    }

    async Task BatchParameters()
    {
        foreach (bool positional in new[] { false, true })
        foreach (var behavior in new[] { CommandBehavior.Default, CommandBehavior.SequentialAccess })
        {
            using var batch = new NpgsqlBatch(connection) { Timeout = 0 };
            batch.BatchCommands.Add(new NpgsqlBatchCommand(positional ? "SELECT $1" : "SELECT @p")
            { Parameters = { new NpgsqlParameter(positional ? "" : "p", 8) } });
            batch.BatchCommands.Add(new NpgsqlBatchCommand(positional ? "SELECT $1, $2" : "SELECT @p1, @p2")
            { Parameters = { new NpgsqlParameter(positional ? "" : "p1", 9), new NpgsqlParameter(positional ? "" : "p2", 10) } });
            await using var reader = await batch.ExecuteReaderAsync(behavior);
            check.True(await reader.ReadAsync(), "batch first row");
            check.Equal(1, reader.FieldCount, "batch first FieldCount");
            check.Equal<object>(8, reader[0], "batch value 8");
            check.False(await reader.ReadAsync(), "batch first end");
            check.True(await reader.NextResultAsync(), "batch NextResult");
            check.True(await reader.ReadAsync(), "batch second row");
            check.Equal(2, reader.FieldCount, "batch second FieldCount");
            check.Equal<object>(9, reader[0], "batch value 9");
            check.Equal<object>(10, reader[1], "batch value 10");
            check.False(await reader.ReadAsync(), "batch second end");
            check.False(await reader.NextResultAsync(), "batch all end");
        }
    }

    async Task BatchRecords()
    {
        await Execute("CREATE TEMP TABLE batch_records(name text)");
        using var batch = new NpgsqlBatch(connection) { Timeout = 0 };
        foreach (string sql in new[] {
            "INSERT INTO batch_records VALUES ('a'), ('b')",
            "UPDATE batch_records SET name='c' WHERE name='b'",
            "UPDATE batch_records SET name='d' WHERE name='doesnt_exist'",
            "SELECT name FROM batch_records", "DELETE FROM batch_records" })
            batch.BatchCommands.Add(new NpgsqlBatchCommand(sql));
        await using var reader = await batch.ExecuteReaderAsync();
        await reader.CloseAsync();
        int[] affected = [2, 1, 0, -1, 2];
        ulong[] rows = [2, 1, 0, 2, 2];
        for (int i = 0; i < affected.Length; i++)
        {
            check.Equal(affected[i], batch.BatchCommands[i].RecordsAffected, "batch RecordsAffected " + i);
            check.Equal(rows[i], batch.BatchCommands[i].Rows, "batch Rows " + i);
        }
        check.Equal(5, reader.RecordsAffected, "batch aggregate RecordsAffected");
    }

    async Task BatchRuntimeError()
    {
        await Execute("CREATE OR REPLACE FUNCTION npgsql_batch_fail() RETURNS VOID AS 'BEGIN RAISE EXCEPTION ''testexception'' USING ERRCODE = ''12345''; END;' LANGUAGE 'plpgsql'");
        var batch = connection.CreateBatch();
        batch.Timeout = 0;
        batch.BatchCommands.Add(new NpgsqlBatchCommand("SELECT 1"));
        batch.BatchCommands.Add(new NpgsqlBatchCommand("SELECT npgsql_batch_fail()"));
        await using (var reader = await batch.ExecuteReaderAsync())
        {
            var error = await check.ThrowsAsync<PostgresException>(async () => { await reader.NextResultAsync(); }, "upstream runtime batch error");
            check.True(ReferenceEquals(batch.BatchCommands[1], error.BatchCommand), "upstream runtime batch command identity");
            check.Equal("12345", error.SqlState, "upstream runtime SQLSTATE");
            check.Equal("testexception", error.MessageText, "upstream runtime error message");
        }
        batch.Dispose();
        using var next = connection.CreateBatch();
        check.False(ReferenceEquals(batch, next), "exception-referenced batch is not recycled");
        check.Equal<object?>(42, await Scalar("SELECT 42"), "query after upstream runtime batch error");
    }

    async Task BatchError()
    {
        using var batch = new NpgsqlBatch(connection) { Timeout = 0 };
        batch.BatchCommands.Add(new NpgsqlBatchCommand("SELECT 1"));
        batch.BatchCommands.Add(new NpgsqlBatchCommand("SELECT 1 / 0"));
        batch.BatchCommands.Add(new NpgsqlBatchCommand("SELECT 3"));
        var error = await check.ThrowsAsync<PostgresException>(async () => { await batch.ExecuteNonQueryAsync(); }, "batch division error");
        check.Equal(PostgresErrorCodes.DivisionByZero, error.SqlState, "batch error SQLSTATE");
        check.True(ReferenceEquals(batch.BatchCommands[1], error.BatchCommand), "batch error command identity, actual=" + (error.BatchCommand?.CommandText ?? "<null>"));
        check.Equal<object?>(42, await Scalar("SELECT 42"), "query after batch error");
    }

    async Task Transactions()
    {
        await Execute("CREATE TEMP TABLE transaction_values(name text)");
        foreach (bool commit in new[] { true, false })
        foreach (bool prepare in new[] { false, true })
        foreach (bool asynchronous in new[] { false, true })
        {
            await Execute("TRUNCATE transaction_values");
            var tx = asynchronous ? await connection.BeginTransactionAsync() : connection.BeginTransaction();
            using (tx)
            {
                using var cmd = Command("INSERT INTO transaction_values VALUES ('X')");
                cmd.Transaction = tx;
                if (prepare) cmd.Prepare();
                if (asynchronous) await cmd.ExecuteNonQueryAsync(); else cmd.ExecuteNonQuery();
                check.Equal<object?>(1L, await Scalar("SELECT COUNT(*) FROM transaction_values"), "transaction own write visible");
                if (commit) { if (asynchronous) await tx.CommitAsync(); else tx.Commit(); }
                else { if (asynchronous) await tx.RollbackAsync(); else tx.Rollback(); }
                check.Throws<InvalidOperationException>(() => { _ = tx.IsolationLevel; }, "completed transaction rejects reuse");
                _ = tx.Connection; // Upstream requires this getter not to throw before disposal.
                check.Equal<object?>(commit ? 1L : 0L, await Scalar("SELECT COUNT(*) FROM transaction_values"), "commit/rollback final count");
            }
            check.Throws<ObjectDisposedException>(() => { _ = tx.Connection; }, "disposed transaction Connection");
        }
    }

    async Task TransactionDispose()
    {
        await Execute("TRUNCATE transaction_values");
        await using (var tx = await connection.BeginTransactionAsync())
        {
            using var cmd = Command("INSERT INTO transaction_values VALUES ('X')");
            cmd.Transaction = tx;
            await cmd.ExecuteNonQueryAsync();
        }
        check.Equal<object?>(0L, await Scalar("SELECT COUNT(*) FROM transaction_values"), "transaction disposal rollback");
        using var empty = connection.BeginTransaction();
        await empty.RollbackAsync();
        check.Throws<InvalidOperationException>(() => { _ = empty.IsolationLevel; }, "empty rollback completed");
    }

    async Task SqlStateRecovery()
    {
        foreach (var (sql, state) in new[] {
            ("BAD QUERY", PostgresErrorCodes.SyntaxError),
            ("SELECT 1 / 0", PostgresErrorCodes.DivisionByZero),
            ("SELECT * FROM acceptance_missing_table", PostgresErrorCodes.UndefinedTable) })
        {
            var error = await check.ThrowsAsync<PostgresException>(async () => { await Execute(sql); }, "SQL failure " + state);
            check.Equal(state, error.SqlState, "SQLSTATE " + state);
            check.Equal("ERROR", error.Severity, "error severity");
            check.Equal(ConnectionState.Open, connection.State, "connection survives SQL error");
            check.Equal<object?>(42, await Scalar("SELECT 42"), "query recovers " + state);
        }
        using var tx = connection.BeginTransaction();
        await Execute("INSERT INTO transaction_values VALUES ('will roll back')");
        var first = await check.ThrowsAsync<PostgresException>(async () => { await Execute("BAD QUERY"); }, "failed transaction original error");
        check.Equal(PostgresErrorCodes.SyntaxError, first.SqlState, "failed transaction original SQLSTATE");
        var aborted = await check.ThrowsAsync<PostgresException>(async () => { await Scalar("SELECT 1"); }, "aborted transaction");
        check.Equal(PostgresErrorCodes.InFailedSqlTransaction, aborted.SqlState, "aborted SQLSTATE");
        tx.Rollback();
        check.Throws<InvalidOperationException>(() => { _ = tx.IsolationLevel; }, "failed transaction rollback completed");
        check.Equal<object?>(0L, await Scalar("SELECT count(*) FROM transaction_values"), "failed transaction insert rolled back");
    }

    async Task EarlyDispose()
    {
        foreach (var behavior in new[] { CommandBehavior.Default, CommandBehavior.SequentialAccess })
        {
            using var cmd = Command("SELECT i, repeat('Ž', 9000) FROM generate_series(1, 20) AS g(i); SELECT 99");
            await using (var reader = await cmd.ExecuteReaderAsync(behavior))
            {
                check.True(await reader.ReadAsync(), "early dispose first row");
                check.Equal(1, reader.GetInt32(0), "early dispose first field");
            }
            check.Equal<object?>(42, await Scalar("SELECT 42"), "query after early dispose");
        }
        using var unreadCommand = Command("SELECT generate_series(1,100); SELECT 9");
        using (unreadCommand.ExecuteReader()) { }
        check.Equal<object?>(42, await Scalar("SELECT 42"), "query after entirely unread sync reader");
    }

    async Task SameConnectionMisuse()
    {
        using var first = Command("SELECT 1");
        await using (var reader = await first.ExecuteReaderAsync())
        {
            using var other = Command("SELECT 2");
            check.Throws<NpgsqlOperationInProgressException>(() => other.ExecuteNonQuery(), "concurrent nonquery");
            await check.ThrowsAsync<NpgsqlOperationInProgressException>(async () => { await other.ExecuteScalarAsync(); }, "concurrent scalar async");
            check.Throws<NpgsqlOperationInProgressException>(() => other.ExecuteReader(), "concurrent reader");
            check.Throws<NpgsqlOperationInProgressException>(() => other.Prepare(), "concurrent prepare");
            check.True(await reader.ReadAsync(), "first reader preserved");
            check.Equal(1, reader.GetInt32(0), "first reader value preserved");
        }
        check.Equal<object?>(42, await Scalar("SELECT 42"), "query after rejected misuse");
    }

    async Task ParameterReuse()
    {
        using var cmd = Command("SELECT @p");
        cmd.Parameters.AddWithValue("p", 8);
        await cmd.ExecuteNonQueryAsync();
        cmd.Parameters[0].Value = 9;
        check.Equal<object?>(9, await cmd.ExecuteScalarAsync(), "same command new parameter value");
        cmd.Parameters.RemoveAt(0);
        cmd.Parameters.AddWithValue("p", 10);
        check.Equal<object?>(10, await cmd.ExecuteScalarAsync(), "same command new parameter instance");
    }
}
