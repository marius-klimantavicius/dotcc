#nullable enable

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

// Positive cases and the reached traps have matching pinned-Zig 0.17 native controls
// in libghostty/build/direct-noreturn-r29/native-results.json. Regressions for bottom
// flow are deliberately independent of the broader direct-libghostty closure claim.
[Collection("ZigFrontend")]
public sealed class ZigNoreturnExecutionTests
{
    [Theory]
    [InlineData("fn stop() noreturn { while (true) {} } fn choose(ok: bool) u8 { return if (ok) 42 else stop(); } pub fn main() u8 { return choose(true); }")]
    [InlineData("fn stop() noreturn { while (true) {} } fn choose(ok: bool) u8 { const answer = if (ok) stop() else @as(u8, 42); return answer; } pub fn main() u8 { return choose(false); }")]
    [InlineData("fn stop() noreturn { while (true) {} } fn choose(value: u8) u8 { const answer = switch (value) { 0 => stop(), else => @as(u8, 42) }; return answer; } pub fn main() u8 { return choose(1); }")]
    [InlineData("fn stop() noreturn { while (true) {} } fn forward() u8 { return stop(); } fn choose(ok: bool) u8 { return if (ok) 42 else forward(); } pub fn main() u8 { return choose(true); }")]
    [InlineData("fn stop() noreturn { while (true) {} } fn choose(ok: bool) u8 { if (if (ok) true else stop()) return 42; return 0; } pub fn main() u8 { return choose(true); }")]
    [InlineData("fn tag(comptime T: type) u8 { return switch (@typeInfo(T)) { .noreturn => 20, .void => 22, else => 0 }; } pub fn main() u8 { return tag(noreturn) + tag(void); }")]
    [InlineData("fn stop() noreturn { while (true) {} } fn choose(ok: bool, p: *const u8) *const u8 { return if (ok) p else stop(); } pub fn main() u8 { const value: u8 = 42; return choose(true, &value).*; }")]
    [InlineData("fn stop() noreturn { while (true) {} } fn choose(ok: bool, callback: *const fn () noreturn) u8 { return if (ok) 42 else callback(); } pub fn main() u8 { return choose(true, &stop); }")]
    [InlineData("fn stop() noreturn { while (true) {} } fn choose(ok: bool) u8 { if (ok or stop()) return 42; return 0; } pub fn main() u8 { return choose(true); }")]
    public void Native_valid_bottom_flow_executes_with_exit_42(string source)
    {
        var result = FixtureRunner.CompileAndRunCapturingStreams(Emit(source), Array.Empty<string>());
        result.exit.ShouldBe(42);
        result.stdout.ShouldBeEmpty();
        result.stderr.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("pub fn main() u8 { unreachable; }")]
    [InlineData("pub fn main() noreturn { unreachable; }")]
    [InlineData("pub fn main() u8 { @panic(\"R29_NATIVE_PANIC\"); }")]
    [InlineData("fn stop() noreturn { unreachable; } pub fn main() u8 { stop(); }")]
    public void Reached_bottom_fails_instead_of_returning_zero(string source)
    {
        var error = Should.Throw<TargetInvocationException>(() =>
            InvokeLibraryMain(Emit(source, EmitMode.ManagedLib)));
        error.InnerException.ShouldBeOfType<UnreachableException>();
    }

    [Fact]
    public void Noreturn_main_has_a_compilable_console_entry()
    {
        var source = Emit("pub fn main() noreturn { unreachable; }", EmitMode.Csproj);
        CompileEmitted(source, OutputKind.ConsoleApplication);
    }

    private static void InvokeLibraryMain(string source)
        => CompileEmitted(source, OutputKind.DynamicallyLinkedLibrary).GetType("DotCcLib", throwOnError: true)!
            .GetMethod("main", BindingFlags.Public | BindingFlags.Static)!.Invoke(null, null);

    private static Assembly CompileEmitted(string source, OutputKind kind)
    {
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create("ZigNoreturn_" + Guid.NewGuid().ToString("N"),
            new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview)) }, references,
            new CSharpCompilationOptions(kind, allowUnsafe: true));
        using var pe = new MemoryStream();
        var result = compilation.Emit(pe);
        result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
        pe.Position = 0;
        return new AssemblyLoadContext("zig-noreturn-" + Guid.NewGuid().ToString("N"), isCollectible: false).LoadFromStream(pe);
    }

    private static string Emit(string source, EmitMode emit = EmitMode.File)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-zig-noreturn-execution-" + Guid.NewGuid().ToString("N") + ".zig");
        File.WriteAllText(path, source);
        try { return Compiler.EmitCSharp(new[] { path }, emit: emit); }
        finally { File.Delete(path); }
    }
}
