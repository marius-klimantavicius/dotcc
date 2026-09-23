using System.Text;

namespace Managed.Emulation.Host;

/// <summary>Frozen-during-execution mount namespace over machine-owned stores.</summary>
public sealed class MountedFileSystem : IGuestFileSystem
{
    private sealed record MountPoint(string Path, IGuestFileSystem Store, bool ReadOnly, bool Owned, bool PrivateStorage);
    private sealed class OpenState(bool append) { internal bool Append = append; }
    private sealed class Handle(MountPoint mount, int descriptor, string path, OpenState state)
    {
        internal readonly MountPoint Mount = mount;
        internal readonly int Descriptor = descriptor;
        internal string Path = path;
        internal readonly OpenState State = state;
    }
    private readonly object gate = new();
    private readonly List<MountPoint> mounts = new();
    private readonly Dictionary<int, Handle> handles = new();
    private readonly Dictionary<(MountPoint, ulong), ulong> inodes = new();
    private ulong nextInode = 1;
    private bool leased, disposed;
    private readonly long privateWritableLimit;
    private readonly int privateNodeLimit;
    public MountedFileSystem(IGuestFileSystem root, bool ownsRoot = false, long privateWritableLimit = long.MaxValue, int privateNodeLimit = int.MaxValue)
    {
        ArgumentNullException.ThrowIfNull(root);
        if (privateWritableLimit < 0 || root.WritableBytes > privateWritableLimit) throw new ArgumentOutOfRangeException(nameof(privateWritableLimit));
        if (privateNodeLimit < 1 || root.NodeCount > privateNodeLimit) throw new ArgumentOutOfRangeException(nameof(privateNodeLimit));
        this.privateWritableLimit = privateWritableLimit;
        this.privateNodeLimit = privateNodeLimit;
        mounts.Add(new("/", root, false, ownsRoot, true));
    }
    public long WritableBytes { get { lock (gate) return mounts.Select(m => m.Store).Distinct().Sum(s => s.WritableBytes); } }
    public int OpenDescriptors { get { lock (gate) return handles.Count; } }
    public int NodeCount { get { lock (gate) return mounts.Select(m => m.Store).Distinct().Sum(s => s.NodeCount); } }
    public long PathBytes { get { lock (gate) return mounts.Select(m => m.Store).Distinct().Sum(s => s.PathBytes); } }
    public void Mount(string guestPath, IGuestFileSystem source, bool readOnly = false,
        bool ownsSource = false, bool replaceExistingDirectory = false, bool privateStorage = false)
    {
        ArgumentNullException.ThrowIfNull(source);
        lock (gate)
        {
            Configure();
            if (!guestPath.StartsWith('/')) throw new ArgumentException("Mount path must be absolute.", nameof(guestPath));
            var path = Resolve(guestPath, "/");
            if (!path.Succeeded) throw new ArgumentException("Invalid mount path: " + path.Error, nameof(guestPath));
            if (mounts.Any(m => m.Path == path.Value)) throw new ArgumentException("Duplicate mount path.", nameof(guestPath));
            var old = Stat(path.Value);
            if (old.Succeeded && (!old.Value.Directory || !replaceExistingDirectory))
                throw new ArgumentException("Mounting over an existing directory requires explicit replacement.", nameof(guestPath));
            if (!old.Succeeded && old.Error != GuestError.NoEntry) throw new ArgumentException("Mount destination lookup failed.", nameof(guestPath));
            var root = source.Stat("/");
            if (!root.Succeeded || !root.Value.Directory) throw new ArgumentException("Mount source must have a directory root.", nameof(source));
            if (privateStorage && !mounts.Any(m => m.PrivateStorage && ReferenceEquals(m.Store, source)) && source.WritableBytes > PrivateRemaining())
                throw new ArgumentException("Aggregate private storage limit exceeded.", nameof(source));
            if (privateStorage && !mounts.Any(m => m.PrivateStorage && ReferenceEquals(m.Store, source)) && source.NodeCount > PrivateNodesRemaining())
                throw new ArgumentException("Aggregate private node limit exceeded.", nameof(source));
            mounts.Add(new(path.Value, source, readOnly, ownsSource, privateStorage));
        }
    }
    public void Unmount(string guestPath)
    {
        lock (gate)
        {
            Configure();
            var path = Resolve(guestPath, "/");
            var mount = path.Succeeded ? mounts.SingleOrDefault(m => m.Path == path.Value && m.Path != "/") : null;
            if (mount == null) throw new ArgumentException("Mount does not exist.", nameof(guestPath));
            if (mounts.Any(m => m != mount && Within(m.Path, mount.Path))) throw new InvalidOperationException("Unmount children first.");
            mounts.Remove(mount);
            if (mount.Owned && !mounts.Any(m => ReferenceEquals(m.Store, mount.Store))) mount.Store.Dispose();
        }
    }
    public GuestFileSystemSession AcquireSession()
    {
        lock (gate) { Configure(); leased = true; return new(this, Release); }
    }
    private void Configure()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (leased || handles.Count != 0) throw new InvalidOperationException("Filesystem configuration is frozen while in use.");
    }
    private void Release()
    {
        lock (gate)
        {
            foreach (int fd in handles.Keys.ToArray()) Close(fd);
            leased = false;
        }
    }
    private static bool Within(string path, string root) => root == "/" || path == root || path.StartsWith(root + "/", StringComparison.Ordinal);
    private static string Parent(string path) => path[..Math.Max(1, path.LastIndexOf('/'))];
    private MountPoint Select(string path) => mounts.Where(m => Within(path, m.Path)).MaxBy(m => m.Path.Length)!;
    private static string Local(MountPoint mount, string path) => mount.Path == "/" ? path : path == mount.Path ? "/" : path[mount.Path.Length..];
    private static HostResult<T> Fail<T>(GuestError error) => HostResult<T>.Failure(error);
    private static HostResult<T> Ok<T>(T value) => HostResult<T>.Success(value);
    private long PrivateRemaining() => Math.Max(0, privateWritableLimit - mounts.Where(m => m.PrivateStorage).Select(m => m.Store).Distinct().Sum(s => s.WritableBytes));
    private int PrivateNodesRemaining() => Math.Max(0, privateNodeLimit - mounts.Where(m => m.PrivateStorage).Select(m => m.Store).Distinct().Sum(s => s.NodeCount));
    private HostResult<string> Resolve(string path, string cwd)
    {
        if (disposed) return Fail<string>(GuestError.BadDescriptor);
        if (string.IsNullOrEmpty(path)) return Fail<string>(GuestError.NoEntry);
        if (path.Contains('\0') || !cwd.StartsWith('/')) return Fail<string>(GuestError.Invalid);
        if (Encoding.UTF8.GetByteCount(path) > 4096) return Fail<string>(GuestError.NameTooLong);
        string current = path.StartsWith('/') ? "/" : cwd;
        foreach (string part in path.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            var mount = Select(current);
            var directory = mount.Store.Stat(Local(mount, current));
            if (!directory.Succeeded) return Fail<string>(directory.Error);
            if (!directory.Value.Directory) return Fail<string>(GuestError.NotDirectory);
            if (part == ".") continue;
            if (part == "..")
            {
                if (current == "/") return Fail<string>(GuestError.Access);
                current = Parent(current);
            }
            else current = (current == "/" ? "" : current) + "/" + part;
        }
        if (Encoding.UTF8.GetByteCount(current) > 4096) return Fail<string>(GuestError.NameTooLong);
        if (path.EndsWith('/'))
        {
            var m = Select(current); var d = m.Store.Stat(Local(m, current));
            if (!d.Succeeded) return Fail<string>(d.Error);
            if (!d.Value.Directory) return Fail<string>(GuestError.NotDirectory);
        }
        return Ok(current);
    }
    private HostResult<T> PathCall<T>(string path, string cwd, bool write, Func<MountPoint, string, HostResult<T>> call)
    {
        lock (gate)
        {
            var p = Resolve(path, cwd); if (!p.Succeeded) return Fail<T>(p.Error);
            var m = Select(p.Value);
            return write && m.ReadOnly ? Fail<T>(GuestError.ReadOnly) : call(m, Local(m, p.Value));
        }
    }
    private HostResult<T> FdCall<T>(int fd, bool write, Func<Handle, HostResult<T>> call)
    {
        lock (gate) return disposed || !handles.TryGetValue(fd, out var h) ? Fail<T>(GuestError.BadDescriptor)
            : write && h.Mount.ReadOnly ? Fail<T>(GuestError.ReadOnly) : call(h);
    }
    private VirtualFileStat Metadata(MountPoint mount, VirtualFileStat stat)
    {
        if (!inodes.TryGetValue((mount, stat.Inode), out ulong inode)) inodes.Add((mount, stat.Inode), inode = nextInode++);
        return stat with { Inode = inode, Immutable = stat.Immutable || mount.ReadOnly };
    }
    public HostResult<int> Open(string path, FileAccessMode access, bool create = false, bool exclusive = false,
        bool truncate = false, bool append = false, string cwd = "/", bool allowDirectory = false,
        bool requireDirectory = false, uint creationMode = 0x180)
    {
        lock (gate)
        {
            var p = Resolve(path, cwd); if (!p.Succeeded) return Fail<int>(p.Error);
            var m = Select(p.Value);
            if (m.ReadOnly && (create || truncate || (access & FileAccessMode.Write) != 0)) return Fail<int>(GuestError.ReadOnly);
            if (create && m.PrivateStorage && PrivateNodesRemaining() == 0)
            {
                var existing = m.Store.Stat(Local(m, p.Value));
                if (!existing.Succeeded) return Fail<int>(existing.Error == GuestError.NoEntry ? GuestError.NoSpace : existing.Error);
            }
            var result = m.Store.Open(Local(m, p.Value), access, create, exclusive, truncate, append, "/", allowDirectory, requireDirectory, creationMode);
            if (!result.Succeeded) return result;
            int fd = 3; while (handles.ContainsKey(fd)) ++fd;
            handles.Add(fd, new(m, result.Value, p.Value, new(append))); return Ok(fd);
        }
    }
    public HostResult<int> Read(int descriptor, Span<byte> destination)
    {
        lock (gate) return disposed || !handles.TryGetValue(descriptor, out var h) ? Fail<int>(GuestError.BadDescriptor) : h.Mount.Store.Read(h.Descriptor, destination);
    }
    public HostResult<int> ReadAt(int descriptor, Span<byte> destination, long offset)
    {
        lock (gate) return disposed || !handles.TryGetValue(descriptor, out var h) ? Fail<int>(GuestError.BadDescriptor) : h.Mount.Store.ReadAt(h.Descriptor, destination, offset);
    }
    public HostResult<int> Write(int descriptor, ReadOnlySpan<byte> source) => WriteCore(descriptor, source, null);
    public HostResult<int> WriteAt(int descriptor, ReadOnlySpan<byte> source, long offset) => WriteCore(descriptor, source, offset);
    private HostResult<int> WriteCore(int fd, ReadOnlySpan<byte> bytes, long? offset)
    {
        lock (gate)
        {
            if (disposed || !handles.TryGetValue(fd, out var h)) return Fail<int>(GuestError.BadDescriptor);
            if (h.Mount.ReadOnly) return Fail<int>(GuestError.ReadOnly);
            if (offset < 0) return Fail<int>(GuestError.Invalid);
            if (h.Mount.PrivateStorage && !bytes.IsEmpty)
            {
                var stat = h.Mount.Store.FStat(h.Descriptor); if (!stat.Succeeded) return Fail<int>(stat.Error);
                var position = h.State.Append ? Ok(stat.Value.Length) : offset is long value ? Ok(value) : h.Mount.Store.Seek(h.Descriptor, 0, SeekOrigin.Current);
                if (!position.Succeeded) return Fail<int>(position.Error);
                long remaining = PrivateRemaining();
                long available = position.Value <= stat.Value.Length
                    ? Math.Min(long.MaxValue - (stat.Value.Length - position.Value), remaining) + stat.Value.Length - position.Value
                    : remaining - (position.Value - stat.Value.Length);
                if (available <= 0) return Fail<int>(GuestError.NoSpace);
                bytes = bytes[..(int)Math.Min(bytes.Length, available)];
            }
            return offset is long at ? h.Mount.Store.WriteAt(h.Descriptor, bytes, at) : h.Mount.Store.Write(h.Descriptor, bytes);
        }
    }
    public HostResult<int> Close(int descriptor)
    {
        lock (gate) return handles.Remove(descriptor, out var h) ? h.Mount.Store.Close(h.Descriptor) : Fail<int>(GuestError.BadDescriptor);
    }
    public HostResult<int> Duplicate(int descriptor) => FdCall(descriptor, false, h =>
    {
        var d = h.Mount.Store.Duplicate(h.Descriptor); if (!d.Succeeded) return d;
        int fd = 3; while (handles.ContainsKey(fd)) ++fd;
        handles.Add(fd, new(h.Mount, d.Value, h.Path, h.State)); return Ok(fd);
    });
    public HostResult<long> ReadAtLength(int descriptor) => FdCall(descriptor, false, h => h.Mount.Store.ReadAtLength(h.Descriptor));
    public HostResult<long> Seek(int descriptor, long offset, SeekOrigin origin) => FdCall(descriptor, false, h => h.Mount.Store.Seek(h.Descriptor, offset, origin));
    public HostResult<int> SetAppend(int descriptor, bool append) => FdCall(descriptor, false, h =>
    { var r = h.Mount.Store.SetAppend(h.Descriptor, append); if (r.Succeeded) h.State.Append = append; return r; });
    public HostResult<int> Synchronize(int descriptor) => FdCall(descriptor, false, h => h.Mount.Store.Synchronize(h.Descriptor));
    public HostResult<int> AdvisoryLock(int descriptor, int operation) => FdCall(descriptor, false, h => h.Mount.Store.AdvisoryLock(h.Descriptor, operation));
    public HostResult<int> Truncate(int descriptor, long length) => FdCall(descriptor, true, h =>
    { var s = h.Mount.Store.FStat(h.Descriptor); if (!s.Succeeded) return Fail<int>(s.Error); return h.Mount.PrivateStorage && length > s.Value.Length && length - s.Value.Length > PrivateRemaining() ? Fail<int>(GuestError.NoSpace) : h.Mount.Store.Truncate(h.Descriptor, length); });
    public HostResult<int> Truncate(string path, long length, string cwd = "/") => PathCall(path, cwd, true, (m, p) =>
    { var s = m.Store.Stat(p); if (!s.Succeeded) return Fail<int>(s.Error); return m.PrivateStorage && length > s.Value.Length && length - s.Value.Length > PrivateRemaining() ? Fail<int>(GuestError.NoSpace) : m.Store.Truncate(p, length); });
    public HostResult<int> ChangeMode(int descriptor, uint mode) => FdCall(descriptor, true, h => h.Mount.Store.ChangeMode(h.Descriptor, mode));
    public HostResult<int> ChangeMode(string path, uint mode, string cwd = "/") => PathCall(path, cwd, true, (m, p) => m.Store.ChangeMode(p, mode));
    public HostResult<int> ChangeOwner(int descriptor, uint user, uint group) => FdCall(descriptor, true, h => h.Mount.Store.ChangeOwner(h.Descriptor, user, group));
    public HostResult<int> ChangeOwner(string path, uint user, uint group, string cwd = "/") => PathCall(path, cwd, true, (m, p) => m.Store.ChangeOwner(p, user, group));
    public HostResult<int> SetTimes(int descriptor, VirtualFileTimeUpdate access, VirtualFileTimeUpdate modify) => FdCall(descriptor, true, h => h.Mount.Store.SetTimes(h.Descriptor, access, modify));
    public HostResult<int> SetTimes(string path, VirtualFileTimeUpdate access, VirtualFileTimeUpdate modify, string cwd = "/") => PathCall(path, cwd, true, (m, p) => m.Store.SetTimes(p, access, modify));
    public HostResult<VirtualFileStat> Stat(string path, string cwd = "/") => PathCall(path, cwd, false, (m, p) =>
    { var s = m.Store.Stat(p); return s.Succeeded ? Ok(Metadata(m, s.Value)) : s; });
    public HostResult<VirtualFileStat> FStat(int descriptor) => FdCall(descriptor, false, h =>
    { var s = h.Mount.Store.FStat(h.Descriptor); return s.Succeeded ? Ok(Metadata(h.Mount, s.Value)) : s; });
    public HostResult<VirtualFileSystemCapacity> Capacity(string path, string cwd = "/") => PathCall(path, cwd, false, (m, p) => m.Store.Capacity(p));
    public HostResult<VirtualFileSystemCapacity> Capacity(int descriptor) => FdCall(descriptor, false, h => h.Mount.Store.Capacity(h.Descriptor));
    public HostResult<string> CanonicalPath(string path, string cwd = "/", bool requireDirectory = false)
    {
        lock (gate)
        {
            var p = Resolve(path, cwd); if (!p.Succeeded) return p;
            var s = Stat(p.Value); if (!s.Succeeded) return Fail<string>(s.Error);
            return requireDirectory && !s.Value.Directory ? Fail<string>(GuestError.NotDirectory) : p;
        }
    }
    public HostResult<string> DirectoryPath(int descriptor) => FdCall(descriptor, false, h =>
    { var p = h.Mount.Store.DirectoryPath(h.Descriptor); return p.Succeeded ? Ok(h.Path) : Fail<string>(p.Error); });
    public HostResult<VirtualDirectoryEntry[]> DirectorySnapshot(int descriptor, int entryLimit, int nameBytesLimit) => FdCall(descriptor, false, h =>
    {
        var rows = h.Mount.Store.DirectorySnapshot(h.Descriptor, entryLimit, nameBytesLimit);
        if (!rows.Succeeded) return rows;
        var result = rows.Value.ToDictionary(e => e.Name, StringComparer.Ordinal);
        foreach (var mount in mounts.Where(m => m.Path != "/" && Parent(m.Path) == h.Path))
        {
            var s = Stat(mount.Path); if (!s.Succeeded) return Fail<VirtualDirectoryEntry[]>(s.Error);
            string name = mount.Path[(mount.Path.LastIndexOf('/') + 1)..]; result[name] = new(name, s.Value.Inode, 4);
        }
        foreach (string name in result.Keys.ToArray())
        {
            string path = name == "." ? h.Path : name == ".." ? Parent(h.Path) : (h.Path == "/" ? "" : h.Path) + "/" + name;
            var s = Stat(path); if (!s.Succeeded) return Fail<VirtualDirectoryEntry[]>(s.Error);
            result[name] = result[name] with { Inode = s.Value.Inode };
        }
        if (result.Count > entryLimit || result.Keys.Sum(n => Encoding.UTF8.GetByteCount(n) + 1) > nameBytesLimit) return Fail<VirtualDirectoryEntry[]>(GuestError.NoMemory);
        return Ok(result.Values.OrderBy(e => e.Name == "." ? 0 : e.Name == ".." ? 1 : 2).ThenBy(e => e.Name, StringComparer.Ordinal).ToArray());
    });
    public HostResult<int> MakeDirectory(string path, uint mode, string cwd = "/") => PathCall(path.TrimEnd('/'), cwd, true, (m, p) =>
    {
        if (m.PrivateStorage && PrivateNodesRemaining() == 0)
        { var existing = m.Store.Stat(p); return Fail<int>(existing.Succeeded ? GuestError.Exists : existing.Error == GuestError.NoEntry ? GuestError.NoSpace : existing.Error); }
        return m.Store.MakeDirectory(p, mode);
    });
    public HostResult<int> RejectSpecialNode(string path, string cwd = "/") => PathCall(path, cwd, true, (m, p) => m.Store.RejectSpecialNode(p));
    private bool ProtectsMount(string path) => mounts.Any(m => Within(m.Path, path));
    public HostResult<int> Unlink(string path, bool removeDirectory = false, string cwd = "/", string protectedCwd = "/")
    {
        lock (gate)
        {
            var p = Resolve(path, cwd); if (!p.Succeeded) return Fail<int>(p.Error);
            if (ProtectsMount(p.Value) || p.Value == protectedCwd) return Fail<int>(GuestError.Busy);
            return PathCall(p.Value, "/", true, (m, local) => m.Store.Unlink(local, removeDirectory));
        }
    }
    public HostResult<string> Rename(string oldPath, string newPath, string oldCwd = "/", string newCwd = "/", string protectedCwd = "/")
    {
        lock (gate)
        {
            var a = Resolve(oldPath, oldCwd); var b = Resolve(newPath, newCwd);
            if (!a.Succeeded || !b.Succeeded) return Fail<string>(!a.Succeeded ? a.Error : b.Error);
            if (ProtectsMount(a.Value) || ProtectsMount(b.Value)) return Fail<string>(GuestError.Busy);
            var m = Select(a.Value); if (m != Select(b.Value)) return Fail<string>(GuestError.CrossDevice);
            if (m.ReadOnly) return Fail<string>(GuestError.ReadOnly);
            var renamed = m.Store.Rename(Local(m, a.Value), Local(m, b.Value), protectedCwd: Within(protectedCwd, m.Path) ? Local(m, protectedCwd) : "/");
            if (!renamed.Succeeded) return Fail<string>(renamed.Error);
            foreach (var h in handles.Values.Where(h => h.Mount == m && Within(h.Path, a.Value))) h.Path = b.Value + h.Path[a.Value.Length..];
            return Ok(Within(protectedCwd, a.Value) ? b.Value + protectedCwd[a.Value.Length..] : protectedCwd);
        }
    }
    public void Dispose()
    {
        lock (gate)
        {
            if (disposed) return;
            Release(); disposed = true;
            foreach (var store in mounts.Where(m => m.Owned).Select(m => m.Store).Distinct()) store.Dispose();
            mounts.Clear(); inodes.Clear();
        }
    }
}

public sealed class GuestFileSystemSession : IDisposable
{
    private Action? release;
    public IGuestFileSystem FileSystem { get; }
    internal GuestFileSystemSession(IGuestFileSystem fileSystem, Action release) { FileSystem = fileSystem; this.release = release; }
    public void Dispose() => Interlocked.Exchange(ref release, null)?.Invoke();
}
