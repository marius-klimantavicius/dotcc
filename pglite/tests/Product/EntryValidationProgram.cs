using Managed.PGlite;

// Runs against the actual owning assembly and real translated references.
// These negative entry contracts require no cluster or SQL substitute.
if (args.Length != 2) throw new ArgumentException("Expected actual-bootstrap-prefix private-work-parent.");
string work = Path.Combine(Path.GetFullPath(args[1]), "entry-validation-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(work);
int assertions = 0;
void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
    assertions++;
}
T Reject<T>(Action action, string message) where T : Exception
{
    try { action(); }
    catch (Exception error)
    {
        if (error is T expected) { assertions++; return expected; }
        throw new InvalidOperationException(message, error);
    }
    throw new InvalidOperationException("Accepted " + message);
}
void CheckLeaseReleased(string data)
{
    using var lease = new FileStream(data + ".dotcc-owner", FileMode.Open,
        FileAccess.ReadWrite, FileShare.None);
    Check(lease.CanWrite, "Rejected entry retained its directory lease.");
}
try
{
    var options = new PgliteOptions(Path.Combine(work, "absent-data"), args[0], Path.Combine(work, "diagnostics"));
    Reject<ArgumentNullException>(() => PgliteDatabase.Create(null!), "null Create options");
    Reject<ArgumentNullException>(() => PgliteDatabase.Open(null!), "null Open options");
    var missing = Reject<IOException>(() => PgliteDatabase.Open(options), "Open without a real initialized cluster");
    Check(missing.Message.Contains("Open requires a cluster", StringComparison.Ordinal), "Missing-cluster failure changed to a lingering owner error.");
    Check(!Directory.Exists(options.DataDirectory), "Rejected Open created a data directory.");
    CheckLeaseReleased(options.DataDirectory);

    string nonempty = Path.Combine(work, "caller-data");
    Directory.CreateDirectory(nonempty);
    string sentinel = Path.Combine(nonempty, "caller-owned.txt");
    File.WriteAllText(sentinel, "Caller bytes Žąsis must survive rejected Create.");
    var existing = options with { DataDirectory = nonempty };
    var rejected = Reject<IOException>(() => PgliteDatabase.Create(existing), "Create on a nonempty caller directory");
    Check(rejected.Message.Contains("Create requires a new or empty", StringComparison.Ordinal), "Rejected Create did not reach its expected pre-core validation.");
    Check(File.ReadAllText(sentinel) == "Caller bytes Žąsis must survive rejected Create.", "Rejected Create altered caller-owned data.");
    Check(Directory.EnumerateFileSystemEntries(nonempty).Count() == 1, "Rejected Create added cluster files to caller data.");
    CheckLeaseReleased(nonempty);

    // Several failed entries in one process must release both process and file
    // exclusion. A second ordinary error must not become an owner-conflict error.
    for (int attempt = 0; attempt < 3; attempt++)
    {
        var next = options with { DataDirectory = Path.Combine(work, "absent-" + attempt) };
        var error = Reject<IOException>(() => PgliteDatabase.Open(next), "Open after rejected lifetime " + attempt);
        Check(error.Message.Contains("Open requires a cluster", StringComparison.Ordinal), "A failed entry poisoned process ownership.");
        CheckLeaseReleased(next.DataDirectory);
    }
    var mismatched = options with { DataDirectory = Path.Combine(work, "invalid-path-data"),
        BackendExecutable = Path.Combine(work, "different-postgres") };
    var invalid = Reject<ArgumentException>(() => PgliteDatabase.Create(mismatched), "mismatched backend before acquisition");
    Check(invalid.ParamName == nameof(PgliteOptions.BackendExecutable), "Actual API did not apply the executable normalization contract.");
    Check(!File.Exists(mismatched.DataDirectory + ".dotcc-owner"), "Invalid options acquired a directory lease.");
    Console.WriteLine($"PASS {assertions} actual API entry/lease/caller-data assertions; no SQL execution.");
    return 0;
}
finally { Directory.Delete(work, true); }
