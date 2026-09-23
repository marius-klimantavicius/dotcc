using System.Text;

namespace Managed.Emulation.Host;

public enum GuestError
{
    None = 0, NoEntry = 2, Io = 5, BadDescriptor = 9, Again = 11, NoMemory = 12, Access = 13, Busy = 16, Exists = 17,
    CrossDevice = 18, NotDirectory = 20, IsDirectory = 21, Invalid = 22, TooManyFiles = 24,
    NoSpace = 28, IllegalSeek = 29, ReadOnly = 30, BrokenPipe = 32, NameTooLong = 36, NotEmpty = 39,
    NotSocket = 88, Unsupported = 95, AddressInUse = 98, AddressUnavailable = 99,
    NetworkDown = 100, NetworkUnreachable = 101, NetworkReset = 102, ConnectionAborted = 103,
    ConnectionReset = 104, NoBufferSpace = 105, AlreadyConnected = 106, NotConnected = 107,
    TimedOut = 110, ConnectionRefused = 111, HostUnreachable = 113, AlreadyInProgress = 114, InProgress = 115, Canceled = 125
}

public readonly record struct HostResult<T>(T Value, GuestError Error)
{
    public bool Succeeded => Error == GuestError.None;
    public static HostResult<T> Success(T value) => new(value, GuestError.None);
    public static HostResult<T> Failure(GuestError error) => new(default!, error);
}

[Flags]
public enum FileAccessMode { Read = 1, Write = 2 }
public readonly record struct VirtualFileStat(long Length, bool Immutable, bool Directory,
    ulong Inode = 0, uint Mode = 0, long AccessTicks = 0, long ModifyTicks = 0, long ChangeTicks = 0, ulong Links = 1,
    int AccessSubtick = 0, int ModifySubtick = 0, int ChangeSubtick = 0);
public readonly record struct VirtualFileSystemCapacity(ulong TotalBytes, ulong FreeBytes, ulong TotalNodes, ulong FreeNodes);
public readonly record struct VirtualDirectoryEntry(string Name, ulong Inode, byte Type);

/// <summary>A private Linux-path namespace. It never consults the host filesystem.</summary>
public sealed partial class VirtualFileSystem : IGuestFileSystem
{
    private sealed class Node(byte[] bytes, bool immutable, ulong inode, uint mode)
    {
        internal byte[] Bytes = bytes;
        internal readonly bool Immutable = immutable;
        internal readonly ulong Inode = inode;
        internal uint Mode = mode;
        internal bool Linked = true;
        internal VirtualFileTime AccessTime = VirtualFileTime.UtcNow;
        internal VirtualFileTime ModifyTime = VirtualFileTime.UtcNow;
        internal VirtualFileTime ChangeTime = VirtualFileTime.UtcNow;
    }
    private sealed class Description(Node node, FileAccessMode access, bool append, string? directoryPath = null)
    {
        internal readonly Node Node = node;
        internal readonly FileAccessMode Access = access;
        internal bool Append = append;
        internal string? DirectoryPath = directoryPath;
        internal long Position;
    }
    private readonly object sync = new();
    private Dictionary<string, Node> files = new(StringComparer.Ordinal);
    private HashSet<string> directories = new(StringComparer.Ordinal) { "/" };
    private Dictionary<string, Node> directoryNodes = new(StringComparer.Ordinal);
    private readonly Dictionary<int, Description> descriptors = new();
    private HashSet<Node> detachedNodes = new();
    private readonly long writableLimit;
    private readonly long imageBytes;
    private readonly int descriptorLimit;
    private readonly int nodeLimit;
    private readonly long pathBytesLimit;
    private long pathBytes = 1; // The root directory's UTF-8 name.
    private long writableBytes;
    private ulong nextInode = 1;
    private bool disposed;

