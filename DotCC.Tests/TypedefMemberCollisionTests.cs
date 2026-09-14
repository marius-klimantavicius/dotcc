using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class TypedefMemberCollisionTests
{
    [Theory]
    [InlineData("struct Holder { Number decimal; };", "struct Holder")]
    [InlineData("typedef struct { Number decimal; } Holder;", "Holder")]
    public void Member_namespace_can_reuse_a_typedef_name(string declaration, string type)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-typedef-member-" + Guid.NewGuid().ToString("N") + ".c");
        try
        {
            File.WriteAllText(path, "typedef long decimal; typedef struct { decimal value; } Number; "
                + declaration + " int main(void) { " + type + " h; " + type
                + " *p = &h; decimal n = 7; p->decimal.value = n; return (int)h.decimal.value; }");
            var output = Compiler.EmitCSharp(new[] { path });
            output.ShouldContain("Number @decimal;");
            output.ShouldContain("p->@decimal.value");
            output.ShouldContain("h.@decimal.value");
        }
        finally { File.Delete(path); }
    }
}
