using ManagedTests;
using Managed.Security.S2n;

if (args.Length != 6 || args[0] != "--case" || args[2] != "--fixtures" || args[4] != "--source")
{
    Console.Error.WriteLine("ManagedTests --case <name> --fixtures <credentials> --source <s2n-source>");
    Console.Error.WriteLine("Cases: " + string.Join(", ", LifecycleCases.Names));
    return 1;
}
try
{
    await LifecycleCases.RunAsync(args[1], args[3], args[5]);
    var state = S2nRuntime.Inspect();
    if (args[1] == "init-failure-quarantine")
    {
        if (state.References != 0 || state.Failure == null || state.Initializations != 0 || state.FinalCleanups != 0)
            throw new InvalidOperationException("Failed initialization was not quarantined: " + state);
        Console.WriteLine("{\"case\":\"init-failure-quarantine\",\"status\":\"passed\",\"quarantined\":true,\"final_cleanup\":false}");
        return 0;
    }
    if (state.References != 0 || state.Initializations != state.FinalCleanups || state.Failure != null)
        throw new InvalidOperationException("Runtime did not drain: " + state);
    Console.WriteLine("{\"case\":\"" + args[1] + "\",\"status\":\"passed\",\"final_cleanup\":true}");
    return 0;
}
catch (Exception error)
{
    Console.Error.WriteLine(error);
    return 1;
}
