using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// Acceptance-only decomposition of generated declarations. This is not a new
// production source-splitting option or an expression/string rewriting pass.
internal static class SourceSplit
{
    public static void Write(string generated, string output)
    {
        if (Path.Exists(output)) throw new ArgumentException("Split output must be new.");
        var root = CSharpSyntaxTree.ParseText(File.ReadAllText(Path.Combine(generated, "Generated.cs"))).GetCompilationUnitRoot();
        if (root.GetDiagnostics().Any(diagnostic => diagnostic.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error))
            throw new InvalidOperationException("Generated source did not parse.");
        var space = root.Members.OfType<FileScopedNamespaceDeclarationSyntax>().Single();
        var program = space.Members.OfType<ClassDeclarationSyntax>().Single(type => type.Identifier.ValueText == "Program");
        var partial = program.WithModifiers(program.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.PartialKeyword)));
        var methods = program.Members.OfType<MethodDeclarationSyntax>().ToArray();
        var shell = partial.WithMembers(SyntaxFactory.List(program.Members.Where(member => member is not MethodDeclarationSyntax)));
        Directory.CreateDirectory(output);
        File.WriteAllText(Path.Combine(output, "Program.Shell.cs"), root.ReplaceNode(program, shell).NormalizeWhitespace().ToFullString() + "\n");
        for (var index = methods.Length - 1; index >= 0; --index)
        {
            var part = partial.WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(methods[index]));
            var singleSpace = space.WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(part));
            var singleRoot = root.WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(singleSpace));
            File.WriteAllText(Path.Combine(output, $"Program.Method.{methods.Length - index:D4}.cs"), singleRoot.NormalizeWhitespace().ToFullString() + "\n");
        }
        File.Copy(Path.Combine(generated, "Generated.csproj"), Path.Combine(output, "Generated.csproj"));
        Console.WriteLine($"Split {methods.Length} methods from the storage/constructor shell in reverse file order.");
    }
}
