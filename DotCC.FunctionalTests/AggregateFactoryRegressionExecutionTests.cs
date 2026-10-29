using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class AggregateFactoryRegressionExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Factories_preserve_complete_operand_sequence_and_typed_floating_payloads_in_Release(bool objects)
    {
        string directory = Path.Combine(Path.GetTempPath(), "dotcc-factory-regressions-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string source = Path.Combine(AppContext.BaseDirectory, "Fixtures", "aggregate-factory-regressions", "main.c");
            if (objects)
            {
                string fragment = Path.Combine(directory, "fixture.o");
                File.WriteAllText(fragment, Compiler.EmitObject(source, outputOptions: new(AggregateData: true)));
                source = fragment;
            }
            var options = new CSharpOutputOptions(Runtime: RuntimeProfile.C, NestTypes: true, AggregateData: !objects);
            var files = objects
                ? Compiler.LinkObjectFiles([source], emit: EmitMode.ManagedLib, className: "GhosttyVt", namespaceName: "Managed",
                    outputOptions: options, split: SourceSplit.Size, splitSize: 2048)
                : Compiler.EmitCSharpFiles([source], emit: EmitMode.ManagedLib, className: "GhosttyVt", namespaceName: "Managed",
                    outputOptions: options, split: SourceSplit.Size, splitSize: 2048);
            var trees = files.Select(file => CSharpSyntaxTree.ParseText(file.Value,
                new CSharpParseOptions(LanguageVersion.Preview), path: file.Key)).ToArray();
            var declarations = trees.SelectMany(tree => tree.GetRoot().DescendantNodes()).ToArray();
            foreach (var helper in declarations.OfType<ClassDeclarationSyntax>()
                .Where(type => type.Identifier.ValueText.StartsWith("__DotccAggregate", StringComparison.Ordinal)))
            {
                foreach (var method in helper.Members.OfType<MethodDeclarationSyntax>())
                    method.ParameterList.Parameters.Count.ShouldBeLessThanOrEqualTo(2, method.Identifier.ValueText);
                helper.DescendantNodes().OfType<ArrayCreationExpressionSyntax>().ShouldBeEmpty();
                helper.DescendantNodes().OfType<LambdaExpressionSyntax>().ShouldBeEmpty();
            }
            // The addressed-local conditional must lower inside its selected
            // branch, without a closure that captures C stack storage.
            declarations.OfType<MethodDeclarationSyntax>()
                .Single(method => method.Identifier.ValueText == "factory_addressed_sequence")
                .DescendantNodes().OfType<LambdaExpressionSyntax>().ShouldBeEmpty();

            var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
                .Select(path => MetadataReference.CreateFromFile(path));
            var compilation = CSharpCompilation.Create("AggregateFactoryRegressions_" + Guid.NewGuid().ToString("N"), trees, references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true,
                    optimizationLevel: OptimizationLevel.Release));
            using var image = new MemoryStream();
            var result = compilation.Emit(image, cancellationToken: TestContext.Current.CancellationToken);
            result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)));
            byte[] bytes = image.ToArray();
            var context = new AssemblyLoadContext("aggregate-factory-regressions-" + Guid.NewGuid(), isCollectible: true);
            try
            {
                var assembly = context.LoadFromStream(new MemoryStream(bytes));
                var api = assembly.GetType("Managed.GhosttyVt", throwOnError: true)!;
                for (int pass = 0; pass < 3; ++pass)
                {
                    api.GetMethod("factory_comma_trace")!.Invoke(null, null).ShouldBe(1234);
                    api.GetMethod("factory_mixed_sequence")!.Invoke(null, null).ShouldBe(1);
                    api.GetMethod("factory_addressed_sequence")!.Invoke(null, [0]).ShouldBe(1);
                    api.GetMethod("factory_addressed_sequence")!.Invoke(null, [1]).ShouldBe(1);
                    api.GetMethod("factory_lazy_and_loop")!.Invoke(null, null).ShouldBe(1);
                    api.GetMethod("factory_typed_floating")!.Invoke(null, null).ShouldBe(1);
                }
                AggregateFactoryAudit.RequireEmbeddedAllocationFree(AggregateFactoryAudit.Inspect(bytes, assembly));
                AggregateFactoryAudit.RequireAllocationFreeFactories(AggregateFactoryAudit.InspectFactories(assembly),
                    AggregateFactoryAudit.InspectFactoryIl(assembly));
            }
            finally { context.Unload(); }
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
