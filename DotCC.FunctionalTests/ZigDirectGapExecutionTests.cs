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

// The same genuine Zig fixtures are controls for pinned Zig 0.17.0. These tests
// exercise DotCC's public compiler API and execute the emitted code. They are
// regressions for individual gaps, not proof that the libghostty engine builds.
[Collection("ZigFrontend")]
public sealed class ZigDirectGapExecutionTests
{
    [Theory]
    [InlineData("zig-direct-noreturn")]
    [InlineData("zig-direct-enum-anytype")]
    [InlineData("zig-direct-result-reexport")]
    public void Native_valid_program_executes_with_exit_42(string fixture)
    {
        WithFixture(fixture, source =>
        {
            var emitted = Compiler.EmitCSharp(new[] { source }, emit: EmitMode.Csproj);
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, Array.Empty<string>());
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        });
    }

    [Theory]
    [InlineData("zig-direct-top-comptime", "DOTCC_ZIG_DIRECT_ROOT_COMPTIME_MUST_FAIL")]
    [InlineData("zig-direct-imported-comptime", "DOTCC_ZIG_DIRECT_IMPORTED_COMPTIME_MUST_FAIL")]
    public void Native_rejected_program_cannot_produce_an_executable(string fixture, string diagnostic)
    {
        WithFixture(fixture, source =>
        {
            var error = Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { source }, emit: EmitMode.Csproj));
            error.Message.ShouldContain(diagnostic);
        });
    }

    [Theory]
    [InlineData(EmitMode.ManagedLib)]
    [InlineData(EmitMode.SharedLib)]
    public void Export_roots_a_private_body_under_both_requested_names(EmitMode emit)
    {
        WithFixture("zig-direct-export-alias", source =>
        {
            var assembly = CompileLibrary(Compiler.EmitCSharp(new[] { source }, emit: emit));
            AssertExportExecutes(assembly, emit, "zig_direct_answer");
            AssertExportExecutes(assembly, emit, "zig_direct_answer_alias");
            if (emit == EmitMode.SharedLib)
                ExportNames(assembly).ShouldNotContain("answer");
        });
    }

    [Theory]
    [InlineData(EmitMode.ManagedLib)]
    [InlineData(EmitMode.SharedLib)]
    public void Imported_top_comptime_export_roots_an_otherwise_uncalled_body(EmitMode emit)
    {
        WithFixture("zig-direct-imported-export", source =>
        {
            var assembly = CompileLibrary(Compiler.EmitCSharp(new[] { source }, emit: emit));
            AssertExportExecutes(assembly, emit, "zig_direct_imported_answer");
            if (emit == EmitMode.SharedLib)
                ExportNames(assembly).ShouldNotContain("answer");
        });
    }

    private static unsafe void AssertExportExecutes(Assembly assembly, EmitMode emit, string name)
    {
        var type = assembly.GetType(emit == EmitMode.ManagedLib ? "DotCcLib" : "DotCcExports", throwOnError: true)!;
        var method = type.GetMethod(name, BindingFlags.Public | BindingFlags.Static);
        method.ShouldNotBeNull($"the requested public export '{name}' must exist");
        method!.GetParameters().ShouldBeEmpty();
        method.ReturnType.ShouldBe(typeof(int));
        if (emit == EmitMode.ManagedLib)
        {
            method.Invoke(null, null).ShouldBe(42);
        }
        else
        {
            var export = method.GetCustomAttribute<UnmanagedCallersOnlyAttribute>();
            export.ShouldNotBeNull();
            export!.EntryPoint.ShouldBe(name);
            export.CallConvs.ShouldNotBeNull();
            export.CallConvs!.ShouldContain(typeof(CallConvCdecl));
            // Invoke the emitted unmanaged entry, including its alias forwarding
            // body. Reflection alone cannot invoke an UnmanagedCallersOnly method.
            var entry = (delegate* unmanaged[Cdecl]<int>)method.MethodHandle.GetFunctionPointer();
            entry().ShouldBe(42);
        }
    }

    private static string?[] ExportNames(Assembly assembly) => assembly.GetType("DotCcExports", throwOnError: true)!
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Select(method => method.GetCustomAttribute<UnmanagedCallersOnlyAttribute>()?.EntryPoint)
        .ToArray();

    private static Assembly CompileLibrary(string source)
    {
        var platformAssemblies = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;
        var references = platformAssemblies.Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create("ZigDirect_" + Guid.NewGuid().ToString("N"),
            new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview)) },
            references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
        using var pe = new MemoryStream();
        var result = compilation.Emit(pe);
        result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
        pe.Position = 0;
        var context = new AssemblyLoadContext("zig-direct-" + Guid.NewGuid().ToString("N"), isCollectible: false);
        return context.LoadFromStream(pe);
    }

    private static void WithFixture(string name, Action<string> test, [CallerFilePath] string testFile = "")
    {
        // Resolve from this source file because fixture-copy configuration belongs
        // to the parent integration; each compile gets a fresh tiny import tree.
        var fixture = Path.Combine(Path.GetDirectoryName(testFile)!, "Fixtures", name);
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-zig-direct-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            foreach (var source in Directory.EnumerateFiles(fixture, "*.zig"))
                File.Copy(source, Path.Combine(directory, Path.GetFileName(source)));
            test(Path.Combine(directory, "main.zig"));
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
