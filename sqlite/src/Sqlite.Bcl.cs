using System;
using System.Text;
using System.Threading;

namespace Managed.Database;

public static unsafe partial class Sqlite
{
    public static int sqlite3_open_v2(string databasePath, sqlite3** db, int connectionFlags, string? vfsName)
    {
        var databasePathUtf8 = Encoding.UTF8.GetBytes(databasePath);
        var vfsNameUtf8 = default(byte[]);
        if (vfsName != null)
            vfsNameUtf8 = Encoding.UTF8.GetBytes(vfsName);

        fixed (byte* databasePathPtr = databasePathUtf8)
        fixed (byte* vfsNamePtr = vfsNameUtf8)
            return sqlite3_open_v2(databasePathPtr, db, connectionFlags, vfsNamePtr);
    }

    public static int sqlite3_prepare_v2(sqlite3* db, string statementText, sqlite3_stmt** statement)
    {
        fixed (char* statementPtr = statementText)
            return sqlite3_prepare16_v2(db, statementPtr, statementText.Length * sizeof(char), statement, null);
    }

    public static int sqlite3_bind_text16(sqlite3_stmt* statement, int index, string value)
    {
        fixed (char* ptr = value)
            return sqlite3_bind_text16(statement, index, ptr, value.Length * sizeof(char), SQLITE_TRANSIENT);
    }

    public static int sqlite3_bind_text16(sqlite3_stmt* statement, int index, int c)
    {
        return sqlite3_bind_text16(statement, index, &c, sizeof(char), SQLITE_TRANSIENT);
    }

    public static int sqlite3_bind_blob(sqlite3_stmt* statement, int index, ReadOnlySpan<byte> value)
    {
        fixed (byte* ptr = value)
            return sqlite3_bind_blob(statement, index, ptr, value.Length, SQLITE_TRANSIENT);
    }

    public static int dotcc_host_vfs_init() => HostVfs.RegisterVfs();

    public static int dotcc_host_vfs_end() => HostVfs.UnregisterVfs();

    public static sqlite3_mutex_methods* dotcc_host_mutex_methods() => HostMutex.GetMutextMethods();

    public static void dotcc_host_memory_barrier() => Thread.MemoryBarrier();
}