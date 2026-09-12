using System.Runtime.CompilerServices;
using Managed.Transport.Api;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        string mode = args is ["--malformed"] ? "malformed"
            : args is ["--amplification"] ? "amplification"
            : throw new ArgumentException("Expected --malformed or --amplification");
        await using (var runtime = await QuicRuntime.CreateAsync(new() { ProcessorCount = 1 }))
        {
            string revision = runtime.GetLibrarySourceRevision();
            if (revision.Length != 40 || revision.Any(c => c is not (>= '0' and <= '9' or >= 'a' and <= 'f')) ||
                revision != Environment.GetEnvironmentVariable("DOTCC_REQUIRED_SOURCE_REVISION") ||
                runtime.GetTlsProvider() != QuicTlsProvider.Picotls)
                throw new InvalidOperationException("Actual endpoint library metadata differs from the qualified pin/provider");
            Console.WriteLine($"EVIDENCE endpoint metadata {{\"aot\":{(!RuntimeFeature.IsDynamicCodeSupported ? "true" : "false")},\"source_revision\":\"{revision}\",\"provider\":\"picotls\"}}");
        }
        if (mode == "malformed") await MalformedInputs.RunAsync();
        else await AmplificationControls.RunAsync();
        Console.WriteLine($"PASS endpoint-controls mode={mode}");
    }
}
