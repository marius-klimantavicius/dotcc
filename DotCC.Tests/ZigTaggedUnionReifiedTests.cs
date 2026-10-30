using System;
using System.Collections.Generic;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("ZigFrontend")]
public sealed class ZigTaggedUnionReifiedTests
{
    public static IEnumerable<object[]> InvalidTagsAndVariants()
    {
        yield return new object[] { "const Bad = union(Missing) { a };", "Missing" };
        yield return new object[] { "const Key = u8; const Bad = union(Key) { a };", "declared enum type" };
        yield return new object[] { "const Key = struct {}; const Bad = union(Key) { a };", "declared enum type" };
        yield return new object[] { "const Key = enum(u8) { a, b }; const Bad = union(Key) { a, c };", "not a member" };
        yield return new object[] { "const Key = enum(u8) { a, b }; const Bad = union(Key) { a };", "missing variant" };
        yield return new object[] { "const Key = enum(u8) { a, b }; const Bad = union(Key) { a, a };", "duplicate variant" };
        yield return new object[] { "const Bad = union(Key) { a, c, pub const Key = @import(\"tag.zig\").Make(); };", "not a member" };
        yield return new object[] { "const Bad = union(Key) { a, pub const Key = @import(\"tag.zig\").Make(); };", "missing variant" };
    }

    [Theory]
    [MemberData(nameof(InvalidTagsAndVariants))]
    public void Native_invalid_tags_and_variant_sets_are_rejected_by_source_and_object_emission(string declaration, string diagnostic)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-union-tag-reject-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "main.zig");
        File.WriteAllText(Path.Combine(directory, "tag.zig"),
            "pub fn Make() type { return @Enum(u8, .exhaustive, &.{ \"a\", \"b\" }, &.{ 1, 5 }); }");
        File.WriteAllText(path, declaration + "\nexport fn check() usize { return @sizeOf(Bad); }\n");
        try
        {
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path })).Message.ShouldContain(diagnostic);
            Should.Throw<CompileException>(() => Compiler.EmitObject(path)).Message.ShouldContain(diagnostic);
        }
        finally { Directory.Delete(directory, true); }
    }
}
