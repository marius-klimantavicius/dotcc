using System;
using System.IO;
using Xunit;

namespace DotCC.Tests;

public sealed class MixedDeclaratorInitializerTests
{
    [Theory]
    [InlineData("struct entry a={1,.second=2}, b={.first=3,4};")]
    [InlineData("static struct entry a={1,.second=2}, b={.first=3,4};")]
    [InlineData("void f(void) { struct entry a={1,.second=2}, b={.first=3,4}; }")]
    [InlineData("void f(void) { static struct entry a={1,.second=2}, b={.first=3,4}; }")]
    [InlineData("struct entry a={1,.second=2,}, b={.first=3,4,};")]
    [InlineData("void f(void) { struct entry a={1,.second=2,}, b={.first=3,4,}; }")]
    public void Mixed_lists_are_interpreted_against_each_declarator(string declaration)
    {
        var path = Path.Combine(Path.GetTempPath(), $"dotcc-mixed-declarators-{Guid.NewGuid():N}.c");
        File.WriteAllText(path, "struct entry { int first, second, third; }; " + declaration);
        try { Assert.NotEmpty(Compiler.EmitObject(path)); }
        finally { File.Delete(path); }
    }
}
