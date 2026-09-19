using global::System;
using global::System.Text;
namespace Managed.Emulation;
#if BLINK_FULL_CORE
public static partial class BlinkCore
#else
public static partial class Blink
#endif
{
    private static bool PermissionIdentity()
    {
        if (io == null || identity == null) { IoError(19); return false; }
        if (identity.UserId != 0 || identity.GroupId != 0) { IoError(1); return false; }
        return true;
    }
    private static unsafe bool PermissionPath(byte* path, out string name)
    {
        name = "";
        if (path == null) { IoError(14); return false; }
        int length = 0; while (length < PathLimit && path[length] != 0) ++length;
        if (length == PathLimit) { IoError(36); return false; }
        try { name = PathEncoding.GetString(new ReadOnlySpan<byte>(path, length)); return true; }
        catch (DecoderFallbackException) { IoError(22); return false; }
    }
    public static int blink_host_fchmod(int fd, uint mode)
    {
        try { return PermissionIdentity() ? (int)IoResult(io!.ChangeMode(fd, mode)) : -1; }
        catch (Exception error) { return IoException(error); }
    }
    public static unsafe int blink_host_fchmodat(int directory, byte* path, uint mode, int flags)
    {
        try {
            if (!PermissionIdentity() || !PermissionPath(path, out var name)) return -1;
            return (int)IoResult(io!.ChangeModeAt(directory, name, mode, flags));
        } catch (Exception error) { return IoException(error); }
    }
    public static unsafe int blink_host_chmod(byte* path, uint mode) => blink_host_fchmodat(-100, path, mode, 0);
    public static int blink_host_fchown(int fd, uint user, uint group)
    {
        try { return PermissionIdentity() ? (int)IoResult(io!.ChangeOwner(fd, user, group)) : -1; }
        catch (Exception error) { return IoException(error); }
    }
    public static unsafe int blink_host_fchownat(int directory, byte* path, uint user, uint group, int flags)
    {
        try {
            if (!PermissionIdentity() || !PermissionPath(path, out var name)) return -1;
            return (int)IoResult(io!.ChangeOwnerAt(directory, name, user, group, flags));
        } catch (Exception error) { return IoException(error); }
    }
    public static uint blink_host_umask(uint mask)
    {
        try {
            if (io == null) return unchecked((uint)IoError(19));
            var result = io.SetCreationMask(mask);
            return result.Succeeded ? result.Value : unchecked((uint)IoError((int)result.Error));
        } catch (Exception error) { return unchecked((uint)IoException(error)); }
    }
}
