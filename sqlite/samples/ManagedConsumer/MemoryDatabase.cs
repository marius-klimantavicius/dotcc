using Managed.Database;
using static Managed.Database.Sqlite;

internal static unsafe partial class Program
{
    private static void CheckMemoryDatabase()
    {
        fixed (byte* name = "dotcc-memory\0"u8)
            if (sqlite3_vfs_find(name) != null)
                throw new InvalidOperationException("Test-only VFS registered in the product");

        var handles = HostVfs.OpenHandleCount;
        sqlite3* db = null;
        fixed (byte* name = ":memory:\0"u8)
            Check(sqlite3_open(name, &db), db, "open built-in memory database");
        try
        {
            Expect(db, "PRAGMA journal_mode", "memory");
            Execute(db, "CREATE TABLE sample(value); INSERT INTO sample VALUES(42)");
            GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
            Expect(db, "SELECT value FROM sample", "42");
            if (HostVfs.OpenHandleCount != handles)
                throw new InvalidOperationException("Memory database opened a host file");
        }
        finally { Check(sqlite3_close(db), db, "close memory database"); }

        fixed (byte* name = ":memory:\0"u8)
            Check(sqlite3_open(name, &db), db, "reopen built-in memory database");
        try { Expect(db, "SELECT count(*) FROM sqlite_schema WHERE name='sample'", "0"); }
        finally { Check(sqlite3_close(db), db, "close fresh memory database"); }
    }
}
