using System.Diagnostics;
using System.Globalization;
using Managed.Database;
using static global::Managed.Database.Sqlite;

internal static unsafe class Program
{
    private static int Main()
    {
        sqlite3* db = null;
        sqlite3_stmt* statement = null;
        try
        {
            fixed (byte* name = ":memory:\0"u8)
                Require(sqlite3_open(name, &db) == 0, "open");
            fixed (byte* sql = "CREATE TABLE data(value INTEGER); WITH RECURSIVE t(x) AS (VALUES(1) UNION ALL SELECT x+1 FROM t WHERE x<1000) INSERT INTO data SELECT x FROM t;\0"u8)
                Require(sqlite3_exec(db, sql, null, null, null) == 0, "seed");
            fixed (byte* sql = "SELECT sum(value) FROM data WHERE value%3=1\0"u8)
                Require(sqlite3_prepare_v2(db, sql, -1, &statement, null) == 0, "prepare");
            for (var i = 0; i < 1000; i++) Require(Query(statement) == 167167, "warmup");
            for (var round = 0; round < 3; round++)
            {
                var allocated = GC.GetAllocatedBytesForCurrentThread();
                var started = Stopwatch.GetTimestamp();
                long total = 0;
                for (var i = 0; i < 3000; i++) total += Query(statement);
                var elapsed = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
                var bytes = GC.GetAllocatedBytesForCurrentThread() - allocated;
                Require(total == 501501000, "measured query results");
                Console.WriteLine("{\"total\":" + total + ",\"iterations\":3000,\"elapsed_ms\":" + elapsed.ToString(CultureInfo.InvariantCulture) + ",\"managed_bytes\":" + bytes + "}");
            }
            return 0;
        }
        finally
        {
            if (statement != null) Require(sqlite3_finalize(statement) == 0, "finalize");
            if (db != null) Require(sqlite3_close(db) == 0, "close");
        }
    }

    private static long Query(sqlite3_stmt* statement)
    {
        Require(sqlite3_step(statement) == 100, "row");
        var value = sqlite3_column_int64(statement, 0);
        Require(sqlite3_step(statement) == 101, "done");
        Require(sqlite3_reset(statement) == 0, "reset");
        return value;
    }

    private static void Require(bool condition, string operation)
    {
        if (!condition) throw new InvalidOperationException(operation);
    }
}
