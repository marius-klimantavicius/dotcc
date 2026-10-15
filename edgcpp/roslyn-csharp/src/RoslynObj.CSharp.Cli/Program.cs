using System.Text.Json;
using RoslynObj;
using RoslynObj.CSharp;

using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, arguments) => { arguments.Cancel = true; cancellation.Cancel(); };
try
{
    if (args.Length < 2 || args[0] is not ("inspect" or "inventory" or "translate"))
    {
        Console.Error.WriteLine("Usage: roslynobj-csharp inspect INPUT [--report FILE] | inventory MANIFEST --report FILE [--metrics FILE] | translate MANIFEST [--report FILE] [--metrics FILE]");
        return 2;
    }
    var options = new Dictionary<string, string>(StringComparer.Ordinal);
    for (var index = 2; index < args.Length; index += 2)
    {
        if (index + 1 >= args.Length || args[index] is not ("--report" or "--metrics") || !options.TryAdd(args[index], args[index + 1]))
            throw new ArgumentException("Options require one value and may appear only once: --report FILE, --metrics FILE.");
    }
    void WriteJson(string? path, object value)
    {
        var json = JsonSerializer.Serialize(value, ProgramManifest.JsonOptions) + "\n";
        if (path is null) Console.Write(json);
        else
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
            File.WriteAllText(path, json, new System.Text.UTF8Encoding(false));
        }
    }
    options.TryGetValue("--report", out var reportPath);
    if (args[0] == "inspect")
    {
        var input = Path.GetFullPath(args[1]);
        var hash = Translator.HashFile(input, cancellation.Token);
        using var obj = RoslynObject.Open(input);
        var manifest = obj.Manifest;
        WriteJson(reportPath, new
        {
            input = new InputArtifact { Path = input, Sha256 = hash, ArtifactIdentity = manifest.ArtifactIdentity,
                Producer = manifest.Producer, EdgRevision = manifest.EdgRevision, EdgVersion = manifest.EdgVersion,
                CaptureProfile = manifest.CaptureProfile },
            formatMajor = manifest.FormatMajor, formatMinor = manifest.FormatMinor, stage = manifest.Stage,
            target = new { manifest.Target.Abi, manifest.Target.AddressableUnitBits, manifest.Target.Endianness,
                pointerBits = manifest.Target.PointerBits.ToString(System.Globalization.CultureInfo.InvariantCulture),
                pointerAlignment = manifest.Target.PointerAlignment.ToString(System.Globalization.CultureInfo.InvariantCulture) },
            languageMode = obj.TranslationUnit.LanguageMode,
            capabilities = obj.TranslationUnit.Capabilities.Select(c => new CapturedCapability(c.Name, c.Value, c.IsRequired, c.Enabled, c.State.Name)),
            invocationArguments = obj.TranslationUnit.InvocationArguments.ToArray()
        });
        return 0;
    }
    if (args[0] == "inventory" && reportPath is null) throw new ArgumentException("Inventory requires --report FILE.");
    var request = ProgramManifest.Load(args[1]);
    var translator = new Translator();
    var result = args[0] == "inventory" ? translator.Inventory(request, cancellation.Token) : translator.Translate(request, cancellation.Token);
    if (reportPath is not null)
    {
        var json = JsonSerializer.Serialize(result.ProgramReport, ProgramManifest.JsonOptions) + "\n";
        if (System.Text.Encoding.UTF8.GetByteCount(json) > request.Budgets.MaximumOutputBytes)
            throw new IOException("Inventory report exceeds the manifest output budget; report was not written.");
        ProgramManifest.WriteReport(reportPath, result.ProgramReport);
    }
    if (options.TryGetValue("--metrics", out var metricsPath)) WriteJson(metricsPath, result.Metrics);
    foreach (var diagnostic in result.Diagnostics)
        Console.Error.WriteLine($"{diagnostic.Severity.ToString().ToLowerInvariant()} {diagnostic.Code} {diagnostic.Input} {diagnostic.Locator}: {diagnostic.Message}");
    Console.WriteLine($"{result.Status}: inputs={result.ProgramReport.Inputs.Count}, functions={result.ProgramReport.Functions.Count}, imports={result.ProgramReport.Imports.Count}, requiredFamilies={result.ProgramReport.Families.Count(f => f.RequiredCount != 0)}, generatedFiles={result.GeneratedFiles.Count}");
    return result.Status == TranslationStatus.Cancelled ? 130 : result.Success ? 0 : 1;
}
catch (OperationCanceledException) { Console.Error.WriteLine("Cancelled; the attempt is incomplete."); return 130; }
catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or JsonException or InvalidOperationException or NullReferenceException)
{ Console.Error.WriteLine(exception.Message); return 2; }
