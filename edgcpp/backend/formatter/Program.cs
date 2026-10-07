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

    private static int Main(string[] args)
    {
        if (args.SequenceEqual(new[] { "--shard-methods" })) return MethodShards.Run();
        if (args.SequenceEqual(new[] { "--complete-partials" })) return CompletePartials();
        if (args.Length != 0) throw new ArgumentException("Unknown formatter mode.");
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

    private static int CompletePartials()
    {
        var files = new Dictionary<string, string>(StringComparer.Ordinal);
        var diagnostics = new List<FormatDiagnostic>();
        try
        {
            using JsonDocument request = JsonDocument.Parse(Console.In.ReadToEnd());
            var inputs = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["<contracts>"] = request.RootElement.GetProperty("contracts").GetString()!
            };
            foreach (var provider in request.RootElement.GetProperty("providers").EnumerateObject())
                inputs.Add(provider.Name, provider.Value.GetString()!);
            var options = new CSharpParseOptions(LanguageVersion.Latest);
            var trees = inputs.Select(p => CSharpSyntaxTree.ParseText(p.Value, options, path: p.Key)).ToArray();
            foreach (var tree in trees)
                foreach (var error in tree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error))
                {
                    var position = error.Location.GetLineSpan().StartLinePosition;
                    diagnostics.Add(new(tree.FilePath, error.Id, error.GetMessage(), position.Line + 1, position.Character + 1, "error"));
                }
            if (diagnostics.Count != 0) { WriteResponse(files, diagnostics); return 2; }
            // Bind only the small runtime contract/provider set. Generated
            // program bodies are deliberately not part of this compilation.
            var references = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") ?? "")
                .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
                .Select(path => MetadataReference.CreateFromFile(path));
            var compilation = CSharpCompilation.Create("RuntimePartialInventory", trees, references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true, nullableContextOptions: NullableContextOptions.Enable));
            var globalUsings = trees.SelectMany(tree => ((CompilationUnitSyntax)tree.GetRoot()).Usings)
                .Where(directive => directive.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword)).ToArray();
            var definitions = new HashSet<string>(StringComparer.Ordinal);
            var methods = new List<(SyntaxTree Tree, MethodDeclarationSyntax Method, string Key)>();
            foreach (var tree in trees)
            {
                var model = compilation.GetSemanticModel(tree);
                foreach (var method in tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
                {
                    if (!IsRuntimeMethod(method) || !method.Modifiers.Any(SyntaxKind.PartialKeyword)) continue;
                    string key = PartialKey(method, model, globalUsings);
                    if (method.Body == null && method.ExpressionBody == null) definitions.Add(key);
                    else if (tree.FilePath != "<contracts>") methods.Add((tree, method, key));
                }
            }
            var missing = new HashSet<MethodDeclarationSyntax>();
            foreach (var entry in methods)
                if (definitions.Add(entry.Key)) missing.Add(entry.Method);
            int index = 0;
            foreach (var tree in trees.Skip(1))
            {
                if (!missing.Any(method => method.SyntaxTree == tree)) continue;
                var root = (CompilationUnitSyntax)new PartialDefinitionView(missing, compilation).Visit(tree.GetRoot())!;
                string text = root.WithAttributeLists(default).WithLeadingTrivia(default(SyntaxTriviaList))
                    .NormalizeWhitespace(indentation: "    ", eol: "\n").ToFullString();
                files.Add($"Managed.Runtime.Supplement{index++:D3}.cs",
                    "// <auto-generated />\n#nullable enable\n" + text + "\n");
            }
            WriteResponse(files, diagnostics);
            return 0;
        }
        catch (Exception error) when (error is ArgumentException or JsonException or InvalidOperationException or KeyNotFoundException)
        {
            diagnostics.Add(new("<runtime-provider>", "PARTIAL_CONTRACT_INPUT", error.Message, 0, 0, "error"));
            WriteResponse(files, diagnostics);
            return 1;
        }
    }

    private static bool IsRuntimeMethod(MethodDeclarationSyntax method)
    {
        if (method.Parent is not ClassDeclarationSyntax owner || owner.Identifier.ValueText != "Runtime" ||
            owner.TypeParameterList != null || owner.Parent is TypeDeclarationSyntax) return false;
        string ns = string.Join(".", owner.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().Reverse()
            .Select(node => string.Concat(node.Name.DescendantTokens().Select(token =>
                token.IsKind(SyntaxKind.IdentifierToken) ? token.ValueText : token.Text))
                .Replace("global::", "", StringComparison.Ordinal)));
        return ns == "Managed";
    }

    private static string PartialKey(MethodDeclarationSyntax method, SemanticModel model, UsingDirectiveSyntax[] globalUsings)
    {
        var parameters = method.ParameterList.Parameters.Select(parameter =>
        {
            string refKind = parameter.Modifiers.Any(SyntaxKind.OutKeyword) ? "out:" :
                parameter.Modifiers.Any(SyntaxKind.RefKeyword) ? "ref:" :
                parameter.Modifiers.Any(SyntaxKind.InKeyword) ? "in:" : "value:";
            return refKind + PartialTypeKey(parameter.Type ?? throw new ArgumentException("Partial parameter requires a type."), method, model, globalUsings);
        });
        // Return type, accessibility and static are deliberately excluded:
        // disagreement there must remain an ordinary compiler error against
        // the existing required definition, not create a substitute contract.
        return method.Identifier.ValueText + "`" + (method.TypeParameterList?.Parameters.Count ?? 0) +
            "(" + string.Join(",", parameters) + ")";
    }

    private static string? BoundTypeKey(ITypeSymbol? symbol)
    {
        switch (symbol)
        {
            case null: return null;
            case IDynamicTypeSymbol: return "System.Object";
            case ITypeParameterSymbol parameter: return "!" + parameter.Ordinal;
            case IPointerTypeSymbol pointer:
                return BoundTypeKey(pointer.PointedAtType) is { } target ? target + "*" : null;
            case IArrayTypeSymbol array:
                return BoundTypeKey(array.ElementType) is { } element ? element + "[" + new string(',', array.Rank - 1) + "]" : null;
            case INamedTypeSymbol named:
                if (named.TypeKind == TypeKind.Error) return null;
                if (named.IsTupleType) return BoundTypeKey(named.TupleUnderlyingType);
                var arguments = named.TypeArguments.Select(BoundTypeKey).ToArray();
                if (arguments.Any(argument => argument == null)) return null;
                string prefix = named.ContainingType is { } containing ? BoundTypeKey(containing) + "." :
                    named.ContainingNamespace.IsGlobalNamespace ? "" : named.ContainingNamespace.ToDisplayString() + ".";
                return prefix + named.Name + (arguments.Length == 0 ? "" : "<" + string.Join(",", arguments) + ">");
            case IFunctionPointerTypeSymbol:
                // Handle all function pointers syntactically below so explicit
                // and implicit managed conventions share one canonical key,
                // including aliases to unresolved generated owner types.
                return null;
            default: return null;
        }
    }

    private static string PartialTypeKey(TypeSyntax type, MethodDeclarationSyntax method, SemanticModel model, UsingDirectiveSyntax[] globalUsings)
    {
        if (BoundTypeKey(model.GetTypeInfo(type).Type) is { } bound) return bound;
        var aliases = globalUsings.Concat(((CompilationUnitSyntax)method.SyntaxTree.GetRoot()).Usings).Concat(method.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().Reverse()
            .SelectMany(ns => ns.Usings))
            .Where(usingDirective => usingDirective.Alias != null)
            .GroupBy(usingDirective => usingDirective.Alias!.Name.Identifier.ValueText)
            .ToDictionary(group => group.Key, group => group.Last().NamespaceOrType!);
        string Name(NameSyntax name, HashSet<string> seen, bool allowAlias = true)
        {
            if (name is AliasQualifiedNameSyntax qualified)
            {
                if (qualified.Alias.Identifier.ValueText == "global") return Name(qualified.Name, seen, false);
                string alias = qualified.Alias.Identifier.ValueText;
                if (aliases.TryGetValue(alias, out var value) && seen.Add(alias)) return Syntax(value, seen) + "." + Name(qualified.Name, seen, false);
                throw new ArgumentException($"Unsupported external alias in partial provider: {name}");
            }
            if (name is QualifiedNameSyntax dotted) return Name(dotted.Left, seen, allowAlias) + "." + Name(dotted.Right, seen, false);
            if (name is GenericNameSyntax generic)
                return generic.Identifier.ValueText + "<" + string.Join(",", generic.TypeArgumentList.Arguments.Select(a => Syntax(a, new(seen)))) + ">";
            string identifier = ((IdentifierNameSyntax)name).Identifier.ValueText;
            if (allowAlias && aliases.TryGetValue(identifier, out var aliasType) && seen.Add(identifier)) return Syntax(aliasType, seen);
            return identifier;
        }
        string Syntax(TypeSyntax syntax, HashSet<string> seen)
        {
            var syntaxModel = syntax.SyntaxTree == model.SyntaxTree ? model : model.Compilation.GetSemanticModel(syntax.SyntaxTree);
            if (BoundTypeKey(syntaxModel.GetTypeInfo(syntax).Type) is { } resolved) return resolved;
            if (syntax is FunctionPointerTypeSyntax pointerType)
            {
                var convention = pointerType.CallingConvention;
                string calling = convention?.ManagedOrUnmanagedKeyword.IsKind(SyntaxKind.UnmanagedKeyword) == true
                    ? "unmanaged" : "managed";
                if (convention?.UnmanagedCallingConventionList is { } list)
                    calling += "[" + string.Join(",", list.CallingConventions.Select(item => item.Name.ValueText).OrderBy(name => name, StringComparer.Ordinal)) + "]";
                return "delegate*" + calling + "<" + string.Join(",", pointerType.ParameterList.Parameters.Select(parameter =>
                    string.Join(" ", parameter.Modifiers.Select(token => token.ValueText)) + ":" + Syntax(parameter.Type, new(seen)))) + ">";
            }
            // Names within a qualified name denote namespaces/types; a lone
            // unresolved identifier needs an explicit alias or qualification.
            if ((syntax is IdentifierNameSyntax identifier && !aliases.ContainsKey(identifier.Identifier.ValueText)) || syntax is GenericNameSyntax)
                throw new ArgumentException($"Cannot resolve partial-provider type '{syntax}' in {method.SyntaxTree.FilePath}; qualify the type or add an explicit using alias. Generated program sources are not semantically compiled here.");
            return syntax switch
            {
                NameSyntax name => Name(name, seen),
                PointerTypeSyntax pointer => Syntax(pointer.ElementType, new(seen)) + "*",
                ArrayTypeSyntax array => Syntax(array.ElementType, new(seen)) + string.Concat(array.RankSpecifiers.Select(rank => "[" + new string(',', rank.Rank - 1) + "]")),
                _ => BoundTypeKey(syntaxModel.GetTypeInfo(syntax).Type) ?? throw new ArgumentException($"Unsupported unresolved partial-provider type '{syntax}' in {method.SyntaxTree.FilePath}."),
            };
        }
        return Syntax(type, new());
    }

    private sealed class PartialDefinitionView(HashSet<MethodDeclarationSyntax> missing, CSharpCompilation compilation) : CSharpSyntaxRewriter
    {
        private SyntaxTriviaList NullableTrivia(MethodDeclarationSyntax method)
        {
            var context = compilation.GetSemanticModel(method.SyntaxTree).GetNullableContext(method.SpanStart);
            return SyntaxFactory.ParseLeadingTrivia("#nullable " + (context.HasFlag(NullableContext.AnnotationsEnabled) ? "enable" : "disable") +
                " annotations\n#nullable " + (context.HasFlag(NullableContext.WarningsEnabled) ? "enable" : "disable") + " warnings\n");
        }
        private SyntaxList<MemberDeclarationSyntax> Members(SyntaxList<MemberDeclarationSyntax> members) =>
            SyntaxFactory.List(members.Select(member => Visit(member)).OfType<MemberDeclarationSyntax>());
        public override SyntaxNode? VisitCompilationUnit(CompilationUnitSyntax node) => node
            .WithUsings(SyntaxFactory.List(node.Usings.Where(usingDirective => !usingDirective.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword))))
            .WithMembers(Members(node.Members));
        public override SyntaxNode? VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            var members = Members(node.Members);
            return members.Count == 0 ? null : node.WithMembers(members);
        }
        public override SyntaxNode? VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
        {
            var members = Members(node.Members);
            return members.Count == 0 ? null : node.WithMembers(members);
        }
        public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var methods = node.Members.OfType<MethodDeclarationSyntax>().Where(missing.Contains).Select(method =>
                method.WithAttributeLists(default).WithBody(null).WithExpressionBody(null)
                    .WithModifiers(SyntaxFactory.TokenList(method.Modifiers.Where(token => !token.IsKind(SyntaxKind.AsyncKeyword))))
                    .WithTypeParameterList(method.TypeParameterList?.WithParameters(SyntaxFactory.SeparatedList(method.TypeParameterList.Parameters.Select(parameter => parameter.WithAttributeLists(default)))))
                    .WithParameterList(method.ParameterList.WithParameters(SyntaxFactory.SeparatedList(method.ParameterList.Parameters.Select(parameter => parameter.WithAttributeLists(default)))))
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                    .WithLeadingTrivia(NullableTrivia(method)).WithoutTrailingTrivia());
            var members = SyntaxFactory.List<MemberDeclarationSyntax>(methods);
            return members.Count == 0 ? null : node.WithMembers(members).WithBaseList(null).WithParameterList(null).WithAttributeLists(default)
                .WithoutLeadingTrivia().WithoutTrailingTrivia();
        }
        public override SyntaxNode? VisitStructDeclaration(StructDeclarationSyntax node) => null;
        public override SyntaxNode? VisitInterfaceDeclaration(InterfaceDeclarationSyntax node) => null;
        public override SyntaxNode? VisitRecordDeclaration(RecordDeclarationSyntax node) => null;
        public override SyntaxNode? VisitEnumDeclaration(EnumDeclarationSyntax node) => null;
        public override SyntaxNode? VisitDelegateDeclaration(DelegateDeclarationSyntax node) => null;
        public override SyntaxNode? VisitGlobalStatement(GlobalStatementSyntax node) => null;
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