    public VirtualFileSystem(IReadOnlyDictionary<string, ReadOnlyMemory<byte>> image,
        long writableLimit = 1 << 20, int descriptorLimit = 128, long imageLimit = 16 << 20,
        IReadOnlySet<string>? executablePaths = null, int nodeLimit = 1024,
        long pathBytesLimit = 256 << 10)
    {
        ArgumentNullException.ThrowIfNull(image);
        if (writableLimit < 0 || writableLimit > int.MaxValue) throw new ArgumentOutOfRangeException(nameof(writableLimit));
        if (descriptorLimit < 1) throw new ArgumentOutOfRangeException(nameof(descriptorLimit));
        if (imageLimit < 0) throw new ArgumentOutOfRangeException(nameof(imageLimit));
        if (nodeLimit < 1) throw new ArgumentOutOfRangeException(nameof(nodeLimit));
        if (pathBytesLimit < 1) throw new ArgumentOutOfRangeException(nameof(pathBytesLimit));
        long imageBytes = 0;
        this.writableLimit = writableLimit;
        this.descriptorLimit = descriptorLimit;
        this.nodeLimit = nodeLimit;
        this.pathBytesLimit = pathBytesLimit;
        directoryNodes.Add("/", new([], true, nextInode++, 0x4000 | 0x1ed));
        foreach (var (path, contents) in image)
        {
            var normalized = Normalize(path, "/");
            if (!normalized.Succeeded || normalized.Value == "/") throw new ArgumentException("Invalid image file path: " + path, nameof(image));
            if (contents.Length > imageLimit - imageBytes) throw new ArgumentException("Image byte limit exceeded.", nameof(image));
            imageBytes += contents.Length;
            if (files.ContainsKey(normalized.Value)) throw new ArgumentException("Duplicate image path: " + path, nameof(image));
            ReserveImageName(normalized.Value);
            files.Add(normalized.Value, new Node(contents.ToArray(), true, nextInode++, 0x8000 | 0x124));
            var parent = Parent(normalized.Value);
            while (!directories.Contains(parent))
            {
                ReserveImageName(parent);
                directories.Add(parent);
                directoryNodes.Add(parent, new([], true, nextInode++, 0x4000 | 0x1ed));
                parent = Parent(parent);
            }
        }
        if (files.Keys.Any(directories.Contains)) throw new ArgumentException("An image file also names a directory.", nameof(image));
        if (executablePaths != null)
            foreach (string path in executablePaths)
            {
                var normalized = Normalize(path, "/");
                if (!normalized.Succeeded || !files.TryGetValue(normalized.Value, out var node))
                    throw new ArgumentException("Executable path must name an image file: " + path, nameof(executablePaths));
                files[normalized.Value] = new(node.Bytes, true, node.Inode, 0x8000 | 0x16d);
            }
        this.imageBytes = imageBytes;
    }

    public long WritableBytes { get { lock (sync) return writableBytes; } }
    public int OpenDescriptors { get { lock (sync) return descriptors.Count; } }
    public int NodeCount { get { lock (sync) return files.Count + directoryNodes.Count + detachedNodes.Count; } }
    public long PathBytes { get { lock (sync) return pathBytes; } }

    /// <summary>Exact private byte and node quotas. Namespace and allocation
    /// limits remain independent constraints on future writes/creation.</summary>
    public HostResult<VirtualFileSystemCapacity> Capacity(string path, string cwd = "/")
    {
        lock (sync)
        {
            var resolved = CanonicalPath(path, cwd);
            return resolved.Succeeded ? HostResult<VirtualFileSystemCapacity>.Success(CapacitySnapshot())
                : Fail<VirtualFileSystemCapacity>(resolved.Error);
        }
    }
    public HostResult<VirtualFileSystemCapacity> Capacity(int descriptor)
    {
        lock (sync) return TryDescription(descriptor, out _)
            ? HostResult<VirtualFileSystemCapacity>.Success(CapacitySnapshot())
            : Fail<VirtualFileSystemCapacity>(GuestError.BadDescriptor);
    }
    private VirtualFileSystemCapacity CapacitySnapshot() => new(
        (ulong)imageBytes + (ulong)writableLimit, (ulong)(writableLimit - writableBytes),
        (ulong)nodeLimit, (ulong)(nodeLimit - files.Count - directoryNodes.Count - detachedNodes.Count));

