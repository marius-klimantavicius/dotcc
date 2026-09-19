namespace Managed.Emulation.Host;

public sealed partial class InstanceIo
{
    public HostResult<int> MakeDirectoryAt(int directoryFd, string path, uint mode)
    {
        lock (sync)
        {
            if ((mode & ~0x1ffU) != 0) return Fail<int>(GuestError.Unsupported);
            var directory = ResolveDirectory(directoryFd, path);
            return directory.Succeeded ? files.MakeDirectory(path, ApplyCreationMask(mode), directory.Value)
                : Fail<int>(directory.Error);
        }
    }

    public HostResult<int> UnlinkAt(int directoryFd, string path, int flags = 0)
    {
        lock (sync)
        {
            if ((flags & ~512) != 0) return Fail<int>(GuestError.Unsupported);
            var directory = ResolveDirectory(directoryFd, path);
            return directory.Succeeded ? files.Unlink(path, (flags & 512) != 0, directory.Value, currentDirectory)
                : Fail<int>(directory.Error);
        }
    }

    public HostResult<int> RenameAt(int oldDirectoryFd, string oldPath, int newDirectoryFd, string newPath)
    {
        lock (sync)
        {
            var oldDirectory = ResolveDirectory(oldDirectoryFd, oldPath);
            if (!oldDirectory.Succeeded) return Fail<int>(oldDirectory.Error);
            var newDirectory = ResolveDirectory(newDirectoryFd, newPath);
            if (!newDirectory.Succeeded) return Fail<int>(newDirectory.Error);
            var result = files.Rename(oldPath, newPath, oldDirectory.Value, newDirectory.Value, currentDirectory);
            if (!result.Succeeded) return Fail<int>(result.Error);
            currentDirectory = result.Value;
            return HostResult<int>.Success(0);
        }
    }
}
