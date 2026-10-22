using System.ComponentModel;

namespace Managed.PGlite;

/// <summary>Private cluster, translated bootstrap assets and diagnostic paths.
/// Persistent directories and owned temporary directories use real host files.</summary>
public sealed record PgliteOptions(string DataDirectory, string BootstrapPrefix, string WorkDirectory)
{
    public PgliteOptions(PgliteStorage storage, string bootstrapPrefix, string workDirectory)
        : this((storage ?? throw new ArgumentNullException(nameof(storage))).DataDirectory, bootstrapPrefix, workDirectory)
    { Storage = storage; }

    /// <summary>Compatibility validation only. The executable is an internal
    /// initdb descriptor and must be BootstrapPrefix/bin/postgres.</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("BootstrapPrefix selects the translated bootstrap descriptors. BackendExecutable is retained only for compatibility.")]
    public string? BackendExecutable { get; init; }
    public int MaximumResponseBytes { get; init; } = 64 * 1024 * 1024;
    public PgliteStorage? Storage { get; init; }
    public IReadOnlyList<PgliteExtension> Extensions { get; init; } = [];
    public Action<PgliteError>? OnNotice { get; init; }
    public PgliteCodecs? Codecs { get; init; }
    public PgliteConfiguration Configuration { get; init; } = new();

    internal PgliteOptions Normalize()
    {
        ArgumentException.ThrowIfNullOrEmpty(DataDirectory);
        ArgumentException.ThrowIfNullOrEmpty(BootstrapPrefix);
        ArgumentException.ThrowIfNullOrEmpty(WorkDirectory);
        if (MaximumResponseBytes <= 0) throw new ArgumentOutOfRangeException(nameof(MaximumResponseBytes));
        ArgumentNullException.ThrowIfNull(Configuration);
        var configuration = Configuration.Normalize();
        string prefix = Path.GetFullPath(BootstrapPrefix);
        string data = Path.TrimEndingDirectorySeparator(Path.GetFullPath(DataDirectory));
        string work = Path.TrimEndingDirectorySeparator(Path.GetFullPath(WorkDirectory));
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (string.Equals(data, work, comparison) || work.StartsWith(data + Path.DirectorySeparatorChar, comparison))
            throw new ArgumentException("WorkDirectory must be outside the PostgreSQL data directory.");
        if (Storage != null && !string.Equals(data, Storage.DataDirectory, comparison))
            throw new ArgumentException("Storage and DataDirectory must identify the same directory.");
        Storage?.RequireAvailable();
        ArgumentNullException.ThrowIfNull(Extensions);
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var extension in Extensions)
        {
            ArgumentNullException.ThrowIfNull(extension);
            extension.Validate();
            if (!names.Add(extension.Name)) throw new ArgumentException($"Duplicate extension namespace '{extension.Name}'.");
        }
        foreach (string relative in new[] { "share/postgresql/postgres.bki", "lib/postgresql/plpgsql",
            "lib/postgresql/dict_snowball", "bin/initdb", "bin/postgres" })
        {
            string path = Path.Combine(prefix, relative);
            if (!File.Exists(path)) throw new FileNotFoundException("Staged translated bootstrap input is required.", path);
        }
        string executable = Path.Combine(prefix, "bin/postgres");
#pragma warning disable CS0618
        if (BackendExecutable != null &&
            !string.Equals(Path.GetFullPath(BackendExecutable), executable, comparison))
            throw new ArgumentException("BackendExecutable must be BootstrapPrefix/bin/postgres, as selected by original initdb.", nameof(BackendExecutable));
        foreach (string path in new[] { prefix, data, work, executable })
            if (path.Contains('\0')) throw new ArgumentException("Paths cannot contain NUL.");
        return this with { DataDirectory = data, BootstrapPrefix = prefix, WorkDirectory = work,
            BackendExecutable = executable, Extensions = Array.AsReadOnly(Extensions.ToArray()), Codecs = Codecs?.Clone(), Configuration = configuration };
#pragma warning restore CS0618
    }
}