    private bool CanAddName(string name) => files.Count + directoryNodes.Count + detachedNodes.Count < nodeLimit &&
        Encoding.UTF8.GetByteCount(name) <= pathBytesLimit - pathBytes;

    private void ReserveImageName(string name)
    {
        if (!CanAddName(name)) throw new ArgumentException("Image namespace limit exceeded.", "image");
        pathBytes += Encoding.UTF8.GetByteCount(name);
    }

    public HostResult<int> Open(string path, FileAccessMode access, bool create = false,
        bool exclusive = false, bool truncate = false, bool append = false, string cwd = "/",
        bool allowDirectory = false, bool requireDirectory = false, uint creationMode = 0x180)
    {
        lock (sync)
        {
            if (disposed) return Fail<int>(GuestError.BadDescriptor);
            if ((creationMode & ~0x1ffU) != 0) return Fail<int>(GuestError.Unsupported);
            if (access == 0 || (access & ~(FileAccessMode.Read | FileAccessMode.Write)) != 0 ||
                (truncate && !access.HasFlag(FileAccessMode.Write))) return Fail<int>(GuestError.Invalid);
            var resolved = Resolve(path, cwd);
            if (!resolved.Succeeded) return Fail<int>(resolved.Error);
            var name = resolved.Value;
            if (directoryNodes.TryGetValue(name, out var directory))
            {
                if (create && exclusive) return Fail<int>(GuestError.Exists);
                if (!allowDirectory || access != FileAccessMode.Read || create || truncate) return Fail<int>(GuestError.IsDirectory);
                if (append) return Fail<int>(GuestError.Unsupported);
                if (descriptors.Count >= descriptorLimit) return Fail<int>(GuestError.TooManyFiles);
                int directoryFd = NextDescriptor();
                descriptors.Add(directoryFd, new(directory, access, false, name));
                return HostResult<int>.Success(directoryFd);
            }
            if (requireDirectory) return Fail<int>(files.ContainsKey(name) ? GuestError.NotDirectory : GuestError.NoEntry);
            if (!directories.Contains(Parent(name))) return Fail<int>(files.ContainsKey(Parent(name)) ? GuestError.NotDirectory : GuestError.NoEntry);
            bool exists = files.TryGetValue(name, out var node);
            if (exists && create && exclusive) return Fail<int>(GuestError.Exists);
            if (!exists && !create) return Fail<int>(GuestError.NoEntry);
            if (node?.Immutable == true && access.HasFlag(FileAccessMode.Write)) return Fail<int>(GuestError.ReadOnly);
            // Validate every failing condition before creation or truncation.
            if (descriptors.Count >= descriptorLimit) return Fail<int>(GuestError.TooManyFiles);
            if (!exists && !CanAddName(name)) return Fail<int>(GuestError.NoSpace);
            node ??= new Node([], false, nextInode++, 0x8000 | creationMode);
            if (!exists)
            {
                files.Add(name, node);
                pathBytes += Encoding.UTF8.GetByteCount(name);
                var parent = directoryNodes[Parent(name)];
                parent.ModifyTime = parent.ChangeTime = VirtualFileTime.UtcNow;
            }
            if (truncate)
            {
                writableBytes -= node.Bytes.Length;
                node.Bytes = [];
                node.ModifyTime = node.ChangeTime = VirtualFileTime.UtcNow;
            }
            int descriptor = NextDescriptor();
            descriptors.Add(descriptor, new Description(node, access, append));
            return HostResult<int>.Success(descriptor);
        }
    }

