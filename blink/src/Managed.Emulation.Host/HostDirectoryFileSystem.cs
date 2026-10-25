using System.Text;

namespace Managed.Emulation.Host;

/// <summary>A live host-directory mapping implemented only with System.IO.
/// Component/reparse checks are conservative but not atomic with later I/O.
/// The caller must trust host-side namespace writers and avoid hard-link aliases
/// and special files. This is guest mediation, not a hostile-host sandbox.</summary>
public sealed class HostDirectoryFileSystem : IGuestFileSystem
{
    private sealed class InodeState(string path, ulong number)
    {
        internal string Path = path;
        internal readonly ulong Number = number;
        internal bool Linked = true;
        internal readonly HashSet<Description> OpenDescriptions = new();
        internal long ChangeTicks;
        internal VirtualFileStat DetachedMetadata;
    }
    private sealed class Description(InodeState node, FileStream? stream, FileAccessMode access, bool append)
    {
        internal readonly InodeState Node = node;
        internal string Path => Node.Path;
        internal ulong Inode => Node.Number;
        internal bool Linked => Node.Linked;
        internal readonly FileStream? Stream = stream;
        internal readonly FileAccessMode Access = access;
        internal bool Append = append;
        internal int References;
        internal long Position;
    }
    private sealed class FsError(GuestError error) : IOException { internal GuestError Error => error; }
    private readonly object gate = new();
    private readonly string root;
    private readonly bool readOnly;
    private readonly int descriptorLimit, nodeLimit;
    private readonly long writableLimit, pathBytesLimit;
    private readonly Dictionary<int, Description> descriptions = new();
    private readonly Dictionary<string, InodeState> inodes = new(StringComparer.Ordinal);
    private readonly HashSet<InodeState> detachedInodes = new();
    private ulong nextInode = 1;
    private bool disposed;
    public static bool SupportsPersistentUnixModes => !OperatingSystem.IsWindows();

