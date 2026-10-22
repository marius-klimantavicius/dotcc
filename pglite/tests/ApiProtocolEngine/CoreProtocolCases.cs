using System.Buffers.Binary;
using System.Text;

namespace Managed.PGlite.ProductTests;

internal static class CoreProtocolCases
{
    private static int checks;
    internal static int Assertions => checks;
    private static void Check(bool condition, string label)
    {
        if (!condition) throw new InvalidOperationException("Core engine acceptance: " + label);
        checks++;
        Console.WriteLine("PASS core/" + label);
    }
    private static void Reject<T>(Action action, string label) where T : Exception
    {
        try { action(); }
        catch (T) { Check(true, label); return; }
        throw new InvalidOperationException("Accepted " + label);
    }
    private static string Types(byte[] data) => string.Concat(new PgliteProtocolDecoder().Decode(data).Messages.Select(m => m.Type));

    internal static void Run(PgliteDatabase database)
    {
        checks = 0;
        Metadata(database);
        DecodedStream(database);
        PendingExchange(database);
        InvalidInput(database);
        TransactionState(database);
        CopyAndBinary(database);
        Callbacks(database);
        GenericMessages();
        Console.WriteLine($"PASS {checks} additional real-engine core integration assertions.");
    }

    private static void Metadata(PgliteDatabase database)
    {
        var result = database.Exec("CREATE TABLE api_core_metadata(id int); INSERT INTO api_core_metadata VALUES(1),(2); " +
            "SELECT * FROM api_core_metadata ORDER BY id; UPDATE api_core_metadata SET id=id+10; " +
            "DELETE FROM api_core_metadata WHERE id=11; SELECT * FROM api_core_metadata;");
        Check(result.ResultSets.Select(s => s.Command).SequenceEqual(new[] { "CREATE TABLE", "INSERT 0 2", "SELECT 2", "UPDATE 2", "DELETE 1", "SELECT 1" }),
            "metadata/original-command-tags");
        Check(result.ResultSets.Select(s => s.CommandName).SequenceEqual(new[] { "CREATE", "INSERT", "SELECT", "UPDATE", "DELETE", "SELECT" }),
            "metadata/command-names");
        Check(result.ResultSets.Select(s => s.RowCount).SequenceEqual(new long?[] { null, 2, 2, 2, 1, 1 }),
            "metadata/per-statement-row-count");
        Check(result.ResultSets.Select(s => s.AffectedRows).SequenceEqual(new long[] { 0, 2, 2, 4, 5, 5 }) && result.AffectedRows == 5,
            "metadata/cumulative-affected-rows");
        var empty = database.Exec("").ResultSets.Single();
        Check(empty.CommandName == null && empty.RowCount == null && empty.AffectedRows == 0, "metadata/empty-query");
        var copied = database.Exec("COPY api_core_metadata TO STDOUT");
        Check(copied.ResultSets.Single().CommandName == "COPY" && copied.ResultSets[0].RowCount == 1 && copied.AffectedRows == 1,
            "metadata/copy-count");
        Check(database.Exec("SELECT 1").AffectedRows == 0, "metadata/counts-reset-between-operations");
        var merged = database.Exec("MERGE INTO api_core_metadata AS target USING (VALUES(12),(13)) AS source(id) ON target.id=source.id " +
            "WHEN MATCHED THEN UPDATE SET id=source.id WHEN NOT MATCHED THEN INSERT VALUES(source.id)");
        Check(merged.ResultSets[0].CommandName == "MERGE" && merged.ResultSets[0].RowCount == 2 && merged.AffectedRows == 2,
            "metadata/merge-count");
        var unchanged = database.Exec("UPDATE api_core_metadata SET id=0 WHERE false");
        Check(unchanged.ResultSets[0].RowCount == 0 && unchanged.AffectedRows == 0, "metadata/zero-row-count");
    }

