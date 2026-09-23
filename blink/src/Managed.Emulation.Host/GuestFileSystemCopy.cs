namespace Managed.Emulation.Host;

public readonly record struct FileSystemCopyResult(long Bytes, int Files, int Directories);

/// <summary>Bounded eager copy for frozen COW imports and explicit exports.
/// Failure may leave a partial destination; callers discard a new private
/// destination or report partial export, never overwrite existing files.</summary>
public static class GuestFileSystemCopy
{
    private static T Value<T>(HostResult<T> result) => result.Succeeded ? result.Value
        : throw new IOException("Filesystem copy failed: " + result.Error);
    public static FileSystemCopyResult Copy(IGuestFileSystem source, IGuestFileSystem destination,
        long byteLimit, int nodeLimit = 1024)
    {
        ArgumentNullException.ThrowIfNull(source); ArgumentNullException.ThrowIfNull(destination);
        if (ReferenceEquals(source, destination)) throw new ArgumentException("Copy stores must differ.");
        if (byteLimit < 0 || nodeLimit < 1) throw new ArgumentOutOfRangeException(nameof(byteLimit));
        long bytes = 0; int files = 0, directories = 0;
        byte[] buffer = new byte[65536];
        var pending = new Stack<string>(); pending.Push("/");
        var modes = new List<(string Path, uint Mode)>();
        while (pending.Count != 0)
        {
            string parent = pending.Pop();
            int directory = Value(source.Open(parent, FileAccessMode.Read, allowDirectory: true, requireDirectory: true));
            VirtualDirectoryEntry[] entries;
            try { entries = Value(source.DirectorySnapshot(directory, nodeLimit + 2, checked(nodeLimit * 256 + 5))); }
            finally { source.Close(directory); }
            foreach (var entry in entries)
            {
                if (entry.Name is "." or "..") continue;
                if (files + directories >= nodeLimit) throw new IOException("Copy node quota exceeded.");
                string path = parent.TrimEnd('/') + "/" + entry.Name;
                var stat = Value(source.Stat(path));
                if (stat.Directory)
                {
                    Value(destination.MakeDirectory(path, 0x1ed)); ++directories;
                    modes.Add((path, stat.Mode & 0x1ff)); pending.Push(path); continue;
                }
                if ((stat.Mode & 0xf000) != 0x8000 || stat.Links > 1) throw new IOException("Only singly-linked regular files and directories can be imported.");
                if (stat.Length < 0 || stat.Length > byteLimit - bytes) throw new IOException("Copy byte quota exceeded.");
                int input = Value(source.Open(path, FileAccessMode.Read));
                try
                {
                    var before = Value(source.FStat(input));
                    if (before.Length != stat.Length || before.Inode != stat.Inode) throw new IOException("Import source changed before opening.");
                    int output = Value(destination.Open(path, FileAccessMode.Write, create: true, exclusive: true));
                    try
                    {
                        long copied = 0;
                        while (copied < before.Length)
                        {
                            int count = Value(source.Read(input, buffer.AsSpan(0, (int)Math.Min(buffer.Length, before.Length - copied))));
                            if (count == 0) throw new IOException("Import source shortened while copying.");
                            int written = 0;
                            while (written < count)
                            {
                                int n = Value(destination.Write(output, buffer.AsSpan(written, count - written)));
                                if (n == 0) throw new IOException("Copy destination made no progress.");
                                written += n;
                            }
                            copied += count;
                        }
                        if (Value(source.Read(input, buffer.AsSpan(0, 1))) != 0) throw new IOException("Import source grew while copying.");
                        var after = Value(source.FStat(input));
                        if (before.Length != after.Length || before.ModifyTicks != after.ModifyTicks || before.ModifySubtick != after.ModifySubtick)
                            throw new IOException("Import source changed while copying.");
                        if ((Value(destination.FStat(output)).Mode & 0x1ff) != (before.Mode & 0x1ff))
                            Value(destination.ChangeMode(output, before.Mode & 0x1ff));
                        Value(destination.Synchronize(output)); bytes += copied; ++files;
                    }
                    finally { destination.Close(output); }
                }
                finally { source.Close(input); }
            }
        }
        foreach (var item in modes.OrderByDescending(item => item.Path.Length))
            if ((Value(destination.Stat(item.Path)).Mode & 0x1ff) != item.Mode) Value(destination.ChangeMode(item.Path, item.Mode));
        return new(bytes, files, directories);
    }

    /// <summary>Explicitly discard an idle private store's content. The caller
    /// must select private storage; this method must never be used on live grants.</summary>
    public static void Clear(IGuestFileSystem privateStore, int nodeLimit = 1024)
    {
        ArgumentNullException.ThrowIfNull(privateStore);
        var entries = new List<(string Path, bool Directory)>();
        var pending = new Stack<string>(); pending.Push("/");
        while (pending.Count != 0)
        {
            string parent = pending.Pop(); int fd = Value(privateStore.Open(parent, FileAccessMode.Read, allowDirectory: true, requireDirectory: true));
            VirtualDirectoryEntry[] names;
            try { names = Value(privateStore.DirectorySnapshot(fd, nodeLimit + 2, checked(nodeLimit * 256 + 5))); }
            finally { privateStore.Close(fd); }
            foreach (var name in names)
            {
                if (name.Name is "." or "..") continue;
                if (entries.Count == nodeLimit) throw new IOException("Discard node bound exceeded.");
                string path = parent.TrimEnd('/') + "/" + name.Name; bool directory = Value(privateStore.Stat(path)).Directory;
                entries.Add((path, directory)); if (directory) pending.Push(path);
            }
        }
        foreach (var entry in entries.OrderByDescending(e => e.Path.Length)) Value(privateStore.Unlink(entry.Path, entry.Directory));
    }
}
