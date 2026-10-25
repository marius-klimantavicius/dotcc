namespace Managed.Emulation.Host;
public sealed partial class InstanceIo
{
    public HostResult<int> WriteAt(int fd, ReadOnlySpan<byte> source, long offset)
    {
        lock (sync) return !Find(fd,out var description) ? Fail<int>(GuestError.BadDescriptor)
            : description.ProcessSnapshot ? Fail<int>(GuestError.BadDescriptor)
            : description.Kind == Kind.MemoryFile ? WriteMemoryFile(description.MemoryFile!, source,
                offset < 0 ? offset : (description.StatusFlags & 1024) != 0 ? description.MemoryFile!.Length : offset)
            : description.Kind == Kind.File ? WriteRegularFile(description,source,offset) : Fail<int>(GuestError.IllegalSeek);
    }
    public HostResult<int> TruncateFile(int fd,long length)
    {
        lock (sync)
        {
            if (!Find(fd, out var description)) return Fail<int>(GuestError.BadDescriptor);
            if (description.ProcessSnapshot) return Fail<int>(GuestError.Invalid);
            if (description.Kind == Kind.MemoryFile) return TruncateMemoryFile(description.MemoryFile!, length);
            if (description.Kind != Kind.File) return Fail<int>(GuestError.Invalid);
            MemoryFile? cached = null;
            if (regularSharedFiles.Count != 0)
            {
                var stat = files.FStat(description.Handle);
                if (!stat.Succeeded) return Fail<int>(stat.Error);
                cached = RegularSharedFile(stat.Value);
            }
            var result = files.Truncate(description.Handle, length);
            if (result.Succeeded && cached != null) TruncateMemoryFile(cached, length);
            return result;
        }
    }
    public HostResult<int> TruncateFile(string path,long length)
    {
        lock (sync)
        {
            if (disposed) return Fail<int>(GuestError.BadDescriptor);
            MemoryFile? cached = null;
            if (regularSharedFiles.Count != 0)
            {
                var stat = files.Stat(path, currentDirectory);
                if (!stat.Succeeded) return Fail<int>(stat.Error);
                cached = RegularSharedFile(stat.Value);
            }
            var result = files.Truncate(path, length, currentDirectory);
            if (result.Succeeded && cached != null) TruncateMemoryFile(cached, length);
            return result;
        }
    }
    public HostResult<int> SynchronizeFile(int fd)
    {
        lock (sync) return !Find(fd,out var description) ? Fail<int>(GuestError.BadDescriptor)
            : description.Kind == Kind.MemoryFile ? HostResult<int>.Success(0) // All bytes are already in this private store.
            : description.Kind == Kind.File ? files.Synchronize(description.Handle) : Fail<int>(GuestError.Invalid);
    }
}