    private static void DecodedStream(PgliteDatabase database)
    {
        var messages = new List<char>();
        PgliteQueryResult Send(byte[] data) => database.ExecProtocolStream(data, message => messages.Add(message.Type));
        Check(!Send(PgliteProtocol.EncodeParse("SELECT 42::int AS answer")).IsComplete, "decoded/parse-partial");
        Send(PgliteProtocol.EncodeBind([]));
        var description = Send(PgliteProtocol.EncodeDescribe('P'));
        Check(description.DescribedColumns.Single().Name == "answer", "decoded/description-separate-call");
        var executed = Send(PgliteProtocol.EncodeExecute());
        Check(executed.ResultSets.Single().Columns.Single().TypeOid == 23 &&
              executed.ResultSets[0].TypedRows[0][0] is int answer && answer == 42 && !executed.IsComplete,
            "decoded/execute-preserves-description");
        Check(Send(PgliteProtocol.EncodeSync()).IsComplete && messages.SequenceEqual("12TDCZ"), "decoded/ordered-stream-and-real-sync");

        database.ExecProtocol(PgliteProtocol.Concat(PgliteProtocol.EncodeParse("SELECT generate_series(1,3) AS n"),
            PgliteProtocol.EncodeBind([]), PgliteProtocol.EncodeDescribe('P')));
        var first = database.ExecProtocol(PgliteProtocol.EncodeExecute(maximumRows: 1));
        var second = database.ExecProtocol(PgliteProtocol.EncodeExecute(maximumRows: 1));
        var third = database.ExecProtocol(PgliteProtocol.EncodeExecute());
        Check(first.ResultSets[0].TypedRows[0][0] is int a && a == 1 && second.ResultSets[0].TypedRows[0][0] is int b && b == 2 &&
              third.ResultSets[0].TypedRows[0][0] is int c && c == 3, "decoded/suspended-portal-resumes-with-columns");
        database.ExecProtocol(PgliteProtocol.EncodeSync());
    }

    private static void RejectHighLevel(PgliteDatabase database, string stage)
    {
        Reject<InvalidOperationException>(() => database.Query("SELECT 900"), stage + "/query");
        Reject<InvalidOperationException>(() => database.Query("SELECT $1::int", new object?[] { 900 }), stage + "/parameters");
        Reject<InvalidOperationException>(() => database.Exec("SELECT 900"), stage + "/exec");
        Reject<InvalidOperationException>(() => database.DescribeQuery("SELECT 900"), stage + "/describe");
        Reject<InvalidOperationException>(() => database.RefreshArrayTypes(), stage + "/refresh");
        bool invoked = false;
        Reject<InvalidOperationException>(() => database.RunExclusive(() => { invoked = true; }), stage + "/exclusive");
        Reject<InvalidOperationException>(() => database.Transaction(tx => { invoked = true; }), stage + "/transaction");
        Check(!invoked && database.IsReady, stage + "/callbacks-not-entered-and-owner-healthy");
    }

    private static void PendingExchange(PgliteDatabase database)
    {
        database.ExecProtocolRaw(PgliteProtocol.EncodeParse("SELECT 23"));
        RejectHighLevel(database, "pending/parse");
        database.ExecProtocolRaw(PgliteProtocol.Concat(PgliteProtocol.EncodeBind([]), PgliteProtocol.EncodeExecute()));
        RejectHighLevel(database, "pending/execute-with-command-complete");
        Check(Types(database.ExecProtocolRaw(PgliteProtocol.EncodeSync())) == "Z", "pending/sync-completes-exchange");
        Check(database.Query("SELECT 24").EnsureSuccess().ResultSets[0].Rows[0][0] == "24", "pending/query-after-sync");
        Check(Types(database.ExecProtocolRaw(PgliteProtocol.EncodeQuery("SELECT 25"))) == "TDCZ", "pending/raw-query-after-completion");

        database.ExecProtocolRaw(PgliteProtocol.EncodeParse("SELECT FROM"));
        RejectHighLevel(database, "pending/extended-error");
        Check(database.ExecProtocolRaw(PgliteProtocol.EncodeQuery("SELECT 26")).Length == 0, "pending/raw-query-still-discarded-after-error");
        RejectHighLevel(database, "pending/discarded-query-does-not-close-exchange");
        database.ExecProtocolRaw(PgliteProtocol.EncodeSync());
        Check(database.Query("SELECT 27").EnsureSuccess().ResultSets[0].Rows[0][0] == "27", "pending/error-recovery");

        byte[] parse = PgliteProtocol.EncodeParse("SELECT 28");
        Check(database.ExecProtocolRaw(parse[..3]).Length == 0, "pending/fragment-retained");
        RejectHighLevel(database, "pending/fragment");
        database.ExecProtocolRaw(PgliteProtocol.Concat(parse[3..], PgliteProtocol.EncodeSync()));
        database.RunExclusive(() => database.Query("SELECT 29").EnsureSuccess());
        Check(database.IsReady, "pending/exclusive-after-sync");

        database.ExecProtocolRaw(PgliteProtocol.Concat(PgliteProtocol.EncodeParse("SELECT 35"), PgliteProtocol.EncodeSync(),
            PgliteProtocol.EncodeParse("SELECT 36")));
        RejectHighLevel(database, "pending/earlier-ready-does-not-close-later-exchange");
        database.ExecProtocolRaw(PgliteProtocol.EncodeSync());
        database.ExecProtocolRaw(PgliteProtocol.Concat(PgliteProtocol.EncodeParse("SELECT FROM"), PgliteProtocol.EncodeQuery("SELECT 37"),
            PgliteProtocol.EncodeSync(), PgliteProtocol.EncodeQuery("SELECT 38")));
        Check(database.Query("SELECT 39").IsSuccess, "pending/batched-error-sync-query-completes");
    }

