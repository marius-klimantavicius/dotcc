using System;
using System.IO;
using System.Linq;
using DotCC.Frontends;
using DotCC.Ir;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("ZigFrontend")]
public sealed class ZigPackedBitSizeTests
{
    [Theory]
    [InlineData("const T = packed struct { a: bool, b: bool, c: bool, d: bool, e: bool, f: bool, g: bool, h: bool };", 8, 1)]
    [InlineData("const T = packed struct { a: u3, b: u5 };", 8, 1)]
    [InlineData("const T = packed struct { a: u3, b: u6 };", 9, 2)]
    [InlineData("const T = packed struct(u16) { a: u8, b: u8 };", 16, 2)]
    [InlineData("const T = packed struct(i8) { a: u3, b: u5 };", 8, 1)]
    [InlineData("const T = packed struct {};", 0, 0)]
    public void Cir_keeps_logical_bit_counts_separate_from_storage(string declaration, int bits, int bytes)
    {
        InDirectory(directory =>
        {
            var path = Path.Combine(directory, "main.zig");
            File.WriteAllText(path, declaration + "\npub fn width() usize { return @bitSizeOf(T); }\n");
            var ir = new ZigFrontend().BuildIr(new FrontendRequest(new[] { path }));
            var value = ir.Functions.Single(f => f.Sym.Name == "width").Body.Stmts.OfType<Return>().Single().Value!;
            ir.ConstEval(value).ShouldBe((long)bits);
            ir.SizeOfConst(new CType.Named("T")).ShouldBe((long)bytes);
        });
    }

    [Theory]
    [InlineData("const T = struct { a: bool, b: u8 };", "T")]
    [InlineData("const N = struct { a: u32 }; const T = packed struct { n: N };", "T")]
    [InlineData("const T = packed struct(u16) { a: u3, b: u5 };", "T")]
    [InlineData("const T = packed struct { bytes: [2]u8 };", "T")]
    [InlineData("const E = enum { a, b, c }; const T = packed struct { e: E, rest: u6 };", "T")]
    public void Unproved_layouts_and_native_invalid_backing_are_loud(string declaration, string type)
    {
        InDirectory(directory =>
        {
            var path = Path.Combine(directory, "main.zig");
            File.WriteAllText(path, declaration + $"\npub fn main() u8 {{ return @intCast(@bitSizeOf({type})); }}\n");
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path })).Message.ShouldContain("@bitSizeOf");
            Should.Throw<CompileException>(() => Compiler.EmitObject(path)).Message.ShouldContain("@bitSizeOf");
        });
    }

    [Fact]
    public void Existing_uniform_struct_compatibility_proof_is_preserved()
    {
        // Compatibility coverage only: pinned Zig 0.17 rejects auto-layout @bitSizeOf.
        InDirectory(directory =>
        {
            var path = Path.Combine(directory, "main.zig");
            File.WriteAllText(path, "const T = struct { a: u32, b: u32 }; pub fn main() u8 { return @bitSizeOf(T); }");
            Compiler.EmitCSharp(new[] { path }).ShouldContain("64");
            Compiler.EmitObject(path).ShouldContain("64");
        });
    }

    [Fact]
    public void Metadata_capture_does_not_evaluate_an_unused_backing()
    {
        InDirectory(directory =>
        {
            var path = Path.Combine(directory, "main.zig");
            File.WriteAllText(path, "fn Backing() type { @compileError(\"unused backing\"); } const Unused = packed struct(Backing()) { a: u8 }; pub fn main() u8 { return 42; }");
            Compiler.EmitCSharp(new[] { path }).ShouldContain("42");
            Compiler.EmitObject(path).ShouldContain("42");
        });
    }

    [Theory]
    [InlineData("fn Make() type { return packed struct { value: u3 }; } pub fn main() u8 { return @bitSizeOf(Make()); }")]
    [InlineData("pub fn main() u8 { const T = packed struct { value: u3 }; return @bitSizeOf(T); }")]
    public void Unrecorded_local_and_reified_packed_layouts_do_not_guess(string source)
    {
        InDirectory(directory =>
        {
            var path = Path.Combine(directory, "main.zig");
            File.WriteAllText(path, source);
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path })).Message.ShouldContain("@bitSizeOf");
            Should.Throw<CompileException>(() => Compiler.EmitObject(path)).Message.ShouldContain("@bitSizeOf");
        });
    }

    private static void InDirectory(Action<string> test)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-packed-bits-reject-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try { test(directory); }
        finally { Directory.Delete(directory, true); }
    }
}
