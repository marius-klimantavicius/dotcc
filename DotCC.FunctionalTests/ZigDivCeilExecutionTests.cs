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
public sealed class ZigDivCeilExecutionTests
{
    [Theory]
    [InlineData("integer-widths.zig", false)]
    [InlineData("integer-widths.zig", true)]
    [InlineData("comptime-bounds.zig", false)]
    [InlineData("comptime-bounds.zig", true)]
    [InlineData("side-effects.zig", false)]
    [InlineData("side-effects.zig", true)]
    [InlineData("narrow-widths.zig", false)]
    [InlineData("narrow-widths.zig", true)]
    public void Native_backed_integer_controls_execute(string fixture, bool objects)
    {
        WithSource(File.ReadAllText(Path.Combine(FixtureDirectory(), fixture)), path => {
            var result = FixtureRunner.CompileAndRunCapturingStreams(Emit(path, objects), Array.Empty<string>());
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        });
    }

    [Theory]
    [InlineData("zero", false)]
    [InlineData("zero", true)]
    [InlineData("i8-overflow", false)]
    [InlineData("i8-overflow", true)]
    [InlineData("i3-nested-overflow", false)]
    [InlineData("i3-nested-overflow", true)]
    public void Runtime_invalid_operands_throw_instead_of_wrapping(string kind, bool objects)
    {
        var overflow = kind != "zero";
        var source = kind switch {
            "i8-overflow" => "fn calc(a: i8, b: i8) i8 { return @divCeil(a,b); } pub fn main() u8 { var a: i8 = -128; const ptr = &a; return @bitCast(calc(ptr.*, -1)); }",
            "i3-nested-overflow" => "fn calc(a: i3, b: i3) i3 { return @divCeil(@divCeil(a,b),-1); } pub fn main() u8 { var a: i3 = -4; const ptr = &a; return @intCast(calc(ptr.*, 1)); }",
            _ => "fn calc(a: u8, b: u8) u8 { return @divCeil(a,b); } pub fn main() u8 { var b: u8 = 0; const ptr = &b; return calc(7,ptr.*); }",
        };
        WithSource(source, path => {
            // Console emission starts a worker thread. Invoke the emitted library's
            // main on this thread so an expected invalid operand stays catchable.
            var emitted = Emit(path, objects, EmitMode.ManagedLib);
            var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
                .Select(p => MetadataReference.CreateFromFile(p));
            var compilation = CSharpCompilation.Create("DivCeilInvalid_" + Guid.NewGuid().ToString("N"),
                new[] { CSharpSyntaxTree.ParseText(emitted, new CSharpParseOptions(LanguageVersion.Preview)) }, references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
            using var pe = new MemoryStream();
            var result = compilation.Emit(pe);
            result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            pe.Position = 0;
            var context = new AssemblyLoadContext("divceil-invalid-" + Guid.NewGuid().ToString("N"), isCollectible: true);
            try
            {
                var type = context.LoadFromStream(pe).GetType("DotCcLib", throwOnError: true)!;
                var main = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                    .Single(m => m.Name == "main" || m.Name.StartsWith("main__", StringComparison.Ordinal));
                var error = Should.Throw<TargetInvocationException>(() => main.Invoke(null, null));
                if (overflow) error.InnerException.ShouldBeOfType<OverflowException>();
                else error.InnerException.ShouldBeOfType<DivideByZeroException>();
            }
            finally { context.Unload(); }
        });
    }

    private static string Emit(string path, bool objects, EmitMode output = EmitMode.Csproj)
    {
        if (!objects) return Compiler.EmitCSharp(new[] { path }, emit: output);
        var fragment = Path.Combine(Path.GetDirectoryName(path)!, "main.object.cs");
        File.WriteAllText(fragment, Compiler.EmitObject(path));
        return Compiler.LinkObjects(new[] { fragment }, emit: output);
    }

    private static string FixtureDirectory([CallerFilePath] string file = "")
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "zig-divceil-r35");

    private static void WithSource(string source, Action<string> test)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-divceil-run-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "main.zig");
        File.WriteAllText(path, source);
        try { test(path); }
        finally { Directory.Delete(directory, true); }
    }
}
