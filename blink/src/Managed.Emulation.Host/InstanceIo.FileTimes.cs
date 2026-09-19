namespace Managed.Emulation.Host;

public sealed partial class InstanceIo
{
    public HostResult<int> SetFileTimes(int fd, VirtualFileTimeUpdate access, VirtualFileTimeUpdate modify)
    {
        lock (sync) return !Find(fd, out var description) ? Fail<int>(GuestError.BadDescriptor)
            : description.Kind == Kind.File ? files.SetTimes(description.Handle, access, modify)
            : Fail<int>(GuestError.Unsupported);
    }

    public HostResult<int> SetFileTimesAt(int directoryFd, string path,
        VirtualFileTimeUpdate access, VirtualFileTimeUpdate modify, int flags = 0)
    {
        lock (sync)
        {
            if (disposed) return Fail<int>(GuestError.BadDescriptor);
            // Linux utimensat does not resolve the path or inspect flags for
            // an all-OMIT request. futimens still validates its descriptor.
            if (access.Selection == FileTimeSelection.Omit && modify.Selection == FileTimeSelection.Omit)
                return HostResult<int>.Success(0);
            // The private namespace has no symlinks. NOFOLLOW is equivalent to
            // the ordinary lookup; other flag contracts are unsupported.
            if ((flags & ~256) != 0) return Fail<int>(GuestError.Unsupported);
            var directory = ResolveDirectory(directoryFd, path);
            return directory.Succeeded ? files.SetTimes(path, access, modify, directory.Value)
                : Fail<int>(directory.Error);
        }
    }
}
