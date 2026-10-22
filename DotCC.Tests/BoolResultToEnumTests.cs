using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class BoolResultToEnumTests
{
    [Theory]
    [InlineData("x > 0")]
    [InlineData("x && 2")]
    [InlineData("boolean")]
    [InlineData("(x++, x > 0)")]
    public void Explicit_enum_conversion_normalizes_the_rendered_boolean(string expression)
    {
        Check(expression, explicitCast: true);
    }

    [Theory]
    [InlineData("x > 0")]
    [InlineData("x || 0")]
    [InlineData("boolean")]
    [InlineData("(x++, x > 0)")]
    public void Implicit_enum_store_normalizes_the_rendered_boolean(string expression)
    {
        Check(expression, explicitCast: false);
    }

    private static void Check(string expression, bool explicitCast)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-boolean-enum-" + Guid.NewGuid().ToString("N") + ".c");
        var value = explicitCast ? "(enum Choice)(" + expression + ")" : expression;
        File.WriteAllText(path, "enum Choice { No, Yes }; enum Choice read(int x) { "
            + "_Bool boolean = x; enum Choice result = " + value + "; return result; }");
        string obj = path + ".cs";
        try
        {
            File.WriteAllText(obj, Compiler.EmitObject(path));
            Compiler.LinkObjects(new[] { obj }, emit: EmitMode.ManagedLib).ShouldContain("(Choice)(int)");
        }
        finally { File.Delete(obj); File.Delete(path); }
    }
}
