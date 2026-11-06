using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("ZigFrontend")]
public sealed class ZigExternBitcastWidthTests
{
    [Theory]
    [InlineData("return @intCast(abs(@bitCast(@as(u24, 1))));")]
    [InlineData("const result: u24 = @bitCast(abs(1)); return @intCast(result);")]
    public void Extern_bitcasts_require_equal_logical_widths(string body)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-extern-width-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "root.zig");
            File.WriteAllText(path, "extern \"c\" fn abs(value: c_int) c_int; pub fn main() u8 { " + body + " }");
            Should.Throw<CompileException>(() => Compiler.EmitCSharp([path])).Message
                .ShouldContain("equal logical source and target bit widths");
        }
        finally { Directory.Delete(directory, true); }
    }
}
