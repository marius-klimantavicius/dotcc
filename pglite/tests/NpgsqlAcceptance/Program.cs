using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Managed.PGlite;
using Npgsql;
using NpgsqlAcceptance;

return await AcceptanceProgram.MainAsync(args);

internal static class AcceptanceProgram
{
    internal const string CorePin = "29b6424aa312ec3556247db069939006dd27f878";

    internal static async Task<int> MainAsync(string[] args)
    {
        string root = Path.Combine(Path.GetTempPath(), "dotcc-npgsql-acceptance", Guid.NewGuid().ToString("N"));
        string prefix = Path.Combine(AppContext.BaseDirectory, "bootstrap-prefix");
        string? report = null;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--help")
            {
                Console.WriteLine("NpgsqlAcceptance [--work PATH] [--bootstrap-prefix PATH] [--report PATH]");
                return 0;
            }
            if (i + 1 == args.Length) throw new ArgumentException("Missing value for " + args[i]);
            switch (args[i++])
            {
                case "--work": root = args[i]; break;
                case "--bootstrap-prefix": prefix = args[i]; break;
                case "--report": report = args[i]; break;
                default: throw new ArgumentException("Unknown argument: " + args[i - 1]);
            }
        }
        root = Path.GetFullPath(root);
        prefix = Path.GetFullPath(prefix);
        Directory.CreateDirectory(root);
        report ??= Path.Combine(root, "acceptance.json");
        var check = new Checks();
        var results = new List<CaseResult>();
        var options = new PgliteOptions(Path.Combine(root, "data"), prefix, Path.Combine(root, "engine-work"))
        {
            Configuration = new PgliteConfiguration
            {
                Username = "postgres", Database = "postgres",
                Settings = new Dictionary<string, string> { ["TimeZone"] = "UTC" }
            }
        };
        try
        {
            await using (var source = NpgsqlEmbeddedDataSource.Create(options))
            {
                await using (var connection = await source.OpenConnectionAsync())
                {
                    var suite = new Suite(connection, check, root);
                    foreach (var test in suite.ProtocolCases().Concat(suite.CodecCases()))
                        await Run(test, check, results);
                    {
                        foreach (var test in suite.ExclusionCases()) await Run(test, check, results);
                        await Run(new AcceptanceCase("lifetime.exclusive-sessions", async () =>
                        {
                            check.Throws<InvalidOperationException>(() => { using var other = source.OpenConnection(); }, "second synchronous lease");
                            await check.ThrowsAsync<InvalidOperationException>(async () => { await using var other = await source.OpenConnectionAsync(); }, "second asynchronous lease");
                            check.Equal(System.Data.ConnectionState.Open, connection.State, "first lease retained");
                            using var query = new NpgsqlCommand("SELECT 42", connection) { CommandTimeout = 0 };
                            check.Equal<object?>(42, await query.ExecuteScalarAsync(), "first lease usable after second rejection");
                        }), check, results);
                        await Run(new AcceptanceCase("lifetime.other-datasource-exclusive", async () =>
                        {
                            var otherOptions = options with
                            {
                                DataDirectory = Path.Combine(root, "other-data"),
                                WorkDirectory = Path.Combine(root, "other-engine-work")
                            };
                            await using var otherSource = NpgsqlEmbeddedDataSource.Create(otherOptions);
                            await check.ThrowsAsync<InvalidOperationException>(async () => { await using var other = await otherSource.OpenConnectionAsync(); }, "other datasource cannot acquire process owner");
                            using var query = new NpgsqlCommand("SELECT 42", connection) { CommandTimeout = 0 };
                            check.Equal<object?>(42, await query.ExecuteScalarAsync(), "first datasource survives global ownership rejection");
                        }), check, results);
                        await Run(new AcceptanceCase("lifetime.prepare-reopen-fixture", async () =>
                        {
                            using var cmd = new NpgsqlCommand("CREATE TABLE npgsql_persistent(id int primary key, note text); INSERT INTO npgsql_persistent VALUES (1, 'Žąsis'); CREATE TEMP TABLE npgsql_session_only(id int); SET application_name='dirty-session'", connection) { CommandTimeout = 0 };
                            await cmd.ExecuteNonQueryAsync();
                            using var prepare = new NpgsqlCommand("SELECT 12345", connection) { CommandTimeout = 0 };
                            prepare.Prepare();
                            check.True(prepare.IsPrepared, "session prepared fixture");
                            using var tx = connection.BeginTransaction();
                            using var insert = new NpgsqlCommand("INSERT INTO npgsql_persistent VALUES (2, 'rollback on close')", connection, tx) { CommandTimeout = 0 };
                            await insert.ExecuteNonQueryAsync();
                            // Close the lease with a transaction still open, then dispose its wrapper.
                            await connection.CloseAsync();
                            check.Equal(System.Data.ConnectionState.Closed, connection.State, "CloseAsync closes connection");
                        }), check, results);
                    }
                }
                await Run(new AcceptanceCase("lifetime.tracing-open", async () =>
                {
                    var activities = new System.Collections.Concurrent.ConcurrentQueue<Activity>();
                    using var listener = new ActivityListener
                    {
                        ShouldListenTo = item => item.Name == "Npgsql",
                        Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
                        SampleUsingParentId = (ref ActivityCreationOptions<string> _) => ActivitySamplingResult.AllDataAndRecorded,
                        ActivityStopped = item => activities.Enqueue(item)
                    };
                    ActivitySource.AddActivityListener(listener);
                    await using (var traced = await source.OpenConnectionAsync())
                    {
                        using var command = new NpgsqlCommand("SELECT 42", traced) { CommandTimeout = 0 };
                        check.Equal<object?>(42, await command.ExecuteScalarAsync(), "AllData traced open and query succeed");
                    }
                    var completed = activities.ToArray();
                    var opens = completed.Where(item => item.OperationName.StartsWith("CONNECT ", StringComparison.Ordinal)).ToArray();
                    check.Equal(1, opens.Length, "one actual physical-open activity");
                    check.True(opens[0].IsAllDataRequested, "physical open actually requested AllData");
                    check.Equal(ActivityKind.Client, opens[0].Kind, "physical-open activity kind");
                    check.Equal(ActivityStatusCode.Unset, opens[0].Status, "physical-open activity success");
                    check.Equal<object?>("postgresql", opens[0].GetTagItem("db.system.name"), "physical-open database system");
                    check.Equal<object?>("postgres", opens[0].GetTagItem("db.namespace"), "physical-open actual database");
                    check.Equal(1, completed.Count(item => Equals(item.GetTagItem("db.query.text"), "SELECT 42")), "one query activity after traced open");
                }), check, results);
                await Run(new AcceptanceCase("lifetime.same-datasource-reopen-reset", async () =>
                    {
                        await using var connection = await source.OpenConnectionAsync();
                        await AssertReopened(connection, check);
                    }), check, results);
            }
            await Run(new AcceptanceCase("lifetime.new-datasource-persistence", async () =>
                {
                    await using var reopened = NpgsqlEmbeddedDataSource.Create(options, create: false);
                    await using var connection = await reopened.OpenConnectionAsync();
                    await AssertReopened(connection, check);
                    using var command = new NpgsqlCommand("INSERT INTO npgsql_persistent VALUES (3, 'reopened'); SELECT count(*) FROM npgsql_persistent", connection) { CommandTimeout = 0 };
                    check.Equal<object?>(2L, await command.ExecuteScalarAsync(), "persistent writes after new datasource reopen");
                }), check, results);
            await Run(new AcceptanceCase("lifetime.extension-close-failure", async () =>
            {
                int closes = 0;
                var expectedError = new ApplicationException("npgsql-extension-close-fixture");
                var extensionOptions = options with
                {
                    DataDirectory = Path.Combine(root, "extension-close-data"),
                    WorkDirectory = Path.Combine(root, "extension-close-work"),
                    Extensions = [new PgliteExtension("close-fixture", _ => new PgliteExtensionSetup(Close: () =>
                    {
                        if (Interlocked.Increment(ref closes) == 1) throw expectedError;
                    }))]
                };
                await using var extensionSource = NpgsqlEmbeddedDataSource.Create(extensionOptions);
                var connection = await extensionSource.OpenConnectionAsync();
                using (var create = new NpgsqlCommand("CREATE TABLE close_error_persistent(note text); INSERT INTO close_error_persistent VALUES ('Žąsis')", connection) { CommandTimeout = 0 })
                    await create.ExecuteNonQueryAsync();
                var error = await check.ThrowsAsync<NpgsqlException>(async () => { await connection.CloseAsync(); }, "managed extension close failure propagates");
                check.True(error.InnerException is AggregateException, "extension cleanup AggregateException retained as inner error");
                var cleanupError = (AggregateException)error.InnerException!;
                check.True(cleanupError.Flatten().InnerExceptions.Any(item => ReferenceEquals(item, expectedError)), "original extension close exception retained");
                check.Equal(System.Data.ConnectionState.Closed, connection.State, "connection logically closed despite callback error");
                check.Equal(1, Volatile.Read(ref closes), "failing close callback ran exactly once");
                await connection.DisposeAsync();
                await using (var reopened = await extensionSource.OpenConnectionAsync())
                {
                    using var query = new NpgsqlCommand("SELECT note FROM close_error_persistent", reopened) { CommandTimeout = 0 };
                    check.Equal<object?>("Žąsis", await query.ExecuteScalarAsync(), "same source lease released and committed data persisted");
                    query.CommandText = "INSERT INTO close_error_persistent VALUES ('after-close-error'); SELECT count(*) FROM close_error_persistent";
                    check.Equal<object?>(2L, await query.ExecuteScalarAsync(), "reopened source can persist new writes after close error");
                }
                check.Equal(2, Volatile.Read(ref closes), "fresh lifetime closes normally");
            }), check, results);
            await Run(new AcceptanceCase("lifetime.failed-open-retry-same-datasource", async () =>
            {
                var ownerOptions = options with
                {
                    DataDirectory = Path.Combine(root, "retry-owner-data"),
                    WorkDirectory = Path.Combine(root, "retry-owner-work")
                };
                await using var ownerSource = NpgsqlEmbeddedDataSource.Create(ownerOptions);
                await using var retrySource = NpgsqlEmbeddedDataSource.Create(options, create: false);
                await using (var activeOwner = await ownerSource.OpenConnectionAsync())
                {
                    await check.ThrowsAsync<InvalidOperationException>(async () => { await using var rejected = await retrySource.OpenConnectionAsync(); }, "second source busy open rejects before owning a core");
                    using var query = new NpgsqlCommand("SELECT 42", activeOwner) { CommandTimeout = 0 };
                    check.Equal<object?>(42, await query.ExecuteScalarAsync(), "active owner survives other source failed open");
                }
                await using (var retried = await retrySource.OpenConnectionAsync())
                {
                    using var query = new NpgsqlCommand("SELECT 42", retried) { CommandTimeout = 0 };
                    check.Equal<object?>(42, await query.ExecuteScalarAsync(), "same rejected datasource opens after prior owner closes");
                    query.CommandText = "SELECT count(*) FROM npgsql_persistent";
                    check.Equal<object?>(2L, await query.ExecuteScalarAsync(), "retry opens original persisted cluster");
                    query.CommandText = "INSERT INTO npgsql_persistent VALUES (4, 'retry-Žąsis')";
                    check.Equal(1, await query.ExecuteNonQueryAsync(), "same rejected datasource can commit a new row");
                }
                await using (var repeated = await retrySource.OpenConnectionAsync())
                {
                    using var query = new NpgsqlCommand("SELECT note FROM npgsql_persistent WHERE id=4", repeated) { CommandTimeout = 0 };
                    check.Equal<object?>("retry-Žąsis", await query.ExecuteScalarAsync(), "second successful lease preserves retry write");
                    query.CommandText = "SELECT 42";
                    check.Equal<object?>(42, await query.ExecuteScalarAsync(), "same datasource remains reusable across another lease");
                }
            }), check, results);
        }
        catch (Exception exception)
        {
            results.Add(new CaseResult("harness.startup-or-shutdown", 0, 0, exception.ToString()));
            Console.Error.WriteLine(exception);
        }
        WriteReport(report, root, results, check.Assertions);
        int failures = results.Count(result => result.Error != null);
        Console.WriteLine($"{(failures == 0 ? "PASS" : "FAIL")} {check.Assertions} assertions; {results.Count - failures}/{results.Count} cases; report={report}");
        return failures == 0 ? 0 : 1;
    }

    static async Task AssertReopened(NpgsqlConnection connection, Checks check)
    {
        using var command = new NpgsqlCommand("SELECT count(*), min(note) FROM npgsql_persistent", connection) { CommandTimeout = 0 };
        await using (var reader = await command.ExecuteReaderAsync())
        {
            check.True(await reader.ReadAsync(), "reopened persistent row");
            check.Equal(1L, reader.GetInt64(0), "close rolled back outstanding transaction");
            check.Equal("Žąsis", reader.GetString(1), "Unicode persistence");
        }
        command.CommandText = "SELECT to_regclass('pg_temp.npgsql_session_only') IS NULL";
        check.Equal<object?>(true, await command.ExecuteScalarAsync(), "temp table reset on reopen");
        command.CommandText = "SHOW application_name";
        check.False(string.Equals("dirty-session", (string?)await command.ExecuteScalarAsync(), StringComparison.Ordinal), "session GUC reset");
        command.CommandText = "SELECT count(*) FROM pg_prepared_statements WHERE statement='SELECT 12345'";
        check.Equal<object?>(0L, await command.ExecuteScalarAsync(), "prepared statements reset");
        command.CommandText = "SAVEPOINT should_fail_outside_transaction";
        var error = await check.ThrowsAsync<PostgresException>(async () => { await command.ExecuteNonQueryAsync(); }, "transaction state reset");
        check.Equal(PostgresErrorCodes.NoActiveSqlTransaction, error.SqlState, "reopened has no active transaction");
        command.CommandText = "SELECT 42";
        check.Equal<object?>(42, await command.ExecuteScalarAsync(), "reopened recovers after SQL error");
    }

    static async Task Run(AcceptanceCase test, Checks check, List<CaseResult> results)
    {
        int before = check.Assertions;
        var watch = Stopwatch.StartNew();
        string? error = null;
        try { await test.Execute(); }
        catch (Exception exception) { error = exception.ToString(); Console.Error.WriteLine($"FAIL {test.Id}: {error}"); }
        int assertions = check.Assertions - before;
        results.Add(new CaseResult(test.Id, assertions, watch.Elapsed.TotalMilliseconds, error));
        Console.WriteLine($"{(error == null ? "PASS" : "FAIL")} {test.Id} assertions={assertions} elapsed_ms={watch.Elapsed.TotalMilliseconds:F0}");
    }

    static void WriteReport(string path, string root, List<CaseResult> results, int assertions)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        using var output = File.Create(path);
        using var json = new Utf8JsonWriter(output, new JsonWriterOptions { Indented = true });
        json.WriteStartObject();
        json.WriteString("status", results.All(result => result.Error == null) ? "passed" : "failed");
        json.WriteString("core_pin", CorePin);
        json.WriteString("npgsql_release", "v10.0.1");
        json.WriteString("execution", "actual-translated-Managed.PGlite");
        json.WriteString("runtime", RuntimeFeature.IsDynamicCodeSupported ? "jit" : "nativeaot");
        json.WriteString("work", root);
        json.WriteNumber("assertions", assertions);
        json.WriteNumber("numeric_vectors", NumericVectors.Cases.Length);
        json.WriteNumber("case_count", results.Count);
        json.WriteNumber("failed_cases", results.Count(result => result.Error != null));
        json.WriteStartArray("cases");
        foreach (var result in results)
        {
            json.WriteStartObject();
            json.WriteString("id", result.Id);
            json.WriteString("status", result.Error == null ? "passed" : "failed");
            json.WriteNumber("assertions", result.Assertions);
            json.WriteNumber("elapsed_ms", result.Milliseconds);
            if (result.Error != null) json.WriteString("error", result.Error);
            json.WriteEndObject();
        }
        json.WriteEndArray();
        json.WriteEndObject();
        json.Flush();
    }
}