    public HostResult<int> Read(int descriptor, Span<byte> destination)
    {
        lock (sync)
        {
            if (!TryDescription(descriptor, out var file) || !file.Access.HasFlag(FileAccessMode.Read)) return Fail<int>(GuestError.BadDescriptor);
            if (file.DirectoryPath != null) return Fail<int>(GuestError.IsDirectory);
            int count = (int)Math.Min(destination.Length, Math.Max(0, file.Node.Bytes.LongLength - file.Position));
            if (count != 0) file.Node.Bytes.AsSpan((int)file.Position, count).CopyTo(destination);
            file.Position += count;
            if (destination.Length != 0) file.Node.AccessTime = VirtualFileTime.UtcNow;
            return HostResult<int>.Success(count);
        }
    }
    public HostResult<int> ReadAt(int descriptor, Span<byte> destination, long offset)
    {
        lock (sync)
        {
            if (!TryDescription(descriptor, out var file) || !file.Access.HasFlag(FileAccessMode.Read)) return Fail<int>(GuestError.BadDescriptor);
            if (file.DirectoryPath != null) return Fail<int>(GuestError.IsDirectory);
            if (offset < 0) return Fail<int>(GuestError.Invalid);
            int count = (int)Math.Min(destination.Length, Math.Max(0, file.Node.Bytes.LongLength - offset));
            if (count != 0) file.Node.Bytes.AsSpan((int)offset, count).CopyTo(destination);
            if (destination.Length != 0) file.Node.AccessTime = VirtualFileTime.UtcNow;
            return HostResult<int>.Success(count);
        }
    }
    public HostResult<long> ReadAtLength(int descriptor)
    {
        lock (sync)
        {
            if (!TryDescription(descriptor, out var file) || !file.Access.HasFlag(FileAccessMode.Read)) return Fail<long>(GuestError.BadDescriptor);
            return file.DirectoryPath != null ? Fail<long>(GuestError.IsDirectory) : HostResult<long>.Success(file.Node.Bytes.LongLength);
        }
    }

    public HostResult<int> Write(int descriptor, ReadOnlySpan<byte> source) => WriteCore(descriptor, source, null);
    public HostResult<int> WriteAt(int descriptor, ReadOnlySpan<byte> source, long offset) => WriteCore(descriptor, source, offset);

    private HostResult<int> WriteCore(int descriptor, ReadOnlySpan<byte> source, long? offset)
    {
        lock (sync)
        {
            if (!TryDescription(descriptor, out var file) || !file.Access.HasFlag(FileAccessMode.Write)) return Fail<int>(GuestError.BadDescriptor);
            if (file.Node.Immutable) return Fail<int>(GuestError.ReadOnly);
            if (offset < 0) return Fail<int>(GuestError.Invalid);
            if (source.Length == 0) return HostResult<int>.Success(0);
            long position = file.Append ? file.Node.Bytes.LongLength : offset ?? file.Position;
            long maximumLength = file.Node.Bytes.LongLength + writableLimit - writableBytes;
            long available = maximumLength - position;
            if (available <= 0 || position > int.MaxValue) return Fail<int>(GuestError.NoSpace);
            int count = (int)Math.Min(source.Length, available);
            long length = Math.Max(file.Node.Bytes.LongLength, position + count);
            if (length > int.MaxValue) return Fail<int>(GuestError.NoSpace);
            if (length != file.Node.Bytes.LongLength)
            {
                int previous = file.Node.Bytes.Length;
                try { Array.Resize(ref file.Node.Bytes, (int)length); } // New gaps are zero filled.
                catch (OutOfMemoryException) { return Fail<int>(GuestError.NoMemory); }
                writableBytes += length - previous;
            }
            source[..count].CopyTo(file.Node.Bytes.AsSpan((int)position, count));
            if (!offset.HasValue) file.Position = position + count;
            file.Node.ModifyTime = file.Node.ChangeTime = VirtualFileTime.UtcNow;
            return HostResult<int>.Success(count);
        }
    }

