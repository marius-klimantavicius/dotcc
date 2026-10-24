using Npgsql;

namespace NpgsqlAcceptance;

internal sealed partial class Suite
{
    internal IEnumerable<AcceptanceCase> ExclusionCases()
    {
        yield return new("excluded.copy-apis", CopyApis);
        yield return new("excluded.copy-sql", CopySql);
        yield return new("supported.file-copy", FileCopy);
        yield return new("excluded.cancel-timeout-tokens", CancellationTimeout);
        yield return new("excluded.pooling-multiplexing-profile", Profile);
        yield return new("excluded.wait-and-profile-escape", WaitAndEscape);
    }

    async Task CopyApis()
    {
        const string import = "COPY npgsql_meta FROM STDIN (FORMAT BINARY)";
        const string export = "COPY npgsql_meta TO STDOUT (FORMAT BINARY)";
        check.Throws<NotSupportedException>(() => { using var copy = connection.BeginBinaryImport(import); }, "binary importer exclusion");
        check.Throws<NotSupportedException>(() => { using var copy = connection.BeginBinaryExport(export); }, "binary exporter exclusion");
        check.Throws<NotSupportedException>(() => { using var copy = connection.BeginTextImport("COPY npgsql_meta FROM STDIN"); }, "text importer exclusion");
        check.Throws<NotSupportedException>(() => { using var copy = connection.BeginTextExport("COPY npgsql_meta TO STDOUT"); }, "text exporter exclusion");
        check.Throws<NotSupportedException>(() => { using var copy = connection.BeginRawBinaryCopy(export); }, "raw COPY exclusion");
        await check.ThrowsAsync<NotSupportedException>(async () => { await using var copy = await connection.BeginBinaryImportAsync(import); }, "async binary importer exclusion");
        await check.ThrowsAsync<NotSupportedException>(async () => { await using var copy = await connection.BeginBinaryExportAsync(export); }, "async binary exporter exclusion");
        await check.ThrowsAsync<NotSupportedException>(async () => { using var copy = await connection.BeginTextImportAsync("COPY npgsql_meta FROM STDIN"); }, "async text importer exclusion");
        await check.ThrowsAsync<NotSupportedException>(async () => { using var copy = await connection.BeginTextExportAsync("COPY npgsql_meta TO STDOUT"); }, "async text exporter exclusion");
        await check.ThrowsAsync<NotSupportedException>(async () => { await using var copy = await connection.BeginRawBinaryCopyAsync(export); }, "async raw COPY exclusion");
        check.Equal<object?>(42, await Scalar("SELECT 42"), "queries survive excluded COPY APIs");
    }

