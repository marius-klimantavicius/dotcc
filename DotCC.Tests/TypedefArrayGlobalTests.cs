using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class TypedefArrayGlobalTests
{
    [Fact]
    public void Const_qualifier_on_array_alias_reaches_its_elements()
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-const-array-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "main.c");
        try
        {
            File.WriteAllText(path, "typedef int Values[2]; const Values value; int main(void) { value[0] = 4; return 0; }");
            Should.Throw<CompileException>(() => Compiler.EmitCSharp([path])).Message.ShouldContain("read-only");
        }
        finally { Directory.Delete(directory, true); }
    }

    [Theory]
    [InlineData("typedef unsigned char Bytes[16]; const Bytes value = {255, 1};", "Libc.L")]
    [InlineData("typedef int Matrix[2][3]; Matrix value = {{1, 2}, {3, 4, 5}};", "Libc.GlobalArrayFrom")]
    public void Brace_initializers_use_typedef_array_storage(string source, string storage)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-typedef-array-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "main.c");
        try
        {
            File.WriteAllText(path, source + "\nint main(void) { return sizeof(value); }");
            Compiler.EmitCSharp([path]).ShouldContain(storage);
        }
        finally { Directory.Delete(directory, true); }
    }
}
