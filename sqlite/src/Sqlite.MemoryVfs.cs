namespace Managed.Database;

public static unsafe partial class Sqlite
{
    public const string DOTCC_MEMORY_VFS_NAME = "dotcc-memory";
    public const int DOTCC_VFS_FAIL_OPEN = 1, DOTCC_VFS_FAIL_READ = 2, DOTCC_VFS_FAIL_WRITE = 4,
        DOTCC_VFS_FAIL_TRUNCATE = 8, DOTCC_VFS_FAIL_SYNC = 16, DOTCC_VFS_FAIL_DELETE = 32;
    public static sqlite3_vfs* dotcc_memory_vfs() => MemoryVfs.GetVfs();
    public static int dotcc_memory_vfs_reset() => MemoryVfs.Reset();
    public static int dotcc_memory_vfs_file_count() => MemoryVfs.FileCount();
    public static int dotcc_memory_vfs_handle_count() => MemoryVfs.HandleCount();
    public static void dotcc_memory_vfs_fail_after(int mask, int successfulCalls, int result) => MemoryVfs.FailAfter(mask, successfulCalls, result);
    public static void dotcc_memory_vfs_seed(uint seed) => MemoryVfs.Seed(seed);
    public static void dotcc_memory_vfs_time(long unixMilliseconds) => MemoryVfs.SetTime(unixMilliseconds);
    public static int dotcc_memory_vfs_export(byte* name, void* buffer, long capacity, long* size) => MemoryVfs.Export(name, buffer, capacity, size);
    public static int dotcc_memory_vfs_import(byte* name, void* buffer, long size) => MemoryVfs.Import(name, buffer, size);
}
