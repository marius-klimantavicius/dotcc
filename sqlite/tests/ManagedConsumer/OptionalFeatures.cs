using System.Runtime.InteropServices;
using Managed.Database;
using static global::Managed.Database.Sqlite;

internal static unsafe partial class Program
{
    private static void CheckOptionalFeatures(sqlite3* db)
    {
        Expect(db, "SELECT sqlite_compileoption_used('ENABLE_MATH_FUNCTIONS') AND sqlite_compileoption_used('ENABLE_PERCENTILE') AND sqlite_compileoption_used('ENABLE_COLUMN_METADATA')", "1");
        Expect(db, "SELECT sqrt(81)=9 AND pow(2,10)=1024 AND mod(7,3)=1 AND ceil(-1.5)=-1 AND floor(-1.5)=-2 AND trunc(-1.5)=-1", "1");
        Expect(db, "SELECT abs(log(100)-2)<1e-12 AND abs(log2(32)-5)<1e-12 AND abs(log(2,8)-3)<1e-12 AND abs(ln(exp(2))-2)<1e-12 AND abs(sin(pi()/2)-1)<1e-12", "1");
        Expect(db, "SELECT abs(asinh(sinh(1))-1)<1e-12 AND abs(acosh(cosh(1))-1)<1e-12 AND abs(atanh(tanh(0.5))-0.5)<1e-12 AND abs(degrees(pi())-180)<1e-12", "1");
        Expect(db, "SELECT sqrt(-1) IS NULL AND sqrt(NULL) IS NULL AND log(0) IS NULL AND acos(2) IS NULL", "1");
        Expect(db, "WITH t(x) AS (VALUES(9),(1),(NULL),(3),(3)) SELECT median(x)=3 AND percentile(x,25)=2.5 AND percentile_cont(x,0.75)=4.5 AND percentile_disc(x,0.75)=3 FROM t", "1");
        Expect(db, "SELECT median(x) IS NULL FROM (SELECT 1 AS x WHERE 0)", "1");
        ExpectRows(db, "WITH t(i,x) AS (VALUES(1,1),(2,8),(3,3),(4,6),(5,9)) SELECT median(x) OVER (ORDER BY i ROWS BETWEEN 1 PRECEDING AND 1 FOLLOWING) FROM t ORDER BY i",
            ["4.5"], ["3.0"], ["6.0"], ["6.0"], ["7.5"]);

        Execute(db, "CREATE TABLE metadata_base(id INTEGER PRIMARY KEY,\"naïve\" TEXT); INSERT INTO metadata_base VALUES(1,'one'); CREATE VIEW metadata_view AS SELECT id,\"naïve\" AS label FROM metadata_base; ATTACH ':memory:' AS extra; CREATE TABLE extra.metadata_other(value TEXT); INSERT INTO extra.metadata_other VALUES('two');");
        sqlite3_stmt* statement = null;
        fixed (byte* sql = "SELECT v.label AS renamed,o.value,v.id+1 AS expression FROM metadata_view AS v CROSS JOIN extra.metadata_other AS o\0"u8)
            Check(sqlite3_prepare_v2(db, sql, -1, &statement, null), db, "metadata prepare");
        try
        {
            if (Utf8(sqlite3_column_name(statement, 0)) != "renamed"
                || Utf8(sqlite3_column_decltype(statement, 0)) != "TEXT"
                || Utf8(sqlite3_column_database_name(statement, 0)) != "main"
                || Utf8(sqlite3_column_table_name(statement, 0)) != "metadata_base"
                || Utf8(sqlite3_column_origin_name(statement, 0)) != "naïve")
                throw new InvalidOperationException("UTF8 view/alias column metadata mismatch");
            if (Marshal.PtrToStringUni((nint)sqlite3_column_database_name16(statement, 0)) != "main"
                || Marshal.PtrToStringUni((nint)sqlite3_column_table_name16(statement, 0)) != "metadata_base"
                || Marshal.PtrToStringUni((nint)sqlite3_column_origin_name16(statement, 0)) != "naïve")
                throw new InvalidOperationException("UTF16 column metadata mismatch");
            if (Utf8(sqlite3_column_database_name(statement, 1)) != "extra"
                || Utf8(sqlite3_column_table_name(statement, 1)) != "metadata_other"
                || Utf8(sqlite3_column_origin_name(statement, 1)) != "value")
                throw new InvalidOperationException("Attached database column metadata mismatch");
            if (sqlite3_column_database_name(statement, 2) != null
                || sqlite3_column_table_name(statement, 2) != null
                || sqlite3_column_origin_name(statement, 2) != null
                || sqlite3_column_database_name16(statement, 2) != null
                || sqlite3_column_table_name16(statement, 2) != null
                || sqlite3_column_origin_name16(statement, 2) != null)
                throw new InvalidOperationException("Expression should have no source column metadata");
            GC.Collect();
            if (sqlite3_step(statement) != Row) throw new InvalidOperationException("Missing metadata row");
            Check(sqlite3_reset(statement), db, "metadata reset");
            if (Utf8(sqlite3_column_origin_name(statement, 0)) != "naïve"
                || sqlite3_step(statement) != Row || sqlite3_step(statement) != Done)
                throw new InvalidOperationException("Metadata reset changed the statement");
        }
        finally { Check(sqlite3_finalize(statement), db, "metadata finalize"); }
        Execute(db, "DETACH extra; DROP VIEW metadata_view; DROP TABLE metadata_base;");
        Console.WriteLine("managed optional features: math, percentile aggregates/windows, UTF8/UTF16 column metadata and cleanup passed");
    }
}
