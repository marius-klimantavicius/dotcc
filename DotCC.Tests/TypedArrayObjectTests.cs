using DotCC;
using DotCC.Frontends;
using DotCC.Ir;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class TypedArrayObjectTests
{
    [Fact]
    public void Extern_incomplete_array_address_keeps_its_source_array_object_type()
    {
        string path = Path.Combine(Path.GetTempPath(), "dotcc-array-object-ir-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, "extern unsigned char key[]; void *probe(void) { return &key; }");
        try
        {
            var ir = new CFrontend().BuildIr(new FrontendRequest([path]));
            var address = ir.Functions.Single().Body.Stmts.OfType<Return>().Single().Value.ShouldBeOfType<Unary>();
            address.Op.ShouldBe(UnOp.AddrOf);
            var source = address.Operand.ShouldBeOfType<VarRef>();
            source.Sym.DeclaredArrayType.ShouldNotBeNull().Count.ShouldBeNull();
            address.Type.ShouldBeOfType<CType.Pointer>().Pointee.ShouldBe(source.Sym.DeclaredArrayType);
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
