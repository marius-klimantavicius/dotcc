namespace Managed.Emulation.Host;
public sealed partial class InstanceIo
{
    public HostResult<int> WriteAt(int fd, ReadOnlySpan<byte> source, long offset)
    {
        lock (sync) return !Find(fd,out var description) ? Fail<int>(GuestError.BadDescriptor)
            : description.Kind == Kind.File ? files.WriteAt(description.Handle,source,offset) : Fail<int>(GuestError.IllegalSeek);
    }
    public HostResult<int> TruncateFile(int fd,long length)
    {
        lock (sync) return !Find(fd,out var description) ? Fail<int>(GuestError.BadDescriptor)
            : description.Kind == Kind.File ? files.Truncate(description.Handle,length) : Fail<int>(GuestError.Invalid);
    }
    public HostResult<int> TruncateFile(string path,long length)
    {
        lock (sync) return disposed ? Fail<int>(GuestError.BadDescriptor) : files.Truncate(path,length,currentDirectory);
    }
    public HostResult<int> SynchronizeFile(int fd)
    {
        lock (sync) return !Find(fd,out var description) ? Fail<int>(GuestError.BadDescriptor)
            : description.Kind == Kind.File ? files.Synchronize(description.Handle) : Fail<int>(GuestError.Invalid);
    }
}
