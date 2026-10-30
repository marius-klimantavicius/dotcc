using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("ZigFrontend")]
public sealed class ZigTopComptimeTests
{
    [Theory]
    [InlineData("const unused = @import(\"api.zig\");")]
    [InlineData("comptime { if (false) { _ = @import(\"api.zig\"); } }")]
    public void Unreached_import_keeps_its_top_comptime_inert(string prefix)
    {
        WithSource(prefix + "\nexport fn answer() u8 { return 42; }", path =>
        {
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(path)!, "api.zig"),
                "comptime { @compileError(\"UNREACHED_IMPORT\"); }");
            Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib).ShouldContain("answer");
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Import_analysis_preserves_unused_declarations(bool used)
    {
        WithSource("""
            const api = @import("api.zig");
            export fn answer() u8 { return REFERENCE; }
            """.Replace("REFERENCE", used ? "api.marker" : "42"), path =>
        {
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(path)!, "api.zig"), """
                const poison = @compileError("UNUSED_DECLARATION");
                comptime { if (false) @compileError("UNTOPEN"); }
                pub const marker: u8 = 42;
                """);
            Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib).ShouldContain("answer");
        });
    }

    [Fact]
    public void Discarding_an_import_analyzes_its_top_comptime()
    {
        WithSource("comptime { _ = @import(\"api.zig\"); }\nexport fn answer() u8 { return 42; }", path =>
        {
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(path)!, "api.zig"),
                "comptime { @compileError(\"DISCARDED_IMPORT_REACHED\"); }");
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path })).Message.ShouldContain("DISCARDED_IMPORT_REACHED");
        });
    }

    [Theory]
    [InlineData("while (i < 3) : (i += 1) { if (i == 2) @compileError(\"LOOP_REACHED\"); }")]
    [InlineData("for (0..3) |j| { if (j == 2) @compileError(\"LOOP_REACHED\"); }")]
    public void Iteration_analyzes_only_reached_conditions(string loop)
    {
        WithSource("comptime { " + (loop.StartsWith("while", StringComparison.Ordinal) ? "var i: usize = 0; " : "") + loop + " }\nexport fn answer() u8 { return 42; }", path =>
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path })).Message.ShouldContain("LOOP_REACHED"));
    }

    [Fact]
    public void Deferred_guard_reads_the_value_at_scope_exit()
    {
        WithSource("""
            comptime {
                var value: u8 = 0;
                defer { if (value == 2) @compileError("DEFER_REACHED"); }
                value = 2;
            }
            export fn answer() u8 { return 42; }
            """, path => Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path })).Message.ShouldContain("DEFER_REACHED"));
    }

    [Theory]
    [InlineData(".{ .name = \"answer\", .linkage = .weak }", "only the .name")]
    [InlineData(".{ .name = \"answer\" }", "callconv(.c)")]
    public void Unsupported_export_contracts_fail_loudly(string options, string diagnostic)
    {
        WithSource("fn body() c_int { return 42; }\ncomptime { @export(&body, " + options + "); }", path =>
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib)).Message.ShouldContain(diagnostic));
    }

    private static void WithSource(string source, Action<string> test)
    {
        var directory = Path.Combine(Path.GetTempPath(), "zig-top-comptime-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "main.zig");
            File.WriteAllText(path, source);
            test(path);
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
