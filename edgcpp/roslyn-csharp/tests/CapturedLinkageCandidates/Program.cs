using RoslynObj;
using System.Security.Cryptography;
using System.Text.Json.Nodes;

if (args.Length != 2) throw new ArgumentException("Checks CONTROL_ASSEMBLY_OBJECT VARIABLE_SEMANTICS_OBJECT");
var observations = new JsonArray();
foreach (var (path, source, assembly, routine) in new[]
{
    (args[0], "named_routine", "alternate_routine", true),
    (args[1], "variable_assembly_name", "roslyn_assembly_variable", false)
})
{
    var bytes = File.ReadAllBytes(path);
    var hash = Convert.ToHexStringLower(SHA256.HashData(bytes));
    using var obj = RoslynObject.Open(path);
    var model = obj.TranslationUnit.SemanticModel;
    var original = model.FindSymbols(source).Take(17).ToArray();
    if (original.Length != 1) throw new InvalidOperationException("Original source lookup is not unique within its explicit bound.");
    var selected = original[0];
    var retainedAssembly = selected switch
    {
        FunctionSymbol function when routine => function.GnuSupplement?.AssemblyName,
        VariableSymbol variable when !routine => variable.AssemblyName,
        _ => null
    };
    if (retainedAssembly != assembly) throw new InvalidOperationException("Actual captured GNU assembly-name evidence differs.");
    var exact = model.FindCapturedLinkageCandidates(assembly, 1);
    if (exact.Count != 1 || !exact[0].Equals(selected)) throw new InvalidOperationException("Exact override lookup did not preserve current public identity.");
    if (model.FindCapturedLinkageCandidates(source, 1).Any(candidate => candidate.Equals(selected)))
        throw new InvalidOperationException("Source spelling incorrectly bypassed a captured assembly-name override.");
    var current = selected.CurrentName;
    if (current is not null && current != assembly && model.FindCapturedLinkageCandidates(current, 1).Any(candidate => candidate.Equals(selected)))
        throw new InvalidOperationException("Captured current name incorrectly bypassed the assembly-name override.");
    if (Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(path))) != hash) throw new InvalidOperationException("Input changed.");
    observations.Add((JsonNode)new JsonObject
    {
        ["path"] = Path.GetFullPath(path), ["bytes"] = bytes.Length, ["sha256"] = hash,
        ["sourceDiscoveryHint"] = source, ["capturedCurrentName"] = current, ["capturedAssemblyName"] = assembly,
        ["exactCurrentPublicIdentity"] = true, ["sourceSpellingDoesNotOverride"] = true,
        ["currentNameDoesNotOverrideAssemblyName"] = true, ["bound"] = 1,
        ["returnedRows"] = exact.Count, ["overflowSentinelObserved"] = false, ["inputUnchanged"] = true,
        ["readerCommandsIncludingOpen"] = obj.Statistics.Commands, ["readerRowsIncludingOpen"] = obj.Statistics.RowsRead
    });
}
Console.WriteLine(new JsonObject { ["status"] = "passed-genuine-captured-linkage-overrides", ["observations"] = observations }.ToJsonString());
