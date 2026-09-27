using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class EnumBitFieldTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Fixed_enum_bitfields_preserve_explicit_signedness(bool objects)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-enum-fields-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, """
            enum Signed : int { S0, S3 = 3 };
            enum Unsigned : unsigned int { U0, U7 = 7 };
            struct Fields { enum Signed s : 3; enum Unsigned u : 3; };
            int main(void) {
                struct Fields f = {0};
                f.s = (enum Signed)-4; f.u = U7;
                return f.s != -4 || f.u != 7;
            }
            """);
        try
        {
            string generated;
            if (objects)
            {
                File.WriteAllText(path + ".o.cs", Compiler.EmitObject(path, dialect: CDialect.Parse("c23")));
                generated = Compiler.LinkObjects([path + ".o.cs"], emit: EmitMode.Csproj);
            }
            else generated = Compiler.EmitCSharp([path], emit: EmitMode.Csproj, dialect: CDialect.Parse("c23"));
            FixtureRunner.CompileAndRunCapturingExit(generated, []).exit.ShouldBe(0);
        }
        finally { File.Delete(path); File.Delete(path + ".o.cs"); }
    }
}
