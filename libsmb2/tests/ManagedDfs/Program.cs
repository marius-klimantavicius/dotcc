using Managed.Smb;

if (args.Length < 1 || args.Skip(1).Any(argument => argument is not ("--resolve-only" or "--encrypt")))
{
    Console.Error.WriteLine("Usage: ManagedDfs UNC_PATH [--resolve-only] [--encrypt]");
    return 2;
}

try
{
    await using var client = SmbDfsClient.CreateKerberosWithExistingCredentials(encrypt: args.Contains("--encrypt"));
    if (args.Contains("--resolve-only"))
    {
        var targets = await client.ResolvePathAsync(args[0]);
        Console.WriteLine($"ManagedDfs: RESOLVED targets={targets.Count}");
        if (Environment.GetEnvironmentVariable("LIBSMB2_TRACE") == "1")
            foreach (var target in targets)
                Console.Error.WriteLine($"target={target.Server}/{target.Share}/{target.RelativePath}");
        return 0;
    }
    var entries = await client.ListAsync(args[0]);
    Console.WriteLine($"ManagedDfs: PASS entries={entries.Count}");
    return 0;
}
catch (Exception error)
{
    Console.Error.WriteLine(Environment.GetEnvironmentVariable("LIBSMB2_TRACE") == "1"
        ? error.ToString() : error.Message);
    return 1;
}
