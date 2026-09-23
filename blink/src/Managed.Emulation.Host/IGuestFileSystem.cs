namespace Managed.Emulation.Host;

/// <summary>One guest filesystem session; handles and paths never name ambient host resources.</summary>
public interface IGuestFileSystem : IDisposable
{
    long WritableBytes { get; }
    int OpenDescriptors { get; }
    int NodeCount { get; }
    long PathBytes { get; }
    HostResult<int> SetTimes(int descriptor, VirtualFileTimeUpdate access, VirtualFileTimeUpdate modify);
    HostResult<int> SetTimes(string path, VirtualFileTimeUpdate access, VirtualFileTimeUpdate modify, string cwd = "/");
    HostResult<int> AdvisoryLock(int descriptor, int operation);
    HostResult<int> MakeDirectory(string path, uint mode, string cwd = "/");
    HostResult<int> Unlink(string path, bool removeDirectory = false, string cwd = "/", string protectedCwd = "/");
    HostResult<string> Rename(string oldPath, string newPath, string oldCwd = "/", string newCwd = "/", string protectedCwd = "/");
    HostResult<int> ChangeMode(int descriptor, uint mode);
    HostResult<int> ChangeMode(string path, uint mode, string cwd = "/");
    HostResult<int> ChangeOwner(int descriptor, uint user, uint group);
    HostResult<int> ChangeOwner(string path, uint user, uint group, string cwd = "/");
    HostResult<int> RejectSpecialNode(string path, string cwd = "/");
    HostResult<VirtualFileSystemCapacity> Capacity(string path, string cwd = "/");
    HostResult<VirtualFileSystemCapacity> Capacity(int descriptor);
    HostResult<int> Open(string path, FileAccessMode access, bool create = false,
        bool exclusive = false, bool truncate = false, bool append = false, string cwd = "/",
        bool allowDirectory = false, bool requireDirectory = false, uint creationMode = 0x180);
    HostResult<int> Read(int descriptor, Span<byte> destination);
    HostResult<int> ReadAt(int descriptor, Span<byte> destination, long offset);
    HostResult<long> ReadAtLength(int descriptor);
    HostResult<int> Write(int descriptor, ReadOnlySpan<byte> source);
    HostResult<int> WriteAt(int descriptor, ReadOnlySpan<byte> source, long offset);
    HostResult<int> Truncate(int descriptor, long length);
    HostResult<int> Truncate(string path, long length, string cwd = "/");
    HostResult<int> Synchronize(int descriptor);
    HostResult<long> Seek(int descriptor, long offset, SeekOrigin origin);
    HostResult<int> Duplicate(int descriptor);
    HostResult<int> Close(int descriptor);
    HostResult<VirtualFileStat> Stat(string path, string cwd = "/");
    HostResult<VirtualFileStat> FStat(int descriptor);
    HostResult<string> CanonicalPath(string path, string cwd = "/", bool requireDirectory = false);
    HostResult<string> DirectoryPath(int descriptor);
    HostResult<VirtualDirectoryEntry[]> DirectorySnapshot(int descriptor, int entryLimit, int nameBytesLimit);
    HostResult<int> SetAppend(int descriptor, bool append);
}
