using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("ZigFrontend")]
public sealed class ZigReflectedUnionSemanticsTests
{
    [Theory]
    [InlineData("&.{ \"x\", \"x\" }, &.{ u32, u32 }, &.{ .{}, .{} }", "duplicate union field")]
    [InlineData("&.{ \"x\" }, &.{ u8 }, &.{ .{ .@\"align\" = 3 } }", "invalid union field alignment")]
    public void Invalid_reflected_union_descriptions_fail(string fields, string diagnostic)
    {
        Check("fn U() type { return @Union(.@\"extern\", null, " + fields + "); } pub fn main() u8 { return @intCast(@sizeOf(U())); }", diagnostic);
    }

    [Fact]
    public void Type_valued_aggregates_cannot_escape_into_runtime_signatures()
        => Check("const Config = struct { padding: type }; fn escape(config: Config) u8 { _ = config; return 1; } pub fn main() u8 { return escape(.{ .padding = u8 }); }", "comptime-only aggregate escaped");

    [Theory]
    [InlineData("@Union(.auto, u8, &.{ \"first\", \"last\" }, &.{ u8, u64 }, &@splat(.{}))", "tag type must be an enum or null")]
    [InlineData("@Union(.auto, Tag, &.{ \"first\" }, &.{ u8 }, &@splat(.{}))", "missing variant")]
    [InlineData("@Union(.auto, Tag, &.{ \"first\", \"other\" }, &.{ u8, u64 }, &@splat(.{}))", "not a member of enum")]
    [InlineData("@Union(.auto, Tag, &.{ \"last\", \"first\" }, &.{ u64, u8 }, &@splat(.{}))", "field order does not match")]
    [InlineData("@Union(.@\"extern\", Tag, &.{ \"first\", \"last\" }, &.{ u8, u64 }, &@splat(.{}))", "requires auto layout")]
    [InlineData("@Union(.@\"packed\", Tag, &.{ \"first\", \"last\" }, &.{ u8, u64 }, &@splat(.{}))", "requires auto layout")]
    [InlineData("@Union(.auto, Tag, &.{ \"first\", \"first\" }, &.{ u8, u64 }, &@splat(.{}))", "duplicate union field")]
    [InlineData("@Union(.auto, Tag, &.{ \"first\", \"last\" }, &.{ u8, u64 }, &.{ .{}, .{ .@\"align\" = 3 } })", "invalid union field alignment")]
    public void Invalid_tagged_reflected_union_descriptions_fail_in_source_and_object(string constructor, string diagnostic)
        => Check("const Tag = enum(u8) { first = 3, last = 19 }; const U = " + constructor
            + "; pub fn main() u8 { return @intCast(@sizeOf(U)); }", diagnostic, sourceTypes: true);

    [Theory]
    [InlineData("noreturn", "undefined", "cannot materialize a value of type 'noreturn'")]
    [InlineData("noreturn", "unreachable", "cannot materialize a value of type 'noreturn'")]
    [InlineData("void", "42", "void field requires a void value")]
    public void Invalid_tagged_union_payloads_are_rejected(string payload, string value, string diagnostic)
        => Check("const Tag = enum(u8) { first = 3, last = 19 }; const U = @Union(.auto, Tag, &.{ \"first\", \"last\" }, &.{ "
            + payload + ", u64 }, &@splat(.{})); pub fn main() u8 { const x = @unionInit(U, \"first\", "
            + value + "); _ = x; return 42; }", diagnostic, sourceTypes: true);

    [Fact]
    public void Explicit_tagged_field_alignment_is_an_explicit_storage_cut()
        => Check("const Tag = enum(u8) { first = 3, last = 19 }; const U = @Union(.auto, Tag, &.{ \"first\", \"last\" }, "
            + "&.{ u8, u32 }, &.{ .{}, .{ .@\"align\" = 16 } }); pub fn main() u8 { return @intCast(@sizeOf(U)); }",
            "tagged @Union with explicit field alignment is not modeled", sourceTypes: true);

    private static void Check(string source, string diagnostic, bool sourceTypes = false)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-reflected-union-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "main.zig");
            File.WriteAllText(path, source);
            ZigCompilationOptions? options = null;
            if (sourceTypes)
            {
                var library = Path.Combine(directory, "lib");
                Directory.CreateDirectory(Path.Combine(library, "std"));
                File.WriteAllText(Path.Combine(library, "std", "std.zig"), "");
                options = new ZigCompilationOptions("root", [new("root", path, [])], zigLibDir: library,
                    stdlibTypes: ZigStdlibTypeModel.Source, functionPointerAbi: ZigFunctionPointerAbi.Managed);
            }
            Should.Throw<CompileException>(() => Compiler.EmitCSharp([path], zigOptions: options)).Message.ShouldContain(diagnostic);
            Should.Throw<CompileException>(() => Compiler.EmitObject(path, zigOptions: options)).Message.ShouldContain(diagnostic);
        }
        finally { Directory.Delete(directory, true); }
    }
}