    private static void InvalidInput(PgliteDatabase database)
    {
        Reject<ArgumentException>(() => database.ExecProtocolRaw(PgliteProtocol.EncodeTerminate()), "input/terminate-owned-by-dispose");
        Reject<ArgumentException>(() => database.ExecProtocolRaw(new byte[] { (byte)'P', 0, 0, 0, 3 }), "input/invalid-length");
        Reject<ArgumentException>(() => database.ExecProtocolRaw(PgliteProtocol.Concat(PgliteProtocol.EncodeParse("SELECT 1"),
            PgliteProtocol.EncodeTerminate())), "input/staging-rejects-entire-invalid-batch");
        Check(database.Query("SELECT 30").EnsureSuccess().ResultSets[0].Rows[0][0] == "30", "input/rejection-preserves-owner");
        byte[] parse = PgliteProtocol.EncodeParse("SELECT 31");
        database.ExecProtocolRaw(parse[..4]);
        Reject<ArgumentException>(() => database.ExecProtocolRaw(new byte[] { 0 }), "input/invalid-continuation");
        database.ExecProtocolRaw(PgliteProtocol.Concat(parse[4..], PgliteProtocol.EncodeSync()));
        Check(database.Query("SELECT 32").IsSuccess, "input/rejection-preserves-original-fragment");
        Reject<ArgumentException>(() => database.Query("SELECT \0"), "input/sql-nul");
        Check(database.Query("SELECT 33").IsSuccess, "input/high-level-validation-preserves-owner");
        var failingCodec = PgliteCodecs.Default.Register(23, text => text, _ => throw new FormatException("expected caller encoding failure"));
        Reject<FormatException>(() => database.Query("SELECT $1::int", new object?[] { 1 },
            new PgliteQueryOptions { Codecs = failingCodec }), "input/encoder-exception");
        Check(database.Query("SELECT 34").IsSuccess && database.IsReady, "input/encoder-failure-synchronized-without-quarantine");
    }

