using Managed.PGlite.Initialization;

string temporary = Path.Combine(Path.GetTempPath(), "dotcc-original-initdb-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(temporary);
try
{
    int assertions = 0;
    foreach (string option in new[] { "--help", "--version", "--invalid-dotcc-option" })
    {
        string input = Path.Combine(temporary, "stdin"), output = Path.Combine(temporary, "stdout"), error = Path.Combine(temporary, "stderr");
        File.WriteAllBytes(input, []);
        var environment = new[] { "PGDATA", "PGCLIENTENCODING", "PGLOCALEDIR", "PGSYSCONFDIR", "LC_ALL", "LC_CTYPE" }
            .ToDictionary(name => name, Environment.GetEnvironmentVariable);
        using (var frontend = new InitializationFrontend("/unavailable/translated/postgres", temporary,
            _ => throw new InvalidOperationException("An option-only invocation cannot enter the backend.")))
        {
            int status = frontend.Run(["/translated/initdb", option], input, output, error);
            if (status != (option == "--invalid-dotcc-option" ? 1 : 0))
                throw new InvalidOperationException("Original initdb exit status: " + status);
            if (frontend.ProcessError != null) throw new InvalidOperationException("Unexpected process callback.", frontend.ProcessError);
            assertions += 2;
            if (environment.Any(pair => Environment.GetEnvironmentVariable(pair.Key) != pair.Value))
                throw new InvalidOperationException("Original initdb environment escaped its invocation.");
            assertions++;
        }
        string text = File.ReadAllText(option == "--invalid-dotcc-option" ? error : output);
        if (!text.Contains(option == "--help" ? "Usage:" : option == "--version" ? "18.6" : "invalid-dotcc-option", StringComparison.Ordinal))
            throw new InvalidOperationException("Missing actual initdb output for " + option + ": " + text);
        assertions++;
        File.Delete(input); File.Delete(output); File.Delete(error);
    }
    if (Directory.EnumerateFileSystemEntries(temporary).Any()) throw new InvalidOperationException("Frontend left owned files open.");
    Console.WriteLine($"PASS {assertions + 1} original translated initdb option/exit/stdio/fresh-owner assertions; initialization unrun");
    return 0;
}
finally { Directory.Delete(temporary, true); }
