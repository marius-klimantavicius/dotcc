using System.Formats.Tar;
using System.Text;

namespace Managed.PGlite.ApiServicesTests;

/// <summary>Link into the ordinary consumer. Every SQL assertion runs the actual
/// delivered engine; no alternative backend or mocked database is used.</summary>
public static class ApiServicesCases
{
    public static int Run(PgliteDatabase db, Action<string>? report = null)
    {
        var cases = new Cases(report);
        cases.Check("lifecycle-ready", db.Ready && !db.Closed);
        db.WaitReady().GetAwaiter().GetResult();
        db.Exec("CREATE TABLE api_services_values (id integer PRIMARY KEY, value text)");
        PgliteTransaction? retained = null;
        int committed = db.Transaction(tx =>
        {
            retained = tx;
            tx.Query("INSERT INTO api_services_values VALUES ($1, $2)", [1, "Žąsis"]);
            cases.Throws<InvalidOperationException>("transaction-direct-db-rejected", () => db.Query("SELECT 1"));
            cases.Throws<InvalidOperationException>("transaction-nested-rejected", () => db.Transaction(_ => 0));
            cases.Throws<InvalidOperationException>("transaction-close-rejected", db.Close);
            cases.Throws<InvalidOperationException>("transaction-sql-commit-rejected", () => tx.Exec("SELECT ';'; /* nested /* block */ */ COMMIT"));
            cases.Check("transaction-token-query", tx.Query("SELECT count(*) FROM api_services_values").ResultSets.Single().Rows.Single()[0] == "1");
            return 17;
        });
        cases.Check("transaction-generic-return", committed == 17);
        db.Transaction(tx =>
        {
            tx.Exec("PREPARE api_services$prepared$(int) AS SELECT $1::int AS value");
            cases.Check("transaction-prepare-statement-allowed", tx.Query("EXECUTE api_services$prepared$(42)").ResultSets.Single().Rows.Single()[0] == "42");
            tx.Exec("DEALLOCATE api_services$prepared$; SAVEPOINT api_services_savepoint; INSERT INTO api_services_values VALUES(99,'savepoint'); ROLLBACK /* comment */ TO SAVEPOINT api_services_savepoint");
            cases.Check("transaction-savepoint-rollback-allowed", tx.Query("SELECT count(*) FROM api_services_values").ResultSets.Single().Rows.Single()[0] == "1");
            cases.Throws<InvalidOperationException>("transaction-prepare-transaction-rejected", () => tx.Exec("PREPARE /* comment */ TRANSACTION 'must-not-escape'"));
            tx.Exec("SAVEPOINT recover_error");
            try { tx.Query("SELECT 1 / 0"); }
            catch (PgliteSqlException) { }
            tx.Exec("ROLLBACK TO SAVEPOINT recover_error");
            cases.Check("transaction-savepoint-recovers-aborted-state", tx.Query("SELECT 42").ResultSets.Single().Rows.Single()[0] == "42");
        });
        db.Exec("CREATE TEMP TABLE api_guard$tag$(id int)");
        cases.Throws<InvalidOperationException>("transaction-dollar-identifier-cannot-hide-commit", () => db.Transaction(tx =>
        {
            tx.Exec("INSERT INTO api_guard$tag$ VALUES(1); COMMIT");
            throw new ApplicationException("This callback must not commit.");
        }));
        cases.Check("transaction-hidden-commit-left-no-row", Scalar(db, "SELECT count(*) FROM api_guard$tag$") == "0");
        db.Exec("CREATE DOMAIN api_guard$e AS text");
        cases.Throws<InvalidOperationException>("transaction-type-name-suffix-is-not-escape-prefix", () => db.Transaction(tx =>
        {
            tx.Exec("INSERT INTO api_guard$tag$ VALUES(1); SELECT api_guard$e'\\'; COMMIT; --'");
            throw new ApplicationException("A type-name suffix must not hide COMMIT.");
        }));
        cases.Check("transaction-type-name-commit-left-no-row", Scalar(db, "SELECT count(*) FROM api_guard$tag$") == "0");
        cases.Throws<InvalidOperationException>("transaction-unicode-dollar-tag-control-rejected", () => db.Transaction(tx =>
        {
            tx.Exec("SET LOCAL check_function_bodies=off");
            tx.Exec("INSERT INTO api_guard$tag$ VALUES(1); CREATE FUNCTION api_guard_unicode() RETURNS text AS $a\u0301$' $a\u0301$ LANGUAGE sql; COMMIT; --'");
            throw new ApplicationException("A Unicode dollar tag must not hide COMMIT.");
        }));
        cases.Check("transaction-unicode-tag-left-no-row", Scalar(db, "SELECT count(*) FROM api_guard$tag$") == "0");
        cases.Check("transaction-token-closed-after-commit", retained!.Closed && !db.IsInTransaction);
        cases.Throws<InvalidOperationException>("transaction-closed-query", () => retained.Query("SELECT 1"));
        cases.Throws<InvalidOperationException>("transaction-closed-exec", () => retained.Exec("SELECT 1"));
        cases.Throws<InvalidOperationException>("transaction-closed-sql", () => retained.Sql(PgliteTemplate.Raw("SELECT 1")));
        db.Transaction(tx => { retained = tx; tx.Exec("INSERT INTO api_services_values VALUES (2, 'rollback')"); tx.Rollback(); });
        cases.Check("transaction-explicit-rollback", retained!.Closed && Scalar(db, "SELECT count(*) FROM api_services_values") == "1");
        cases.Throws<ApplicationException>("transaction-callback-error", () => db.Transaction(tx =>
        { retained = tx; tx.Exec("INSERT INTO api_services_values VALUES (3, 'exception')"); throw new ApplicationException("callback failure"); }));
        cases.Check("transaction-error-token-closed", retained!.Closed && Scalar(db, "SELECT count(*) FROM api_services_values") == "1");
        cases.Throws<PgliteSqlException>("transaction-sql-error-rollback", () => db.Transaction(tx =>
        { tx.Exec("INSERT INTO api_services_values VALUES (4, 'error')"); tx.Query("SELECT 1/0"); }));
        cases.Check("transaction-error-recovered", db.Ready && !db.IsInTransaction && Scalar(db, "SELECT count(*) FROM api_services_values") == "1");
        cases.Throws<ArgumentException>("transaction-async-delegate-rejected", () => db.Transaction(_ => Task.FromResult(1)));
        db.ExecProtocolRaw(PgliteProtocol.EncodeQuery("BEGIN"));
        cases.Check("transaction-raw-state-visible", db.IsInTransaction);
        cases.Throws<InvalidOperationException>("transaction-raw-owner-not-adopted", () => db.Transaction(_ => 0));
        db.ExecProtocolRaw(PgliteProtocol.EncodeQuery("ROLLBACK"));
        cases.Check("transaction-raw-state-idle", !db.IsInTransaction);
        cases.Check("exclusive-result", db.RunExclusive(() => Scalar(db, "SELECT 41")) == "41");
        cases.Throws<ArgumentException>("exclusive-async-rejected", () => db.RunExclusive(() => Task.FromResult(1)));

        int first = 0, second = 0, global = 0;
        Action<string> firstCallback = payload => { if (payload != "payload") throw new Exception("wrong notification payload"); first++; };
        Action<string> secondCallback = _ => second++;
        using var globalSubscription = db.OnNotification((channel, payload) =>
        { if (channel == "api_services_channel" && payload == "payload") global++; });
        using var firstSubscription = db.Listen("API_Services_Channel", firstCallback);
        using var secondSubscription = db.Listen("api_services_channel", secondCallback);
        db.Exec("NOTIFY api_services_channel, 'payload'");
        cases.Check("notifications-two-listeners-and-global", first == 1 && second == 1 && global == 1);
        firstSubscription.Dispose();
        db.Exec("NOTIFY api_services_channel, 'payload'");
        cases.Check("notifications-partial-unlisten", first == 1 && second == 2 && global == 2);
        db.Transaction(tx =>
        {
            tx.Exec("NOTIFY api_services_channel, 'payload'");
            cases.Check("notifications-deferred-before-commit", second == 2);
        });
        cases.Check("notifications-delivered-after-commit", second == 3);
        db.Transaction(tx => { tx.Exec("NOTIFY api_services_channel, 'payload'"); tx.Rollback(); });
        cases.Check("notifications-discarded-after-rollback", second == 3);
        secondSubscription.Dispose();
        db.Exec("NOTIFY api_services_channel, 'payload'");
        cases.Check("notifications-unlisten-last", second == 3 && global == 3);
        int special = 0;
        using (db.Listen("\"Quoted \"\"Channel\"\"\"", _ => special++)) db.Exec("NOTIFY \"Quoted \"\"Channel\"\"\", 'payload'");
        cases.Check("notifications-quoted-identifier", special == 1);
        cases.Throws<ArgumentException>("notifications-injection-rejected", () => db.Listen("channel; SELECT 1", _ => { }));

        int notices = 0; bool reentryRejected = false;
        Action<PgliteError> noticeCallback = _ =>
        {
            notices++;
            try { db.Query("SELECT 1"); } catch (InvalidOperationException) { reentryRejected = true; }
        };
        db.OnNotice += noticeCallback;
        db.Exec("DO $$ BEGIN RAISE NOTICE 'managed notice'; END $$");
        db.OnNotice -= noticeCallback;
        cases.Check("notice-after-core-no-reentry", notices == 1 && reentryRejected && db.Ready);
        Action<PgliteError> throwingCallback = _ => throw new ApplicationException("notice callback failure");
        db.OnNotice += throwingCallback;
        cases.Throws<AggregateException>("notice-callback-error-contained", () => db.Exec("DO $$ BEGIN RAISE NOTICE 'failure'; END $$"));
        db.OnNotice -= throwingCallback;
        cases.Check("notice-callback-error-not-quarantined", Scalar(db, "SELECT 81") == "81" && db.Ready);

        var csv = db.CopyTo(path => $"COPY api_services_values TO {path} WITH (FORMAT csv)");
        cases.Check("copy-csv-bytes", Encoding.UTF8.GetString(csv.Data) == "1,Žąsis\n" && csv.Result.ResultSets.Single().Command == "COPY 1");
        db.Exec("CREATE TABLE api_services_copy (LIKE api_services_values INCLUDING ALL)");
        var imported = db.CopyFrom(path => $"COPY api_services_copy FROM {path} WITH (FORMAT csv)", csv.Data);
        cases.Check("copy-csv-import", imported.ResultSets.Single().Command == "COPY 1" && Scalar(db, "SELECT value FROM api_services_copy") == "Žąsis");
        var binary = db.CopyTo(path => $"COPY api_services_values TO {path} WITH (FORMAT binary)");
        db.Exec("TRUNCATE api_services_copy");
        using (var binaryInput = new MemoryStream(binary.Data)) db.CopyFrom(path => $"COPY api_services_copy FROM {path} WITH (FORMAT binary)", binaryInput);
        cases.Check("copy-binary-stream-roundtrip", Scalar(db, "SELECT value FROM api_services_copy") == "Žąsis");
        cases.Throws<PgliteSqlException>("copy-invalid-data-error", () => db.CopyFrom(path => $"COPY api_services_copy FROM {path} WITH (FORMAT csv)", Encoding.UTF8.GetBytes("bad-id,value\n")));
        cases.Check("copy-error-recovery", db.Ready && Scalar(db, "SELECT count(*) FROM api_services_copy") == "1");

        int updates = 0; PgliteLiveResult? latest = null;
        using var live = db.Live.Query("SELECT id, value FROM api_services_values ORDER BY id", callback: result => { updates++; latest = result; });
        cases.Check("live-initial-results", live.InitialResults.Rows.Count == 1 && (int)live.InitialResults.Rows[0]["id"]! == 1);
        using var changes = db.Live.Changes("SELECT id, value FROM api_services_values ORDER BY id", "id");
        cases.Check("live-initial-insert", changes.InitialChanges.Count == 1 && changes.InitialChanges[0].Operation == PgliteLiveOperation.Insert && changes.InitialChanges[0].After == null);
        IReadOnlyList<PgliteLiveChange>? lastChanges = null;
        changes.Subscribe(batch => lastChanges = batch);
        using var incremental = db.Live.IncrementalQuery("SELECT id, value FROM api_services_values ORDER BY id", "id");
        db.Transaction(tx =>
        {
            tx.Exec("INSERT INTO api_services_values VALUES (5, 'live')");
            cases.Check("live-no-uncommitted-refresh", updates == 0);
        });
        cases.Check("live-auto-refresh-at-commit", updates == 1 && latest!.Rows.Count == 2 && incremental.Current.Rows.Count == 2);
        cases.Check("live-ordered-insert-change", lastChanges is { Count: 1 } && lastChanges[0].Operation == PgliteLiveOperation.Insert && (int)lastChanges[0].After! == 1);
        db.Exec("UPDATE api_services_values SET value = 'changed' WHERE id = 5");
        cases.Check("live-update-changed-columns", lastChanges![0].Operation == PgliteLiveOperation.Update && lastChanges[0].ChangedColumns.SequenceEqual(new[] { "value" }));
        db.Exec("DELETE FROM api_services_values WHERE id = 1");
        cases.Check("live-delete-and-order-change", lastChanges!.Any(change => change.Operation == PgliteLiveOperation.Delete) && lastChanges.Any(change => change.ChangedColumns.Contains("__after__")));
        int beforeRollback = updates;
        db.Transaction(tx => { tx.Exec("INSERT INTO api_services_values VALUES (6, 'not-live')"); tx.Rollback(); });
        cases.Check("live-no-rollback-result-change", updates == beforeRollback && live.Current.Rows.Count == 1);
        using var window = db.Live.Query(new PgliteLiveQueryOptions("SELECT id, value FROM api_services_values ORDER BY id") { Offset = 0, Limit = 1 });
        cases.Check("live-window", window.Current.TotalCount == 1 && window.Current.Rows.Count == 1);
        window.Refresh(offset: 1, limit: 1);
        cases.Check("live-window-refresh", window.Current.Rows.Count == 0 && window.Current.TotalCount == 1);
        using (var cancellation = new CancellationTokenSource())
        {
            int cancelledCallbacks = 0;
            using var cancelled = db.Live.Query("SELECT id FROM api_services_values ORDER BY id", callback: _ => cancelledCallbacks++, cancellationToken: cancellation.Token);
            cancellation.Cancel();
            db.Exec("SELECT 1");
            cases.Check("live-cancellation-observed-on-owner", cancelled.Closed && cancelledCallbacks == 0);
        }
        cases.Throws<ArgumentException>("live-duplicate-key-rejected", () => db.Live.Changes("SELECT 1 AS id UNION ALL SELECT 1", "id"));
        cases.Throws<PgliteSqlException>("live-readonly-enforced", () => db.Live.Query("DELETE FROM api_services_values RETURNING id"));
        live.Dispose();
        cases.Throws<ObjectDisposedException>("live-resubscribe-after-dispose-rejected", () => live.Subscribe(_ => { }));
        db.SyncToFs();
        cases.Check("storage-sync-idle", db.Ready && !db.IsInTransaction);
        cases.Throws<InvalidOperationException>("storage-dump-in-transaction-rejected", () => db.Transaction(_ => db.DumpDataDir()));
        changes.Dispose(); incremental.Dispose(); window.Dispose();
        db.Exec("DROP TABLE api_services_copy; DROP TABLE api_services_values");
        return cases.Count;
    }

