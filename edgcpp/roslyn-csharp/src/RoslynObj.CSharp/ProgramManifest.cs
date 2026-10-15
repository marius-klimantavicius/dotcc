using System.Text.Json;
using System.Text.Json.Serialization;

namespace RoslynObj.CSharp;

public static class ProgramManifest
{
    public static JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectNullableAnnotations = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public static TranslationRequest Load(string path)
    {
        var fullPath = System.IO.Path.GetFullPath(path);
        var request = JsonSerializer.Deserialize<TranslationRequest>(File.ReadAllText(fullPath), JsonOptions)
            ?? throw new JsonException("The manifest must contain a request object.");
        var directory = System.IO.Path.GetDirectoryName(fullPath)!;
        string Resolve(string value) => string.IsNullOrWhiteSpace(value) ? value : System.IO.Path.GetFullPath(value, directory);
        return request with
        {
            Inputs = request.Inputs.Select(input => input with { Path = Resolve(input.Path) }).ToArray(),
            Output = request.Output with { Directory = Resolve(request.Output.Directory) },
            Runtime = request.Runtime with
            {
                RuntimeProjectPath = request.Runtime.RuntimeProjectPath is { } runtime ? Resolve(runtime) : null,
                Bindings = request.Runtime.Bindings.Select(binding => binding with
                {
                    LibraryPath = binding.LibraryPath is { } library ? Resolve(library) : null
                }).ToArray()
            }
        };
    }

    public static void WriteReport(string path, ProgramReport report)
    {
        var directory = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(path));
        System.IO.Directory.CreateDirectory(directory!);
        File.WriteAllText(path, JsonSerializer.Serialize(report, JsonOptions) + "\n", new System.Text.UTF8Encoding(false));
    }
}
