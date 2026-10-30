using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("ZigFrontend")]
public sealed class ZigQuotedMemberIdentityTests
{
    [Theory]
    [InlineData("a-b", "a?b")]
    [InlineData("a-b", "a_b")]
    [InlineData("256", "_256")]
    [InlineData("café", "caf_")]
    public void Unknown_field_diagnostics_keep_the_exact_query(string declared, string queried)
        => WithSource("const S = struct { @\"" + declared + "\": u8 }; pub fn main() u8 { return @field(S{ .@\""
            + declared + "\" = 42 }, \"" + queried + "\"); }", path =>
            Should.Throw<CompileException>(() => Compiler.EmitObject(path)).Message.ShouldContain("has no field '" + queried + "'"));

    [Theory]
    [InlineData("const E = enum(u8) { @\"café\", @\"caf\\xc3\\xa9\" }; pub fn main() u8 { return @intFromEnum(E.@\"café\"); }")]
    [InlineData("const S = struct { @\"café\": u8, @\"caf\\u{e9}\": u8 }; pub fn main() u8 { return @sizeOf(S); }")]
    public void Equivalent_decoded_source_names_are_duplicate_members(string source)
        => WithSource(source, path =>
            Should.Throw<CompileException>(() => Compiler.EmitObject(path)).Message.ShouldContain("duplicate"));

    [Fact]
    public void An_explicit_union_tag_cannot_match_a_different_punctuation_spelling()
        => WithSource("const E = enum(u8) { @\"a-b\" }; const U = union(E) { @\"a?b\": u8 }; "
            + "pub fn main() u8 { return @sizeOf(U); }", path =>
            Should.Throw<CompileException>(() => Compiler.EmitObject(path)).Message.ShouldContain("a?b"));

    private static void WithSource(string source, Action<string> test)
    {
        var directory = Path.Combine(Path.GetTempPath(), "zig-quoted-member-diagnostic-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "main.zig");
        File.WriteAllText(path, source);
        try { test(path); }
        finally { Directory.Delete(directory, true); }
    }
}
