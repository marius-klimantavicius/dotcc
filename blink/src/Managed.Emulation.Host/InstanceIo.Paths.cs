namespace Managed.Emulation.Host;

public sealed partial class InstanceIo
{
    private string currentDirectory = "/";
    public HostResult<string> GetWorkingDirectory()
    {
        lock (sync) return disposed ? Fail<string>(GuestError.BadDescriptor) : HostResult<string>.Success(currentDirectory);
    }
    public HostResult<int> ChangeDirectory(string path)
    {
        lock (sync)
        {
            if (disposed) return Fail<int>(GuestError.BadDescriptor);
            var resolved = files.CanonicalPath(path, currentDirectory, true);
            if (!resolved.Succeeded) return Fail<int>(resolved.Error);
            currentDirectory = resolved.Value;
            return HostResult<int>.Success(0);
        }
    }
    public HostResult<int> ChangeDirectory(int descriptor)
    {
        lock (sync)
        {
            if (!Find(descriptor, out var description)) return Fail<int>(GuestError.BadDescriptor);
            if (description.Kind != Kind.File) return Fail<int>(GuestError.NotDirectory);
            var directory = files.DirectoryPath(description.Handle);
            if (!directory.Succeeded) return Fail<int>(directory.Error);
            currentDirectory = directory.Value;
            return HostResult<int>.Success(0);
        }
    }
    public HostResult<string> CanonicalPath(string path)
    {
        lock (sync) return disposed ? Fail<string>(GuestError.BadDescriptor) : files.CanonicalPath(path, currentDirectory);
    }
}
