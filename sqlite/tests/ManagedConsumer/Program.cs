using System.Runtime.InteropServices;
using System.Text;
using static DotCcLib;

// A separately compiled C# application registers its managed callbacks directly.
// No native SQLite, assembly discovery, reflection, or extension loading is used.
internal static unsafe class Program
{
    private const int Ok = 0, Row = 100, Done = 101;
    private static int destroyed;

    private struct ExtensionState
    {
        public long Bias;
        public int Calls;
    }

    private static void AddBias(sqlite3_context* context, int count, sqlite3_value** values)
    {
        var state = (ExtensionState*)sqlite3_user_data(context);
        ++state->Calls;
        sqlite3_result_int64(context, state->Bias + sqlite3_value_int64(values[0]));
    }

    private static void Destroy(void* state)
    {
        ++destroyed;
        NativeMemory.Free(state);
    }

    private static string Utf8(byte* text) => Marshal.PtrToStringUTF8((nint)text) ?? "";

    private static void Check(int result, sqlite3* db, string operation)
    {
        if (result != Ok)
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
            if (result != Row) throw new InvalidOperationException($"Expected row: {result}: {Utf8(sqlite3_errmsg(db))}");
            var value = Utf8(sqlite3_column_text(statement, 0));
            if (sqlite3_step(statement) != Done) throw new InvalidOperationException("Expected exactly one row");
            return value;
        }
        finally { Check(sqlite3_finalize(statement), db, "finalize"); }
    }

    private static void Expect(sqlite3* db, string sql, string expected)
    {
        var actual = Query(db, sql);
        if (actual != expected) throw new InvalidOperationException($"{sql}: expected {expected}, got {actual}");
    }

    private static int Main()
    {
        sqlite3* db = null;
        try
        {
            fixed (byte* name = "managed-consumer.db\0"u8)
                Check(sqlite3_open(name, &db), db, "open");
            Expect(db, "SELECT sqlite_version()", "3.50.4");
            Expect(db, "SELECT json_extract(jsonb('{\"name\":\"λ\",\"n\":42}'),'$.name')", "λ");
            Expect(db, "SELECT json_valid(jsonb('[1,2,3]'),8)", "1");
            Expect(db, "SELECT sqlite_compileoption_used('ENABLE_FTS5')", "0");

            var state = (ExtensionState*)NativeMemory.AllocZeroed((nuint)sizeof(ExtensionState));
            state->Bias = 35;
            // create_function_v2 owns this context even when registration fails.
            fixed (byte* name = "managed_bias\0"u8)
                Check(sqlite3_create_function_v2(db, name, 1, 1, state, &AddBias, null, null, &Destroy), db, "register callback");
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            Expect(db, "SELECT managed_bias(7)", "42");
            Expect(db, "SELECT managed_bias(json_extract(jsonb('{\"n\":8}'),'$.n'))", "43");
            if (state->Calls != 2) throw new InvalidOperationException("Callback count mismatch");
            fixed (byte* name = "managed_bias\0"u8)
                Check(sqlite3_create_function_v2(db, name, 1, 1, null, null, null, null, null), db, "unregister callback");
            if (destroyed != 1) throw new InvalidOperationException("Context destructor must run exactly once");

            Check(sqlite3_close(db), db, "close");
            db = null;
            if (dotcc_memory_vfs_handle_count() != 0) throw new InvalidOperationException("Leaked VFS handle");
            if (dotcc_memory_vfs_reset() != Ok || sqlite3_shutdown() != Ok)
                throw new InvalidOperationException("Shutdown failed");
            Console.WriteLine("managed consumer: SQLite 3.50.4, JSONB, explicit C# callback, GC and cleanup passed");
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
            return 1;
        }
        finally { if (db != null) sqlite3_close_v2(db); }
    }
}