    async Task CopySql()
    {
        foreach (string sql in new[] {
            "COPY (SELECT 1) TO STDOUT", "COPY npgsql_meta FROM STDIN",
            " /* comment */ COPY (SELECT 1) TO STDOUT", "SELECT 1; COPY (SELECT 2) TO STDOUT",
            "DO $$ BEGIN EXECUTE chr(67) || 'OPY (SELECT 1) TO STDOUT'; END $$" })
        {
            var error = await check.ThrowsAsync<PostgresException>(async () => { await Execute(sql); }, "client COPY SQL rejection " + sql);
            check.Equal(PostgresErrorCodes.FeatureNotSupported, error.SqlState, "COPY SQLSTATE 0A000");
            check.Equal<object?>(42, await Scalar("SELECT 42"), "query recovers after client COPY rejection");
        }
        using (var prepared = Command("COPY (SELECT 17) TO STDOUT"))
        {
            await prepared.PrepareAsync();
            check.True(prepared.IsPrepared, "client COPY can be prepared before execution guard");
            var error = await check.ThrowsAsync<PostgresException>(async () => { await prepared.ExecuteNonQueryAsync(); }, "prepared client COPY rejects at actual utility execution");
            check.Equal(PostgresErrorCodes.FeatureNotSupported, error.SqlState, "prepared COPY SQLSTATE");
            check.Equal<object?>(42, await Scalar("SELECT 42"), "query after prepared COPY rejection");
        }
        foreach (bool prepare in new[] { false, true })
        {
            using var batch = new NpgsqlBatch(connection) { Timeout = 0 };
            batch.BatchCommands.Add(new NpgsqlBatchCommand("SELECT 1"));
            batch.BatchCommands.Add(new NpgsqlBatchCommand("COPY (SELECT 17) TO STDOUT"));
            batch.BatchCommands.Add(new NpgsqlBatchCommand("SELECT 3"));
            if (prepare) await batch.PrepareAsync();
            var error = await check.ThrowsAsync<PostgresException>(async () => { await batch.ExecuteNonQueryAsync(); }, "NpgsqlBatch client COPY rejection, prepared=" + prepare);
            check.Equal(PostgresErrorCodes.FeatureNotSupported, error.SqlState, "batch COPY SQLSTATE");
            check.True(ReferenceEquals(batch.BatchCommands[1], error.BatchCommand), "batch COPY error command identity");
            check.Equal<object?>(42, await Scalar("SELECT 42"), "query after batch COPY rejection");
        }
        await Execute("CREATE FUNCTION npgsql_client_copy() RETURNS void LANGUAGE plpgsql AS $$ BEGIN EXECUTE chr(67) || 'OPY (SELECT 1) TO STDOUT'; END $$");
        var functionError = await check.ThrowsAsync<PostgresException>(async () => { await Execute("SELECT npgsql_client_copy()"); }, "stored function client COPY rejection");
        check.Equal(PostgresErrorCodes.FeatureNotSupported, functionError.SqlState, "stored function COPY SQLSTATE");
        check.Equal<object?>(42, await Scalar("SELECT 42"), "query recovers after stored function COPY");
        // A lexical guard must not reject a keyword that is ordinary string data.
        check.Equal<object?>("COPY (SELECT 1) TO STDOUT", await Scalar("SELECT 'COPY (SELECT 1) TO STDOUT'"), "COPY literal remains ordinary SQL");
        check.Equal<object?>(42, await Scalar("SELECT 42 /* COPY table FROM STDIN */"), "COPY comment remains ordinary SQL");
        check.Equal<object?>(42, await Scalar("SELECT 42"), "queries survive excluded COPY SQL");
    }

    async Task FileCopy()
    {
        string path = Path.Combine(workRoot, "file-copy.tsv");
        string literal = "'" + path.Replace("'", "''") + "'";
        check.Equal(1, await Execute("COPY (SELECT 17, 'Žąsis'::text) TO " + literal), "ordinary file COPY writes row");
        check.Equal("17\tŽąsis\n", await File.ReadAllTextAsync(path), "ordinary file COPY exact UTF-8 content");
        await Execute("CREATE TEMP TABLE npgsql_file_copy(id int, note text)");
        check.Equal(1, await Execute("COPY npgsql_file_copy FROM " + literal), "ordinary file COPY reads row");
        check.Equal<object?>("Žąsis", await Scalar("SELECT note FROM npgsql_file_copy WHERE id=17"), "ordinary file COPY read data");
    }

