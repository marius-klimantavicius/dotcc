// OS calls only. The SQLite Unix VFS algorithms are translated from upstream C.
using System;
using System.Runtime.InteropServices;
namespace Managed.Database.UpstreamUnix;
public static unsafe partial class Libc
{
    [DllImport("dotcc_sqlite_os", EntryPoint="uvfs_open", SetLastError=true)]
    private static extern int Native_open(byte* path, int flags, int mode);
    public static int uvfs_open(byte* path, int flags, params ReadOnlySpan<VaArg> tail) { var result = Native_open(path, flags, tail.Length == 0 ? 0 : (int)tail[0]); errno = Marshal.GetLastPInvokeError(); return result; }
    [DllImport("dotcc_sqlite_os", EntryPoint="uvfs_close", SetLastError=true)]
    private static extern int Native_close(int fd);
    public static int uvfs_close(int fd) { var result = Native_close(fd); errno = Marshal.GetLastPInvokeError(); return result; }
    [DllImport("dotcc_sqlite_os", EntryPoint="uvfs_access", SetLastError=true)]
    private static extern int Native_access(byte* path, int mode);
    public static int uvfs_access(byte* path, int mode) { var result = Native_access(path, mode); errno = Marshal.GetLastPInvokeError(); return result; }
    [DllImport("dotcc_sqlite_os", EntryPoint="uvfs_getcwd", SetLastError=true)]
    private static extern byte* Native_getcwd(byte* path, ulong size);
    public static byte* uvfs_getcwd(byte* path, ulong size) { var result = Native_getcwd(path, size); errno = Marshal.GetLastPInvokeError(); return result; }
    [DllImport("dotcc_sqlite_os", EntryPoint="uvfs_stat", SetLastError=true)]
    private static extern int Native_stat(byte* path, uvfs_stat* s);
    public static int uvfs_stat(byte* path, uvfs_stat* s) { var result = Native_stat(path, s); errno = Marshal.GetLastPInvokeError(); return result; }
    [DllImport("dotcc_sqlite_os", EntryPoint="uvfs_fstat", SetLastError=true)]
    private static extern int Native_fstat(int fd, uvfs_stat* s);
    public static int uvfs_fstat(int fd, uvfs_stat* s) { var result = Native_fstat(fd, s); errno = Marshal.GetLastPInvokeError(); return result; }
    [DllImport("dotcc_sqlite_os", EntryPoint="uvfs_lstat", SetLastError=true)]
    private static extern int Native_lstat(byte* path, uvfs_stat* s);
    public static int uvfs_lstat(byte* path, uvfs_stat* s) { var result = Native_lstat(path, s); errno = Marshal.GetLastPInvokeError(); return result; }
    [DllImport("dotcc_sqlite_os", EntryPoint="uvfs_ftruncate", SetLastError=true)]
    private static extern int Native_ftruncate(int fd, long size);
    public static int uvfs_ftruncate(int fd, long size) { var result = Native_ftruncate(fd, size); errno = Marshal.GetLastPInvokeError(); return result; }
    [DllImport("dotcc_sqlite_os", EntryPoint="uvfs_read", SetLastError=true)]
    private static extern long Native_read(int fd, void* buf, ulong size);
    public static long uvfs_read(int fd, void* buf, ulong size) { var result = Native_read(fd, buf, size); errno = Marshal.GetLastPInvokeError(); return result; }
    [DllImport("dotcc_sqlite_os", EntryPoint="uvfs_write", SetLastError=true)]
    private static extern long Native_write(int fd, void* buf, ulong size);
    public static long uvfs_write(int fd, void* buf, ulong size) { var result = Native_write(fd, buf, size); errno = Marshal.GetLastPInvokeError(); return result; }
    [DllImport("dotcc_sqlite_os", EntryPoint="uvfs_pread", SetLastError=true)]
    private static extern long Native_pread(int fd, void* buf, ulong size, long offset);
    public static long uvfs_pread(int fd, void* buf, ulong size, long offset) { var result = Native_pread(fd, buf, size, offset); errno = Marshal.GetLastPInvokeError(); return result; }
    [DllImport("dotcc_sqlite_os", EntryPoint="uvfs_pwrite", SetLastError=true)]
    private static extern long Native_pwrite(int fd, void* buf, ulong size, long offset);
    public static long uvfs_pwrite(int fd, void* buf, ulong size, long offset) { var result = Native_pwrite(fd, buf, size, offset); errno = Marshal.GetLastPInvokeError(); return result; }
    [DllImport("dotcc_sqlite_os", EntryPoint="uvfs_lseek", SetLastError=true)]
    private static extern long Native_lseek(int fd, long offset, int whence);
    public static long uvfs_lseek(int fd, long offset, int whence) { var result = Native_lseek(fd, offset, whence); errno = Marshal.GetLastPInvokeError(); return result; }
    [DllImport("dotcc_sqlite_os", EntryPoint="uvfs_fchmod", SetLastError=true)]
    private static extern int Native_fchmod(int fd, uint mode);
    public static int uvfs_fchmod(int fd, uint mode) { var result = Native_fchmod(fd, mode); errno = Marshal.GetLastPInvokeError(); return result; }
    [DllImport("dotcc_sqlite_os", EntryPoint="uvfs_fchown", SetLastError=true)]
    private static extern int Native_fchown(int fd, uint uid, uint gid);
    public static int uvfs_fchown(int fd, uint uid, uint gid) { var result = Native_fchown(fd, uid, gid); errno = Marshal.GetLastPInvokeError(); return result; }
    [DllImport("dotcc_sqlite_os", EntryPoint="uvfs_geteuid", SetLastError=true)]
    private static extern uint Native_geteuid();
    public static uint uvfs_geteuid() { var result = Native_geteuid(); errno = Marshal.GetLastPInvokeError(); return result; }
    [DllImport("dotcc_sqlite_os", EntryPoint="uvfs_getpid", SetLastError=true)]
    private static extern int Native_getpid();
    public static int uvfs_getpid() { var result = Native_getpid(); errno = Marshal.GetLastPInvokeError(); return result; }
    [DllImport("dotcc_sqlite_os", EntryPoint="uvfs_unlink", SetLastError=true)]
    private static extern int Native_unlink(byte* path);
    public static int uvfs_unlink(byte* path) { var result = Native_unlink(path); errno = Marshal.GetLastPInvokeError(); return result; }
    [DllImport("dotcc_sqlite_os", EntryPoint="uvfs_mkdir", SetLastError=true)]
    private static extern int Native_mkdir(byte* path, uint mode);
    public static int uvfs_mkdir(byte* path, uint mode) { var result = Native_mkdir(path, mode); errno = Marshal.GetLastPInvokeError(); return result; }
    [DllImport("dotcc_sqlite_os", EntryPoint="uvfs_rmdir", SetLastError=true)]
    private static extern int Native_rmdir(byte* path);
    public static int uvfs_rmdir(byte* path) { var result = Native_rmdir(path); errno = Marshal.GetLastPInvokeError(); return result; }
    [DllImport("dotcc_sqlite_os", EntryPoint="uvfs_readlink", SetLastError=true)]
    private static extern long Native_readlink(byte* path, byte* buf, ulong size);
    public static long uvfs_readlink(byte* path, byte* buf, ulong size) { var result = Native_readlink(path, buf, size); errno = Marshal.GetLastPInvokeError(); return result; }
    [DllImport("dotcc_sqlite_os", EntryPoint="uvfs_fsync", SetLastError=true)]
    private static extern int Native_fsync(int fd);
    public static int uvfs_fsync(int fd) { var result = Native_fsync(fd); errno = Marshal.GetLastPInvokeError(); return result; }
    [DllImport("dotcc_sqlite_os", EntryPoint="uvfs_fdatasync", SetLastError=true)]
    private static extern int Native_fdatasync(int fd);
    public static int uvfs_fdatasync(int fd) { var result = Native_fdatasync(fd); errno = Marshal.GetLastPInvokeError(); return result; }
    [DllImport("dotcc_sqlite_os", EntryPoint="uvfs_getpagesize", SetLastError=true)]
    private static extern int Native_getpagesize();
    public static int uvfs_getpagesize() { var result = Native_getpagesize(); errno = Marshal.GetLastPInvokeError(); return result; }
    [DllImport("dotcc_sqlite_os", EntryPoint="uvfs_mmap", SetLastError=true)]
    private static extern void* Native_mmap(void* address, ulong size, int prot, int flags, int fd, long offset);
    public static void* uvfs_mmap(void* address, ulong size, int prot, int flags, int fd, long offset) { var result = Native_mmap(address, size, prot, flags, fd, offset); errno = Marshal.GetLastPInvokeError(); return result; }
    [DllImport("dotcc_sqlite_os", EntryPoint="uvfs_munmap", SetLastError=true)]
    private static extern int Native_munmap(void* address, ulong size);
    public static int uvfs_munmap(void* address, ulong size) { var result = Native_munmap(address, size); errno = Marshal.GetLastPInvokeError(); return result; }
    [DllImport("dotcc_sqlite_os", EntryPoint="uvfs_fcntl", SetLastError=true)]
    private static extern int Native_fcntl(int fd, int command, long argument);
    public static int uvfs_fcntl(int fd, int command, params ReadOnlySpan<VaArg> tail)
    {
        var result = Native_fcntl(fd, command, tail.Length == 0 ? 0 : (long)tail[0]);
        errno = Marshal.GetLastPInvokeError();
        return result;
    }
    [DllImport("dotcc_sqlite_os", EntryPoint="uvfs_sysconf", SetLastError=true)]
    private static extern long Native_sysconf(int n);
    public static long uvfs_sysconf(int n) { var result=Native_sysconf(n); errno=Marshal.GetLastPInvokeError(); return result; }
    [DllImport("dotcc_sqlite_os", EntryPoint="uvfs_utimes", SetLastError=true)]
    private static extern int Native_utimes(byte* path, void* times);
    public static int uvfs_utimes(byte* path, void* times) { var result=Native_utimes(path,times); errno=Marshal.GetLastPInvokeError(); return result; }
}
