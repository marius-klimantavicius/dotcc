using System;
using System.IO;
using Xunit;

namespace DotCC.Tests;

public sealed class QualifiedCallbackStorageTests
{
    [Theory]
    [InlineData("int (*const *slot)(int) = &callback; *slot=target;")]
    [InlineData("int (*const *const slot)(int) = &callback; slot=&callback;")]
    [InlineData("int (*const *slot)(int) = (int (*const *)(int))&callback; *slot=target;")]
    public void Const_is_retained_at_the_callback_pointer_level(string declaration)
    {
        var error = Assert.ThrowsAny<CompileException>(() => Emit("""
            int target(int x) { return x + 1; }
            int main(void) {
                int (*const callback)(int) = target;
            """ + declaration + " return 0; }"));
        Assert.Contains("read-only", error.Message);
    }

    [Theory]
    [InlineData("const")]
    [InlineData("volatile")]
    [InlineData("const volatile")]
    public void Qualified_callback_storage_types_compose_declaration_and_cast(string qualifiers)
    {
        Emit("int target(int x) { return x; } int main(void) { int (*" + qualifiers +
             " callback)(int)=target; int (*" + qualifiers + " *slot)(int)=&callback; " +
             "int (*" + qualifiers + " **chain)(int)=&slot; " +
             "return (**(int (*" + qualifiers + " **)(int))chain)(42); }");
    }

    private static void Emit(string text)
    {
        var path = Path.Combine(Path.GetTempPath(), $"dotcc-qualified-callback-{Guid.NewGuid():N}.c");
        File.WriteAllText(path, text);
        try { Assert.NotEmpty(Compiler.EmitObject(path)); }
        finally { File.Delete(path); }
    }
}