    async Task CancellationTimeout()
    {
        using var cmd = Command("SELECT 42");
        check.Throws<NotSupportedException>(() => cmd.Cancel(), "Cancel exclusion");
        await check.ThrowsAsync<NotSupportedException>(async () => { cmd.CommandTimeout = 1; await cmd.ExecuteScalarAsync(); }, "nonzero command timeout exclusion");
        cmd.CommandTimeout = 0;
        using (var batch = new NpgsqlBatch(connection))
        {
            batch.BatchCommands.Add(new NpgsqlBatchCommand("SELECT 42"));
            await check.ThrowsAsync<NotSupportedException>(async () => { batch.Timeout = 1; await batch.ExecuteScalarAsync(); }, "nonzero batch timeout exclusion");
        }
        using var live = new CancellationTokenSource();
        await check.ThrowsAsync<NotSupportedException>(async () => { await cmd.ExecuteScalarAsync(live.Token); }, "live cancellable command token exclusion");
        await check.ThrowsAsync<NotSupportedException>(async () => { await cmd.ExecuteNonQueryAsync(live.Token); }, "live nonquery token exclusion");
        await check.ThrowsAsync<NotSupportedException>(async () => { await using var ignored = await cmd.ExecuteReaderAsync(live.Token); }, "live execute-reader token exclusion");
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        await check.ThrowsAsync<OperationCanceledException>(async () => { await cmd.ExecuteScalarAsync(cancelled.Token); }, "pre-cancelled token before entry");
        check.Equal<object?>(42, await cmd.ExecuteScalarAsync(CancellationToken.None), "None token succeeds");
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            await check.ThrowsAsync<NotSupportedException>(async () => { await reader.ReadAsync(live.Token); }, "live reader token exclusion");
            check.True(await reader.ReadAsync(), "reader usable after rejected token");
            check.Equal(42, reader.GetInt32(0), "reader value after rejected token");
            await check.ThrowsAsync<NotSupportedException>(async () => { await reader.GetFieldValueAsync<int>(0, live.Token); }, "live field token exclusion");
            await check.ThrowsAsync<NotSupportedException>(async () => { await reader.IsDBNullAsync(0, live.Token); }, "live IsDBNull token exclusion");
            await check.ThrowsAsync<NotSupportedException>(async () => { await reader.NextResultAsync(live.Token); }, "live NextResult token exclusion");
            check.Equal(42, reader.GetInt32(0), "reader field intact after rejected tokens");

        }
        check.Equal<object?>(42, await Scalar("SELECT 42"), "queries survive cancellation exclusions");
    }

    async Task WaitAndEscape()
    {
        check.Throws<NotSupportedException>(() => connection.Wait(), "notification Wait exclusion");
        check.Throws<NotSupportedException>(() => connection.Wait(1), "notification Wait timeout exclusion");
        check.Throws<NotSupportedException>(() => connection.Wait(TimeSpan.FromMilliseconds(1)), "notification Wait TimeSpan exclusion");
        await check.ThrowsAsync<NotSupportedException>(async () => { await connection.WaitAsync(); }, "notification WaitAsync exclusion");
        await check.ThrowsAsync<NotSupportedException>(async () => { await connection.WaitAsync(1); }, "notification timed WaitAsync exclusion");
        await check.ThrowsAsync<NotSupportedException>(async () => { await connection.WaitAsync(TimeSpan.FromMilliseconds(1)); }, "notification TimeSpan WaitAsync exclusion");
        check.Throws<NotSupportedException>(() => connection.ChangeDatabase("postgres"), "ChangeDatabase exclusion");
        check.Throws<NotSupportedException>(() => { using var escaped = connection.CloneWith(connection.ConnectionString); }, "CloneWith exclusion");
        await check.ThrowsAsync<NotSupportedException>(async () => { await using var escaped = await connection.CloneWithAsync(connection.ConnectionString); }, "CloneWithAsync exclusion");
        using var clone = (NpgsqlConnection)((ICloneable)connection).Clone();
        check.Equal(System.Data.ConnectionState.Closed, clone.State, "plain Clone is closed");
        check.Equal(connection.ConnectionString, clone.ConnectionString, "plain Clone retains embedded configuration");
        check.Throws<NotSupportedException>(() => clone.ConnectionString = "Host=localhost;Pooling=true", "closed embedded clone cannot mutate connection string");
        check.Throws<InvalidOperationException>(() => clone.Open(), "plain Clone retains embedded exclusive datasource");
        check.Equal<object?>(42, await Scalar("SELECT 42"), "queries survive wait and socket-escape rejections");
    }

    Task Profile()
    {
        var settings = new NpgsqlConnectionStringBuilder(connection.ConnectionString);
        check.False(settings.Pooling, "embedded pooling disabled");
        check.False(settings.Multiplexing, "embedded multiplexing disabled");
        check.Equal(0, settings.CommandTimeout, "embedded command timeout disabled");
        return Task.CompletedTask;
    }
}
