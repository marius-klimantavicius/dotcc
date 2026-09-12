using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class ZigAlignedArithmeticTests
{
    [Fact]
    public void Aligned_u128_parameter_storage_preserves_wrapping_addition()
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-zig-aligned-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var source = Path.Combine(directory, "main.zig");
            File.WriteAllText(source, """
                fn add(a: u128, b: u128) u128 { return a +% b; }
                pub fn main() u8 {
                    const maximum: u128 = 340282366920938463463374607431768211455;
                    return if (add(maximum, 1) == 0 and add(maximum, 2) == 1) 0 else 1;
                }
                """);
            var emitted = Compiler.EmitCSharp(new[] { source });
            FixtureRunner.CompileAndRunCapturingExit(emitted, Array.Empty<string>()).exit.ShouldBe(0);
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