    public HostResult<int> Truncate(int descriptor, long length)
    {
        lock (sync)
        {
            if (!TryDescription(descriptor, out var file)) return Fail<int>(GuestError.BadDescriptor);
            if (file.DirectoryPath != null || !file.Access.HasFlag(FileAccessMode.Write)) return Fail<int>(GuestError.Invalid);
            return Resize(file.Node, length);
        }
    }
    public HostResult<int> Truncate(string path, long length, string cwd = "/")
    {
        lock (sync)
        {
            if (disposed) return Fail<int>(GuestError.BadDescriptor);
            if (length < 0) return Fail<int>(GuestError.Invalid);
            var resolved = Resolve(path, cwd);
            if (!resolved.Succeeded) return Fail<int>(resolved.Error);
            if (directories.Contains(resolved.Value)) return Fail<int>(GuestError.IsDirectory);
            return files.TryGetValue(resolved.Value, out var node) ? Resize(node, length) : Fail<int>(GuestError.NoEntry);
        }
    }
    private HostResult<int> Resize(Node node, long length)
    {
        if (length < 0) return Fail<int>(GuestError.Invalid);
        if (node.Immutable) return Fail<int>(GuestError.ReadOnly);
        long previous = node.Bytes.LongLength;
        if (length > int.MaxValue || length - previous > writableLimit - writableBytes) return Fail<int>(GuestError.NoSpace);
        try { Array.Resize(ref node.Bytes, (int)length); }
        catch (OutOfMemoryException) { return Fail<int>(GuestError.NoMemory); }
        writableBytes += length - previous;
        node.ModifyTime = node.ChangeTime = VirtualFileTime.UtcNow;
        return HostResult<int>.Success(0);
    }
    // Every successful operation is already committed to this ephemeral memory
    // filesystem. No persistent host storage or delayed write queue exists.
    public HostResult<int> Synchronize(int descriptor)
    {
        lock (sync) return TryDescription(descriptor, out _) ? HostResult<int>.Success(0) : Fail<int>(GuestError.BadDescriptor);
    }

    public HostResult<long> Seek(int descriptor, long offset, SeekOrigin origin)
    {
        lock (sync)
        {
            if (!TryDescription(descriptor, out var file)) return Fail<long>(GuestError.BadDescriptor);
            if (file.DirectoryPath != null) return Fail<long>(GuestError.Unsupported);
            long start;
            switch (origin)
            {
                case SeekOrigin.Begin: start = 0; break;
                case SeekOrigin.Current: start = file.Position; break;
                case SeekOrigin.End: start = file.Node.Bytes.LongLength; break;
                default: return Fail<long>(GuestError.Invalid);
            }
            if (offset < -start || offset > long.MaxValue - start) return Fail<long>(GuestError.Invalid);
            file.Position = start + offset;
            return HostResult<long>.Success(file.Position);
        }
    }

    public HostResult<int> Duplicate(int descriptor)
    {
        lock (sync)
        {
            if (!TryDescription(descriptor, out var file)) return Fail<int>(GuestError.BadDescriptor);
            if (descriptors.Count >= descriptorLimit) return Fail<int>(GuestError.TooManyFiles);
            int copy = NextDescriptor();
            descriptors.Add(copy, file); // Duplicates share the open-file position.
            return HostResult<int>.Success(copy);
        }
    }

    public HostResult<int> Close(int descriptor)
    {
        lock (sync)
        {
            if (!descriptors.Remove(descriptor, out var description)) return Fail<int>(GuestError.BadDescriptor);
            if (!descriptors.Values.Any(file => ReferenceEquals(file, description)))
                advisoryLocks.Remove(description);
            if (!description.Node.Linked && !descriptors.Values.Any(file => ReferenceEquals(file.Node, description.Node)))
            {
                detachedNodes.Remove(description.Node);
                writableBytes -= description.Node.Bytes.Length;
            }
            return HostResult<int>.Success(0);
        }
    }

    public HostResult<VirtualFileStat> Stat(string path, string cwd = "/")
    {
        lock (sync)
        {
            if (disposed) return Fail<VirtualFileStat>(GuestError.BadDescriptor);
            var resolved = Resolve(path, cwd);
            if (!resolved.Succeeded) return Fail<VirtualFileStat>(resolved.Error);
            if (directoryNodes.TryGetValue(resolved.Value, out var directory))
                return HostResult<VirtualFileStat>.Success(Metadata(directory, true) with
                { Links = (ulong)(2 + directories.Count(p => p != "/" && Parent(p) == resolved.Value)) });
            return files.TryGetValue(resolved.Value, out var node)
                ? HostResult<VirtualFileStat>.Success(Metadata(node, false))
                : Fail<VirtualFileStat>(GuestError.NoEntry);
        }
    }

