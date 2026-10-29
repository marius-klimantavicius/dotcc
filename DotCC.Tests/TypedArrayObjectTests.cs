using DotCC;
using DotCC.Frontends;
using DotCC.Ir;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class TypedArrayObjectTests
{
    [Theory]
    [InlineData("extern unsigned char")]
    [InlineData("unsigned char extern")]
    [InlineData("_Alignas(16) extern unsigned char")]
    [InlineData("unsigned char _Alignas(16) extern")]
    public void Extern_incomplete_array_address_keeps_its_source_array_object_type(string specifiers)
    {
        string path = Path.Combine(Path.GetTempPath(), "dotcc-array-object-ir-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, specifiers + " key[]; void *probe(void) { return &key; }");
        try
        {
            var ir = new CFrontend().BuildIr(new FrontendRequest([path]));
            ir.Globals.ShouldBeEmpty();
            var address = ir.Functions.Single().Body.Stmts.OfType<Return>().Single().Value.ShouldBeOfType<Unary>();
            address.Op.ShouldBe(UnOp.AddrOf);
            var source = address.Operand.ShouldBeOfType<VarRef>();
            source.Sym.Storage.ShouldBe(Storage.Extern);
            source.Sym.IsGlobal.ShouldBeTrue();
            // Ordered declarations retain the array directly; the legacy
            // extern path keeps it beside its execution pointer instead.
            var array = source.Sym.DeclaredArrayType ?? source.Sym.Type.ShouldBeOfType<CType.Array>();
            array.Count.ShouldBeNull();
            array.Element.ShouldBe(CType.UChar);
            address.Type.ShouldBeOfType<CType.Pointer>().Pointee.ShouldBe(array);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Incomplete_extern_array_and_later_definition_keep_one_source_object()
    {
        string path = Path.Combine(Path.GetTempPath(), "dotcc-array-completion-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, """
            _Alignas(16) extern unsigned char key[];
            void *early(void) { return &key; }
            _Alignas(16) unsigned char key[3] = {5, 7, 11};
            unsigned char extern key[];
            void *late(void) { return &key; }
            _Static_assert(sizeof key == 3, "completed array");
            """);
        try
        {
            var ir = new CFrontend().BuildIr(new FrontendRequest([path]));
            var key = ir.Globals.Single();
            key.Sym.Type.ShouldBeOfType<CType.Array>().Count.ShouldBe(3);
            key.Sym.Alignment.ShouldBe(16);
            key.Init.ShouldBeOfType<PinnedArray>().Elems!
                .Select(value => value.ShouldBeOfType<LitInt>().Value).ShouldBe(new long?[] {5, 7, 11});
            foreach (var function in ir.Functions)
            {
                var address = function.Body.Stmts.OfType<Return>().Single().Value.ShouldBeOfType<Unary>();
                address.Op.ShouldBe(UnOp.AddrOf);
                address.Operand.ShouldBeOfType<VarRef>().Sym.ShouldBeSameAs(key.Sym);
            }
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData("sizeof key")]
    [InlineData("sizeof *(&key)")]
    [InlineData("(unsigned long)(&key + 1)")]
    [InlineData("(unsigned long)(&key - &key)")]
    public void Incomplete_array_storage_does_not_masquerade_as_a_pointer_object(string expression)
    {
        string path = Path.Combine(Path.GetTempPath(), "dotcc-array-object-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, "extern unsigned char key[]; unsigned long probe(void) { return " + expression + "; }");
        try
        {
            Should.Throw<CompileException>(() => Compiler.EmitObject(path)).Message.ShouldContain("complete array type");
        }
        finally { File.Delete(path); }
    }
}
