using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

namespace Managed.PGlite;

/// <summary>Meaningful native embedding settings. Engine/initializer executable
/// paths and JavaScript/WASM memory/module choices are internal to the product.</summary>
public sealed record PgliteConfiguration
{
    /// <summary>Bootstrap superuser name when creating a cluster. Open uses the
    /// cluster's existing bootstrap superuser; it does not perform network login.</summary>
    public string Username { get; init; } = "postgres";
    /// <summary>An existing database, including template1 or postgres on a new
    /// cluster. Create a custom database with SQL before reopening it by name.</summary>
    public string Database { get; init; } = "template1";
    public int Debug { get; init; }
    /// <summary>Allow commits to return before WAL is durably synchronized.
    /// SyncToFs, checkpoint, and clean close provide explicit durability fences.</summary>
    public bool RelaxedDurability { get; init; }
    /// <summary>PostgreSQL settings passed as individual -c arguments. Ownership
    /// paths, module loading, and multiprocess execution remain product-owned.</summary>
    public IReadOnlyDictionary<string, string> Settings { get; init; }
        = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());

    internal PgliteConfiguration Normalize()
    {
        ArgumentException.ThrowIfNullOrEmpty(Username);
        ArgumentException.ThrowIfNullOrEmpty(Database);
        if (Username.Contains('\0') || Database.Contains('\0') || Database[0] == '-' ||
            Encoding.UTF8.GetByteCount(Username) > 63 || Encoding.UTF8.GetByteCount(Database) > 63)
            throw new ArgumentException("User/database names must fit PostgreSQL identifiers and cannot contain NUL or option prefixes.");
        if (Debug is < 0 or > 5) throw new ArgumentOutOfRangeException(nameof(Debug));
        ArgumentNullException.ThrowIfNull(Settings);
        var settings = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (key, value) in Settings)
        {
            if (string.IsNullOrEmpty(key) || key.Any(c => !(char.IsAsciiLetterOrDigit(c) || c is '_' or '.')) || key.Contains('.'))
                throw new ArgumentException("Settings must use built-in PostgreSQL setting names.", nameof(Settings));
            ArgumentNullException.ThrowIfNull(value);
            if (value.Contains('\0')) throw new ArgumentException("Setting values cannot contain NUL.", nameof(Settings));
            string name = key.ToLowerInvariant();
            if (name is "data_directory" or "config_file" or "hba_file" or "ident_file" or "external_pid_file" or
                "dynamic_library_path" or "shared_preload_libraries" or "session_preload_libraries" or "local_preload_libraries" or
                "autovacuum" or "io_method" or "max_worker_processes" or "max_parallel_workers" or
                "max_parallel_workers_per_gather" or "max_parallel_maintenance_workers" or "client_encoding")
                throw new ArgumentException($"Setting '{name}' belongs to the translated embedding profile.", nameof(Settings));
            settings.Add(name, value);
        }
        return this with { Settings = new ReadOnlyDictionary<string, string>(settings) };
    }

    internal string[] BackendArguments(PgliteOptions options)
    {
        var args = new List<string>
        {
            Path.Combine(options.BootstrapPrefix, "bin/postgres"), "--single", "-D", options.DataDirectory,
            "-c", "shared_buffers=16MB", "-c", "max_connections=10", "-c", "autovacuum=off",
            "-c", "max_worker_processes=0", "-c", "max_parallel_workers=0",
            "-c", "max_parallel_workers_per_gather=0", "-c", "max_parallel_maintenance_workers=0",
            "-c", "io_method=sync", "-c", "client_encoding=UTF8",
            "-c", "log_min_messages=" + (Debug == 0 ? "warning" : "debug" + Debug.ToString(CultureInfo.InvariantCulture)),
            "-c", "dynamic_library_path=" + Path.Combine(options.BootstrapPrefix, "lib/postgresql")
        };
        if (RelaxedDurability) { args.Add("-c"); args.Add("synchronous_commit=off"); }
        foreach (var (key, value) in Settings) { args.Add("-c"); args.Add(key + "=" + value); }
        args.Add(Database);
        return args.ToArray();
    }
}
