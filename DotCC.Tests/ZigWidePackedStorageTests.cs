using System;
using System.IO;
using DotCC.Frontends;
using DotCC.Ir;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("ZigFrontend")]
public sealed class ZigWidePackedStorageTests
{
    [Theory]
    [InlineData(64, 8, 8)]
    [InlineData(65, 16, 16)]
    [InlineData(72, 16, 16)]
    [InlineData(85, 16, 16)]
    [InlineData(96, 16, 16)]
    [InlineData(127, 16, 16)]
    [InlineData(128, 16, 16)]
    public void Retained_source_width_drives_extent_alignment_and_object_contract(int bits, int size, int alignment)
    {
        WithSource($"const T = packed struct(u{bits}) {{ a: u{bits} }}; pub fn main() u8 {{ return @sizeOf(T); }}", path =>
        {
            var ir = new ZigFrontend().BuildIr(new FrontendRequest([path]));
            var proof = ir.ZigStorageLayouts["T"];
            proof.LogicalBits.ShouldBe(bits);
            proof.NativeSize.ShouldBe(size);
            proof.NativeAlignment.ShouldBe(alignment);
            ir.SizeOfConst(new CType.Named("T")).ShouldBe(size);
            var contract = ZigStorageObjectMetadata.Contract(proof);
            var restored = ZigStorageObjectMetadata.ReadContract(contract);
            restored.Bits.ShouldBe(bits);
            restored.Size.ShouldBe(size);
            restored.Alignment.ShouldBe(alignment);
            // A wide descriptor cannot claim a narrowed physical extent.
            if (bits > 64)
                Should.Throw<CompileException>(() => ZigStorageObjectMetadata.ReadContract(contract.Replace("|16|16|", "|8|8|")));
        });
    }

    [Fact]
    public void Beyond_the_bounded_carrier_remains_an_explicit_error()
    {
        WithSource("const T = packed struct { a: u64, b: u64, c: bool }; pub fn main() u8 { return @sizeOf(T); }", path =>
        {
            Should.Throw<DotCC.Layout.OffsetLayoutException>(() => Compiler.EmitCSharp([path])).Message.ShouldContain("outside 1..128");
            Should.Throw<DotCC.Layout.OffsetLayoutException>(() => Compiler.EmitObject(path)).Message.ShouldContain("outside 1..128");
        });
    }

    [Fact]
    public void Wide_enums_do_not_claim_a_managed_enum_representation()
    {
        WithSource("const E = enum(u65) { a }; const T = packed struct { e: E }; pub fn main() u8 { return @sizeOf(T); }", path =>
        {
            Should.Throw<DotCC.Layout.OffsetLayoutException>(() => Compiler.EmitObject(path)).Message.ShouldContain("enum logical width above 64");
        });
    }

    private static void WithSource(string source, Action<string> test)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-wide-packed-proof-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try { var path = Path.Combine(directory, "main.zig"); File.WriteAllText(path, source); test(path); }
        finally { Directory.Delete(directory, true); }
    }
}
