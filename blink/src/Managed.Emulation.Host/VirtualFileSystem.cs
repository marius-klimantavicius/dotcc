namespace Managed.Emulation.Host;

public enum GuestError
{
    None = 0, NoEntry = 2, Io = 5, BadDescriptor = 9, Again = 11, NoMemory = 12, Access = 13, Exists = 17,
    NotDirectory = 20, IsDirectory = 21, Invalid = 22, TooManyFiles = 24,
    NoSpace = 28, IllegalSeek = 29, ReadOnly = 30, BrokenPipe = 32, NameTooLong = 36,
    NotSocket = 88, Unsupported = 95, AddressInUse = 98, AddressUnavailable = 99,
    ConnectionReset = 104, AlreadyConnected = 106, NotConnected = 107, TimedOut = 110, ConnectionRefused = 111, Canceled = 125
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
    ulong Inode = 0, uint Mode = 0, long AccessTicks = 0, long ModifyTicks = 0, long ChangeTicks = 0, ulong Links = 1);

/// <summary>A private Linux-path namespace. It never consults the host filesystem.</summary>
public sealed class VirtualFileSystem : IDisposable
{
    private sealed class Node(byte[] bytes, bool immutable, ulong inode, uint mode)
    {
        internal byte[] Bytes = bytes;
        internal readonly bool Immutable = immutable;
        internal readonly ulong Inode = inode;
        internal readonly uint Mode = mode;
        internal long AccessTicks = DateTime.UtcNow.Ticks;
        internal long ModifyTicks = DateTime.UtcNow.Ticks;
        internal long ChangeTicks = DateTime.UtcNow.Ticks;
    }
    private sealed class Description(Node node, FileAccessMode access, bool append, string? directoryPath = null)
    {
        internal readonly Node Node = node;
        internal readonly FileAccessMode Access = access;
        internal bool Append = append;
        internal readonly string? DirectoryPath = directoryPath;
        internal long Position;
    }
    private readonly object sync = new();
    private readonly Dictionary<string, Node> files = new(StringComparer.Ordinal);
    private readonly HashSet<string> directories = new(StringComparer.Ordinal) { "/" };
    private readonly Dictionary<string, Node> directoryNodes = new(StringComparer.Ordinal);
    private readonly Dictionary<int, Description> descriptors = new();
    private readonly long writableLimit;
    private readonly int descriptorLimit;
    private long writableBytes;
    private ulong nextInode = 1;
    private bool disposed;

