using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigOptionalConditionalExecutionTests
{
    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 1)]
    [InlineData(false, 2)]
    [InlineData(true, 2)]
    [InlineData(false, 3)]
    [InlineData(true, 3)]
    public void Optional_if_arms_keep_the_result_type_and_lazy_effects(bool objects, int variant)
    {
        var source = variant switch
        {
            0 => "fn lookup(x: u8) ?u8 { return if (x > 5) x else null; }\npub fn main() u8 { return (lookup(7) orelse 50) + (lookup(1) orelse 20); }\n",
            1 => "fn lookup(x: u8) ?u8 { return if (x <= 5) null else x; }\npub fn main() u8 { return (lookup(7) orelse 50) + (lookup(1) orelse 20); }\n",
            2 => "fn lookup(x: u8) ?u8 { const value: ?u8 = if (x > 5) x else null; return value; }\npub fn main() u8 { return (lookup(7) orelse 50) + (lookup(1) orelse 20); }\n",
            _ => """
                var calls: u8 = 0;
                fn fallback() u8 { calls += 1; return 20; }
                fn lookup(x: u8) ?u8 { return if (x > 5) x else null; }
                pub fn main() u8 {
                    const first = lookup(7) orelse fallback();
                    const second = lookup(1) orelse fallback();
                    if (calls != 1) return 1;
                    return first + second;
                }
                """
        };
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-zig-optional-if-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "main.zig");
            File.WriteAllText(path, source);
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.object.cs");
                File.WriteAllText(fragment, Compiler.EmitObject(path));
                emitted = Compiler.LinkObjects(new[] { fragment }, emit: EmitMode.Csproj);
            }
            else emitted = Compiler.EmitCSharp(new[] { path }, emit: EmitMode.Csproj);
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, Array.Empty<string>());
            result.exit.ShouldBe(27);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }
}
