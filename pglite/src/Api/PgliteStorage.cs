// Manual .NET adaptation of @electric-sql/pglite src/fs/tarUtils.ts and src/pglite.ts (Apache-2.0).
using System.Formats.Tar;
using System.IO.Compression;

namespace Managed.PGlite;

/// <summary>A host-directory storage descriptor. Temporary storage uses real
/// files and deletes its owned directory only after successful engine shutdown.</summary>
public sealed class PgliteStorage : IDisposable
{
    private readonly object gate = new();
    private readonly string? ownedRoot;
    private PgliteDatabase? owner;
    private bool disposed;
    public string DataDirectory { get; }
    public bool IsTemporary => ownedRoot != null;
    private PgliteStorage(string directory, string? root)
    { DataDirectory = Path.TrimEndingDirectorySeparator(Path.GetFullPath(directory)); ownedRoot = root; }
    public static PgliteStorage Persistent(string directory)
    { ArgumentException.ThrowIfNullOrEmpty(directory); return new(directory, null); }
    public static PgliteStorage Temporary(string? parentDirectory = null)
    {
        string parent = Path.GetFullPath(parentDirectory ?? Path.GetTempPath());
        Directory.CreateDirectory(parent);
        string root = Path.Combine(parent, "dotcc-pglite-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return new(Path.Combine(root, "data"), root);
    }
    internal void RequireAvailable()
    {
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (owner != null) throw new InvalidOperationException("This storage descriptor already has an active database owner.");
        }
    }
    internal void Attach(PgliteDatabase database)
    { lock (gate) { RequireAvailable(); owner = database; } }
    internal void Detach(PgliteDatabase database)
    {
        lock (gate)
        {
            if (!ReferenceEquals(owner, database)) return;
            owner = null;
            if (ownedRoot != null) Dispose();
        }
    }
    public void Dispose()
    {
        lock (gate)
        {
            if (disposed) return;
            if (owner != null) throw new InvalidOperationException("Close the database before disposing its storage.");
            if (ownedRoot != null && Directory.Exists(ownedRoot))
            {
                using (new FileStream(DataDirectory + ".dotcc-owner", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)) { }
                Directory.Delete(ownedRoot, recursive: true);
            }
            disposed = true;
        }
    }
}

public enum PgliteDumpCompression { Auto, None, Gzip }
public enum PgliteCopyFormat { Text, Csv, Binary }
public sealed record PgliteCopyResult(PgliteQueryResult Result, byte[] Data);

public sealed unsafe partial class PgliteDatabase
{
    private bool serviceSnapshotting;

    /// <summary>Run a real PostgreSQL checkpoint. Host directory storage needs no
    /// browser filesystem sync. This call requires an idle transaction state.</summary>
    public void SyncToFs()
    {
        RequireServiceIdle();
        Query("CHECKPOINT").EnsureSuccess();
    }

    /// <summary>Checkpoint, then capture the quiescent single-owner cluster and
    /// WAL as a tar archive. It restores on the same engine/platform disk format;
    /// this is not a portable logical pg_dump or an online multi-process backup.</summary>
    public byte[] DumpDataDir(PgliteDumpCompression compression = PgliteDumpCompression.Auto)
    {
        using var output = new MemoryStream();
        DumpDataDir(output, compression);
        return output.ToArray();
    }

    public void DumpDataDir(Stream destination, PgliteDumpCompression compression = PgliteDumpCompression.Auto)
    {
        ArgumentNullException.ThrowIfNull(destination);
        if (!destination.CanWrite) throw new ArgumentException("Archive destination must be writable.", nameof(destination));
        if (!Enum.IsDefined(compression)) throw new ArgumentOutOfRangeException(nameof(compression));
        RequireServiceIdle();
        serviceBatchDepth++;
        try
        {
            Query("CHECKPOINT").EnsureSuccess();
            serviceSnapshotting = true;
            using var zipped = compression != PgliteDumpCompression.None
                ? new GZipStream(destination, CompressionLevel.Optimal, leaveOpen: true) : null;
            using var tar = new TarWriter(zipped ?? destination, TarEntryFormat.Pax, leaveOpen: true);
            WriteArchiveDirectory(tar, options.DataDirectory, options.DataDirectory);
        }
        finally
        {
            serviceSnapshotting = false;
            serviceBatchDepth--;
        }
        DrainServiceResponses();
    }

