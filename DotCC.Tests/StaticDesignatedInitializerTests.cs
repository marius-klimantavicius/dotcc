#nullable enable
using System;
using System.IO;
using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class StaticDesignatedInitializerTests
{
    [Fact]
    public void Static_local_designated_aggregate_keeps_program_lifetime_storage()
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-static-designated-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, """
            struct point { const char *name; int count; };
            int first(void) { static struct point point = {.name="first", .count=7}; return point.count++; }
            int second(void) { static struct point point = {.name="second"}; return point.count++; }
            int main(void) { return first() + first() + second() - 15; }
            """);
        try
        {
            var emitted = Compiler.EmitCSharp(new[] { path });
            emitted.ShouldContain("public static unsafe point point__s0");
            emitted.ShouldContain("public static unsafe point point__s1");
        }
        finally { File.Delete(path); }
    }
}
