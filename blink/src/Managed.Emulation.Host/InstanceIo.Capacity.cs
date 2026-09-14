namespace Managed.Emulation.Host;

public sealed partial class InstanceIo
{
    public HostResult<VirtualFileSystemCapacity> FileSystemCapacity(string path)
    {
        lock (sync) return disposed ? Fail<VirtualFileSystemCapacity>(GuestError.BadDescriptor)
            : files.Capacity(path, currentDirectory);
    }
    public HostResult<VirtualFileSystemCapacity> FileSystemCapacity(int fd)
    {
        lock (sync)
        {
            if (!Find(fd, out var description)) return Fail<VirtualFileSystemCapacity>(GuestError.BadDescriptor);
            return description.Kind == Kind.File ? files.Capacity(description.Handle)
                : Fail<VirtualFileSystemCapacity>(GuestError.Unsupported);
        }
    }
}
