using Managed.PGlite;
using Managed.PGlite.ProductTests;

if (args.Length != 3)
    throw new ArgumentException("Usage: ApiProtocolEngine <fresh-data-directory> <existing-bootstrap-prefix> <private-diagnostics-directory>");
Console.WriteLine("EARLY INTEGRATION: authored API with prebuilt canonical translated engine; final regenerated matrix is separate.");
var options = new PgliteOptions(args[0], args[1], args[2]);
int sharedAssertions = PgliteApiCases.Run(options);
CoreProtocolCases.ClosePending(options with { DataDirectory = options.DataDirectory + "-api" });
Console.WriteLine($"PASS early integration: {sharedAssertions} shared API assertions and {CoreProtocolCases.Assertions} additional core assertions.");
