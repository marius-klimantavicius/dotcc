#nullable enable
using System.Runtime.InteropServices;

namespace Edg.Runtime;

// Portable translated LP64 records, never passed directly to a host OS API.
// Kept in sync with runtime/posix/include and verified against emitted C layouts.
[StructLayout(LayoutKind.Sequential)]
public struct PosixStat
{
    public ulong st_dev, st_ino;
    public uint st_mode;
    public ulong st_nlink;
    public uint st_uid, st_gid;
    public long st_size, st_atime, st_mtime, st_ctime;
    public ulong st_rdev;
    public long st_blksize, st_blocks;
}

[StructLayout(LayoutKind.Sequential)]
public struct PosixFlock
{
    public short l_type, l_whence;
    public long l_start, l_len;
    public int l_pid;
}

[StructLayout(LayoutKind.Sequential)]
public struct PosixTimeval
{
    public long tv_sec, tv_usec;
}

[StructLayout(LayoutKind.Sequential)]
public struct PosixStatvfs
{
    public ulong f_bsize, f_frsize, f_blocks, f_bfree, f_bavail;
    public ulong f_files, f_ffree, f_favail, f_fsid, f_flag, f_namemax;
}

[StructLayout(LayoutKind.Sequential)]
public struct PosixWinsize
{
    public ushort ws_row, ws_col, ws_xpixel, ws_ypixel;
}
