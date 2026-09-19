using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using DotCC;

if (args.Length != 2)
{
    Console.Error.WriteLine("Usage: TranslationProbe <parse-request.json> <output-directory>");
    return 2;
}
var request = JsonSerializer.Deserialize<Request>(File.ReadAllText(args[0]))!;
var output = Path.GetFullPath(args[1]);
Directory.CreateDirectory(output);
var results = new List<Result>();
foreach (var unit in request.Units)
{
    var clock = Stopwatch.StartNew();
    var capture = new StringWriter();
    var previous = Console.Error;
    string? fragment = null;
    string? failure = null;
    try
    {
        Console.SetError(capture);
        fragment = Compiler.EmitObject(unit, request.Includes, request.Defines);
    }
    catch (Exception ex)
    {
        failure = ex.ToString();
    }
    finally { Console.SetError(previous); }
    var objectPath = Path.Combine(output, Path.GetFileName(unit) + ".cs");
    if (fragment is not null) File.WriteAllText(objectPath, fragment);
    else File.Delete(objectPath);
    var diagnostics = capture.ToString();
    var clean = failure is null && diagnostics.Length == 0;
    results.Add(new(unit, clean, fragment is not null, clock.ElapsedMilliseconds,
        Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(unit))),
        fragment is null ? null : objectPath, diagnostics, failure));
    Console.WriteLine($"{Path.GetFileName(unit)}: {(clean ? "clean" : fragment is not null ? "emitted with diagnostics" : "failed")}");
    File.WriteAllText(Path.Combine(output, "results.json"),
        JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true }) + "\n");
}
Console.WriteLine($"Clean object emission: {results.Count(r => r.Clean)}/{results.Count}");
return results.All(r => r.Clean) ? 0 : 1;

internal sealed record Request(string[] Units, string[] Includes, string[] Defines);
internal sealed record Result(string Unit, bool Clean, bool Emitted, long Milliseconds,
    string SourceSha256, string? ObjectPath, string Diagnostics, string? Failure);
