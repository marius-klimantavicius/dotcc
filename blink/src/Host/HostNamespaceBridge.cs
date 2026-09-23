using global::System;
using Managed.Emulation.Host;
namespace Managed.Emulation;
#if BLINK_FULL_CORE
public partial class BlinkCore
#else
public static partial class Blink
#endif
{
    // Keep this boundary independent of the optional allocated-path bridge.
    private static unsafe HostResult<string> NamespacePath(byte* path)
    {
        if (path == null) return HostResult<string>.Failure((GuestError)14);
        int length = 0; while (length < PathLimit && path[length] != 0) ++length;
        if (length == PathLimit) return HostResult<string>.Failure(GuestError.NameTooLong);
        try { return HostResult<string>.Success(PathEncoding.GetString(new ReadOnlySpan<byte>(path, length))); }
        catch (global::System.Text.DecoderFallbackException) { return HostResult<string>.Failure(GuestError.Invalid); }
    }
    public static unsafe int blink_host_mkdir(byte* path, uint mode) => blink_host_mkdirat(-100, path, mode);
    public static unsafe int blink_host_mkdirat(int fd, byte* path, uint mode)
    {
        try {
            if (io == null) return IoError(19);
            var name = NamespacePath(path);
            return name.Succeeded ? (int)IoResult(io.MakeDirectoryAt(fd, name.Value, mode)) : IoError((int)name.Error);
        } catch (Exception error) { return IoException(error); }
    }
    public static unsafe int blink_host_unlink(byte* path) => blink_host_unlinkat(-100, path, 0);
    public static unsafe int blink_host_rmdir(byte* path) => blink_host_unlinkat(-100, path, 512);
    public static unsafe int blink_host_unlinkat(int fd, byte* path, int flags)
    {
        try {
            if (io == null) return IoError(19);
            var name = NamespacePath(path);
            return name.Succeeded ? (int)IoResult(io.UnlinkAt(fd, name.Value, flags)) : IoError((int)name.Error);
        } catch (Exception error) { return IoException(error); }
    }
    public static unsafe int blink_host_rename(byte* oldPath, byte* newPath) => blink_host_renameat(-100, oldPath, -100, newPath);
    public static unsafe int blink_host_renameat(int oldFd, byte* oldPath, int newFd, byte* newPath)
    {
        try {
            if (io == null) return IoError(19);
            var source = NamespacePath(oldPath); if (!source.Succeeded) return IoError((int)source.Error);
            var destination = NamespacePath(newPath); if (!destination.Succeeded) return IoError((int)destination.Error);
            return (int)IoResult(io.RenameAt(oldFd, source.Value, newFd, destination.Value));
        } catch (Exception error) { return IoException(error); }
    }
}
