using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text;
using Managed.Database;
using static global::Managed.Database.Sqlite;

// A separately compiled C# application registers its managed callbacks directly.
// No native SQLite, assembly discovery, reflection, or extension loading is used.
internal static unsafe partial class Program
{
    private static int destroyed;
    private static int ftsDestroyed;
    private static readonly delegate*<void*, void> FreePointer = SqliteFunctionPointers.sqlite3_free;
    private static readonly delegate*<sqlite3_context*, int, sqlite3_value**, void> CallbackPointer = &AddBias;
    private static readonly delegate*<void*, void> DestroyPointer = &Destroy;
    private static readonly delegate*<void*, void> Transient = (delegate*<void*, void>)(nint)(-1);
    private static readonly delegate*<Fts5ExtensionApi*, Fts5Context*, sqlite3_context*, int, sqlite3_value**, void> FtsCallbackPointer = &FtsHits;
    private static readonly delegate*<void*, void> FtsDestroyPointer = &DestroyFts;
    private static readonly delegate*<void*, int, byte*, int, int, int, int> TokenPointer = &CountToken;

    private struct ExtensionState
    {
        public long Bias;
        public int Calls;
    }

    private static void AddBias(sqlite3_context* context, int count, sqlite3_value** values)
    {
        try
        {
            var state = (ExtensionState*)sqlite3_user_data(context);
            ++state->Calls;
            var input = sqlite3_value_int64(values[0]);
            // Reenter the same connection through a separate statement while
            // the caller's statement and callback context remain live.
            var db = sqlite3_context_db_handle(context);
            sqlite3_stmt* nested = null;
            long result;
            fixed (byte* sql = "SELECT ?1 + json_extract(jsonb_object('n',?2),'$.n')\0"u8)
                Check(sqlite3_prepare_v2(db, sql, -1, &nested, null), db, "nested prepare");
            try
            {
                Check(sqlite3_bind_int64(nested, 1, state->Bias), db, "nested bind bias");
                Check(sqlite3_bind_int64(nested, 2, input), db, "nested bind input");
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                if (sqlite3_step(nested) != SQLITE_ROW) throw new InvalidOperationException("Nested query expected row");
                result = sqlite3_column_int64(nested, 0);
                if (sqlite3_step(nested) != SQLITE_DONE) throw new InvalidOperationException("Nested query expected done");
            }
            finally { Check(sqlite3_finalize(nested), db, "nested finalize"); }
            sqlite3_result_int64(context, result);
        }
        catch (Exception error)
        {
            // Report managed extension failures through SQLite's result API.
            var message = Encoding.UTF8.GetBytes(error.Message);
            fixed (byte* text = message) sqlite3_result_error(context, text, message.Length);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static delegate*<void*, void> CaptureFree() => FreePointer;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static delegate*<sqlite3_context*, int, sqlite3_value**, void> CaptureCallback() => CallbackPointer;

    private static void CheckFunctionIdentity()
    {
        var saved = CaptureFree();
        var callback = CaptureCallback();
        for (var index = 0; index < 50_000; ++index)
        {
            var fresh = CaptureFree();
            if ((nint)fresh != (nint)saved || (nint)CaptureCallback() != (nint)callback)
                throw new InvalidOperationException("Function pointer identity changed during warmup");
            var memory = sqlite3_malloc64(16);
            if (memory == null) throw new OutOfMemoryException();
            fresh(memory);
            if (index % 10_000 == 0) { GC.Collect(); GC.WaitForPendingFinalizers(); }
        }
    }

    private static void Destroy(void* state)
    {
        ++destroyed;
        NativeMemory.Free(state);
    }

    private static void DestroyFts(void* state)
    {
        ++ftsDestroyed;
        NativeMemory.Free(state);
    }

    private static void FtsHits(Fts5ExtensionApi* api, Fts5Context* fts, sqlite3_context* context, int count, sqlite3_value** values)
    {
        var state = (ExtensionState*)api->xUserData(fts);
        var instances = 0;
        if (state == null || state->Bias != 42 || count != 0 || api->xColumnCount(fts) != 2 || api->xInstCount(fts, &instances) != SQLITE_OK)
        {
            fixed (byte* message = "Managed FTS5 auxiliary contract failed\0"u8)
                sqlite3_result_error(context, message, -1);
            return;
        }
        ++state->Calls;
        GC.Collect();
        sqlite3_result_int64(context, api->xRowid(fts) * 100 + instances);
    }

    private static int CountToken(void* context, int flags, byte* token, int length, int start, int end)
    {
        if (token == null || length <= 0 || start < 0 || end <= start) return 1;
        ++*(int*)context;
        return SQLITE_OK;
    }

    private static string Utf8(byte* text) => Marshal.PtrToStringUTF8((nint)text) ?? "";

    private static void Check(int result, sqlite3* db, string operation)
    {
        if (result != SQLITE_OK)
            throw new InvalidOperationException($"{operation}: {result}: {Utf8(sqlite3_errmsg(db))}");
    }

    private static string Query(sqlite3* db, string sql)
    {
        var bytes = Encoding.UTF8.GetBytes(sql + "\0");
        sqlite3_stmt* statement = null;
        fixed (byte* text = bytes)
            Check(sqlite3_prepare_v2(db, text, -1, &statement, null), db, "prepare");
        try
        {
            var result = sqlite3_step(statement);
            if (result != SQLITE_ROW) throw new InvalidOperationException($"Expected row: {result}: {Utf8(sqlite3_errmsg(db))}");
            var value = Utf8(sqlite3_column_text(statement, 0));
            if (sqlite3_step(statement) != SQLITE_DONE) throw new InvalidOperationException("Expected exactly one row");
            return value;
        }
        finally { Check(sqlite3_finalize(statement), db, "finalize"); }
    }

    private static void Expect(sqlite3* db, string sql, string expected)
    {
        var actual = Query(db, sql);
        if (actual != expected) throw new InvalidOperationException($"{sql}: expected {expected}, got {actual}");
    }

    private static void Execute(sqlite3* db, string sql, int? expectedChanges = null)
    {
        var bytes = Encoding.UTF8.GetBytes(sql + "\0");
        byte* error = null;
        try
        {
            fixed (byte* text = bytes)
            {
                var result = sqlite3_exec(db, text, null, null, &error);
                if (result != SQLITE_OK)
                    throw new InvalidOperationException($"{sql}: {result}: {Utf8(error)}");
            }
            if (expectedChanges is int expected && sqlite3_changes(db) != expected)
                throw new InvalidOperationException($"{sql}: expected {expected} changed rows, got {sqlite3_changes(db)}");
        }
        finally { FreePointer(error); }
    }

    private static void ExpectRows(sqlite3* db, string sql, params string?[][] expected)
    {
        var bytes = Encoding.UTF8.GetBytes(sql + "\0");
        sqlite3_stmt* statement = null;
        fixed (byte* text = bytes)
            Check(sqlite3_prepare_v2(db, text, -1, &statement, null), db, "prepare rows");
        try
        {
            var row = 0;
            int result;
            while ((result = sqlite3_step(statement)) == SQLITE_ROW)
            {
                if (row >= expected.Length) throw new InvalidOperationException($"{sql}: unexpected row {row}");
                var columns = sqlite3_column_count(statement);
                if (columns != expected[row].Length)
                    throw new InvalidOperationException($"{sql}: expected {expected[row].Length} columns, got {columns}");
                for (var column = 0; column < columns; ++column)
                {
                    var actual = sqlite3_column_type(statement, column) == 5 ? null : Utf8(sqlite3_column_text(statement, column));
                    if (actual != expected[row][column])
                        throw new InvalidOperationException($"{sql}: row {row}, column {column}: expected {expected[row][column] ?? "NULL"}, got {actual ?? "NULL"}");
                }
                ++row;
            }
            if (result != SQLITE_DONE || row != expected.Length)
                throw new InvalidOperationException($"{sql}: expected {expected.Length} rows and DONE, got {row} rows and {result}: {Utf8(sqlite3_errmsg(db))}");
        }
        finally { Check(sqlite3_finalize(statement), db, "finalize rows"); }
    }

    private static void InsertOrders(sqlite3* db)
    {
        sqlite3_stmt* statement = null;
        fixed (byte* sql = "INSERT INTO orders(id,customer_id,amount,status) VALUES(?1,?2,?3,?4)\0"u8)
            Check(sqlite3_prepare_v2(db, sql, -1, &statement, null), db, "prepare insert");
        try
        {
            foreach (var order in new[] {
                (Id: 101, Customer: 1, Amount: 1200, Status: "paid"),
                (Id: 102, Customer: 1, Amount: 800, Status: "pending"),
                (Id: 103, Customer: 2, Amount: 500, Status: "paid"),
                (Id: 104, Customer: 3, Amount: 1500, Status: "paid"),
                (Id: 105, Customer: 3, Amount: 700, Status: "cancelled"),
                (Id: 106, Customer: 4, Amount: 900, Status: "pending") })
            {
                Check(sqlite3_bind_int64(statement, 1, order.Id), db, "bind order id");
                Check(sqlite3_bind_int64(statement, 2, order.Customer), db, "bind customer");
                Check(sqlite3_bind_int64(statement, 3, order.Amount), db, "bind amount");
                var status = Encoding.UTF8.GetBytes(order.Status);
                fixed (byte* text = status)
                    Check(sqlite3_bind_text(statement, 4, text, status.Length, Transient), db, "bind status");
                if (sqlite3_step(statement) != SQLITE_DONE)
                    throw new InvalidOperationException($"Insert {order.Id}: {Utf8(sqlite3_errmsg(db))}");
                if (sqlite3_changes(db) != 1) throw new InvalidOperationException("Insert expected one changed row");
                Check(sqlite3_reset(statement), db, "reset insert");
                Check(sqlite3_clear_bindings(statement), db, "clear insert bindings");
            }
        }
        finally { Check(sqlite3_finalize(statement), db, "finalize insert"); }
    }

    private static void CheckSqlWorkloads(sqlite3* db)
    {
        Execute(db, "PRAGMA foreign_keys=ON");
        Expect(db, "PRAGMA foreign_keys", "1");
        Execute(db, """
            CREATE TABLE customers(id INTEGER PRIMARY KEY, name TEXT NOT NULL UNIQUE, metadata BLOB NOT NULL);
            CREATE TABLE orders(id INTEGER PRIMARY KEY, customer_id INTEGER NOT NULL REFERENCES customers(id),
                amount INTEGER NOT NULL CHECK(amount>=0), status TEXT NOT NULL);
            CREATE INDEX orders_customer_status ON orders(customer_id,status);
            CREATE TABLE order_audit(order_id INTEGER, old_amount INTEGER, new_amount INTEGER, new_status TEXT);
            CREATE TRIGGER audit_order_update AFTER UPDATE ON orders BEGIN
                INSERT INTO order_audit VALUES(old.id,old.amount,new.amount,new.status);
            END;
            CREATE VIEW paid_totals AS SELECT customer_id,COUNT(*) AS order_count,SUM(amount) AS total
                FROM orders WHERE status='paid' GROUP BY customer_id;
            """);
        Execute(db, """
            INSERT INTO customers VALUES
                (1,'Ada',jsonb('{"tier":"gold","tags":["sql","dotnet"]}')),
                (2,'Linus',jsonb('{"tier":"silver","tags":["systems"]}')),
                (3,'Grace',jsonb('{"tier":"gold","tags":["sql"]}')),
                (4,'Edsger',jsonb('{"tier":"silver","tags":[]}'));
            """, 4);
        Execute(db, "BEGIN");
        InsertOrders(db);
        Execute(db, "COMMIT");

        ExpectRows(db, "SELECT id,name FROM customers ORDER BY id",
            ["1", "Ada"], ["2", "Linus"], ["3", "Grace"], ["4", "Edsger"]);
        ExpectRows(db, "SELECT id,amount FROM orders WHERE status='pending' AND amount>=800 ORDER BY amount DESC",
            ["106", "900"], ["102", "800"]);
        ExpectRows(db, """
            SELECT c.name,t.order_count,t.total FROM customers c JOIN paid_totals t ON t.customer_id=c.id
            WHERE t.total>=500 ORDER BY t.total DESC,c.id
            """, ["Grace", "1", "1500"], ["Ada", "1", "1200"], ["Linus", "1", "500"]);
        ExpectRows(db, """
            SELECT c.name FROM customers c WHERE EXISTS (
                SELECT 1 FROM orders o WHERE o.customer_id=c.id AND o.status='paid'
                AND o.amount>(SELECT AVG(amount) FROM orders WHERE status='paid')) ORDER BY c.id
            """, ["Ada"], ["Grace"]);
        ExpectRows(db, """
            WITH totals AS (SELECT customer_id,SUM(amount) AS total FROM orders WHERE status='paid' GROUP BY customer_id)
            SELECT c.name,t.total,ROW_NUMBER() OVER(ORDER BY t.total DESC,c.id),
                SUM(t.total) OVER(ORDER BY t.total DESC,c.id ROWS UNBOUNDED PRECEDING)
            FROM totals t JOIN customers c ON c.id=t.customer_id ORDER BY t.total DESC,c.id
            """, ["Grace", "1500", "1", "1500"], ["Ada", "1200", "2", "2700"], ["Linus", "500", "3", "3200"]);
        ExpectRows(db, """
            SELECT c.name,json_extract(c.metadata,'$.tier'),typeof(c.metadata)
            FROM customers c,json_each(c.metadata,'$.tags') tag WHERE tag.value='sql' ORDER BY c.id
            """, ["Ada", "gold", "blob"], ["Grace", "gold", "blob"]);
        ExpectRows(db, "SELECT c.name,t.total FROM customers c LEFT JOIN paid_totals t ON t.customer_id=c.id WHERE t.total IS NULL",
            ["Edsger", null]);

        Execute(db, "UPDATE orders SET status='paid' WHERE id=102", 1);
        Execute(db, """
            UPDATE orders SET amount=amount+100 WHERE status='paid' AND EXISTS (
                SELECT 1 FROM customers c WHERE c.id=orders.customer_id AND json_extract(c.metadata,'$.tier')='gold')
            """, 3);
        Execute(db, "INSERT INTO orders VALUES(107,2,1000,'pending')", 1);
        Execute(db, """
            INSERT INTO orders VALUES(107,2,1100,'paid') ON CONFLICT(id)
            DO UPDATE SET amount=excluded.amount,status=excluded.status
            """, 1);
        ExpectRows(db, "SELECT id,amount,status FROM orders WHERE id IN(101,102,104,107) ORDER BY id",
            ["101", "1300", "paid"], ["102", "900", "paid"], ["104", "1600", "paid"], ["107", "1100", "paid"]);
        Expect(db, "SELECT COUNT(*) FROM order_audit", "5");

        Execute(db, "BEGIN");
        Execute(db, "UPDATE orders SET amount=9999 WHERE id=102", 1);
        Expect(db, "SELECT amount FROM orders WHERE id=102", "9999");
        Execute(db, "SAVEPOINT draft");
        Execute(db, "INSERT INTO orders VALUES(108,4,42,'pending')", 1);
        Execute(db, "ROLLBACK TO draft");
        Execute(db, "RELEASE draft");
        Expect(db, "SELECT COUNT(*) FROM orders WHERE id=108", "0");
        Execute(db, "ROLLBACK");
        Expect(db, "SELECT amount FROM orders WHERE id=102", "900");
        Expect(db, "SELECT COUNT(*) FROM order_audit", "5");

        Execute(db, "DELETE FROM orders WHERE status='cancelled'", 1);
        Execute(db, """
            DELETE FROM orders WHERE status='pending' AND NOT EXISTS (
                SELECT 1 FROM orders paid WHERE paid.customer_id=orders.customer_id AND paid.status='paid')
            """, 1);
        Execute(db, "DELETE FROM customers WHERE NOT EXISTS(SELECT 1 FROM orders WHERE customer_id=customers.id)", 1);
        ExpectRows(db, """
            SELECT c.name,t.order_count,t.total FROM customers c JOIN paid_totals t ON t.customer_id=c.id
            ORDER BY t.total DESC,c.id
            """, ["Ada", "2", "2200"], ["Linus", "2", "1600"], ["Grace", "1", "1600"]);
        Expect(db, "SELECT COUNT(*)||':'||SUM(amount) FROM orders", "5:5400");
        ExpectRows(db, "PRAGMA foreign_key_check");
        Expect(db, "PRAGMA integrity_check", "ok");
        Console.WriteLine("managed SQL: tables, bound inserts, joins, CTE/window/JSONB queries, updates, UPSERT, deletes, rollback and integrity passed");
    }

    private static void CheckFullTextSearch(sqlite3* db)
    {
        Expect(db, "SELECT sqlite_compileoption_used('ENABLE_FTS5')", "1");
        Execute(db, "CREATE VIRTUAL TABLE articles USING fts5(title,body,tokenize='unicode61')");
        Execute(db, """
            INSERT INTO articles(rowid,title,body) VALUES
                (1,'SQLite guide','SQLite supports transactions and full text search'),
                (2,'Cooking notes','A recipe for fresh tomato soup'),
                (3,'C# integration','Managed SQLite callbacks and JSONB')
            """, 3);
        ExpectRows(db, "SELECT rowid,title FROM articles WHERE articles MATCH 'sqlite' ORDER BY rowid",
            ["1", "SQLite guide"], ["3", "C# integration"]);
        ExpectRows(db, "SELECT rowid FROM articles WHERE articles MATCH '\"full text\"'", ["1"]);
        Execute(db, "UPDATE articles SET body='SQLite recipes and database tips' WHERE rowid=2", 1);
        ExpectRows(db, "SELECT rowid FROM articles WHERE articles MATCH 'sqlite' ORDER BY rowid", ["1"], ["2"], ["3"]);
        ExpectRows(db, "SELECT rowid FROM articles WHERE articles MATCH 'sqlite AND callbacks'", ["3"]);
        ExpectRows(db, "SELECT highlight(articles,0,'[',']') FROM articles WHERE articles MATCH 'integration'", ["C# [integration]"]);
        Execute(db, "DELETE FROM articles WHERE rowid=1", 1);
        ExpectRows(db, "SELECT rowid FROM articles WHERE articles MATCH 'sqlite' ORDER BY rowid", ["2"], ["3"]);
        Execute(db, "INSERT INTO articles(articles) VALUES('integrity-check')");
        CheckFtsExtension(db);
        Console.WriteLine("managed FTS5: table, inserts, term/phrase/boolean search, highlighting, update, delete and integrity passed");
    }

    private static void CheckFtsExtension(sqlite3* db)
    {
        fts5_api* api = null;
        sqlite3_stmt* statement = null;
        fixed (byte* sql = "SELECT fts5(?1)\0"u8)
            Check(sqlite3_prepare_v2(db, sql, -1, &statement, null), db, "prepare FTS5 API");
        try
        {
            fixed (byte* type = "fts5_api_ptr\0"u8)
            {
                Check(sqlite3_bind_pointer(statement, 1, &api, type, null), db, "bind FTS5 API");
                if (sqlite3_step(statement) != SQLITE_ROW || sqlite3_step(statement) != SQLITE_DONE)
                    throw new InvalidOperationException("FTS5 API query did not return one row");
            }
        }
        finally { Check(sqlite3_finalize(statement), db, "finalize FTS5 API"); }
        if (api == null || api->iVersion < 2) throw new InvalidOperationException("FTS5 API unavailable");

        var state = (ExtensionState*)NativeMemory.AllocZeroed((nuint)sizeof(ExtensionState));
        if (state == null) throw new OutOfMemoryException();
        state->Bias = 42;
        var registered = false;
        try
        {
            fixed (byte* name = "managed_hits\0"u8)
                Check(api->xCreateFunction(api, name, state, FtsCallbackPointer, FtsDestroyPointer), db, "register FTS5 auxiliary");
            registered = true;
            ExpectRows(db, "SELECT managed_hits(articles) FROM articles WHERE articles MATCH 'sqlite' ORDER BY rowid", ["201"], ["301"]);
            if (state->Calls != 2 || ftsDestroyed != 0)
                throw new InvalidOperationException("FTS5 auxiliary lifetime/call count mismatch");
        }
        finally
        {
            // FTS5 retains successful registrations until connection close.
            // Its xCreateFunction does not take ownership when registration fails.
            if (!registered) NativeMemory.Free(state);
        }

        fts5_tokenizer tokenizer = default;
        void* tokenizerContext = null;
        Fts5Tokenizer* instance = null;
        fixed (byte* name = "unicode61\0"u8)
            Check(api->xFindTokenizer(api, name, &tokenizerContext, &tokenizer), db, "find FTS5 tokenizer");
        Check(tokenizer.xCreate(tokenizerContext, null, 0, &instance), db, "create FTS5 tokenizer");
        try
        {
            var count = 0;
            fixed (byte* text = "one two three"u8)
                Check(tokenizer.xTokenize(instance, &count, 1, text, 13, TokenPointer), db, "tokenize from C#");
            if (count != 3) throw new InvalidOperationException("FTS5 tokenizer callback count mismatch");
        }
        finally { tokenizer.xDelete(instance); }
    }

    private static int Main(string[] args)
    {
        if (args.Length != 0) return EndianEntry(args);
        sqlite3* db = null;
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-managed-consumer-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var databasePath = Path.Combine(directory, "managed-consumer.db");
        try
        {
            fixed (byte* name = Encoding.UTF8.GetBytes(databasePath + "\0"))
                Check(sqlite3_open(name, &db), db, "open");
            Expect(db, "SELECT sqlite_version()", SQLITE_VERSION);
            Expect(db, "PRAGMA journal_mode=WAL", "wal");
            Expect(db, "SELECT json_extract(jsonb('{\"name\":\"λ\",\"n\":42}'),'$.name')", "λ");
            Expect(db, "SELECT json_valid(jsonb('[1,2,3]'),8)", "1");
            CheckEndianDatabases(directory, write: true);
            CheckOptionalFeatures(db);
            CheckPreupdateHook(db);
            CheckSqlWorkloads(db);
            CheckFullTextSearch(db);
            CheckFunctionIdentity();
            CheckCursorInitialization();

            var state = (ExtensionState*)NativeMemory.AllocZeroed((nuint)sizeof(ExtensionState));
            state->Bias = 35;
            // create_function_v2 owns this context even when registration fails.
            fixed (byte* name = "managed_bias\0"u8)
                Check(sqlite3_create_function_v2(db, name, 1, SQLITE_UTF8, state, CallbackPointer, null, null, DestroyPointer), db, "register callback");
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            Expect(db, "SELECT managed_bias(7)", "42");
            Expect(db, "SELECT managed_bias(json_extract(jsonb('{\"n\":8}'),'$.n'))", "43");
            if (state->Calls != 2) throw new InvalidOperationException("Callback count mismatch");
            fixed (byte* name = "managed_bias\0"u8)
                Check(sqlite3_create_function_v2(db, name, 1, SQLITE_UTF8, null, null, null, null, null), db, "unregister callback");
            if (destroyed != 1) throw new InvalidOperationException("Context destructor must run exactly once");

            Check(sqlite3_wal_checkpoint_v2(db, null, SQLITE_CHECKPOINT_TRUNCATE, null, null), db, "truncate checkpoint");
            Check(sqlite3_close(db), db, "close");
            db = null;
            if (!File.Exists(databasePath) || new FileInfo(databasePath).Length == 0)
                throw new InvalidOperationException("The default VFS did not persist a database file");
            if (Managed.Database.HostVfs.OpenHandleCount != 0) throw new InvalidOperationException("Leaked host VFS handle");
            if (ftsDestroyed != 1) throw new InvalidOperationException("FTS5 context destructor must run exactly once on close");
            if (dotcc_memory_vfs_handle_count() != 0) throw new InvalidOperationException("Leaked VFS handle");
            if (dotcc_memory_vfs_reset() != SQLITE_OK || sqlite3_shutdown() != SQLITE_OK)
                throw new InvalidOperationException("Shutdown failed");
            Console.WriteLine("managed consumer: SQLite 3.53.4, WAL SQL workloads, JSONB, FTS5, cached C# callbacks, nested SQL, function identity, GC and cleanup passed");
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
            return 1;
        }
        finally
        {
            if (db != null) sqlite3_close_v2(db);
            Directory.Delete(directory, recursive: true);
        }
    }
}