    private static void TransactionState(PgliteDatabase database)
    {
        var begun = database.ExecProtocolRaw(PgliteProtocol.EncodeQuery("BEGIN"));
        Check(begun[^1] == (byte)'T' && database.IsInTransaction, "transaction/raw-ready-observes-actual-begin");
        bool entered = false;
        Reject<InvalidOperationException>(() => database.Transaction(_ => { entered = true; }), "transaction/callback-rejects-raw-transaction");
        Check(!entered, "transaction/rejected-before-callback");
        database.ExecProtocolRaw(PgliteProtocol.EncodeQuery("ROLLBACK"));
        Check(!database.IsInTransaction, "transaction/raw-rollback-observes-actual-idle");
        database.Transaction(tx => tx.Query("SELECT 34"));
        Check(!database.IsInTransaction && database.IsReady, "transaction/high-level-after-raw-rollback");

        database.Exec("CREATE TABLE api_core_transaction(id int PRIMARY KEY); INSERT INTO api_core_transaction VALUES(1)");
        database.ExecProtocolRaw(PgliteProtocol.EncodeQuery("BEGIN"));
        Check(Types(database.ExecProtocolRaw(PgliteProtocol.Concat(PgliteProtocol.EncodeParse("INSERT INTO api_core_transaction VALUES(1)"),
            PgliteProtocol.EncodeBind([]), PgliteProtocol.EncodeExecute()))) == "12E", "transaction/extended-error-in-transaction");
        Check(database.ExecProtocolRaw(PgliteProtocol.Concat(PgliteProtocol.EncodeParse("SELECT 35"), PgliteProtocol.EncodeFlush())).Length == 0,
            "transaction/failed-exchange-discards-until-sync");
        var synchronized = database.ExecProtocolRaw(PgliteProtocol.EncodeSync());
        Check(Types(synchronized) == "Z" && synchronized[^1] == (byte)'E' && database.IsInTransaction,
            "transaction/real-failed-status-after-sync");
        var rejected = database.Query("SELECT 36");
        Check(rejected.Errors.Single().SqlState == "25P02" && rejected.TransactionStatus == PgliteTransactionStatus.FailedTransaction,
            "transaction/high-level-observes-raw-failed-transaction");
        database.ExecProtocolRaw(PgliteProtocol.EncodeQuery("ROLLBACK"));
        Check(database.Query("SELECT 37").IsSuccess && !database.IsInTransaction, "transaction/failed-transaction-recovers-by-rollback");
    }

    private static void CopyAndBinary(PgliteDatabase database)
    {
        var binary = database.ExecProtocol(PgliteProtocol.Concat(PgliteProtocol.EncodeParse("SELECT $1::int AS binary_int"),
            PgliteProtocol.EncodeBindRaw(new byte[]?[] { new byte[] { 0, 0, 0, 42 } }, new short[] { 1 }, new short[] { 1 }),
            PgliteProtocol.EncodeDescribe('P'), PgliteProtocol.EncodeExecute(), PgliteProtocol.EncodeSync())).ResultSets.Single();
        Check(binary.Columns[0].FormatCode == 1 && binary.TypedRows[0][0] is byte[] bytes && bytes.SequenceEqual(new byte[] { 0, 0, 0, 42 }),
            "binary/real-parameter-and-result-format");
        Check(binary.RawRows[0][0]!.SequenceEqual(new byte[] { 0, 0, 0, 42 }), "binary/owned-raw-value");

        database.Exec("CREATE TABLE api_core_copy(n int, text_value text)");
        byte[] payload = Encoding.UTF8.GetBytes("1\t" + new string('x', 12000) + "\n2\tŽąsis\n");
        var copied = database.ExecProtocol(PgliteProtocol.Concat(PgliteProtocol.EncodeQuery("COPY api_core_copy FROM STDIN"),
            PgliteProtocol.EncodeCopyData(payload), PgliteProtocol.EncodeCopyDone()));
        Check(copied.HasReadyForQuery && copied.CopyResponses.Single().Direction == 'G' && copied.AffectedRows == 2,
            "copy/bundled-input-crosses-receive-buffer");
        Check(database.Query("SELECT n, length(text_value) FROM api_core_copy ORDER BY n").ResultSets[0].Rows[0][1] == "12000",
            "copy/actual-imported-data");
        var failed = database.ExecProtocol(PgliteProtocol.Concat(PgliteProtocol.EncodeQuery("COPY api_core_copy FROM STDIN"),
            PgliteProtocol.EncodeCopyFail("caller rejected input")), throwOnError: false);
        Check(failed.Errors.Single().SqlState == "57014" && failed.HasReadyForQuery && database.IsReady, "copy/input-failure-recovers");
        var extended = database.ExecProtocol(PgliteProtocol.Concat(PgliteProtocol.EncodeParse("COPY api_core_copy FROM STDIN"),
            PgliteProtocol.EncodeBind([]), PgliteProtocol.EncodeExecute(), PgliteProtocol.EncodeCopyData("3\tthird\n"u8),
            PgliteProtocol.EncodeCopyDone(), PgliteProtocol.EncodeSync()));
        Check(extended.HasReadyForQuery && extended.AffectedRows == 1 && database.Query("SELECT count(*) FROM api_core_copy").ResultSets[0].Rows[0][0] == "3",
            "copy/extended-input-and-sync");
        var exported = database.ExecProtocol(PgliteProtocol.EncodeQuery("COPY api_core_copy TO STDOUT WITH (FORMAT binary)"));
        Check(exported.CopyResponses.Single().Format == 1 && exported.CopyData.SelectMany(b => b).Take(11).SequenceEqual(
            new byte[] { (byte)'P', (byte)'G', (byte)'C', (byte)'O', (byte)'P', (byte)'Y', 10, 255, 13, 10, 0 }),
            "copy/binary-output-preserved");
    }

