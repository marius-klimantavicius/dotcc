using DotCC.Ir;
using DotCC.Layout;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("ZigFrontend")]
public sealed class ZigOptionalLayoutTests
{
    [Theory]
    [InlineData("o:p:1:1", 2, 1)]
    [InlineData("o:p:2:2", 4, 2)]
    [InlineData("o:p:4:4", 8, 4)]
    [InlineData("o:p:8:8", 16, 8)]
    [InlineData("o:p:16:8", 24, 8)]
    [InlineData("o:a:3:p:4:4", 16, 4)]
    [InlineData("o:p:16:16", 32, 16)]
    public void Managed_optional_extent_includes_flag_alignment_and_tail_padding(string descriptor, int size, int alignment)
    {
        var model = new OffsetLayoutModel(_ => throw new System.Exception("unexpected aggregate"));
        var layout = model.Type(descriptor);
        layout.Size.ShouldBe(size);
        layout.Alignment.ShouldBe(alignment);
    }

    [Theory]
    [InlineData("o:a:0:p:1:1")]
    [InlineData("o:p:16:256")]
    [InlineData("o:unsupported:nested-managed-optional")]
    public void Unproven_optional_payload_storage_is_rejected(string descriptor)
        => Should.Throw<OffsetLayoutException>(() => new OffsetLayoutModel(_ => throw new System.Exception()).Type(descriptor));

    [Fact]
    public void CType_cannot_supply_an_approximate_optional_allocation_size()
        => Should.Throw<IrUnsupportedException>(() => _ = new CType.Optional(CType.ULong).SizeOf);

    [Theory]
    [InlineData("??u32")]
    [InlineData("?struct {}")]
    public void Runtime_size_queries_reject_unsupported_payloads_before_CSharp_compilation(string type)
    {
        var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "dotcc-optional-cut-" + System.Guid.NewGuid().ToString("N"));
        System.IO.Directory.CreateDirectory(directory);
        try
        {
            var path = System.IO.Path.Combine(directory, "main.zig");
            System.IO.File.WriteAllText(path, "pub fn main() u8 { return @intCast(@sizeOf(" + type + ")); }");
            Should.Throw<CompileException>(() => Compiler.EmitCSharp([path])).Message.ShouldContain("optional");
            Should.Throw<CompileException>(() => Compiler.EmitObject(path)).Message.ShouldContain("optional");
        }
        finally { System.IO.Directory.Delete(directory, true); }
    }

    [Fact]
    public void Void_optional_uses_only_its_one_byte_discriminant()
    {
        var descriptor = IrBuilder.LayoutType(new CType.Optional(CType.Void));
        var layout = new OffsetLayoutModel(_ => throw new System.Exception("unexpected aggregate")).Type(descriptor);
        layout.Size.ShouldBe(1);
        layout.Alignment.ShouldBe(1);
    }

    [Fact]
    public void Pointer_backed_tuple_arrays_do_not_claim_inline_source_storage()
    {
        var descriptor = IrBuilder.LayoutType(new CType.Tuple([new CType.Array(CType.UInt, 3), CType.UChar]));
        Should.Throw<OffsetLayoutException>(() => new OffsetLayoutModel(_ => throw new System.Exception()).Type(descriptor));
    }

    [Fact]
    public void Wide_numeric_vector_optional_has_native_payload_and_tail_extent()
    {
        var vector = new CType.Vector(CType.UInt, 32);
        var layout = new OffsetLayoutModel(_ => throw new System.Exception("unexpected aggregate"))
            .Type(IrBuilder.LayoutType(new CType.Optional(vector)));
        layout.Size.ShouldBe(256);
        layout.Alignment.ShouldBe(128);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Vector_layout_does_not_invent_mask_or_small_storage_proofs(bool mask)
    {
        var vector = mask ? new CType.Vector(CType.Bool, 32) : new CType.Vector(CType.UChar, 4);
        Should.Throw<OffsetLayoutException>(() => new OffsetLayoutModel(_ => throw new System.Exception())
            .Type(IrBuilder.LayoutType(new CType.Optional(vector))));
    }
}
