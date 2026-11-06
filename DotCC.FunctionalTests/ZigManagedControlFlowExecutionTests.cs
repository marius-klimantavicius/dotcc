using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigManagedControlFlowExecutionTests
{
    [Theory]
    [InlineData(false, "default-prong.zig")]
    [InlineData(true, "default-prong.zig")]
    [InlineData(false, "while-test-storage.zig")]
    [InlineData(true, "while-test-storage.zig")]
    [InlineData(false, "labeled-slice-continue.zig")]
    [InlineData(true, "labeled-slice-continue.zig")]
    public void Branch_and_loop_preparation_preserve_subjects_selected_effects_and_continuations(bool objects, string fixture)
    {
        var path = Fixture(fixture);
        var options = Options(path);
        var directory = Path.Combine(Path.GetTempPath(), "managed-control-flow-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string code;
            if (objects)
            {
                var fragment = Path.Combine(directory, "engine.csobj");
                File.WriteAllText(fragment, Compiler.EmitObject(path, zigOptions: options));
                code = Compiler.LinkObjects([fragment], emit: EmitMode.Csproj);
            }
            else code = Compiler.EmitCSharp([path], emit: EmitMode.Csproj, zigOptions: options);
            var result = FixtureRunner.CompileAndRunCapturingStreams(code, [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty(); result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    [Theory]
    [InlineData("else => 0, 0...2 => 1, 2...4 => 2", "overlapping case values")]
    [InlineData("0 => 1, else => 0, 0 => 2", "overlapping case values")]
    [InlineData("else => 0, 1 => 1, else => 2", "multiple default prongs")]
    public void Moving_the_default_does_not_accept_overlapping_or_duplicate_source_prongs(string prongs, string diagnostic)
    {
        var directory = Path.Combine(Path.GetTempPath(), "managed-switch-contract-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "invalid.zig");
            File.WriteAllText(path, "fn select(value: u8) u8 { return switch (value) { " + prongs
                + " }; } pub fn main() u8 { return select(0); }");
            Should.Throw<CompileException>(() => Compiler.EmitObject(path, zigOptions: Options(path)))
                .Message.ShouldContain(diagnostic);
        }
        finally { Directory.Delete(directory, true); }
    }

    private static ZigCompilationOptions Options(string path)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        return new("root", [new("root", path, [])], zigLibDir: library,
            stdlibTypes: ZigStdlibTypeModel.Source, functionPointerAbi: ZigFunctionPointerAbi.Managed);
    }
    private static string Fixture(string name, [CallerFilePath] string caller = "")
        => Path.Combine(Path.GetDirectoryName(caller)!, "Fixtures", "ZigManagedControlFlow", name);
}
