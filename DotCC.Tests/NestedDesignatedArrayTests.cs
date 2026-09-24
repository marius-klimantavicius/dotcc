using System;
using System.IO;
using Xunit;

namespace DotCC.Tests;

public sealed class NestedDesignatedArrayTests
{
    [Theory]
    [InlineData("static struct point values[] = {{.y=2,.x=1}, {.y=4}};")]
    [InlineData("void f(void) { static struct point values[] = {{.y=2,.x=1}}; }")]
    [InlineData("void f(void) { struct point values[3] = {{.y=2,.x=1}, {.y=4}}; }")]
    public void Array_entries_accept_member_designators(string declaration)
    {
        WithSource("struct point { int x; int y; }; " + declaration,
            path => Assert.NotEmpty(Compiler.EmitObject(path)));
    }

    [Fact]
    public void Nested_designators_validate_the_array_element_type()
    {
        WithSource("struct point { int x; }; struct point values[] = {{.missing=2}};",
            path => Assert.Contains("unknown initializer member 'missing'", Assert.ThrowsAny<CompileException>(() => Compiler.EmitObject(path)).Message));
    }

    private static void WithSource(string source, Action<string> check)
    {
        var path = Path.Combine(Path.GetTempPath(), $"dotcc-designated-arrays-{Guid.NewGuid():N}.c");
        File.WriteAllText(path, source);
        try { check(path); }
        finally { File.Delete(path); }
    }
}
