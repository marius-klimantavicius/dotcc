namespace Managed.Emulation.Host;

public sealed partial class VirtualFileSystem
{
    public HostResult<int> ChangeMode(int descriptor, uint mode)
    {
        lock (sync) return TryDescription(descriptor, out var description)
            ? ChangeMode(description.Node, mode) : Fail<int>(GuestError.BadDescriptor);
    }
    public HostResult<int> ChangeMode(string path, uint mode, string cwd = "/")
    {
        lock (sync)
        {
            var node = PermissionNode(path, cwd);
            return node.Succeeded ? ChangeMode(node.Value, mode) : Fail<int>(node.Error);
        }
    }
    private static HostResult<int> ChangeMode(Node node, uint mode)
    {
        if (node.Immutable) return Fail<int>(GuestError.ReadOnly);
        if ((mode & ~0x1ffu) != 0) return Fail<int>(GuestError.Unsupported);
        node.Mode = (node.Mode & 0xf000u) | mode;
        node.ChangeTime = VirtualFileTime.UtcNow;
        return HostResult<int>.Success(0);
    }
    public HostResult<int> ChangeOwner(int descriptor, uint user, uint group)
    {
        lock (sync) return TryDescription(descriptor, out var description)
            ? ChangeOwner(description.Node, user, group) : Fail<int>(GuestError.BadDescriptor);
    }
    public HostResult<int> ChangeOwner(string path, uint user, uint group, string cwd = "/")
    {
        lock (sync)
        {
            var node = PermissionNode(path, cwd);
            return node.Succeeded ? ChangeOwner(node.Value, user, group) : Fail<int>(node.Error);
        }
    }
    private static HostResult<int> ChangeOwner(Node node, uint user, uint group)
    {
        if (node.Immutable) return Fail<int>(GuestError.ReadOnly);
        // Every actual node belongs to the private UID/GID zero identity.
        // UINT_MAX is POSIX's unchanged-ID sentinel, not a new owner.
        if (user is not (0 or uint.MaxValue) || group is not (0 or uint.MaxValue))
            return Fail<int>((GuestError)1);
        node.ChangeTime = VirtualFileTime.UtcNow;
        return HostResult<int>.Success(0);
    }
    private HostResult<Node> PermissionNode(string path, string cwd)
    {
        if (disposed) return Fail<Node>(GuestError.BadDescriptor);
        var resolved = Resolve(path, cwd);
        if (!resolved.Succeeded) return Fail<Node>(resolved.Error);
        return files.TryGetValue(resolved.Value, out var file) ? HostResult<Node>.Success(file)
            : directoryNodes.TryGetValue(resolved.Value, out var directory) ? HostResult<Node>.Success(directory)
            : Fail<Node>(GuestError.NoEntry);
    }
}
