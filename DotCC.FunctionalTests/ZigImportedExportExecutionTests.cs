using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigImportedExportExecutionTests
{
    [Fact]
    public void Called_and_addressed_explicit_export_executes_its_canonical_body()
    {
        WithFixture(path =>
        {
            var source = Compiler.EmitCSharp(new[] { path }, emit: EmitMode.Csproj);
            var result = FixtureRunner.CompileAndRunCapturingStreams(source, Array.Empty<string>());
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Uncalled_exports_and_aliases_execute_from_source_and_linked_object(bool objectMode)
    {
        WithFixture(path =>
        {
            // A library root has no calls to these functions: analysis of the
            // imported namespace alone must retain every explicit export.
            File.WriteAllText(path, "comptime { _ = @import(\"api.zig\"); }");
            var options = new ZigCompilationOptions("root",
                new[] { new ZigModuleDefinition("root", path, Array.Empty<ZigModuleDependency>()) }, outputMode: ZigSourceOutputMode.Lib);
            string source;
            if (objectMode)
            {
                var obj = Path.Combine(Path.GetDirectoryName(path)!, "imported.object.cs");
                File.WriteAllText(obj, Compiler.EmitObject(path, zigOptions: options));
                source = Compiler.LinkObjects(new[] { obj }, emit: EmitMode.ManagedLib);
            }
            else { source = Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib, zigOptions: options); }
            var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
                .Select(assembly => MetadataReference.CreateFromFile(assembly));
            var compilation = CSharpCompilation.Create("ZigImportedExport_" + Guid.NewGuid().ToString("N"),
                new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview)) }, references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
            using var stream = new MemoryStream();
            var emitted = compilation.Emit(stream);
            emitted.Success.ShouldBeTrue(string.Join("\n", emitted.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            stream.Position = 0;
            var context = new AssemblyLoadContext("zig-imported-export-" + Guid.NewGuid().ToString("N"), isCollectible: false);
            var library = context.LoadFromStream(stream).GetType("DotCcLib", throwOnError: true)!;
            foreach (var name in new[] { "zig_imported_add", "zig_imported_add_alias" })
                foreach (var value in new[] { -7, 0, 40 })
                    library.GetMethod(name)!.Invoke(null, new object[] { value }).ShouldBe(value + 2);
            library.GetMethod("zig_imported_identity")!.Invoke(null, null).ShouldBe(42);
            unsafe
            {
                var value = 40;
                library.GetMethod("zig_imported_touch")!.Invoke(null, new[] { Pointer.Box(&value, typeof(int*)) });
                value.ShouldBe(42);
                library.GetMethod("zig_imported_touch")!.Invoke(null, new[] { Pointer.Box(&value, typeof(int*)) });
                value.ShouldBe(44);
            }
            foreach (var name in new[] { "zig_imported_options", "zig_imported_options_alias" })
            {
                var method = library.GetMethod(name)!;
                var type = method.GetParameters().Single().ParameterType;
                method.ReturnType.ShouldBe(type);
                Marshal.SizeOf(type).ShouldBe(56);
                Marshal.OffsetOf(type, "extra").ToInt64().ShouldBe(16);
                Marshal.OffsetOf(type, "selection").ToInt64().ShouldBe(48);
                var value = Activator.CreateInstance(type)!;
                type.GetField("size")!.SetValue(value, (ulong)56);
                type.GetField("emit")!.SetValue(value, 40);
                var result = method.Invoke(null, new[] { value })!;
                type.GetField("emit")!.GetValue(result).ShouldBe(42);
                type.GetField("size")!.GetValue(result).ShouldBe((ulong)56);
                type.GetField("emit")!.GetValue(value).ShouldBe(40);
            }
        });
    }

    private static void WithFixture(Action<string> test, [CallerFilePath] string testFile = "")
    {
        var source = Path.Combine(Path.GetDirectoryName(testFile)!, "Fixtures", "ZigImportedExport");
        var directory = Path.Combine(Path.GetTempPath(), "zig-imported-export-execution-" + Guid.NewGuid().ToString("N"));
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
