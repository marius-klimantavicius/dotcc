using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Edg.SourceFormatter;

internal sealed record FormatDiagnostic(
    string File, string Id, string Message, int Line, int Column, string Severity);
internal sealed record FormatResponse(
    Dictionary<string, object> Files, List<FormatDiagnostic> Diagnostics);

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
                // Bulk arena initialization is synthetic, and Roslyn token
                // navigation over its enormous sibling lists is quadratic.
                // Preserve its exact tokens/trivia internally while formatting
                // the surrounding declarations and ordinary source functions.
                var bulk = new BulkInitializers();
                SyntaxNode compact = bulk.Visit(tree.GetRoot())!;
                SyntaxNode normalized = compact.NormalizeWhitespace(
                    indentation: "    ", eol: "\n", elasticTrivia: false);
                string formatted = bulk.Restore(normalized).ToFullString();
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

    private sealed class BulkInitializers : CSharpSyntaxRewriter
    {
        private const string AnnotationKind = "Edg.PreservedBulkInitializer";
        private readonly Dictionary<string, SyntaxNode> originals = new(StringComparer.Ordinal);

        private T Placeholder<T>(SyntaxNode original, T replacement) where T : SyntaxNode
        {
            string id = originals.Count.ToString(System.Globalization.CultureInfo.InvariantCulture);
            originals.Add(id, original);
            return replacement.WithTriviaFrom(original)
                .WithAdditionalAnnotations(new SyntaxAnnotation(AnnotationKind, id));
        }

        public override SyntaxNode? VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            // Translated C++ constructors are ordinary lowered methods. These
            // C# constructors belong to the generated program storage owner.
            // Selecting this outer body also avoids overlapping collection
            // placeholders within the same bulk initialization.
            if (node.Body is { FullSpan.Length: > 131072 } body)
                return node.WithBody(Placeholder(body, SyntaxFactory.Block()));
            return base.VisitConstructorDeclaration(node);
        }

        public override SyntaxNode? VisitCollectionExpression(CollectionExpressionSyntax node)
        {
            if (node.Elements.Count > 4096 && node.Elements.All(element =>
                    element is ExpressionElementSyntax expression &&
                    expression.Expression.IsKind(SyntaxKind.NumericLiteralExpression)))
                return Placeholder(node, SyntaxFactory.CollectionExpression());
            return base.VisitCollectionExpression(node);
        }

        public SyntaxNode Restore(SyntaxNode formatted)
        {
            SyntaxNode[] placeholders = formatted.GetAnnotatedNodes(AnnotationKind).ToArray();
            if (placeholders.Length != originals.Count)
                throw new InvalidOperationException("Bulk formatter placeholder count changed.");
            return formatted.ReplaceNodes(placeholders, (placeholder, _) =>
                originals[placeholder.GetAnnotations(AnnotationKind).Single().Data!]
                    .WithLeadingTrivia(placeholder.GetLeadingTrivia())
                    .WithTrailingTrivia(placeholder.GetTrailingTrivia()));
        }
    }

    private static void WriteResponse(Dictionary<string, string> files,
        List<FormatDiagnostic> diagnostics)
    {
        // System.Text.Json limits individual string values. Transport unusually
        // large generated files as ordered chunks without changing their text.
        const int chunkSize = 16 * 1024 * 1024;
        var transport = new Dictionary<string, object>();
        foreach (var (name, text) in files)
        {
            if (text.Length <= chunkSize)
            {
                transport.Add(name, text);
                continue;
            }
            var chunks = new List<string>();
            for (int start = 0; start < text.Length;)
            {
                int length = Math.Min(chunkSize, text.Length - start);
                if (start + length < text.Length && char.IsHighSurrogate(text[start + length - 1]) &&
                    char.IsLowSurrogate(text[start + length]))
                    --length;
                chunks.Add(text.Substring(start, length));
                start += length;
            }
            transport.Add(name, chunks);
        }
        Console.Out.WriteLine(JsonSerializer.Serialize(new FormatResponse(transport, diagnostics), JsonOptions));
    }
}
