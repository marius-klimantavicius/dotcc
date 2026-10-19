using System;
using System.IO;
using Xunit;

namespace DotCC.Tests;

public sealed class ReturnCommaExpressionTests
{
    [Theory]
    [InlineData("int f(int *depth) { return --*depth,42; }")]
    [InlineData("int *f(int *depth,int *value) { return --*depth,value; }")]
    [InlineData("struct entry { int value; }; struct entry f(int *depth) { struct entry value={7}; return --*depth,value; }")]
    public void Return_accepts_a_full_comma_expression(string source)
    {
        var path = Path.Combine(Path.GetTempPath(), $"dotcc-return-comma-{Guid.NewGuid():N}.c");
        File.WriteAllText(path, source);
        try { Assert.NotEmpty(Compiler.EmitObject(path)); }
        finally { File.Delete(path); }
    }
}
