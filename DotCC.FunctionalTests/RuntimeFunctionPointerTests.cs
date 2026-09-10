using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class RuntimeFunctionPointerTests
{
    [Fact]
    public void Allocator_method_addresses_are_captured_only_in_static_readonly_initializers()
    {
        // Check the runtime actually shipped with emitted programs: equality alone can
        // pass by chance when repeated ldftn instructions return the same address.
        using var stream = typeof(Compiler).Assembly.GetManifestResourceStream("DotCC.Runtime.ZigAlloc.cs")!;
        using var reader = new StreamReader(stream);
        var cancellation = TestContext.Current.CancellationToken;
        var root = CSharpSyntaxTree.ParseText(reader.ReadToEnd(), cancellationToken: cancellation).GetRoot(cancellation);
        var addresses = root.DescendantNodes().OfType<PrefixUnaryExpressionSyntax>()
            .Where(node => node.Kind() == SyntaxKind.AddressOfExpression).ToArray();
        addresses.Length.ShouldBe(12);
        foreach (var address in addresses)
        {
            var field = address.Ancestors().OfType<FieldDeclarationSyntax>().FirstOrDefault();
            field.ShouldNotBeNull($"{address} must be captured once, not on each allocator construction");
            field.Modifiers.Any(token => token.Kind() == SyntaxKind.StaticKeyword).ShouldBeTrue();
            field.Modifiers.Any(token => token.Kind() == SyntaxKind.ReadOnlyKeyword).ShouldBeTrue();
        }
    }
}
