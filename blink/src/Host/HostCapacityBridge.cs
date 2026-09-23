using global::System;
using global::System.Text;
using Managed.Emulation.Host;

namespace Managed.Emulation;

#if BLINK_FULL_CORE
public partial class BlinkCore
#else
public static partial class Blink
#endif
{
    public static unsafe int blink_host_statvfs(byte* path, blink_host_statvfs_record* destination)
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
            var result = io.FileSystemCapacity(name);
            if (!result.Succeeded) return IoError((int)result.Error);
            FillCapacity(destination, result.Value);
            return 0;
        }
        catch (Exception error) { return IoException(error); }
    }
    public static unsafe int blink_host_fstatvfs(int fd, blink_host_statvfs_record* destination)
    {
        try
        {
            if (io == null) return IoError(19);
            if (destination == null) return IoError(14);
            var result = io.FileSystemCapacity(fd);
            if (!result.Succeeded) return IoError((int)result.Error);
            FillCapacity(destination, result.Value);
            return 0;
        }
        catch (Exception error) { return IoException(error); }
    }
    private static unsafe void FillCapacity(blink_host_statvfs_record* destination, VirtualFileSystemCapacity value)
    {
        blink_host_statvfs_record record = default;
        record.f_bsize = 4096; // Preferred transfer size, not the quota unit.
        record.f_frsize = 1; // Exact byte accounting used by the private allocator.
        record.f_blocks = value.TotalBytes;
        record.f_bfree = record.f_bavail = value.FreeBytes;
        record.f_files = value.TotalNodes;
        record.f_ffree = record.f_favail = value.FreeNodes;
        record.f_fsid = 1; // One filesystem within this private namespace.
        record.f_flag = 2 | 4; // NOSUID | NODEV: neither facility exists here.
        record.f_namemax = 4095; // Resolver's maximum root-level component.
        *destination = record;
    }
}
