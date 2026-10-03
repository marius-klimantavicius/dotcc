using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Edg.SourceFormatter;

internal sealed record FormatDiagnostic(
    string File, string Id, string Message, int Line, int Column, string Severity);
internal sealed record FormatResponse(
    Dictionary<string, string> Files, List<FormatDiagnostic> Diagnostics);

internal static class Program
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static int Main()
    {
        var files = new Dictionary<string, string>(StringComparer.Ordinal);
        var diagnostics = new List<FormatDiagnostic>();
        try
        {
            using JsonDocument request = JsonDocument.Parse(Console.In.ReadToEnd());
            if (request.RootElement.ValueKind != JsonValueKind.Object)
                throw new ArgumentException("The request must be a JSON object mapping file names to C# source strings.");
            var inputs = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (JsonProperty property in request.RootElement.EnumerateObject())
            {
                if (property.Value.ValueKind != JsonValueKind.String)
                    throw new ArgumentException($"Source for '{property.Name}' must be a string.");
                if (!inputs.TryAdd(property.Name, property.Value.GetString()!))
                    throw new ArgumentException($"Duplicate source file name '{property.Name}'.");
            }

            var options = new CSharpParseOptions(LanguageVersion.Latest,
                DocumentationMode.Parse, SourceCodeKind.Regular);
            foreach ((string filename, string source) in inputs)
            {
                SyntaxTree tree = CSharpSyntaxTree.ParseText(source, options, path: filename);
                bool invalid = false;
                foreach (Diagnostic diagnostic in tree.GetDiagnostics())
                {
                    if (diagnostic.Severity != DiagnosticSeverity.Error)
                        continue;
                    FileLinePositionSpan location = diagnostic.Location.GetLineSpan();
                    diagnostics.Add(new FormatDiagnostic(filename, diagnostic.Id,
                        diagnostic.GetMessage(System.Globalization.CultureInfo.InvariantCulture),
                        location.StartLinePosition.Line + 1,
                        location.StartLinePosition.Character + 1, "error"));
                    invalid = true;
                }
                // Formatting malformed syntax can disguise the original failure.
                // Keep it intact while reporting every parser error in the batch.
                if (invalid)
                {
                    files.Add(filename, source);
                    continue;
                }
                string formatted = tree.GetRoot()
                    .NormalizeWhitespace(indentation: "    ", eol: "\n", elasticTrivia: false)
                    .ToFullString();
                if (!formatted.EndsWith('\n'))
                    formatted += "\n";
                files.Add(filename, formatted);
            }
            WriteResponse(files, diagnostics);
            return diagnostics.Count == 0 ? 0 : 2;
        }
        catch (Exception error) when (error is JsonException or ArgumentException)
        {
            diagnostics.Add(new FormatDiagnostic("<request>", "FORMAT_INPUT",
                error.Message, 0, 0, "error"));
            WriteResponse(files, diagnostics);
            return 1;
        }
    }

    private static void WriteResponse(Dictionary<string, string> files,
        List<FormatDiagnostic> diagnostics) =>
        Console.Out.WriteLine(JsonSerializer.Serialize(new FormatResponse(files, diagnostics), JsonOptions));
}
