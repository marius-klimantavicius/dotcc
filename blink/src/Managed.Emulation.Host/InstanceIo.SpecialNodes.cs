namespace Managed.Emulation.Host;

public sealed partial class InstanceIo
{
    public HostResult<int> ReadLinkAt(int directoryFd, string path)
    {
        lock (sync)
        {
            var node = StatAt(directoryFd, path);
            // A real lookup distinguishes absence from an ordinary non-link.
            return Fail<int>(node.Succeeded ? GuestError.Invalid : node.Error);
        }
    }
    public HostResult<int> RejectSpecialNodeAt(int directoryFd, string path)
    {
        lock (sync)
        {
            var directory = ResolveDirectory(directoryFd, path);
            return directory.Succeeded ? files.RejectSpecialNode(path, directory.Value)
                : Fail<int>(directory.Error);
        }
    }
    public HostResult<int> RejectHardLinkAt(int sourceFd, string source, int targetFd, string target, int flags)
    {
        lock (sync)
        {
            if (disposed) return Fail<int>(GuestError.BadDescriptor);
            if ((flags & ~1024) != 0) return Fail<int>(GuestError.Unsupported);
            var node = StatAt(sourceFd, source);
            if (!node.Succeeded) return Fail<int>(node.Error);
            if (node.Value.Directory) return Fail<int>((GuestError)1);
            if (node.Value.Immutable) return Fail<int>(GuestError.ReadOnly);
            return RejectSpecialNodeAt(targetFd, target);
        }
    }
}
