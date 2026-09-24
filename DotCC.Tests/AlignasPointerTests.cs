using System;
using System.IO;
using Xunit;

namespace DotCC.Tests;

public sealed class AlignasPointerTests
{
    [Theory]
    [InlineData("struct Q { char lead; _Alignas(64) void **buffer; }; _Static_assert(sizeof(struct Q)==128, \"size\"); _Static_assert(_Alignof(struct Q)==64, \"alignment\");")]
    [InlineData("struct Q { _Alignas(8) struct Big *pointer; }; _Static_assert(sizeof(struct Q)==8, \"pointer alignment\");")]
    [InlineData("struct Q { _Alignas(8) struct Big (*pointer)[2]; };")]
    [InlineData("struct Q { _Alignas(void *) void **pointer; };")]
    [InlineData("struct Q { _Alignas(0) void **pointer; };")]
    [InlineData("struct Q { _Alignas(1) _Alignas(64) int value; }; _Static_assert(_Alignof(struct Q)==64, \"strongest alignment\");")]
    [InlineData("_Alignas(64) void **global; int use(void) { _Alignas(64) void **local=0; static _Alignas(64) void **cached; return local==cached; }")]
    [InlineData("struct Q { _Alignas(8) void (*callback)(void); };")]
    public void Alignment_applies_to_the_complete_declared_type(string source) => Compile(source);

    [Theory]
    [InlineData("struct Q { _Alignas(4) void **pointer; };")]
    [InlineData("struct Q { _Alignas(char) void *pointer; };")]
    [InlineData("struct Q { _Alignas(4) int value, *pointer; };")]
    [InlineData("struct Q { _Alignas(8) struct Big *pointer, object; };")]
    [InlineData("struct Q { _Alignas(1) _Alignas(2) int value; };")]
    [InlineData("_Alignas(4) void **global;")]
    [InlineData("int use(void) { static _Alignas(4) void **pointer; return pointer==0; }")]
    [InlineData("int use(void) { _Alignas(4) int value=0, *pointer=0; return value; }")]
    [InlineData("extern _Alignas(4) void *pointers[2];")]
    public void Weak_alignment_is_rejected_for_each_full_declarator(string source)
    {
        var error = Assert.Throws<CompileException>(() => Compile(source));
        Assert.Contains("less strict than the type's natural alignment", error.Message);
    }

    private static void Compile(string source)
    {
        var path = Path.Combine(Path.GetTempPath(), $"dotcc-alignas-pointer-{Guid.NewGuid():N}.c");
        File.WriteAllText(path, "struct Big { _Alignas(64) int value; }; " + source);
        try { Assert.NotEmpty(Compiler.EmitObject(path, dialect: CDialect.Parse("c11"))); }
        finally { File.Delete(path); }
    }
}
