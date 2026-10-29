using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class AggregateFactoryExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Expression_factories_preserve_values_evaluation_storage_and_callbacks_in_Release(bool objects)
    {
        string fixture = Path.Combine(AppContext.BaseDirectory, "Fixtures", "aggregate-expression-factories");
        string directory = Path.Combine(Path.GetTempPath(), "dotcc-factory-execution-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string source = Path.Combine(fixture, "main.c");
            if (objects)
            {
                string obj = Path.Combine(directory, "fixture.o");
                File.WriteAllText(obj, Compiler.EmitObject(source, outputOptions: new(AggregateData: true)));
                source = obj;
            }
            var options = new CSharpOutputOptions(Runtime: RuntimeProfile.C, NestTypes: true, AggregateData: !objects);
            var files = objects
                ? Compiler.LinkObjectFiles([source], emit: EmitMode.ManagedLib, className: "GhosttyVt", namespaceName: "Managed",
                    outputOptions: options, split: SourceSplit.Size, splitSize: 2048)
                : Compiler.EmitCSharpFiles([source], emit: EmitMode.ManagedLib, className: "GhosttyVt", namespaceName: "Managed",
                    outputOptions: options, split: SourceSplit.Size, splitSize: 2048);
            var trees = files.Select(file => CSharpSyntaxTree.ParseText(file.Value,
                new CSharpParseOptions(LanguageVersion.Preview), path: file.Key)).ToArray();
            foreach (var helper in trees.SelectMany(tree => tree.GetRoot().DescendantNodes()).OfType<ClassDeclarationSyntax>()
                .Where(type => type.Identifier.ValueText.StartsWith("__DotccAggregate", StringComparison.Ordinal)))
            {
                foreach (var method in helper.Members.OfType<MethodDeclarationSyntax>())
                    method.ParameterList.Parameters.Count.ShouldBeLessThanOrEqualTo(2, method.Identifier.ValueText);
                helper.DescendantNodes().OfType<ArrayCreationExpressionSyntax>().ShouldBeEmpty();
                helper.DescendantNodes().OfType<LambdaExpressionSyntax>().ShouldBeEmpty();
            }
            byte[] image = CompileRelease(trees);
            var context = new AssemblyLoadContext("aggregate-factory-release-" + Guid.NewGuid(), isCollectible: true);
            try
            {
                var assembly = context.LoadFromStream(new MemoryStream(image));
                var api = assembly.GetType("Managed.GhosttyVt", throwOnError: true)!;
                for (int pass = 0; pass < 3; ++pass)
                {
                    api.GetMethod("factory_constants")!.Invoke(null, null).ShouldBe(1);
                    api.GetMethod("factory_dynamic")!.Invoke(null, null).ShouldBe(1);
                }
                api.GetMethod("factory_save")!.Invoke(null, null).ShouldBe(1);
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
                GC.WaitForPendingFinalizers();
                api.GetMethod("factory_lifetime")!.Invoke(null, null).ShouldBe(1);

                var payloads = AggregateFactoryAudit.Inspect(image, assembly);
                AggregateFactoryAudit.RequireEmbeddedAllocationFree(payloads);
                var factoryBodies = AggregateFactoryAudit.InspectFactoryIl(assembly);
                var factoryMeasurements = AggregateFactoryAudit.InspectFactories(assembly);
                AggregateFactoryAudit.RequireAllocationFreeFactories(factoryMeasurements, factoryBodies);
                payloads.Select(payload => payload.Backing).Distinct().ShouldContain("UInt16");
                payloads.Select(payload => payload.Backing).Distinct().ShouldContain("UInt32");
                payloads.Select(payload => payload.Backing).Distinct().ShouldContain("UInt64");
            }
            finally { context.Unload(); }
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

    private static byte[] CompileRelease(IEnumerable<SyntaxTree> trees)
    {
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create("AggregateFactory_" + Guid.NewGuid().ToString("N"), trees, references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true,
                optimizationLevel: OptimizationLevel.Release));
        using var image = new MemoryStream();
        var result = compilation.Emit(image, cancellationToken: TestContext.Current.CancellationToken);
        result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)));
        return image.ToArray();
    }
}
