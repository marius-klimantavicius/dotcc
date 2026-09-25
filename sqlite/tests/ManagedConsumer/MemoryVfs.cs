using System.Runtime.InteropServices;
using static global::Managed.Database.Sqlite;

internal static unsafe partial class Program
{
    private static void CheckMemoryVfs()
    {
        var vfs = dotcc_memory_vfs();
        sqlite3* db = null;
        fixed (byte* name = "managed-memory.db\0"u8)
        fixed (byte* vfsName = "dotcc-memory\0"u8)
            Check(sqlite3_open_v2(name, &db, SQLITE_OPEN_READWRITE | SQLITE_OPEN_CREATE, vfsName), db, "memory open");
        try
        {
            Execute(db, "CREATE TABLE data(id INTEGER PRIMARY KEY, text TEXT, bytes BLOB); INSERT INTO data VALUES(1,'après',zeroblob(8192))");
            for (int i = 0; i < 3; i++)
            {
                GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
                GC.WaitForPendingFinalizers();
                Expect(db, "SELECT text||'|'||length(bytes) FROM data", "après|8192");
                if (dotcc_memory_vfs() != vfs)
                    throw new InvalidOperationException("Memory VFS table moved across GC");
            }
            if (dotcc_memory_vfs_reset() != SQLITE_BUSY)
                throw new InvalidOperationException("Memory reset accepted a live connection");
        }
        finally { Check(sqlite3_close(db), null, "memory close"); }

        var file = (sqlite3_file*)NativeMemory.AllocZeroed((nuint)vfs->szOsFile);
        try
        {
            Check(vfs->xOpen(vfs, null, file, SQLITE_OPEN_READWRITE | SQLITE_OPEN_CREATE |
                SQLITE_OPEN_DELETEONCLOSE | SQLITE_OPEN_TEMP_DB, null), null, "temporary memory open");
            // The BCL-backed VFS must report its storage limit without trying to
            // allocate a multi-gigabyte array or throwing across the callback.
            if (file->pMethods->xTruncate(file, (long)Array.MaxLength + 1) != SQLITE_FULL)
                throw new InvalidOperationException("Oversized memory file did not return SQLITE_FULL");
            long size = -1;
            Check(file->pMethods->xFileSize(file, &size), null, "memory size after failed growth");
            if (size != 0) throw new InvalidOperationException("Failed growth changed memory file size");
        }
        finally
        {
            if (file->pMethods != null) Check(file->pMethods->xClose(file), null, "temporary memory close");
            NativeMemory.Free(file);
        }
        if (dotcc_memory_vfs_handle_count() != 0 || dotcc_memory_vfs_reset() != SQLITE_OK
            || dotcc_memory_vfs_file_count() != 0)
            throw new InvalidOperationException("Memory VFS leaked state");
        Console.WriteLine("managed memory VFS: compacting GC, stable tables, bounded storage, busy reset and cleanup passed");
    }
}
