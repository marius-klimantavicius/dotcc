using System.Text.Json;
using Managed.PGlite;
using Managed.PGlite.Initialization;

// Host-contract checks only. Descriptor files and observations below are
// fixtures; no initializer, backend or SQL engine is executed or substituted.
string root = Path.Combine(Path.GetTempPath(), "dotcc API contracts Žąsis " + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);
int passed = 0;
void Check(bool condition, string description)
{
    if (!condition) throw new InvalidOperationException(description);
    passed++;
}
T Reject<T>(Action action, string description) where T : Exception
{
    try { action(); }
    catch (Exception error)
    {
        if (error is T expected) { passed++; return expected; }
        throw new InvalidOperationException("Unexpected failure while rejecting " + description, error);
    }
    throw new InvalidOperationException("Accepted " + description);
}
string FixturePrefix(string name)
{
    string prefix = Path.Combine(root, name);
    foreach (string relative in new[] { "share/postgresql/postgres.bki", "lib/postgresql/plpgsql", "bin/initdb", "bin/postgres" })
    {
        string file = Path.Combine(prefix, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        File.WriteAllText(file, "Path-validation fixture; no database implementation or contents.");
    }
    return prefix;
}
PgliteInitializationTrace Trace(string name)
{
    string directory = Path.Combine(root, name);
    Directory.CreateDirectory(directory);
    return new(directory);
}
BootstrapInvocation Invocation(BootstrapOperation operation, string[]? arguments = null) =>
    new(operation, arguments ?? ["postgres", "fixture argument"],
        Path.Combine(root, "absent-fixture.stdin"), Path.Combine(root, "absent-fixture.stdout"), false);
try
{
    string prefix = FixturePrefix("selected prefix");
    string backend = Path.Combine(prefix, "bin/postgres");
    var options = new PgliteOptions(Path.Combine(root, "data"), prefix, Path.Combine(root, "diagnostics"));
    Check(options.Normalize().BackendExecutable == backend, "Default must select original initdb's sibling postgres path.");
    var equivalent = options with { BackendExecutable = Path.Combine(prefix, "bin/../bin/postgres") };
    Check(equivalent.Normalize().BackendExecutable == backend, "Equivalent path segments must normalize to the selected backend.");
    equivalent = options with
    {
        BootstrapPrefix = Path.GetRelativePath(Environment.CurrentDirectory, prefix),
        BackendExecutable = Path.GetRelativePath(Environment.CurrentDirectory, backend)
    };
    Check(equivalent.Normalize().BackendExecutable == backend, "Relative paths must normalize to the same selected backend.");
    string alternate = Path.Combine(FixturePrefix("other prefix"), "bin/postgres");
    var invalid = options with { BackendExecutable = alternate };
    var mismatch = Reject<ArgumentException>(() => invalid.Normalize(), "existing backend descriptor under another prefix");
    Check(mismatch.ParamName == nameof(PgliteOptions.BackendExecutable), "Mismatch must identify the unsupported option.");
    invalid = options with { BackendExecutable = Path.Combine(root, "missing-postgres") };
    Reject<ArgumentException>(() => invalid.Normalize(), "mismatching absent backend path");
    invalid = options with { BackendExecutable = Path.Combine(prefix, "bin/POSTGRES") };
    Reject<ArgumentException>(() => invalid.Normalize(), "different spelling from initdb's exact transport path");
    File.Delete(backend);
    Reject<FileNotFoundException>(() => options.Normalize(), "missing prefix-selected backend descriptor");

    var countsOnly = Trace("counts-only");
    countsOnly.Record(1, Invocation(BootstrapOperation.Version), 0, null);
    countsOnly.Record(2, Invocation(BootstrapOperation.Check), 0, null);
    countsOnly.Record(3, Invocation(BootstrapOperation.Check), 0, null);
    Reject<IOException>(countsOnly.RequireCompletedOperations, "three successful commands with no bootstrap or single-user operation");
    foreach (var missing in new[] { BootstrapOperation.Bootstrap, BootstrapOperation.SingleUser })
    {
        var partial = Trace("missing-" + missing);
        var present = missing == BootstrapOperation.Bootstrap ? BootstrapOperation.SingleUser : BootstrapOperation.Bootstrap;
        partial.Record(1, Invocation(BootstrapOperation.Version), 0, null);
        partial.Record(2, Invocation(present), 0, null);
        partial.Record(3, Invocation(BootstrapOperation.Check), 0, null);
        Reject<IOException>(partial.RequireCompletedOperations, "missing required " + missing);
        var failed = Trace("failed-" + missing);
        failed.Record(1, Invocation(present), 0, null);
        failed.Record(2, Invocation(missing), 1, null);
        Reject<IOException>(failed.RequireCompletedOperations, "nonzero required " + missing);
        var threw = Trace("threw-" + missing);
        threw.Record(1, Invocation(present), 0, null);
        threw.Record(2, Invocation(missing), null, new IOException("fixture callback failure"));
        Reject<IOException>(threw.RequireCompletedOperations, "failed required " + missing);
    }
    var complete = Trace("operation-evidence");
    string[] argv = ["postgres", "Žąsis \"quoted\"\nargument"];
    complete.Record(1, Invocation(BootstrapOperation.Bootstrap, argv), 0, null);
    argv[1] = "modified by fixture caller";
    complete.Record(2, Invocation(BootstrapOperation.SingleUser), 0, null);
    complete.RequireCompletedOperations();
    passed++;
    using (var observations = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(root, "operation-evidence/backend-invocations.json"))))
    {
        var rows = observations.RootElement;
        Check(rows.GetArrayLength() == 2, "Every observation must persist before completion validation.");
        Check(rows[0].GetProperty("operation").GetString() == "Bootstrap" &&
              rows[1].GetProperty("operation").GetString() == "SingleUser", "Persist exact operation identities.");
        Check(rows[0].GetProperty("arguments")[1].GetString() == "Žąsis \"quoted\"\nargument", "Freeze argument observations and escape JSON without losing UTF-8 content.");
        Check(rows[0].GetProperty("index").GetInt32() == 1 && rows[1].GetProperty("index").GetInt32() == 2,
              "Keep invocation sequence in diagnostics.");
        Check(rows[0].GetProperty("status").GetInt32() == 0 && rows[0].GetProperty("failure").ValueKind == JsonValueKind.Null,
              "Persist real exit status independently of failures.");
    }
    var failedTrace = Trace("failed-evidence");
    failedTrace.Record(1, Invocation(BootstrapOperation.Bootstrap), null, new IOException("fixture failure Žąsis"));
    using (var observations = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(root, "failed-evidence/backend-invocations.json"))))
    {
        var row = observations.RootElement[0];
        Check(row.GetProperty("status").ValueKind == JsonValueKind.Null &&
              row.GetProperty("failure").GetString()!.Contains("fixture failure Žąsis", StringComparison.Ordinal),
              "Persist a failed invocation without inventing an exit status.");
    }

    foreach (bool recordFailure in new[] { true, false })
    {
        string name = recordFailure ? "record-errors" : "copy-errors";
        var trace = Trace(name);
        string directory = Path.Combine(root, name);
        string source = Path.Combine(root, name + ".stdin");
        File.WriteAllText(source, "Diagnostic spool fixture; no database input.");
        var request = new BootstrapInvocation(BootstrapOperation.Bootstrap, ["postgres", "--boot"],
            source, Path.Combine(root, name + ".stdout"), false);
        if (recordFailure) Directory.CreateDirectory(Path.Combine(directory, "backend-invocations.json"));
        else Directory.CreateDirectory(Path.Combine(directory, "bootstrap-001.stdin"));
        // Colliding directories cause real File.Create/File.Copy failures on
        // ordinary accounts, independent of permissions or administrator mode.
        var coreFailure = new InvalidOperationException("Original backend failure " + name);
        void UnwindBothFailures()
        {
            Exception? failure = null;
            try { throw coreFailure; }
            catch (Exception error) { failure = error; throw; }
            finally { trace.Complete(1, request, null, failure); }
        }
        var both = Reject<AggregateException>(UnwindBothFailures,
            "backend failure replaced by " + name);
        Check(ReferenceEquals(both.InnerException, coreFailure) && both.InnerExceptions.Count == 2 &&
              ReferenceEquals(both.InnerExceptions[0], coreFailure), "Original backend failure must be the first inner cause for " + name);
        Check(both.InnerExceptions[1] is IOException or UnauthorizedAccessException,
              "Retain the actual diagnostic I/O failure for " + name);
        Exception? diagnosticsOnly = null;
        try { trace.Complete(1, request, 0, null); }
        catch (Exception error) { diagnosticsOnly = error; }
        Check(diagnosticsOnly is IOException or UnauthorizedAccessException,
              "Surface diagnostic-only failure directly for " + name);
    }
    var preserved = Trace("core-error-only");
    var original = new InvalidOperationException("Original backend failure without diagnostic failure");
    void UnwindOriginalFailure()
    {
        Exception? coreFailure = null;
        try { throw original; }
        catch (Exception error) { coreFailure = error; throw; }
        finally { preserved.Complete(1, Invocation(BootstrapOperation.Bootstrap), null, coreFailure); }
    }
    var propagated = Reject<InvalidOperationException>(UnwindOriginalFailure, "successful diagnostics replacing a backend failure");
    Check(ReferenceEquals(propagated, original), "Successful diagnostics must preserve the original thrown backend exception instance.");
    Console.WriteLine($"PASS {passed} API path/initialization-evidence assertions; no database or SQL execution.");
    return 0;
}
finally { Directory.Delete(root, true); }
