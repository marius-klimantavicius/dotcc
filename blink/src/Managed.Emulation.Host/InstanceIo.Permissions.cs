namespace Managed.Emulation.Host;

public sealed partial class InstanceIo
{
    private uint creationMask = 0x12; // Private initial umask 0022.
    // Caller holds the descriptor/namespace lock. Creation APIs validate mode
    // support before applying this mask, so unsupported bits are not hidden.
    internal uint ApplyCreationMask(uint mode) => mode & 0x1ffu & ~creationMask;
    public HostResult<uint> SetCreationMask(uint value)
    {
        lock (sync)
        {
            if (disposed) return Fail<uint>(GuestError.BadDescriptor);
            uint previous = creationMask; creationMask = value & 0x1ffu;
            return HostResult<uint>.Success(previous);
        }
    }
    public HostResult<int> ChangeMode(int fd, uint mode)
    {
        lock (sync) return !Find(fd, out var description) ? Fail<int>(GuestError.BadDescriptor)
            : description.Kind == Kind.File ? files.ChangeMode(description.Handle, mode) : Fail<int>(GuestError.Unsupported);
    }
    public HostResult<int> ChangeModeAt(int directory, string path, uint mode, int flags = 0)
    {
        lock (sync)
        {
            if ((flags & ~256) != 0) return Fail<int>(GuestError.Unsupported);
            var basePath = ResolveDirectory(directory, path);
            return basePath.Succeeded ? files.ChangeMode(path, mode, basePath.Value) : Fail<int>(basePath.Error);
        }
    }
    public HostResult<int> ChangeOwner(int fd, uint user, uint group)
    {
        lock (sync) return !Find(fd, out var description) ? Fail<int>(GuestError.BadDescriptor)
            : description.Kind == Kind.File ? files.ChangeOwner(description.Handle, user, group) : Fail<int>(GuestError.Unsupported);
    }
    public HostResult<int> ChangeOwnerAt(int directory, string path, uint user, uint group, int flags = 0)
    {
        lock (sync)
        {
            if ((flags & ~(256 | 4096)) != 0) return Fail<int>(GuestError.Unsupported);
            if ((flags & 4096) != 0 && path.Length == 0)
                return directory == -100 ? files.ChangeOwner(".", user, group, currentDirectory) : ChangeOwner(directory, user, group);
            var basePath = ResolveDirectory(directory, path);
            return basePath.Succeeded ? files.ChangeOwner(path, user, group, basePath.Value) : Fail<int>(basePath.Error);
        }
    }
}
