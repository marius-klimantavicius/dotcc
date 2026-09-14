using global::System;
using global::System.Text;
using Managed.Emulation.Host;

namespace Managed.Emulation;

public static partial class Blink
{
    public static unsafe int blink_host_stat(byte* path, blink_host_stat_record* destination)
        => FileMetadataPath(path, destination);
    public static unsafe int blink_host_lstat(byte* path, blink_host_stat_record* destination)
        => FileMetadataPath(path, destination);
    private static unsafe int FileMetadataPath(byte* path, blink_host_stat_record* destination, int directoryFd = -100)
    {
        try
        {
            if (io == null) return IoError(19);
            if (path == null || destination == null) return IoError(14);
            int length = 0;
            while (length < PathLimit && path[length] != 0) ++length;
            if (length == PathLimit) return IoError(36);
            string name;
            try { name = PathEncoding.GetString(new ReadOnlySpan<byte>(path, length)); }
            catch (DecoderFallbackException) { return IoError(22); }
            var result = io.StatAt(directoryFd, name);
            if (!result.Succeeded) return IoError((int)result.Error);
            FillFileMetadata(destination, result.Value);
            return 0;
        }
        catch (Exception error) { return IoException(error); }
    }
    public static unsafe int blink_host_fstat(int fd, blink_host_stat_record* destination)
    {
        try
        {
            if (io == null) return IoError(19);
            if (destination == null) return IoError(14);
            var result = io.FStat(fd);
            if (!result.Succeeded) return IoError((int)result.Error);
            FillFileMetadata(destination, result.Value);
            return 0;
        }
        catch (Exception error) { return IoException(error); }
    }
    public static unsafe int blink_host_fstatat(int fd, byte* path, blink_host_stat_record* destination, int flags)
    {
        try
        {
            if (io == null) return IoError(19);
            if (path == null || destination == null) return IoError(14);
            if ((flags & ~256) != 0) return IoError(95); // AT_SYMLINK_NOFOLLOW only.
            if (path[0] == 0) return IoError(2);
            return FileMetadataPath(path, destination, fd);
        }
        catch (Exception error) { return IoException(error); }
    }
    private static unsafe void FillFileMetadata(blink_host_stat_record* destination, VirtualFileStat value)
    {
        // Fill a local value first: failures never partially overwrite C storage.
        blink_host_stat_record record = default;
        record.st_dev = 1; // One private virtual device, never a host dev_t.
        record.st_ino = value.Inode;
        record.st_nlink = value.Links;
        record.st_mode = value.Mode;
        record.st_size = value.Length;
        record.st_blksize = 4096;
        record.st_blocks = (value.Length + 511) / 512;
        SetFileTime(&record.st_atim, value.AccessTicks);
        SetFileTime(&record.st_mtim, value.ModifyTicks);
        SetFileTime(&record.st_ctim, value.ChangeTicks);
        *destination = record;
    }
    private static unsafe void SetFileTime(blink_host_stat_time* value, long ticks)
    {
        long unixTicks = ticks - DateTime.UnixEpoch.Ticks;
        value->tv_sec = unixTicks / TimeSpan.TicksPerSecond;
        value->tv_nsec = unixTicks % TimeSpan.TicksPerSecond * 100;
    }
}