    /// <summary>Run after closing all other databases. Tests persistence archives,
    /// extension lifecycle, ephemeral cleanup and worker ownership in new lifetimes.</summary>
    public static int RunLifetimes(PgliteOptions seed, Action<string>? report = null)
    {
        var cases = new Cases(report);
        string root = Path.Combine(seed.WorkDirectory, "api-services-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        int setup = 0, initialized = 0, closed = 0;
        object namespaced = new();
        var extension = new PgliteExtension("managed_test", db =>
        {
            setup++;
            cases.Check("extension-setup-ready", db.Ready);
            return new(namespaced, () =>
            {
                initialized++;
                db.Exec("CREATE TABLE api_services_persist (id integer PRIMARY KEY, value text)");
            }, () => { closed++; });
        });
        var storage = PgliteStorage.Temporary(root);
        string temporary = storage.DataDirectory;
        var options = new PgliteOptions(storage, seed.BootstrapPrefix, Path.Combine(root, "diagnostics"))
        { Extensions = [PgliteExtension.Plpgsql, PgliteExtension.Snowball, extension] };
        byte[] gzip, tar;
        using (var db = PgliteDatabase.Create(options))
        {
            cases.Check("extension-managed-namespace", ReferenceEquals(db.GetExtension<object>("managed_test"), namespaced) && setup == 1 && initialized == 1);
            cases.Throws<InvalidOperationException>("storage-active-dispose-rejected", storage.Dispose);
            db.Exec("INSERT INTO api_services_persist VALUES (17, 'archive Žąsis')");
            gzip = db.DumpDataDir(PgliteDumpCompression.Gzip);
            tar = db.DumpDataDir(PgliteDumpCompression.None);
            cases.Check("storage-gzip-header", gzip[0] == 0x1f && gzip[1] == 0x8b);
            cases.Check("storage-tar-output", tar.Length > 1024 && tar.Length % 512 == 0);
            cases.Throws<InvalidOperationException>("lifecycle-foreign-thread-dispose-rejected", () => Task.Run(() => db.DisposeAsync().GetAwaiter().GetResult()).GetAwaiter().GetResult());
        }
        cases.Check("extension-close-once", closed == 1);
        cases.Check("storage-ephemeral-delete-after-close", !Directory.Exists(temporary));
        storage.Dispose();
        var restored = new PgliteOptions(Path.Combine(root, "restored"), seed.BootstrapPrefix, Path.Combine(root, "restore-diagnostics"));
        using (var archive = new MemoryStream(gzip))
        using (var db = PgliteDatabase.Restore(restored, archive))
            cases.Check("storage-gzip-restore-query", Scalar(db, "SELECT value FROM api_services_persist WHERE id = 17") == "archive Žąsis");
        int unwindClose = 0;
        var initFailure = new PgliteExtension("failure", _ => new(Initialize: () => throw new ApplicationException("init failure"), Close: () => unwindClose++));
        cases.Throws<ApplicationException>("extension-init-failure-unwinds", () => PgliteDatabase.Open(restored with { Extensions = [initFailure] }));
        cases.Check("extension-init-failure-closer", unwindClose == 1);
        using (var reopened = PgliteDatabase.Open(restored))
            cases.Check("extension-init-failure-releases-owner", Scalar(reopened, "SELECT 7") == "7");
        var closeFailure = new PgliteExtension("failure", _ => new(Close: () => throw new ApplicationException("close failure")));
        var closeDatabase = PgliteDatabase.Open(restored with { Extensions = [closeFailure] });
        cases.Throws<AggregateException>("extension-close-error-reported", closeDatabase.Close);
        cases.Check("extension-close-error-still-closed", closeDatabase.Closed);
        closeDatabase.Close();
        using (var reopened = PgliteDatabase.Open(restored))
            cases.Check("extension-close-error-releases-owner", Scalar(reopened, "SELECT 9") == "9");
        var uncompressed = restored with { DataDirectory = Path.Combine(root, "restored-tar") };
        using (var archive = new MemoryStream(tar))
        using (var db = PgliteDatabase.Restore(uncompressed, archive))
            cases.Check("storage-uncompressed-restore-query", Scalar(db, "SELECT id FROM api_services_persist") == "17");
        using (var malformed = new MemoryStream())
        {
            using (var writer = new TarWriter(malformed, leaveOpen: true))
                writer.WriteEntry(new PaxTarEntry(TarEntryType.RegularFile, "../escape") { DataStream = new MemoryStream([1]) });
            malformed.Position = 0;
            cases.Throws<InvalidDataException>("storage-traversal-rejected", () => PgliteDatabase.RestoreDataDirectory(malformed, Path.Combine(root, "bad-restore"), PgliteDumpCompression.None));
        }
        cases.Throws<NotSupportedException>("extension-wasm-rejected", () => PgliteDatabase.Open(restored with { Extensions = [new("unavailable", Kind: PgliteExtensionKind.Wasm)] }));
        cases.Throws<NotSupportedException>("extension-native-rejected", () => PgliteDatabase.Open(restored with { Extensions = [new("unavailable", Kind: PgliteExtensionKind.Native)] }));
        int workerCallbackThread = 0;
        var workerOptions = restored with { DataDirectory = Path.Combine(root, "worker") };
        using (var worker = PgliteWorker.Create(workerOptions))
        {
            worker.WaitReady().GetAwaiter().GetResult();
            cases.Check("worker-ready", worker.Ready && !worker.Closed);
            worker.InvokeAsync(db =>
            {
                workerCallbackThread = Environment.CurrentManagedThreadId;
                db.Exec("CREATE TABLE api_services_worker (id integer PRIMARY KEY)");
                cases.Throws<ArgumentException>("worker-async-delegate-rejected", () => worker.InvokeAsync(_ => Task.FromResult(1)));
                cases.Throws<InvalidOperationException>("worker-owner-nested-enqueue-rejected", () => worker.QueryAsync("SELECT 1"));
            }).GetAwaiter().GetResult();
            var operations = Enumerable.Range(1, 6).Select(i => worker.QueryAsync("INSERT INTO api_services_worker VALUES ($1)", [i])).ToArray();
            Task.WhenAll(operations).GetAwaiter().GetResult();
            var count = worker.QueryAsync("SELECT count(*) FROM api_services_worker").GetAwaiter().GetResult();
            cases.Check("worker-concurrent-callers-serialized", count.ResultSets.Single().Rows.Single()[0] == "6");
            cases.Check("worker-owner-thread", workerCallbackThread == worker.OwnerThreadId && workerCallbackThread != Environment.CurrentManagedThreadId);
            using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
            cases.Throws<TaskCanceledException>("worker-prequeue-cancellation", () => worker.QueryAsync("SELECT 1", cancellation.Token).GetAwaiter().GetResult());
            worker.CloseAsync().GetAwaiter().GetResult();
            cases.Check("worker-close", worker.Closed && !worker.Ready);
            cases.Throws<ObjectDisposedException>("worker-query-after-close-rejected", () => worker.QueryAsync("SELECT 1").GetAwaiter().GetResult());
        }
        return cases.Count;
    }

    private static string? Scalar(PgliteDatabase db, string sql) => db.Query(sql).EnsureSuccess().ResultSets.Single().Rows.Single()[0];
    private sealed class Cases(Action<string>? report)
    {
        public int Count { get; private set; }
        public void Check(string name, bool condition)
        {
            if (!condition) throw new InvalidOperationException("API services case failed: " + name);
            Count++; report?.Invoke("PASS api-services/" + name);
        }
        public void Throws<T>(string name, Action action) where T : Exception
        {
            try { action(); }
            catch (T) { Check(name, true); return; }
            throw new InvalidOperationException("Expected " + typeof(T).Name + " in " + name);
        }
    }
}
