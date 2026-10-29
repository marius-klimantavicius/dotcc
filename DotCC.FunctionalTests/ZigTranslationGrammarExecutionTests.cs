using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class ZigTranslationGrammarExecutionTests
{
    [Theory]
    [InlineData("fn add(a: u8, b: u8) u8 { return a + b; }\n" +
                "pub fn main() u8 { return @intCast(add(20, 22,),); }")]
    [InlineData("fn classify(x: u8) u8 { return if (false or switch (x) { 2 => true, else => false, }) 42 else 1; }\n" +
                "pub fn main() u8 { return classify(2); }")]
    public void Upstream_call_and_switch_shapes_execute(string text)
    {
        InDirectory(directory =>
        {
            var source = Path.Combine(directory, "main.zig");
            File.WriteAllText(source, text);
            var emitted = Compiler.EmitCSharp(new[] { source });
            FixtureRunner.CompileAndRunCapturingExit(emitted, Array.Empty<string>()).exit.ShouldBe(42);
        });
    }

    [Fact]
    public void Reexported_module_types_and_primitive_aliases_preserve_values()
    {
        InDirectory(directory =>
        {
            File.WriteAllText(Path.Combine(directory, "types.zig"), """
                pub const Count = u16;
                pub const Item = struct { value: Count };
                """);
            File.WriteAllText(Path.Combine(directory, "facade.zig"), """
                pub const types = @import("types.zig");
                pub const Item = types.Item;
                pub const Count = types.Count;
                """);
            var source = Path.Combine(directory, "main.zig");
            File.WriteAllText(source, """
                const api = @import("facade.zig");
                const Item = api.types.Item;
                const Count = api.Count;
                pub fn main() u8 {
                    const n: Count = 42;
                    const item = Item{ .value = n };
                    return @intCast(item.value);
                }
                """);
            var emitted = Compiler.EmitCSharp(new[] { source });
            FixtureRunner.CompileAndRunCapturingExit(emitted, Array.Empty<string>()).exit.ShouldBe(42);
        });
    }

    private static void InDirectory(Action<string> test)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-zig-translation-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try { test(directory); }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
