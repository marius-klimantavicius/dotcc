#nullable enable
using System;
using System.IO;
using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class GlobalPositionalInitializerTests
{
    [Theory]
    [InlineData("extern struct get_time shared_clock;", "")]
    [InlineData("", "extern struct get_time shared_clock;")]
    [InlineData("struct get_time shared_clock;", "struct get_time shared_clock;")]
    public void Positional_callback_definition_reuses_global_registration(string before, string after)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-global-positional-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, $$"""
            struct get_time { int (*cb)(void); int omitted; };
            int get_time(void) { return 17; }
            {{before}}
            struct get_time shared_clock = {get_time};
            {{after}}
            int main(void) { return shared_clock.cb() + shared_clock.omitted - 17; }
            """);
        try
        {
            var emitted = Compiler.EmitCSharp(new[] { path });
            emitted.Split("get_time shared_clock", StringSplitOptions.None).Length.ShouldBe(2);
        }
        finally { File.Delete(path); }
    }
}
