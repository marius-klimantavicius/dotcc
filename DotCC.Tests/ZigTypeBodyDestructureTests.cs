using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("ZigFrontend")]
public sealed class ZigTypeBodyDestructureTests
{
    [Theory]
    [InlineData("fn Build(flag: bool) type { const a, const b = .{ flag, false }; return if (a or b) u8 else u16; } fn runtime(flag: bool) u8 { const T = Build(flag); const value: T = 42; return @intCast(value); } pub fn main() u8 { return runtime(true); }")]
    [InlineData("fn Build() type { const a, const b = .{ @as(u8, 1), @as(u8, 2) }; if (a + b != 4) @compileError(\"TYPEBODY_REACHED_GUARD\"); return u8; } const T = Build(); pub fn main() T { return 42; }")]
    [InlineData("fn Build() type { var count: u8 = 0; while (true) { count +%= 1; } return u8; } const T = Build(); pub fn main() T { return 42; }")]
    [InlineData("fn Build() type { for (0..1) |_| { return u8; } return u16; } const T = Build(); pub fn main() T { return 42; }")]
    public void A_type_body_must_evaluate_every_reached_statement(string source)
    {
        WithSource(source, path => Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path })));
    }

    [Fact]
    public void A_reached_guard_retains_the_source_diagnostic()
    {
        WithSource("""
            fn Build() type {
                const a, const b = .{ @as(u8, 1), @as(u8, 2) };
                if (a + b != 4) @compileError("TYPEBODY_REACHED_GUARD");
                return u8;
            }
            const T = Build();
            pub fn main() T { return 42; }
            """, path => Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }))
                .Message.ShouldContain("TYPEBODY_REACHED_GUARD"));
    }

    private static void WithSource(string source, Action<string> test)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-typebody-destructure-" + Guid.NewGuid().ToString("N") + ".zig");
        File.WriteAllText(path, source);
        try { test(path); }
        finally { File.Delete(path); }
    }
}
