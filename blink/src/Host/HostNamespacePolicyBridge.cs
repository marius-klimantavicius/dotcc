using global::System;
namespace Managed.Emulation;
#if BLINK_FULL_CORE
public partial class BlinkCore
#else
public static partial class Blink
#endif
{
    public static unsafe long blink_host_readlinkat(int fd, byte* path, byte* output, ulong capacity)
    {
        try {
            if (io == null) return IoError(19);
            if (capacity == 0 || capacity > long.MaxValue) return IoError(22);
            if (output == null) return IoError(14);
            var name = NamespacePath(path);
            return name.Succeeded ? IoResult(io.ReadLinkAt(fd, name.Value)) : IoError((int)name.Error);
        } catch(Exception error) { return IoException(error); }
    }
    public static unsafe long blink_host_readlink(byte* path, byte* output, ulong capacity)
        => blink_host_readlinkat(-100, path, output, capacity);
    public static unsafe int blink_host_linkat(int oldFd, byte* oldPath, int newFd, byte* newPath, int flags)
    {
        try {
            if (io == null) return IoError(19);
            var source = NamespacePath(oldPath); if (!source.Succeeded) return IoError((int)source.Error);
            var target = NamespacePath(newPath); if (!target.Succeeded) return IoError((int)target.Error);
            return (int)IoResult(io.RejectHardLinkAt(oldFd, source.Value, newFd, target.Value, flags));
        } catch(Exception error) { return IoException(error); }
    }
    public static unsafe int blink_host_link(byte* oldPath, byte* newPath)
        => blink_host_linkat(-100, oldPath, -100, newPath, 0);
    public static unsafe int blink_host_symlinkat(byte* target, int fd, byte* path)
    {
        try {
            if (io == null) return IoError(19);
            var contents = NamespacePath(target); if (!contents.Succeeded) return IoError((int)contents.Error);
            if (contents.Value.Length == 0) return IoError(2);
            var name = NamespacePath(path);
            return name.Succeeded ? (int)IoResult(io.RejectSpecialNodeAt(fd, name.Value)) : IoError((int)name.Error);
        } catch(Exception error) { return IoException(error); }
    }
    public static unsafe int blink_host_symlink(byte* target, byte* path)
        => blink_host_symlinkat(target, -100, path);
    public static unsafe int blink_host_mkfifoat(int fd, byte* path, uint mode)
    {
        try {
            if (io == null) return IoError(19);
            if ((mode & ~511U) != 0) return IoError(95);
            var name = NamespacePath(path);
            return name.Succeeded ? (int)IoResult(io.RejectSpecialNodeAt(fd, name.Value)) : IoError((int)name.Error);
        } catch(Exception error) { return IoException(error); }
    }
    public static unsafe int blink_host_mkfifo(byte* path, uint mode)
        => blink_host_mkfifoat(-100, path, mode);
    public static unsafe int blink_host_socketpair(int domain, int type, int protocol, int* descriptors)
    {
        try {
            if (io == null) return IoError(19);
            var owner = io.DescriptorCapacity(); if (!owner.Succeeded) return IoError((int)owner.Error);
            // The selected network is IPv4 TCP. IPv4 has no socketpair service;
            // Unix-domain/other families are outside this profile entirely.
            return IoError(domain == 2 ? 95 : 97);
        } catch(Exception error) { return IoException(error); }
    }
}
