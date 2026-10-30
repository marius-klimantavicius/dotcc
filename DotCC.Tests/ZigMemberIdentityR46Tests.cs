using System;
using System.IO;
using System.Linq;
using DotCC.Frontends;
using DotCC.Ir;
using Shouldly;
using Xunit;
namespace DotCC.Tests;
[Collection("ZigFrontend")]
public sealed class ZigMemberIdentityR46Tests
{
    [Fact]
    public void Annotation_survives_the_frontend_and_backend()
    {
        var registry = new ZigMemberIdentityRegistry();
        var identity = registry.Register("S", ZigMemberKind.Field, new[] { ("a-b", (CType?)CType.UChar) }).Single();
        identity.TargetName.ShouldBe("__zig_member_612D62");
        string dir = Path.Combine(Path.GetTempPath(), "r46-annotation-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "main.zig");
        File.WriteAllText(path,"const E = enum(u8) { @\"a-b\" = 42 }; const S = struct { @\"a-b\": u8 }; pub fn main() u8 { return @intFromEnum(E.@\"a-b\"); }");
        try
        {
            var ir = new ZigFrontend().BuildIr(new FrontendRequest(new[] { path }));
            ir.Enums.Single().Members.Single().TargetName.ShouldBe("__zig_member_612D62");
            ir.Types.Single().Fields.Single().TargetName.ShouldBe("__zig_member_612D62");
            DotCC.Backends.CSharpBackend.Run(ir).Structs.ShouldContain("__zig_member_612D62");
            Compiler.EmitObject(path).ShouldContain("__zig_member_612D62");
        }
        finally { Directory.Delete(dir,true); }
    }
}

[Collection("ZigFrontend")]
public sealed class ZigMemberIdentityR46BoundaryTests
{
    [Theory]
    [InlineData("\\xff", "invalid UTF-8")]
    [InlineData("\\xc3", "invalid UTF-8")]
    [InlineData("\\x00", "NUL-free")]
    [InlineData("", "nonempty")]
    public void Unsupported_member_byte_domains_are_rejected_explicitly(string bytes, string diagnostic)
    {
        WithSource("const S = struct { @\"" + bytes + "\": u8 }; pub fn main() u8 { return @sizeOf(S); }", path =>
            Should.Throw<CompileException>(() => Compiler.EmitObject(path)).Message.ShouldContain(diagnostic));
    }

    [Fact]
    public void Source_identity_metadata_ignores_target_spelling_and_record_copies_keep_it()
    {
        var fields = new[] { new StructField("a-b", CType.UChar) { TargetName = "__zig_member_612D62" } };
        var type = new StructTypeDef("S", fields, false);
        var before = ObjectAggregateMetadata.From(type);
        var changed = fields[0] with { TargetName = "different_target" };
        ObjectAggregateMetadata.From(type with { Fields = new[] { changed } }).ShouldBe(before);
        (fields[0] with { Type = CType.UInt }).TargetName.ShouldBe(fields[0].TargetName);
        ObjectAggregateMetadata.From(type with { Fields = new[] { changed with { Name = "a?b" } } }).ShouldNotBe(before);
    }

    [Fact]
    public void Native_C_enum_references_keep_the_existing_raw_name_fallback()
    {
        var ir = new IrBuilder(null, new DotCC.Backends.CSharpNameLegalizer(), new System.Collections.Generic.Dictionary<string, byte[]>());
        var en = ir.RegisterEnumType("E", CType.Int, new() { new("value", 42) });
        var sym = new Symbol { Name = "value", TargetName = "unrelated_target", Kind = SymKind.EnumConst, Type = en, ConstValue = 42 };
        var fn = new Symbol { Name = "main", TargetName = "main", Kind = SymKind.Func, Type = new CType.Func(CType.Int, Array.Empty<CType>(), false) };
        ir.Functions.Add(new FuncDef(fn, Array.Empty<Symbol>(), new Block(new System.Collections.Generic.List<CStmt>() { new Return(new EnumConstRef(sym) { Type = en }) }), false));
        var text = DotCC.Backends.CSharpBackend.Run(ir).Functions;
        text.ShouldContain("E.value"); text.ShouldNotContain("unrelated_target");
    }

    private static void WithSource(string source, Action<string> action)
    {
        var dir = Path.Combine(Path.GetTempPath(), "r46-boundary-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir); var path = Path.Combine(dir, "main.zig"); File.WriteAllText(path,source);
        try { action(path); } finally { Directory.Delete(dir,true); }
    }
}
