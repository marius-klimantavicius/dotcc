using System;
using System.IO;
using Xunit;

namespace DotCC.Tests;

public sealed class GroupedObjectDeclaratorTests
{
    [Theory]
    [InlineData("struct S { int *(values[3]); }; int f(struct S *p) { return *p->values[2]; }")]
    [InlineData("struct S { int (values)[3]; }; int f(struct S *p) { return p->values[2]; }")]
    [InlineData("int (values[3])={1,2,3}; int f(void) { return values[2]; }")]
    [InlineData("int f(void) { int (values)[3]={1,2,3}; return values[2]; }")]
    [InlineData("int f(void) { int (first)=1, ((second))=2; return first+second; }")]
    [InlineData("int f(void) { int ((values[2])[3]); values[1][2]=7; return values[1][2]+sizeof(values); }")]
    public void Parentheses_preserve_object_declarator_shape(string source)
    {
        var path=Path.Combine(Path.GetTempPath(), $"dotcc-grouped-object-{Guid.NewGuid():N}.c");
        File.WriteAllText(path,source);
        try { Assert.NotEmpty(Compiler.EmitObject(path)); }
        finally { File.Delete(path); }
    }
}
