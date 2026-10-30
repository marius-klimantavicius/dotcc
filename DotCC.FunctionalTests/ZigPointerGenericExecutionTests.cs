using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigPointerGenericExecutionTests
{
    [Theory]
    [InlineData("tuple-rest-callback.zig", 34, false)]
    [InlineData("tuple-rest-callback.zig", 34, true)]
    [InlineData("pointer-element-slices.zig", 36, false)]
    [InlineData("pointer-element-slices.zig", 36, true)]
    [InlineData("array-data-bitcast.zig", 83, false)]
    [InlineData("array-data-bitcast.zig", 83, true)]
    [InlineData("retained-tuple.zig", 190, false)]
    [InlineData("retained-tuple.zig", 190, true)]
    [InlineData("scalar-bitcast.zig", 42, false)]
    [InlineData("scalar-bitcast.zig", 42, true)]
    [InlineData("promoted-bitcast.zig", 42, false)]
    [InlineData("promoted-bitcast.zig", 42, true)]
    public void Pointer_tuples_and_array_bitcasts_match_native_values(string fixture, int expected, bool objects)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-pointer-generic-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "main.zig");
            File.Copy(Path.Combine(FixtureDirectory(), fixture), path);
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.object.cs");
                File.WriteAllText(fragment, Compiler.EmitObject(path));
                emitted = Compiler.LinkObjects(new[] { fragment }, emit: EmitMode.Csproj);
            }
            else emitted = Compiler.EmitCSharp(new[] { path }, emit: EmitMode.Csproj);
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, Array.Empty<string>());
            result.exit.ShouldBe(expected);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    private static string FixtureDirectory([CallerFilePath] string file = "")
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "zig-pointer-generic-r34");

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_returned_literal_pointer_tuple_survives_a_compacting_collection(bool objects)
    {
        var path = Path.Combine(FixtureDirectory(), "retained-tuple.zig");
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-pointer-generic-gc-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string source;
            if (objects)
            {
                var fragment = Path.Combine(directory, "retained.object.cs");
                File.WriteAllText(fragment, Compiler.EmitObject(path));
                source = Compiler.LinkObjects(new[] { fragment }, emit: EmitMode.ManagedLib);
            }
            else source = Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib);
            var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
                .Select(p => MetadataReference.CreateFromFile(p));
            var compilation = CSharpCompilation.Create("PointerTupleGc_" + Guid.NewGuid().ToString("N"),
                new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview)) }, references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
            using var pe = new MemoryStream();
            var result = compilation.Emit(pe);
            result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            pe.Position = 0;
            var context = new AssemblyLoadContext("pointer-tuple-gc-" + Guid.NewGuid().ToString("N"), isCollectible: false);
            var type = context.LoadFromStream(pe).GetType("DotCcLib", throwOnError: true)!;
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            var tuple = methods.Single(m => m.Name == "retain" || m.Name.StartsWith("retain__", StringComparison.Ordinal)).Invoke(null, null);
            GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
            var answer = methods.Single(m => m.Name.StartsWith("consume__", StringComparison.Ordinal)).Invoke(null, new[] { tuple });
            answer.ShouldBe((byte)190);
        }
        finally { Directory.Delete(directory, true); }
    }
}