    public HostResult<VirtualFileStat> FStat(int descriptor)
    {
        lock (sync) return TryDescription(descriptor, out var file)
            ? file.DirectoryPath != null && file.Node.Linked ? Stat(file.DirectoryPath)
                : HostResult<VirtualFileStat>.Success(Metadata(file.Node, file.DirectoryPath != null))
            : Fail<VirtualFileStat>(GuestError.BadDescriptor);
    }

    /// <summary>Resolve an existing private path using the same component walk
    /// as open/stat, with optional final-directory validation.</summary>
    public HostResult<string> CanonicalPath(string path, string cwd = "/", bool requireDirectory = false)
    {
        lock (sync)
        {
            if (disposed) return Fail<string>(GuestError.BadDescriptor);
            var resolved = Resolve(path, cwd);
            if (!resolved.Succeeded) return resolved;
            if (directories.Contains(resolved.Value)) return resolved;
            if (!files.ContainsKey(resolved.Value)) return Fail<string>(GuestError.NoEntry);
            return requireDirectory ? Fail<string>(GuestError.NotDirectory) : resolved;
        }
    }

    public HostResult<string> DirectoryPath(int descriptor)
    {
        lock (sync)
        {
            if (!TryDescription(descriptor, out var file)) return Fail<string>(GuestError.BadDescriptor);
            if (!file.Node.Linked) return Fail<string>(GuestError.NoEntry);
            return file.DirectoryPath != null ? HostResult<string>.Success(file.DirectoryPath) : Fail<string>(GuestError.NotDirectory);
        }
    }

    public HostResult<VirtualDirectoryEntry[]> DirectorySnapshot(int descriptor, int entryLimit, int nameBytesLimit)
    {
        lock (sync)
        {
            if (!TryDescription(descriptor, out var file)) return Fail<VirtualDirectoryEntry[]>(GuestError.BadDescriptor);
            if (file.DirectoryPath == null) return Fail<VirtualDirectoryEntry[]>(GuestError.NotDirectory);
            if (!file.Node.Linked) return Fail<VirtualDirectoryEntry[]>(GuestError.NoEntry);
            if (entryLimit < 2 || nameBytesLimit < 5) return Fail<VirtualDirectoryEntry[]>(GuestError.NoMemory);
            string path = file.DirectoryPath;
            var entries = new List<VirtualDirectoryEntry> {
                new(".", file.Node.Inode, 4), new("..", directoryNodes[Parent(path)].Inode, 4)
            };
            int bytes = 5;
            foreach (var item in files.Concat(directoryNodes))
            {
                if (item.Key == "/" || Parent(item.Key) != path) continue;
                string name = item.Key[(item.Key.LastIndexOf('/') + 1)..];
                int length;
                try { length = new UTF8Encoding(false, true).GetByteCount(name); }
                catch (EncoderFallbackException) { return Fail<VirtualDirectoryEntry[]>(GuestError.Invalid); }
                // The retained supplied C record has d_name[256]. Reject the
                // entire acquisition rather than truncate or hide a late error.
                if (length > 255) return Fail<VirtualDirectoryEntry[]>(GuestError.NameTooLong);
                if (entries.Count == entryLimit || length + 1 > nameBytesLimit - bytes)
                    return Fail<VirtualDirectoryEntry[]>(GuestError.NoMemory);
                bytes += length + 1;
                entries.Add(new(name, item.Value.Inode, (byte)((item.Value.Mode & 0xf000) == 0x4000 ? 4 : 8)));
            }
            entries.Sort(2, entries.Count - 2, Comparer<VirtualDirectoryEntry>.Create((a,b) => StringComparer.Ordinal.Compare(a.Name,b.Name)));
            return HostResult<VirtualDirectoryEntry[]>.Success(entries.ToArray());
        }
    }