    private static void Callbacks(PgliteDatabase database)
    {
        int notices = 0, notifications = 0;
        void Notice(PgliteError _) => notices++;
        database.OnNotice += Notice;
        using (database.Listen("api_core_channel", _ => notifications++))
        {
            var raw = database.ExecProtocolRaw(PgliteProtocol.EncodeQuery("DO $$ BEGIN RAISE NOTICE 'raw notice'; END $$; NOTIFY api_core_channel, 'raw payload'"));
            var decoded = new PgliteProtocolDecoder().Decode(raw);
            Check(decoded.Notices.Count == 1 && decoded.Notifications.Single().Payload == "raw payload" && notices == 0 && notifications == 0,
                "callbacks/raw-messages-bypass-managed-listeners");
            database.Exec("DO $$ BEGIN RAISE NOTICE 'decoded notice'; END $$; NOTIFY api_core_channel, 'decoded payload'");
            Check(notices == 1 && notifications == 1, "callbacks/high-level-delivers-real-messages");
        }
        database.OnNotice -= Notice;

        bool entered = false;
        database.ExecProtocolRawStream(PgliteProtocol.EncodeQuery("SELECT 40"), _ =>
        {
            entered = database.Query("SELECT 41").IsSuccess;
        });
        Check(entered, "callbacks/raw-stream-runs-after-core-entry");
        Reject<ApplicationException>(() => database.ExecProtocolRawStream(PgliteProtocol.EncodeQuery("SELECT 42"),
            _ => throw new ApplicationException("expected stream callback failure")), "callbacks/raw-stream-exception");
        Reject<ApplicationException>(() => database.ExecProtocolStream(PgliteProtocol.EncodeQuery("SELECT 43"),
            _ => throw new ApplicationException("expected decoded callback failure")), "callbacks/decoded-stream-exception");
        Check(database.Query("SELECT 44").IsSuccess && database.IsReady, "callbacks/exceptions-preserve-owner");
    }

    internal static void ClosePending(PgliteOptions options)
    {
        foreach (byte[] message in new[] { PgliteProtocol.EncodeParse("SELECT 45"), PgliteProtocol.EncodeParse("SELECT FROM"),
            PgliteProtocol.EncodeParse("SELECT 46")[..3] })
        {
            var database = PgliteDatabase.Open(options);
            database.ExecProtocolRaw(message);
            database.Dispose();
            Check(!database.IsReady && database.Closed, "lifetime/close-pending-exchange-and-reopen");
        }
    }

    private static void GenericMessages()
    {
        byte[] FunctionResult(byte[]? value)
        {
            byte[] frame = new byte[9 + (value?.Length ?? 0)]; frame[0] = (byte)'V';
            BinaryPrimitives.WriteInt32BigEndian(frame.AsSpan(1), frame.Length - 1);
            BinaryPrimitives.WriteInt32BigEndian(frame.AsSpan(5), value?.Length ?? -1);
            value?.CopyTo(frame, 9); return frame;
        }
        var value = new PgliteProtocolDecoder().Decode(FunctionResult(new byte[] { 0, 255 })).Messages.Single();
        Check(value.Name == "functionCallResponse" && value.Value is byte[] data && data.SequenceEqual(new byte[] { 0, 255 }),
            "protocol/function-result-binary");
        Check(new PgliteProtocolDecoder().Decode(FunctionResult(null)).Messages.Single().Value == null, "protocol/function-result-null");
        Reject<InvalidDataException>(() => new PgliteProtocolDecoder().Decode(new byte[] { (byte)'?', 0, 0, 0, 4 }),
            "protocol/unknown-messages-fail-explicitly");
        Check(new PgliteProtocolDecoder().Decode(new byte[] { (byte)'W', 0, 0, 0, 4 }).Messages.Single().Name == "replicationStart",
            "protocol/pinned-replication-start-message");
    }
}
