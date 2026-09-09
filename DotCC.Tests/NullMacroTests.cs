using System;
using System.IO;
using Xunit;

namespace DotCC.Tests;

public sealed class NullMacroTests
{
    [Theory]
    [InlineData("stddef.h")]
    [InlineData("stdlib.h")]
    [InlineData("stdio.h")]
    [InlineData("locale.h")]
    [InlineData("time.h")]
    public void Standard_header_null_is_a_typed_C_null_pointer_constant(string header)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-null-macro-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, "#include <" + header + ">\n"
            + "_Static_assert(sizeof(NULL) == sizeof(void *), \"NULL pointer size\");\n"
            + "_Static_assert(_Generic(NULL, void *: 1, default: 0), \"NULL pointer type\");\n"
            + "int main(void) { return 0; }\n");
        try { Compiler.EmitCSharp(new[] { path }); }
        finally { File.Delete(path); }
    }
}
