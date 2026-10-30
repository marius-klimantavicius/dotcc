using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigTopComptimeExecutionTests
{
    [Fact]
    public void Native_backed_comptime_and_canonical_function_addresses_execute()
    {
        WithFixture(path =>
        {
            var emitted = Compiler.EmitCSharp(new[] { path }, emit: EmitMode.Csproj);
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, Array.Empty<string>());
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        });
    }

    [Fact]
    public void Imported_export_aliases_forward_arguments_to_one_actual_body()
    {
        WithFixture(path =>
        {
            var emitted = Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib);
            var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
                .Select(assembly => MetadataReference.CreateFromFile(assembly));
            var compilation = CSharpCompilation.Create("ZigTopComptime_" + Guid.NewGuid().ToString("N"),
                new[] { CSharpSyntaxTree.ParseText(emitted, new CSharpParseOptions(LanguageVersion.Preview)) }, references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
            using var stream = new MemoryStream();
            var result = compilation.Emit(stream);
            result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            stream.Position = 0;
            var context = new AssemblyLoadContext("zig-top-comptime-" + Guid.NewGuid().ToString("N"), isCollectible: false);
            var library = context.LoadFromStream(stream).GetType("DotCcLib", throwOnError: true)!;
            foreach (var name in new[] { "zig_top_add", "zig_top_add_alias" })
            {
                var method = library.GetMethod(name, BindingFlags.Public | BindingFlags.Static);
                method.ShouldNotBeNull();
                method!.Invoke(null, new object[] { 40 }).ShouldBe(42);
            }
            library.GetMethod("zig_top_identity_and_answer", BindingFlags.Public | BindingFlags.Static)!
                .Invoke(null, null).ShouldBe(42);
            foreach (var name in new[] { "zig_top_options", "zig_top_options_alias" })
            {
                var method = library.GetMethod(name, BindingFlags.Public | BindingFlags.Static)!;
                method.ShouldNotBeNull();
                var optionsType = method.GetParameters().Single().ParameterType;
                method.ReturnType.ShouldBe(optionsType);
                Marshal.SizeOf(optionsType).ShouldBe(56);
                Marshal.OffsetOf(optionsType, "extra").ToInt64().ShouldBe(16);
                Marshal.OffsetOf(optionsType, "selection").ToInt64().ShouldBe(48);
                var options = Activator.CreateInstance(optionsType)!;
                optionsType.GetField("size")!.SetValue(options, (ulong)56);
                optionsType.GetField("emit")!.SetValue(options, 40);
                var returned = method.Invoke(null, new[] { options })!;
                optionsType.GetField("emit")!.GetValue(returned).ShouldBe(42);
                optionsType.GetField("size")!.GetValue(returned).ShouldBe((ulong)56);
                optionsType.GetField("emit")!.GetValue(options).ShouldBe(40);
            }
        });
    }

    private static void WithFixture(Action<string> test, [CallerFilePath] string testFile = "")
    {
        var source = Path.Combine(Path.GetDirectoryName(testFile)!, "Fixtures", "zig-top-comptime-control-r30");
        var directory = Path.Combine(Path.GetTempPath(), "zig-top-comptime-execution-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            foreach (var file in Directory.EnumerateFiles(source, "*.zig"))
                File.Copy(file, Path.Combine(directory, Path.GetFileName(file)));
            test(Path.Combine(directory, "main.zig"));
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