    public VirtualFileSystem(IReadOnlyDictionary<string, ReadOnlyMemory<byte>> image,
        long writableLimit = 1 << 20, int descriptorLimit = 128, long imageLimit = 16 << 20,
        IReadOnlySet<string>? executablePaths = null)
    {
        ArgumentNullException.ThrowIfNull(image);
        if (writableLimit < 0 || writableLimit > int.MaxValue) throw new ArgumentOutOfRangeException(nameof(writableLimit));
        if (descriptorLimit < 1) throw new ArgumentOutOfRangeException(nameof(descriptorLimit));
        if (imageLimit < 0) throw new ArgumentOutOfRangeException(nameof(imageLimit));
        long imageBytes = 0;
        this.writableLimit = writableLimit;
        this.descriptorLimit = descriptorLimit;
        directoryNodes.Add("/", new([], true, nextInode++, 0x4000 | 0x1ed));
        foreach (var (path, contents) in image)
        {
            var normalized = Normalize(path, "/");
            if (!normalized.Succeeded || normalized.Value == "/") throw new ArgumentException("Invalid image file path: " + path, nameof(image));
            if (contents.Length > imageLimit - imageBytes) throw new ArgumentException("Image byte limit exceeded.", nameof(image));
            imageBytes += contents.Length;
            if (!files.TryAdd(normalized.Value, new Node(contents.ToArray(), true, nextInode++, 0x8000 | 0x124))) throw new ArgumentException("Duplicate image path: " + path, nameof(image));
            var parent = Parent(normalized.Value);
            while (directories.Add(parent))
            {
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
    }

    public long WritableBytes { get { lock (sync) return writableBytes; } }
    public int OpenDescriptors { get { lock (sync) return descriptors.Count; } }

    public HostResult<int> Open(string path, FileAccessMode access, bool create = false,
        bool exclusive = false, bool truncate = false, bool append = false, string cwd = "/",
        bool allowDirectory = false, bool requireDirectory = false)
    {
        lock (sync)
        {
            if (disposed) return Fail<int>(GuestError.BadDescriptor);
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
            node ??= new Node([], false, nextInode++, 0x8000 | 0x180);
            if (!exists)
            {
                files.Add(name, node);
                var parent = directoryNodes[Parent(name)];
                parent.ModifyTicks = parent.ChangeTicks = DateTime.UtcNow.Ticks;
            }
            if (truncate)
            {
                writableBytes -= node.Bytes.Length;
                node.Bytes = [];
                node.ModifyTicks = node.ChangeTicks = DateTime.UtcNow.Ticks;
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
            if (destination.Length != 0) file.Node.AccessTicks = DateTime.UtcNow.Ticks;
            return HostResult<int>.Success(count);
        }
    }

    public HostResult<int> Write(int descriptor, ReadOnlySpan<byte> source)
    {
        lock (sync)
        {
            if (!TryDescription(descriptor, out var file) || !file.Access.HasFlag(FileAccessMode.Write)) return Fail<int>(GuestError.BadDescriptor);
            if (file.Node.Immutable) return Fail<int>(GuestError.ReadOnly);
            if (source.Length == 0) return HostResult<int>.Success(0);
            long position = file.Append ? file.Node.Bytes.LongLength : file.Position;
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
            file.Position = position + count;
            file.Node.ModifyTicks = file.Node.ChangeTicks = DateTime.UtcNow.Ticks;
            return HostResult<int>.Success(count);
        }
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
        lock (sync) return descriptors.Remove(descriptor)
            ? HostResult<int>.Success(0) : Fail<int>(GuestError.BadDescriptor);
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
            ? file.DirectoryPath != null ? Stat(file.DirectoryPath) : HostResult<VirtualFileStat>.Success(Metadata(file.Node, false))
            : Fail<VirtualFileStat>(GuestError.BadDescriptor);
    }

    public HostResult<string> DirectoryPath(int descriptor)
    {
        lock (sync)
        {
            if (!TryDescription(descriptor, out var file)) return Fail<string>(GuestError.BadDescriptor);
            return file.DirectoryPath != null ? HostResult<string>.Success(file.DirectoryPath) : Fail<string>(GuestError.NotDirectory);
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
        node.AccessTicks, node.ModifyTicks, node.ChangeTicks);

    public void Dispose()
    {
        lock (sync)
        {
            disposed = true;
            descriptors.Clear();
            files.Clear();
            directories.Clear();
            directoryNodes.Clear();
            writableBytes = 0;
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
        if (path.Length > 4096) return Fail<string>(GuestError.NameTooLong);
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
        if (path.EndsWith('/') && !directories.Contains(current)) return Fail<string>(files.ContainsKey(current) ? GuestError.NotDirectory : GuestError.NoEntry);
        return HostResult<string>.Success(current);
    }
    private static HostResult<string> Normalize(string path, string cwd)
    {
        if (string.IsNullOrEmpty(path)) return Fail<string>(GuestError.NoEntry);
        if (path.Length > 4096) return Fail<string>(GuestError.NameTooLong);
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
        return HostResult<string>.Success("/" + string.Join('/', parts));
    }
    private static string Parent(string path) => path[..Math.Max(1, path.LastIndexOf('/'))];
    private static HostResult<T> Fail<T>(GuestError error) => HostResult<T>.Failure(error);
}
