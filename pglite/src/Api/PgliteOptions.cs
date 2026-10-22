namespace Managed.PGlite;

/// <summary>Paths belong to the caller. Closing never deletes the database,
/// bootstrap assets or diagnostics. Use a separate private work directory.</summary>
public sealed record PgliteOptions(string DataDirectory, string BootstrapPrefix, string WorkDirectory)
{
    /// <summary>If supplied, must normalize to BootstrapPrefix/bin/postgres,
    /// which original initdb discovers beside its own prefix/bin/initdb.</summary>
    public string? BackendExecutable { get; init; }
    public int MaximumResponseBytes { get; init; } = 64 * 1024 * 1024;

    internal PgliteOptions Normalize()
    {
        ArgumentException.ThrowIfNullOrEmpty(DataDirectory);
        ArgumentException.ThrowIfNullOrEmpty(BootstrapPrefix);
        ArgumentException.ThrowIfNullOrEmpty(WorkDirectory);
        if (MaximumResponseBytes <= 0) throw new ArgumentOutOfRangeException(nameof(MaximumResponseBytes));
        string prefix = Path.GetFullPath(BootstrapPrefix);
        string data = Path.TrimEndingDirectorySeparator(Path.GetFullPath(DataDirectory));
        string work = Path.TrimEndingDirectorySeparator(Path.GetFullPath(WorkDirectory));
        if (data == work || work.StartsWith(data + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            throw new ArgumentException("WorkDirectory must be outside the PostgreSQL data directory.");
        foreach (string relative in new[] { "share/postgresql/postgres.bki", "lib/postgresql/plpgsql", "bin/initdb", "bin/postgres" })
        {
            string path = Path.Combine(prefix, relative);
            if (!File.Exists(path)) throw new FileNotFoundException("Staged translated bootstrap input is required.", path);
        }
        string executable = Path.Combine(prefix, "bin/postgres");
        if (BackendExecutable != null &&
            !string.Equals(Path.GetFullPath(BackendExecutable), executable, StringComparison.Ordinal))
            throw new ArgumentException("BackendExecutable must be BootstrapPrefix/bin/postgres, as selected by original initdb.", nameof(BackendExecutable));
        foreach (string path in new[] { prefix, data, work, executable })
            if (path.Contains('\0')) throw new ArgumentException("Paths cannot contain NUL.");
        return this with { DataDirectory = data, BootstrapPrefix = prefix, WorkDirectory = work, BackendExecutable = executable };
    }
}