    public HostResult<int> SetAppend(int descriptor, bool append)
    {
        lock (sync)
        {
            if (!TryDescription(descriptor, out var file)) return Fail<int>(GuestError.BadDescriptor);
            if (append && file.DirectoryPath != null) return Fail<int>(GuestError.Unsupported);
            file.Append = append;
            return HostResult<int>.Success(0);
        }
    }

    private static VirtualFileStat Metadata(Node node, bool directory) => new(
        node.Bytes.LongLength, node.Immutable, directory, node.Inode, node.Mode,
        node.AccessTime.Ticks, node.ModifyTime.Ticks, node.ChangeTime.Ticks, Links: node.Linked ? 1UL : 0UL,
        AccessSubtick: node.AccessTime.Subtick, ModifySubtick: node.ModifyTime.Subtick, ChangeSubtick: node.ChangeTime.Subtick);

    public void Dispose()
    {
        lock (sync)
        {
            disposed = true;
            descriptors.Clear();
            advisoryLocks.Clear();
            files.Clear();
            directories.Clear();
            directoryNodes.Clear();
            detachedNodes.Clear();
            writableBytes = 0;
            pathBytes = 0;
        }
    }

    private int NextDescriptor()
    {
        int descriptor = 3; // stdin/out/err belong to the instance's separate stream contract.
        while (descriptors.ContainsKey(descriptor)) ++descriptor;
        return descriptor;
    }
    private bool TryDescription(int descriptor, out Description file)
    {
        file = null!;
        return !disposed && descriptors.TryGetValue(descriptor, out file!);
    }
    private HostResult<string> Resolve(string path, string cwd)
    {
        var directory = Normalize(cwd, "/");
        if (!directory.Succeeded) return directory;
        if (!directories.Contains(directory.Value)) return Fail<string>(GuestError.NotDirectory);
        if (string.IsNullOrEmpty(path)) return Fail<string>(GuestError.NoEntry);
        if (path.Length > 4096 || Encoding.UTF8.GetByteCount(path) > 4096) return Fail<string>(GuestError.NameTooLong);
        if (path.Contains('\0')) return Fail<string>(GuestError.Invalid);
        string current = path.StartsWith('/') ? "/" : directory.Value;
        foreach (var part in path.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            if (!directories.Contains(current)) return Fail<string>(files.ContainsKey(current) ? GuestError.NotDirectory : GuestError.NoEntry);
            if (part == ".") continue;
            if (part == "..")
            {
                if (current == "/") return Fail<string>(GuestError.Access);
                current = Parent(current);
            }
            else current = (current == "/" ? "" : current) + "/" + part;
        }
        if (Encoding.UTF8.GetByteCount(current) > 4096) return Fail<string>(GuestError.NameTooLong);
        if (path.EndsWith('/') && !directories.Contains(current)) return Fail<string>(files.ContainsKey(current) ? GuestError.NotDirectory : GuestError.NoEntry);
        return HostResult<string>.Success(current);
    }
    private static HostResult<string> Normalize(string path, string cwd)
    {
        if (string.IsNullOrEmpty(path)) return Fail<string>(GuestError.NoEntry);
        if (path.Length > 4096 || Encoding.UTF8.GetByteCount(path) > 4096) return Fail<string>(GuestError.NameTooLong);
        if (path.Contains('\0')) return Fail<string>(GuestError.Invalid);
        var parts = new List<string>();
        foreach (var part in (path.StartsWith('/') ? path : cwd + "/" + path).Split('/'))
        {
            if (part is "" or ".") continue;
            if (part == "..")
            {
                if (parts.Count == 0) return Fail<string>(GuestError.Access);
                parts.RemoveAt(parts.Count - 1);
            }
            else parts.Add(part);
        }
        string result = "/" + string.Join('/', parts);
        return Encoding.UTF8.GetByteCount(result) > 4096 ? Fail<string>(GuestError.NameTooLong) : HostResult<string>.Success(result);
    }
    private static string Parent(string path) => path[..Math.Max(1, path.LastIndexOf('/'))];
    private static HostResult<T> Fail<T>(GuestError error) => HostResult<T>.Failure(error);
}
