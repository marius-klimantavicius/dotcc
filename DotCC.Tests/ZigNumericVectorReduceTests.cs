using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("ZigFrontend")]
public sealed class ZigNumericVectorReduceTests
{
    [Theory]
    [InlineData("@Vector(8, f32)", "Or", "supported integer lanes")]
    [InlineData("@Vector(128, u8)", "Or", "@reduce on a storage vector")]
    [InlineData("@Vector(8, u8)", "Mul", "is not lowered yet")]
    [InlineData("@Vector(8, u8)", "And", "is not lowered yet")]
    public void Unsupported_reductions_remain_explicit_errors(string type, string operation, string diagnostic)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-reduce-cut-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "main.zig");
            File.WriteAllText(path, $"pub fn main() u8 {{ const v: {type} = @splat(1); _ = @reduce(.{operation}, v); return 42; }}");
            Should.Throw<CompileException>(() => Compiler.EmitObject(path)).Message.ShouldContain(diagnostic);
        }
        finally { Directory.Delete(directory, true); }
    }
}
