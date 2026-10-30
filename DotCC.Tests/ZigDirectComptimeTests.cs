using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

// A top-level comptime block is an obligation of an analyzed module, including
// library output. Silently omitting the block must never count as compilation.
[Collection("ZigFrontend")]
public sealed class ZigDirectComptimeTests
{
    [Theory]
    [InlineData(EmitMode.File)]
    [InlineData(EmitMode.SharedLib)]
    [InlineData(EmitMode.ManagedLib)]
    public void Root_comptime_compile_error_rejects_every_output_shape(EmitMode emit)
    {
        InDirectory(directory =>
        {
            var source = Path.Combine(directory, "main.zig");
            File.WriteAllText(source, """
                comptime {
                    @compileError("DOTCC_ZIG_DIRECT_ROOT_COMPTIME_MUST_FAIL");
                }
                pub fn main() u8 { return 42; }
                """);

            var error = Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { source }, emit: emit));
            error.Message.ShouldContain("DOTCC_ZIG_DIRECT_ROOT_COMPTIME_MUST_FAIL");
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Used_import_runs_its_top_comptime_even_through_a_reexport(bool reexport)
    {
        InDirectory(directory =>
        {
            File.WriteAllText(Path.Combine(directory, "api.zig"), """
                comptime {
                    @compileError("DOTCC_ZIG_DIRECT_IMPORTED_COMPTIME_MUST_FAIL");
                }
                pub fn answer() u8 { return 42; }
                """);
            File.WriteAllText(Path.Combine(directory, "facade.zig"), """
                pub const api = @import("api.zig");
                pub const answer = api.answer;
                """);
            var source = Path.Combine(directory, "main.zig");
            File.WriteAllText(source, "const api = @import(\"" + (reexport ? "facade.zig" : "api.zig") + "\");\n" +
                "pub fn main() u8 { return api.answer(); }\n");

            var error = Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { source }));
            error.Message.ShouldContain("DOTCC_ZIG_DIRECT_IMPORTED_COMPTIME_MUST_FAIL");
        });
    }

    private static void InDirectory(Action<string> test)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-zig-direct-comptime-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try { test(directory); }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