    private HostDirectoryFileSystem(string root, bool readOnly, int descriptorLimit, long writableLimit, int nodeLimit, long pathBytesLimit)
    { this.root = root; this.readOnly = readOnly; this.descriptorLimit = descriptorLimit; this.writableLimit = writableLimit; this.nodeLimit = nodeLimit; this.pathBytesLimit = pathBytesLimit; }
    public static HostDirectoryFileSystem OpenRoot(string hostPath, bool readOnly = false, int descriptorLimit = 128,
        long writableLimit = long.MaxValue, int nodeLimit = 1024, long pathBytesLimit = 256 << 10)
    {
        ArgumentException.ThrowIfNullOrEmpty(hostPath);
        if (descriptorLimit < 1 || writableLimit < 0 || nodeLimit < 1 || pathBytesLimit < 1) throw new ArgumentOutOfRangeException(nameof(writableLimit));
        string full = Path.TrimEndingDirectorySeparator(Path.GetFullPath(hostPath));
        CheckHostComponents(full);
        if (!Directory.Exists(full)) throw new DirectoryNotFoundException("Mount root is not a directory.");
        var store = new HostDirectoryFileSystem(full, readOnly, descriptorLimit, writableLimit, nodeLimit, pathBytesLimit);
        try { store.Inventory(); return store; } catch { store.Dispose(); throw; }
    }
    private static HostResult<T> Ok<T>(T value) => HostResult<T>.Success(value);
    private static HostResult<T> Fail<T>(GuestError error) => HostResult<T>.Failure(error);
    private static void Require(bool value, GuestError error) { if (!value) throw new FsError(error); }
    private static GuestError Error(Exception error) => error switch
    {
        FsError e => e.Error, FileNotFoundException or DirectoryNotFoundException => GuestError.NoEntry,
        UnauthorizedAccessException => GuestError.Access, PathTooLongException => GuestError.NameTooLong,
        ArgumentException or NotSupportedException or OverflowException => GuestError.Invalid,
        IOException when (error.HResult & 0xffff) is 17 or 80 or 183 => GuestError.Exists,
        IOException when (error.HResult & 0xffff) is 39 or 145 => GuestError.NotEmpty,
        IOException when (error.HResult & 0xffff) is 32 or 33 => GuestError.Busy,
        _ => GuestError.Io
    };
    private HostResult<T> Guard<T>(Func<HostResult<T>> action)
    {
        lock (gate)
        {
            if (disposed) return Fail<T>(GuestError.BadDescriptor);
            try { return action(); }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException or OverflowException)
            { return Fail<T>(Error(error)); }
        }
    }
    private void Writable() => Require(!readOnly, GuestError.ReadOnly);
    private Description Get(int fd) => descriptions.TryGetValue(fd, out var d) ? d : throw new FsError(GuestError.BadDescriptor);
    private int Add(Description description)
    {
        int fd = 3; while (descriptions.ContainsKey(fd)) ++fd;
        descriptions.Add(fd, description);
        if (description.References++ == 0) description.Node.OpenDescriptions.Add(description);
        return fd;
    }
    private InodeState Inode(string path)
    { if (!inodes.TryGetValue(path, out var node)) inodes.Add(path, node = new(path, nextInode++)); return node; }
    private void Detach(InodeState node, VirtualFileStat metadata)
    {
        node.Linked = false; node.ChangeTicks = DateTime.UtcNow.Ticks;
        node.DetachedMetadata = metadata with { Links = 0, ChangeTicks = node.ChangeTicks };
        if (node.OpenDescriptions.Count != 0) detachedInodes.Add(node);
    }
    private static string Parent(string path) => path[..Math.Max(1, path.LastIndexOf('/'))];
    private static FileAttributes Attributes(string path)
    {
        var attributes = File.GetAttributes(path);
        Require((attributes & (FileAttributes.ReparsePoint | FileAttributes.Device)) == 0, GuestError.Access);
        FileSystemInfo item = (attributes & FileAttributes.Directory) != 0 ? new DirectoryInfo(path) : new FileInfo(path);
        Require(item.LinkTarget == null, GuestError.Access);
        return attributes;
    }
    private static void CheckHostComponents(string full)
    {
        string current = Path.GetPathRoot(full)!;
        Attributes(current);
        foreach (string part in full[current.Length..].Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
        { current = Path.Combine(current, part); Attributes(current); }
    }
    private string HostPath(string canonical) => canonical == "/" ? root : Path.Combine(root, canonical[1..].Replace('/', Path.DirectorySeparatorChar));
    private string Resolve(string path, string cwd, bool allowMissing = false)
    {
        Require(!string.IsNullOrEmpty(path), GuestError.NoEntry);
        Require(!path.Contains('\0') && cwd.StartsWith('/') && !cwd.Contains('\0'), GuestError.Invalid);
        Require(Encoding.UTF8.GetByteCount(path) <= 4096, GuestError.NameTooLong);
        // On Windows reject alternate separators, drive syntax and alternate
        // data streams instead of interpreting Linux guest bytes as host syntax.
        if (OperatingSystem.IsWindows()) Require(!path.Contains('\\') && !path.Contains(':') && !cwd.Contains('\\') && !cwd.Contains(':'), GuestError.Invalid);
        CheckHostComponents(root);
        string current = "/";
        string input = path.StartsWith('/') ? path : cwd.TrimEnd('/') + "/" + path;
        var parts = input.Split('/', StringSplitOptions.RemoveEmptyEntries);
        for (int index = 0; index < parts.Length; ++index)
        {
            Require((Attributes(HostPath(current)) & FileAttributes.Directory) != 0, GuestError.NotDirectory);
            string part = parts[index];
            if (part == ".") continue;
            if (part == "..") { Require(current != "/", GuestError.Access); current = Parent(current); continue; }
            if (OperatingSystem.IsWindows())
            {
                Require(part.TrimEnd(' ', '.') == part, GuestError.Invalid);
                string stem = part.Split('.')[0].ToUpperInvariant();
                Require(stem is not ("CON" or "PRN" or "AUX" or "NUL" or "CONIN$" or "CONOUT$") &&
                    !(stem.Length == 4 && (stem.StartsWith("COM", StringComparison.Ordinal) || stem.StartsWith("LPT", StringComparison.Ordinal)) && stem[3] is >= '0' and <= '9'), GuestError.Invalid);
            }
            current = current.TrimEnd('/') + "/" + part;
            try { Attributes(HostPath(current)); }
            catch (Exception error) when (allowMissing && index == parts.Length - 1 && error is FileNotFoundException or DirectoryNotFoundException) { }
        }
        Require(Encoding.UTF8.GetByteCount(current) <= 4096, GuestError.NameTooLong);
        if (path.EndsWith('/')) Require((Attributes(HostPath(current)) & FileAttributes.Directory) != 0, GuestError.NotDirectory);
        return current;
    }
    private uint Mode(string path, bool directory)
    {
        uint permission;
        if (!OperatingSystem.IsWindows()) permission = (uint)File.GetUnixFileMode(HostPath(path)) & 0x1ff;
        else permission = directory ? 0x1edu : 0x180u;
        return (directory ? 0x4000u : 0x8000u) | permission;
    }
    private VirtualFileStat Metadata(string path)
    {
        string host = HostPath(path); var attributes = Attributes(host); bool directory = (attributes & FileAttributes.Directory) != 0;
        var node = Inode(path);
        return new(directory ? 0 : new FileInfo(host).Length,
            readOnly || (attributes & FileAttributes.ReadOnly) != 0, directory, node.Number, Mode(path, directory),
            File.GetLastAccessTimeUtc(host).Ticks, File.GetLastWriteTimeUtc(host).Ticks,
            node.ChangeTicks != 0 ? node.ChangeTicks : File.GetCreationTimeUtc(host).Ticks);
    }
    private VirtualFileStat Metadata(Description description)
    {
        var node = description.Node;
        if (description.Stream == null) return node.Linked ? Metadata(node.Path) : node.DetachedMetadata with { ChangeTicks = node.ChangeTicks };
        // Query the open file, including after unlink/replacement. Path-based
        // queries would observe a new file subsequently created at the old name.
        var handle = description.Stream.SafeFileHandle;
        uint permissions = OperatingSystem.IsWindows() ? 0x180u : (uint)File.GetUnixFileMode(handle) & 0x1ff;
        return new(description.Stream.Length, readOnly || (File.GetAttributes(handle) & FileAttributes.ReadOnly) != 0,
            false, node.Number, 0x8000u | permissions,
            File.GetLastAccessTimeUtc(handle).Ticks, File.GetLastWriteTimeUtc(handle).Ticks,
            node.ChangeTicks != 0 ? node.ChangeTicks : File.GetCreationTimeUtc(handle).Ticks, node.Linked ? 1u : 0u);
    }
    public int OpenDescriptors { get { lock (gate) return descriptions.Count; } }
    public long WritableBytes { get { lock (gate) return Inventory().Bytes; } }
    public int NodeCount { get { lock (gate) return Inventory().Nodes; } }
    public long PathBytes { get { lock (gate) return Inventory().PathBytes; } }
    private (long Bytes, int Nodes, long PathBytes) Inventory()
    {
        Require(!disposed, GuestError.BadDescriptor);
        long bytes = 0, pathBytes = 1; int nodes = 1;
        var pending = new Stack<string>(); pending.Push("/");
        while (pending.Count != 0)
        {
            string parent = Resolve(pending.Pop(), "/");
            foreach (string host in Directory.EnumerateFileSystemEntries(HostPath(parent)))
            {
                string path = parent.TrimEnd('/') + "/" + Path.GetFileName(host);
                var attributes = Attributes(host);
                ++nodes; pathBytes = checked(pathBytes + Encoding.UTF8.GetByteCount(path));
                Require(nodes <= nodeLimit && pathBytes <= pathBytesLimit, GuestError.NoSpace);
                if ((attributes & FileAttributes.Directory) != 0) pending.Push(path);
                else { bytes = checked(bytes + new FileInfo(host).Length); Require(bytes <= writableLimit, GuestError.NoSpace); }
            }
        }
        foreach (var node in detachedInodes)
        {
            // Independent opens own different descriptions, but retain one
            // inode's storage. Duplicates keep their description alive too.
            var d = node.OpenDescriptions.First();
            bytes = checked(bytes + (d.Stream?.Length ?? 0)); ++nodes;
        }
        Require(bytes <= writableLimit && nodes <= nodeLimit, GuestError.NoSpace);
        return (bytes, nodes, pathBytes);
    }
    public HostResult<int> Open(string path, FileAccessMode access, bool create = false, bool exclusive = false,
        bool truncate = false, bool append = false, string cwd = "/", bool allowDirectory = false, bool requireDirectory = false, uint creationMode = 0x180) => Guard(() =>
    {
        Require(access != 0 && (access & ~(FileAccessMode.Read | FileAccessMode.Write)) == 0, GuestError.Invalid);
        Require((creationMode & ~0x1ffu) == 0 && (!truncate || access.HasFlag(FileAccessMode.Write)), GuestError.Invalid);
        Require(descriptions.Count < descriptorLimit, GuestError.TooManyFiles);
        if (create || truncate || access.HasFlag(FileAccessMode.Write)) Writable();
        string canonical = Resolve(path, cwd, create), host = HostPath(canonical);
        bool directory = Directory.Exists(host), exists = directory || File.Exists(host);
        if (create && exclusive && exists) return Fail<int>(GuestError.Exists);
        if (directory)
        {
            Require(allowDirectory && access == FileAccessMode.Read && !create && !truncate && !append, GuestError.IsDirectory);
            return Ok(Add(new(Inode(canonical), null, access, false)));
        }
        Require(!requireDirectory, exists ? GuestError.NotDirectory : GuestError.NoEntry);
        if (!exists)
        {
            Require(create, GuestError.NoEntry); var i = Inventory();
            Require(!OperatingSystem.IsWindows() || creationMode == 0x180, GuestError.Unsupported);
            Require(i.Nodes < nodeLimit && Encoding.UTF8.GetByteCount(canonical) <= pathBytesLimit - i.PathBytes, GuestError.NoSpace);
        }
        var stream = new FileStream(host, create ? exclusive ? FileMode.CreateNew : FileMode.OpenOrCreate : FileMode.Open,
            access == FileAccessMode.Read ? FileAccess.Read : access == FileAccessMode.Write ? FileAccess.Write : FileAccess.ReadWrite,
            FileShare.ReadWrite | FileShare.Delete, 1, FileOptions.RandomAccess);
        try
        {
            // Recheck after opening too; neither check is claimed to close the
            // check/open race under hostile host namespace mutation.
            Resolve(path, cwd);
            if (!exists && SupportsPersistentUnixModes) ApplyMode(canonical, creationMode);
            var node = Inode(canonical);
            if (truncate) { stream.SetLength(0); node.ChangeTicks = DateTime.UtcNow.Ticks; }
            return Ok(Add(new(node, stream, access, append)));
        }
        catch { stream.Dispose(); throw; }
    });
    public HostResult<int> Read(int descriptor, Span<byte> destination) => ReadCore(descriptor, destination, null);
    public HostResult<int> ReadAt(int descriptor, Span<byte> destination, long offset) => ReadCore(descriptor, destination, offset);
    private HostResult<int> ReadCore(int fd, Span<byte> bytes, long? offset)
    {
        lock (gate)
        {
            try
            {
                Require(!disposed, GuestError.BadDescriptor); var d = Get(fd);
                Require(d.Access.HasFlag(FileAccessMode.Read), GuestError.BadDescriptor); Require(d.Stream != null, GuestError.IsDirectory);
                long position = offset ?? d.Position; Require(position >= 0, GuestError.Invalid);
                int count = RandomAccess.Read(d.Stream!.SafeFileHandle, bytes, position); if (offset == null) d.Position += count; return Ok(count);
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException or ArgumentException) { return Fail<int>(Error(error)); }
        }
    }
    public HostResult<int> Write(int descriptor, ReadOnlySpan<byte> source) => WriteCore(descriptor, source, null);
    public HostResult<int> WriteAt(int descriptor, ReadOnlySpan<byte> source, long offset) => WriteCore(descriptor, source, offset);
    private HostResult<int> WriteCore(int fd, ReadOnlySpan<byte> bytes, long? offset)
    {
        lock (gate)
        {
            try
            {
                Require(!disposed, GuestError.BadDescriptor); Writable(); var d = Get(fd);
                Require(d.Access.HasFlag(FileAccessMode.Write), GuestError.BadDescriptor); Require(d.Stream != null, GuestError.IsDirectory);
                Require(offset is null or >= 0, GuestError.Invalid);
                long length = d.Stream!.Length, position = d.Append ? length : offset ?? d.Position;
                var i = Inventory(); long allowance = writableLimit - i.Bytes;
                long available = position <= length ? Math.Min(long.MaxValue - (length - position), allowance) + length - position : allowance - (position - length);
                int count = (int)Math.Min(bytes.Length, Math.Max(0, available)); Require(count != 0 || bytes.Length == 0, GuestError.NoSpace);
                RandomAccess.Write(d.Stream.SafeFileHandle, bytes[..count], position);
                if (count != 0) d.Node.ChangeTicks = DateTime.UtcNow.Ticks;
                if (offset == null) d.Position = position + count; return Ok(count);
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException or ArgumentException or OverflowException) { return Fail<int>(Error(error)); }
        }
    }
    public HostResult<long> ReadAtLength(int descriptor) => Guard(() => { var d = Get(descriptor); Require(d.Access.HasFlag(FileAccessMode.Read), GuestError.BadDescriptor); Require(d.Stream != null, GuestError.IsDirectory); return Ok(d.Stream!.Length); });
    public HostResult<long> Seek(int descriptor, long offset, SeekOrigin origin) => Guard(() =>
    {
        var d = Get(descriptor); Require(d.Stream != null, GuestError.Unsupported);
        long start = origin switch { SeekOrigin.Begin => 0, SeekOrigin.Current => d.Position, SeekOrigin.End => d.Stream!.Length, _ => throw new FsError(GuestError.Invalid) };
        long next = checked(start + offset); Require(next >= 0, GuestError.Invalid); d.Position = next; return Ok(next);
    });
    public HostResult<int> Close(int descriptor) => Guard(() =>
    {
        var d = Get(descriptor); descriptions.Remove(descriptor);
        if (--d.References == 0)
        {
            locks.Remove(d); d.Stream?.Dispose();
            d.Node.OpenDescriptions.Remove(d);
            if (d.Node.OpenDescriptions.Count == 0) detachedInodes.Remove(d.Node);
        }
        return Ok(0);
    });
    public HostResult<int> Duplicate(int descriptor) => Guard(() => { var d = Get(descriptor); Require(descriptions.Count < descriptorLimit, GuestError.TooManyFiles); return Ok(Add(d)); });
    public HostResult<int> SetAppend(int descriptor, bool append) => Guard(() => { var d = Get(descriptor); Require(d.Stream != null || !append, GuestError.Unsupported); d.Append = append; return Ok(0); });
    public HostResult<VirtualFileStat> Stat(string path, string cwd = "/") => Guard(() => Ok(Metadata(Resolve(path, cwd))));
    public HostResult<VirtualFileStat> FStat(int descriptor) => Guard(() => Ok(Metadata(Get(descriptor))));
    public HostResult<string> CanonicalPath(string path, string cwd = "/", bool requireDirectory = false) => Guard(() =>
    { string p = Resolve(path, cwd); Require(!requireDirectory || Directory.Exists(HostPath(p)), GuestError.NotDirectory); return Ok(p); });
    public HostResult<string> DirectoryPath(int descriptor) => Guard(() => { var d = Get(descriptor); Require(d.Stream == null, GuestError.NotDirectory); Require(d.Linked, GuestError.NoEntry); return Ok(d.Path); });
    public HostResult<VirtualDirectoryEntry[]> DirectorySnapshot(int descriptor, int entryLimit, int nameBytesLimit) => Guard(() =>
    {
        var d = Get(descriptor); Require(d.Stream == null, GuestError.NotDirectory); Require(d.Linked, GuestError.NoEntry);
        Require(entryLimit >= 2 && nameBytesLimit >= 5, GuestError.NoMemory); Resolve(d.Path, "/");
        var entries = new List<VirtualDirectoryEntry> { new(".", d.Inode, 4), new("..", Inode(Parent(d.Path)).Number, 4) }; int bytes = 5;
        foreach (string host in Directory.EnumerateFileSystemEntries(HostPath(d.Path)))
        {
            string name = Path.GetFileName(host); int size = new UTF8Encoding(false, true).GetByteCount(name);
            Require(size <= 255, GuestError.NameTooLong); Require(entries.Count < entryLimit && size + 1 <= nameBytesLimit - bytes, GuestError.NoMemory);
            var attr = Attributes(host); string path = d.Path.TrimEnd('/') + "/" + name; bytes += size + 1;
            entries.Add(new(name, Inode(path).Number, (attr & FileAttributes.Directory) != 0 ? (byte)4 : (byte)8));
        }
        return Ok(entries.OrderBy(e => e.Name == "." ? 0 : e.Name == ".." ? 1 : 2).ThenBy(e => e.Name, StringComparer.Ordinal).ToArray());
    });
    public HostResult<int> Truncate(int descriptor, long length) => Guard(() =>
    { Writable(); var d = Get(descriptor); Require(d.Access.HasFlag(FileAccessMode.Write), GuestError.BadDescriptor); Require(d.Stream != null && length >= 0, GuestError.Invalid); var i = Inventory(); Require(length <= d.Stream!.Length || length - d.Stream.Length <= writableLimit - i.Bytes, GuestError.NoSpace); d.Stream.SetLength(length); d.Node.ChangeTicks = DateTime.UtcNow.Ticks; return Ok(0); });
    private HostResult<int> OpenAction(string path, string cwd, FileAccessMode access, Func<int, HostResult<int>> action)
    { var fd = Open(path, access, cwd: cwd, allowDirectory: true); if (!fd.Succeeded) return fd; try { return action(fd.Value); } finally { Close(fd.Value); } }
    public HostResult<int> Truncate(string path, long length, string cwd = "/") => OpenAction(path, cwd, FileAccessMode.Write, fd => Truncate(fd, length));
    public HostResult<int> Synchronize(int descriptor) => Guard(() => { var d = Get(descriptor); Require(d.Stream != null, GuestError.Unsupported); d.Stream!.Flush(true); return Ok(0); });
    private void ApplyMode(string path, uint mode)
    {
        Require((mode & ~0x1ffu) == 0, GuestError.Unsupported);
        if (OperatingSystem.IsWindows()) throw new FsError(GuestError.Unsupported);
        Resolve(path, "/"); File.SetUnixFileMode(HostPath(path), (UnixFileMode)mode);
    }
    public HostResult<int> ChangeMode(int descriptor, uint mode) => Guard(() =>
    {
        Writable(); var d = Get(descriptor);
        Require((mode & ~0x1ffu) == 0, GuestError.Unsupported);
        if (OperatingSystem.IsWindows()) return Fail<int>(GuestError.Unsupported);
        if (d.Stream != null) File.SetUnixFileMode(d.Stream.SafeFileHandle, (UnixFileMode)mode);
        else if (d.Linked) ApplyMode(d.Path, mode);
        else d.Node.DetachedMetadata = d.Node.DetachedMetadata with { Mode = 0x4000u | mode };
        d.Node.ChangeTicks = DateTime.UtcNow.Ticks;
        return Ok(0);
    });
    public HostResult<int> ChangeMode(string path, uint mode, string cwd = "/") => Guard(() =>
    { Writable(); string p = Resolve(path, cwd); ApplyMode(p, mode); Inode(p).ChangeTicks = DateTime.UtcNow.Ticks; return Ok(0); });
    public HostResult<int> ChangeOwner(int descriptor, uint user, uint group) => Guard(() =>
    { Writable(); Get(descriptor); Require((user is 0 or uint.MaxValue) && (group is 0 or uint.MaxValue), (GuestError)1); return Ok(0); });
    public HostResult<int> ChangeOwner(string path, uint user, uint group, string cwd = "/") => Guard(() =>
    { Writable(); Resolve(path, cwd); Require((user is 0 or uint.MaxValue) && (group is 0 or uint.MaxValue), (GuestError)1); return Ok(0); });
    public HostResult<int> SetTimes(int descriptor, VirtualFileTimeUpdate access, VirtualFileTimeUpdate modify) => Guard(() =>
    {
        var d = Get(descriptor); ValidateTimes(access, modify);
        if (access.Selection == FileTimeSelection.Omit && modify.Selection == FileTimeSelection.Omit) return Ok(0);
        Writable(); DateTime now = DateTime.UtcNow;
        if (d.Stream != null)
        {
            if (access.Selection != FileTimeSelection.Omit) File.SetLastAccessTimeUtc(d.Stream.SafeFileHandle, SelectedTime(access, now));
            if (modify.Selection != FileTimeSelection.Omit) File.SetLastWriteTimeUtc(d.Stream.SafeFileHandle, SelectedTime(modify, now));
        }
        else if (d.Linked) return SetTimes(d.Path, access, modify);
        else d.Node.DetachedMetadata = d.Node.DetachedMetadata with {
            AccessTicks = access.Selection == FileTimeSelection.Omit ? d.Node.DetachedMetadata.AccessTicks : SelectedTime(access, now).Ticks,
            ModifyTicks = modify.Selection == FileTimeSelection.Omit ? d.Node.DetachedMetadata.ModifyTicks : SelectedTime(modify, now).Ticks };
        d.Node.ChangeTicks = now.Ticks;
        return Ok(0);
    });
    private static DateTime SelectedTime(VirtualFileTimeUpdate update, DateTime now)
        => update.Selection == FileTimeSelection.Now ? now : new DateTime(update.Time.Ticks, DateTimeKind.Utc);
    private static void ValidateTimes(VirtualFileTimeUpdate access, VirtualFileTimeUpdate modify)
    {
        Require(access.IsValid && modify.IsValid, GuestError.Invalid);
        Require((access.Selection != FileTimeSelection.Explicit || access.Time.Subtick == 0) &&
            (modify.Selection != FileTimeSelection.Explicit || modify.Time.Subtick == 0), GuestError.Unsupported);
    }
    public HostResult<int> SetTimes(string path, VirtualFileTimeUpdate access, VirtualFileTimeUpdate modify, string cwd = "/") => Guard(() =>
    {
        ValidateTimes(access, modify); string p = Resolve(path, cwd);
        if (access.Selection == FileTimeSelection.Omit && modify.Selection == FileTimeSelection.Omit) return Ok(0);
        Writable(); DateTime now = DateTime.UtcNow;
        if (access.Selection != FileTimeSelection.Omit) File.SetLastAccessTimeUtc(HostPath(p), SelectedTime(access, now));
        if (modify.Selection != FileTimeSelection.Omit) File.SetLastWriteTimeUtc(HostPath(p), SelectedTime(modify, now));
        Inode(p).ChangeTicks = now.Ticks;
        return Ok(0);
    });
    private readonly Dictionary<Description, int> locks = new();
    public HostResult<int> AdvisoryLock(int descriptor, int operation) => Guard(() =>
    {
        var d = Get(descriptor); int kind = operation & ~4;
        Require((operation & ~15) == 0 && kind is 1 or 2 or 8, GuestError.Invalid);
        if (kind == 8) { locks.Remove(d); return Ok(0); }
        bool conflict = locks.Any(p => p.Key != d && p.Key.Inode == d.Inode && (kind == 2 || p.Value == 2));
        if (conflict && (operation & 4) == 0) return Fail<int>(GuestError.Unsupported);
        locks.Remove(d); if (conflict) return Fail<int>(GuestError.Again); locks[d] = kind; return Ok(0);
    });
    public HostResult<int> MakeDirectory(string path, uint mode, string cwd = "/") => Guard(() =>
    {
        Writable(); Require((mode & ~0x1ffu) == 0 && (!OperatingSystem.IsWindows() || mode == 0x1ed), GuestError.Unsupported); string p = Resolve(path.TrimEnd('/'), cwd, true);
        Require(!File.Exists(HostPath(p)) && !Directory.Exists(HostPath(p)), GuestError.Exists); var i = Inventory();
        Require(i.Nodes < nodeLimit && Encoding.UTF8.GetByteCount(p) <= pathBytesLimit - i.PathBytes, GuestError.NoSpace);
        Directory.CreateDirectory(HostPath(p)); if (SupportsPersistentUnixModes) ApplyMode(p, mode); return Ok(0);
    });
    private static void FinalComponent(string path) => Require(path.TrimEnd('/').Split('/').LastOrDefault() is not ("." or ".." or ""), GuestError.Invalid);
    public HostResult<int> Unlink(string path, bool removeDirectory = false, string cwd = "/", string protectedCwd = "/") => Guard(() =>
    {
        Writable(); FinalComponent(path); string p = Resolve(path, cwd); Require(p != "/" && p != protectedCwd, GuestError.Busy);
        bool directory = Directory.Exists(HostPath(p)); Require(directory == removeDirectory, directory ? GuestError.IsDirectory : GuestError.NotDirectory);
        var node = Inode(p); var metadata = node.OpenDescriptions.Count == 0 ? default : Metadata(p);
        if (directory) Directory.Delete(HostPath(p)); else File.Delete(HostPath(p));
        Detach(node, metadata);
        inodes.Remove(p); return Ok(0);
    });
    public HostResult<string> Rename(string oldPath, string newPath, string oldCwd = "/", string newCwd = "/", string protectedCwd = "/") => Guard(() =>
    {
        Writable(); FinalComponent(oldPath); FinalComponent(newPath);
        string from = Resolve(oldPath, oldCwd), to = Resolve(newPath, newCwd, true);
        if (from == to) return Ok(protectedCwd);
        Require(from != "/" && to != "/" && to != protectedCwd, GuestError.Busy);
        bool directory = Directory.Exists(HostPath(from)); Require(!directory || !to.StartsWith(from + "/", StringComparison.Ordinal), GuestError.Invalid);
        var i = Inventory(); long growth = Math.Max(0, Encoding.UTF8.GetByteCount(to) - Encoding.UTF8.GetByteCount(from));
        Require(growth == 0 || growth <= (pathBytesLimit - i.PathBytes) / i.Nodes, GuestError.NoSpace);
        inodes.TryGetValue(to, out var replaced);
        var replacedMetadata = replaced?.OpenDescriptions.Count > 0 ? Metadata(to) : default;
        if (directory)
        {
            // BCL has no cross-platform atomic replacement of an existing
            // directory. Reject that case rather than delete then rename.
            Require(!Directory.Exists(HostPath(to)) && !File.Exists(HostPath(to)), GuestError.Unsupported);
            Directory.Move(HostPath(from), HostPath(to));
        }
        else { Require(!Directory.Exists(HostPath(to)), GuestError.IsDirectory); File.Move(HostPath(from), HostPath(to), true); }
        if (replaced != null) Detach(replaced, replacedMetadata);
        inodes.Remove(to);
        foreach (var pair in inodes.Where(p => p.Key == from || p.Key.StartsWith(from + "/", StringComparison.Ordinal)).ToArray())
        { inodes.Remove(pair.Key); pair.Value.Path = to + pair.Key[from.Length..]; inodes[pair.Value.Path] = pair.Value; }
        Inode(to).ChangeTicks = DateTime.UtcNow.Ticks;
        return Ok(protectedCwd == from || protectedCwd.StartsWith(from + "/", StringComparison.Ordinal) ? to + protectedCwd[from.Length..] : protectedCwd);
    });
    public HostResult<int> RejectSpecialNode(string path, string cwd = "/") => Guard(() =>
    { string p = Resolve(path, cwd, true); return Fail<int>(File.Exists(HostPath(p)) || Directory.Exists(HostPath(p)) ? GuestError.Exists : GuestError.Unsupported); });
    public HostResult<VirtualFileSystemCapacity> Capacity(string path, string cwd = "/") => Guard(() =>
    { Resolve(path, cwd); var i = Inventory(); return Ok(new VirtualFileSystemCapacity((ulong)writableLimit, (ulong)(writableLimit - i.Bytes), (ulong)nodeLimit, (ulong)(nodeLimit - i.Nodes))); });
    public HostResult<VirtualFileSystemCapacity> Capacity(int descriptor) => Guard(() => { Get(descriptor); return Capacity("/"); });
    public void Dispose()
    { lock (gate) { if (disposed) return; disposed = true; foreach (var d in descriptions.Values.Distinct()) d.Stream?.Dispose(); descriptions.Clear(); locks.Clear(); detachedInodes.Clear(); inodes.Clear(); } }
}
