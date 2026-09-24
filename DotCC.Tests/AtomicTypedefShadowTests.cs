using System;
using System.IO;
using Xunit;

namespace DotCC.Tests;

public sealed class AtomicTypedefShadowTests
{
    [Theory]
    [InlineData("struct server { _Atomic(int) state; state next; };")]
    [InlineData("typedef struct server { _Atomic(int) state; state next; } server;")]
    [InlineData("int read(_Atomic(int) *state) { return *state; } state after;")]
    [InlineData("int local(void) { _Atomic(int) state=3; state++; return state; } state after;")]
    [InlineData("struct server { typeof(int) state; state next; };")]
    [InlineData("typedef struct server { typeof(1 + (int)2) state; state next; } server;")]
    public void Parenthesized_type_specifiers_complete_before_the_declarator_name(string source)
    {
        var path = Path.Combine(Path.GetTempPath(), $"dotcc-atomic-shadow-{Guid.NewGuid():N}.c");
        File.WriteAllText(path, "typedef int state; " + source);
        try { Assert.NotEmpty(Compiler.EmitObject(path, dialect: CDialect.Parse("c23"))); }
        finally { File.Delete(path); }
    }
}