    private static void WriteArchiveDirectory(TarWriter tar, string root, string directory)
    {
        foreach (string path in Directory.EnumerateFileSystemEntries(directory).Order(StringComparer.Ordinal))
        {
            var attributes = File.GetAttributes(path);
            if ((attributes & FileAttributes.ReparsePoint) != 0)
                throw new NotSupportedException("Cluster archives cannot include symbolic links or external tablespaces: " + path);
            string name = Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/');
            if (name is "postmaster.pid" or "postmaster.opts") continue;
            bool isDirectory = (attributes & FileAttributes.Directory) != 0;
            var entry = new PaxTarEntry(isDirectory ? TarEntryType.Directory : TarEntryType.RegularFile, name)
            {
                Mode = isDirectory ? UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute
                    : UnixFileMode.UserRead | UnixFileMode.UserWrite,
                ModificationTime = File.GetLastWriteTimeUtc(path)
            };
            if (isDirectory)
            {
                tar.WriteEntry(entry);
                WriteArchiveDirectory(tar, root, path);
            }
            else
            {
                using var input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                entry.DataStream = input;
                tar.WriteEntry(entry);
            }
        }
    }

    /// <summary>Extract to a private staging directory, reject links/traversal and
    /// install only a complete archive. Restore requires a new or empty target.</summary>
    public static void RestoreDataDirectory(Stream source, string dataDirectory,
        PgliteDumpCompression compression = PgliteDumpCompression.Auto,
        long maximumExtractedBytes = 16L * 1024 * 1024 * 1024)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrEmpty(dataDirectory);
        if (!source.CanRead) throw new ArgumentException("Archive source must be readable.", nameof(source));
        if (!Enum.IsDefined(compression)) throw new ArgumentOutOfRangeException(nameof(compression));
        if (maximumExtractedBytes <= 0) throw new ArgumentOutOfRangeException(nameof(maximumExtractedBytes));
        string target = Path.TrimEndingDirectorySeparator(Path.GetFullPath(dataDirectory));
        string parent = Path.GetDirectoryName(target) ?? throw new ArgumentException("Cannot restore to a filesystem root.");
        Directory.CreateDirectory(parent);
        // The same exclusion file used by the engine prevents restoring over an active owner.
        using var exclusion = new FileStream(target + ".dotcc-owner", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        if (File.Exists(target) || (Directory.Exists(target) && Directory.EnumerateFileSystemEntries(target).Any()))
            throw new IOException("Restore requires a new or empty data directory.");
        string staging = Path.Combine(parent, ".pglite-restore-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(staging);
        try
        {
            int first = source.ReadByte(), second = source.ReadByte();
            using var prefixed = new PrefixReadStream(source, first, second);
            bool gzip = compression == PgliteDumpCompression.Gzip ||
                compression == PgliteDumpCompression.Auto && first == 0x1f && second == 0x8b;
            using var unzipped = gzip ? new GZipStream(prefixed, CompressionMode.Decompress, leaveOpen: true) : null;
            using var reader = new TarReader((Stream?)unzipped ?? prefixed, leaveOpen: true);
            var entries = new HashSet<string>(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
            long total = 0;
            int count = 0;
            TarEntry? entry;
            while ((entry = reader.GetNextEntry()) != null)
            {
                if (++count > 1_000_000) throw new InvalidDataException("Archive has too many entries.");
                string name = entry.Name.TrimEnd('/');
                if (name.Length == 0 || name.Length > 4096 || name.StartsWith('/') || name.Contains('\\') || name.Contains(':') ||
                    name.Contains('\0') || name.Split('/').Any(part => part is "" or "." or ".."))
                    throw new InvalidDataException("Archive contains an unsafe entry path.");
                if (!entries.Add(name)) throw new InvalidDataException("Archive contains duplicate paths.");
                if (entry.EntryType is not (TarEntryType.RegularFile or TarEntryType.V7RegularFile or TarEntryType.Directory))
                    throw new InvalidDataException("Archive links, devices and special files are unsupported.");
                if (entry.Length < 0 || entry.Length > maximumExtractedBytes - total)
                    throw new InvalidDataException("Archive exceeds the extraction limit.");
                total += entry.Length;
                string path = Path.Combine(staging, name.Replace('/', Path.DirectorySeparatorChar));
                if (entry.EntryType == TarEntryType.Directory) Directory.CreateDirectory(path);
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                    using var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                    entry.DataStream?.CopyTo(output);
                    if (output.Length != entry.Length) throw new InvalidDataException("Truncated archive entry.");
                }
                if (entry.EntryType != TarEntryType.Directory) File.SetLastWriteTimeUtc(path, entry.ModificationTime.UtcDateTime);
                if (!OperatingSystem.IsWindows())
                    File.SetUnixFileMode(path, entry.EntryType == TarEntryType.Directory
                        ? UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute
                        : UnixFileMode.UserRead | UnixFileMode.UserWrite);
            }
            if (!File.Exists(Path.Combine(staging, "PG_VERSION")) || !File.Exists(Path.Combine(staging, "global/pg_control")))
                throw new InvalidDataException("Archive does not contain an initialized PostgreSQL cluster.");
            if (!OperatingSystem.IsWindows()) File.SetUnixFileMode(staging,
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
            if (Directory.Exists(target)) Directory.Delete(target);
            Directory.Move(staging, target);
        }
        finally { if (Directory.Exists(staging)) Directory.Delete(staging, recursive: true); }
    }

    public static PgliteDatabase Restore(PgliteOptions options, Stream source,
        PgliteDumpCompression compression = PgliteDumpCompression.Auto)
    {
        ArgumentNullException.ThrowIfNull(options);
        var normalized = options.Normalize();
        RestoreDataDirectory(source, normalized.DataDirectory, compression);
        return Open(normalized);
    }

    /// <summary>The callback receives an escaped SQL file literal for a private
    /// owned transport file, e.g. path => $"COPY my_table TO {path} WITH (FORMAT csv)".
    /// PostgreSQL itself performs COPY and reports conversion/SQL failures.</summary>
    public PgliteCopyResult CopyTo(Func<string, string> copySql)
    {
        using var output = new MemoryStream();
        var result = CopyTo(copySql, output);
        return new(result, output.ToArray());
    }

    public PgliteQueryResult CopyTo(Func<string, string> copySql, Stream destination)
    {
        ArgumentNullException.ThrowIfNull(copySql); ArgumentNullException.ThrowIfNull(destination);
        if (!destination.CanWrite) throw new ArgumentException("COPY destination must be writable.", nameof(destination));
        RequireOwner();
        string path = Path.Combine(diagnostics, "copy-" + Guid.NewGuid().ToString("N"));
        serviceBatchDepth++;
        PgliteQueryResult result;
        try
        {
            result = Query(copySql(SqlFileLiteral(path))).EnsureSuccess();
            RequireCopyResult(result);
            using var input = File.OpenRead(path);
            input.CopyTo(destination);
        }
        finally { File.Delete(path); serviceBatchDepth--; }
        DrainServiceResponses();
        return result;
    }

    public PgliteQueryResult CopyFrom(Func<string, string> copySql, byte[] data)
    { ArgumentNullException.ThrowIfNull(data); using var input = new MemoryStream(data, writable: false); return CopyFrom(copySql, input); }

    public PgliteQueryResult CopyFrom(Func<string, string> copySql, Stream source)
    {
        ArgumentNullException.ThrowIfNull(copySql); ArgumentNullException.ThrowIfNull(source);
        if (!source.CanRead) throw new ArgumentException("COPY source must be readable.", nameof(source));
        RequireOwner();
        string path = Path.Combine(diagnostics, "copy-" + Guid.NewGuid().ToString("N"));
        serviceBatchDepth++;
        PgliteQueryResult result;
        try
        {
            using (var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None)) source.CopyTo(output);
            result = Query(copySql(SqlFileLiteral(path))).EnsureSuccess();
            RequireCopyResult(result);
        }
        finally { File.Delete(path); serviceBatchDepth--; }
        DrainServiceResponses();
        return result;
    }

    private static string SqlFileLiteral(string path) => "E'" + path.Replace("\\", "\\\\").Replace("'", "''") + "'";
    private static void RequireCopyResult(PgliteQueryResult result)
    {
        if (result.ResultSets.Count != 1 || !result.ResultSets[0].Command.StartsWith("COPY ", StringComparison.Ordinal))
            throw new InvalidOperationException("The transport SQL must execute exactly one COPY command.");
    }
    private void RequireServiceIdle()
    {
        RequireOwner();
        if (serviceDeliveringCallbacks || serviceSnapshotting) throw new InvalidOperationException("A service callback or snapshot is active.");
        if (serviceTransaction != null || ReadServiceTransactionStatus() != PgliteTransactionStatus.Idle)
            throw new InvalidOperationException("This operation requires an idle database outside a transaction.");
    }

    private sealed class PrefixReadStream(Stream source, int first, int second) : Stream
    {
        private readonly byte[] prefix = first < 0 ? [] : second < 0 ? [(byte)first] : [(byte)first, (byte)second];
        private int offset;
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override int Read(byte[] buffer, int start, int count) => Read(buffer.AsSpan(start, count));
        public override int Read(Span<byte> buffer)
        {
            if (offset < prefix.Length)
            {
                int count = Math.Min(buffer.Length, prefix.Length - offset);
                prefix.AsSpan(offset, count).CopyTo(buffer); offset += count; return count;
            }
            return source.Read(buffer);
        }
        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
