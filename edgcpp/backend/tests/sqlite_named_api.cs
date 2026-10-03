using System;
using System.Runtime.InteropServices;

// This consumer deliberately uses only original SQLite function and handle
// names. It must compile without a generated-name map or reflection.
internal static unsafe class Program
{
    private static void Require(bool value, string detail)
    {
        if (!value) throw new Exception(detail);
    }

    private static void Execute(SqliteCore core, SqliteCore.sqlite3* database, ReadOnlySpan<byte> sql)
    {
        fixed (byte* text = sql)
            Require(core.sqlite3_exec(database, (sbyte*)text, null, null, null) == 0, "sqlite3_exec");
    }

    private static long Sum(SqliteCore core, SqliteCore.sqlite3* database)
    {
        SqliteCore.sqlite3_stmt* statement = null;
        fixed (byte* sql = "SELECT sum(value) FROM values_table\0"u8)
            Require(core.sqlite3_prepare_v2(database, (sbyte*)sql, -1, &statement, null) == 0,
                    "sqlite3_prepare_v2");
        try
        {
            Require(core.sqlite3_step(statement) == 100, "SQLITE_ROW");
            return core.sqlite3_column_int64(statement, 0);
        }
        finally
        {
            Require(core.sqlite3_finalize(statement) == 0, "sqlite3_finalize");
        }
    }

    public static void Main()
    {
        using var first = new SqliteCore();
        using var second = new SqliteCore();
        SqliteCore.sqlite3* left = null;
        SqliteCore.sqlite3* right = null;
        fixed (byte* name = ":memory:\0"u8)
        {
            Require(first.sqlite3_open((sbyte*)name, &left) == 0, "first sqlite3_open");
            Require(second.sqlite3_open((sbyte*)name, &right) == 0, "second sqlite3_open");
        }
        try
        {
            Require(Marshal.PtrToStringUTF8((nint)first.sqlite3_libversion()) is { Length: > 0 },
                    "sqlite3_libversion");
            Require(first.sqlite3_libversion_number() > 3000000, "sqlite3_libversion_number");
            Execute(first, left, "CREATE TABLE values_table(value); INSERT INTO values_table VALUES(7)\0"u8);
            Execute(second, right, "CREATE TABLE values_table(value); INSERT INTO values_table VALUES(100)\0"u8);
            Execute(first, left, "BEGIN; INSERT INTO values_table VALUES(11)\0"u8);
            Require(Sum(first, left) == 18 && Sum(second, right) == 100, "independent transactions");
            Execute(first, left, "ROLLBACK\0"u8);
            Execute(second, right, "INSERT INTO values_table VALUES(23)\0"u8);
            Require(first.sqlite3_changes(left) == 1 && second.sqlite3_changes(right) == 1,
                    "sqlite3_changes");
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            Require(Sum(first, left) == 7 && Sum(second, right) == 123, "GC and owner isolation");
        }
        finally
        {
            Require(first.sqlite3_close(left) == 0, "first sqlite3_close");
            Require(second.sqlite3_close(right) == 0, "second sqlite3_close");
            Require(first.sqlite3_shutdown() == 0 && second.sqlite3_shutdown() == 0,
                    "sqlite3_shutdown");
        }
        Console.WriteLine("SQLite original public function/type names, independent databases, transactions and GC passed");
    }
}
