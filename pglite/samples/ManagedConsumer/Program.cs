using Managed.PGlite;
using Managed.PGlite.ProductTests;

try
{
    var arguments = new Dictionary<string, string>(StringComparer.Ordinal);
    for (int i = 0; i < args.Length; i += 2)
    {
        if (i + 1 == args.Length || args[i] is not ("--bootstrap-prefix" or "--backend-executable" or "--work" or "--cases") ||
            !arguments.TryAdd(args[i], args[i + 1]))
            throw new ArgumentException("Expected --bootstrap-prefix PATH [--backend-executable PATH] [--work PATH] [--cases PATH].");
    }
    string prefix = arguments.GetValueOrDefault("--bootstrap-prefix", Path.Combine(AppContext.BaseDirectory, "bootstrap-prefix"));
    string parent = arguments.GetValueOrDefault("--work", Path.Combine(Path.GetTempPath(), "dotcc-pglite-consumer"));
    string work = Path.Combine(Path.GetFullPath(parent), "run-" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(work);
    string inventory = arguments.GetValueOrDefault("--cases", Path.Combine(AppContext.BaseDirectory, "cases.json"));
    var options = new PgliteOptions(Path.Combine(work, "data"), prefix, Path.Combine(work, "diagnostics"))
    { BackendExecutable = arguments.GetValueOrDefault("--backend-executable") };
    Console.WriteLine($"Translated embedded PostgreSQL workload; private files: {work}");
    using var cases = SqlCases.Load(inventory);
    int assertions = 0, lifetimes = 0;
    PgliteQueryResult? retained = null;
    foreach (string group in SqlCases.Groups)
    {
        using (var database = lifetimes == 0 ? PgliteDatabase.Create(options) : PgliteDatabase.Open(options))
        {
            if (!database.IsReady) throw new InvalidOperationException("Original backend failed readiness.");
            foreach (var test in cases.Cases(group).EnumerateArray())
            {
                // The callback target and argv remain rooted across collections.
                GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
                var result = database.Query(SqlCases.Sql(test));
                SqlCases.Assert(test, result);
                if (SqlCases.Name(test) == "query-values") retained = result;
                Console.WriteLine($"PASS {group}/{SqlCases.Name(test)}");
                assertions++;
            }
            var sqlError = database.Query("SELECT 1 / 0");
            if (sqlError.Errors.Count != 1 || sqlError.Errors[0].SqlState != "22012" ||
                sqlError.TransactionStatus != PgliteTransactionStatus.Idle)
                throw new InvalidOperationException("Division-by-zero recovery returned unexpected state.");
            var recovered = database.Query("SELECT 73, 'Žąsis', NULL::text").EnsureSuccess();
            var row = recovered.ResultSets.Single().Rows.Single();
            if (!row.SequenceEqual(new string?[] { "73", "Žąsis", null }))
                throw new InvalidOperationException("Ordinary SQL error recovery did not return exact values.");
            assertions += 2;
            try { database.Query("SELECT 1\0"); throw new InvalidOperationException("NUL SQL was accepted."); }
            catch (ArgumentException) { if (!database.IsReady) throw; assertions++; }
            bool wrongThreadRejected = Task.Run(() =>
            {
                try { database.Query("SELECT 1"); return false; }
                catch (InvalidOperationException) { return true; }
            }).GetAwaiter().GetResult();
            if (!wrongThreadRejected) throw new InvalidOperationException("Foreign-thread entry was accepted.");
            assertions++;
            try
            {
                using var second = PgliteDatabase.Open(options);
                throw new InvalidOperationException("A simultaneous owner was accepted.");
            }
            catch (InvalidOperationException error) when (error.Message.Contains("One embedded PGlite owner", StringComparison.Ordinal))
            { assertions++; }
        }
        lifetimes++;
        if (retained != null && !retained.ResultSets.Single().Rows[0].SequenceEqual(new string?[] { "1", "Vilnius Žąsis", "11" }))
            throw new InvalidOperationException("Retained response changed after close/reopen.");
    }
    if (assertions != 36 || lifetimes != 3) throw new InvalidOperationException("The required 21 cases and 15 owner/recovery checks did not all execute.");
    Console.WriteLine($"PASS {assertions} exact SQL/ownership assertions; translated initdb; three clean same-process lifetimes; two persistence reopens.");
    PgliteApiCases.Run(options);
    return 0;
}
catch (Exception error)
{
    Console.Error.WriteLine(error);
    return 1;
}
