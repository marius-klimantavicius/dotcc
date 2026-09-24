using System;
using System.IO;
using Xunit;

namespace DotCC.Tests;

public sealed class PrimitiveTypedefShadowTests
{
    [Theory]
    [InlineData("void release(void *list); list after; void release(void *list) { *(int *)list=42; } list final;")]
    [InlineData("int read(struct list *list) { return list->value; } list after;")]
    [InlineData("typedef void (*callback)(void *list); void invoke(void(*f)(void *list), list *value) { f(value); }")]
    [InlineData("typedef struct Wrapper { int list; list *next; } Wrapper;")]
    [InlineData("int local(void) { int list=4; { int n=(int)list; list=n+2; } return list; } list after;")]
    [InlineData("void invoke(void(*list)(int *), int *value) { list(value); } list after;")]
    [InlineData("struct Callbacks { void(*list)(int *); list *next; }; list after;")]
    public void Declarator_names_shadow_types_only_in_their_own_scope(string source)
    {
        var path = Path.Combine(Path.GetTempPath(), $"dotcc-primitive-shadow-{Guid.NewGuid():N}.c");
        File.WriteAllText(path, "typedef struct list { int value; } list; " + source);
        try { Assert.NotEmpty(Compiler.EmitObject(path)); }
        finally { File.Delete(path); }
    }
}
