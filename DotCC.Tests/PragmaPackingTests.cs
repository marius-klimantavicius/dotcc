using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class PragmaPackingTests
{
    [Fact]
    public void Packing_changes_inside_one_aggregate_are_rejected_until_field_caps_are_modeled()
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-pack-fields-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "main.c");
        File.WriteAllText(path, """
            #pragma pack(push, 4)
            struct Mixed {
                char prefix;
            #pragma pack(1)
                int value;
            };
            #pragma pack(pop)
            int main(void) { return 0; }
            """);
        try
        {
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path })).Message
                .ShouldContain("#pragma pack changes between members");
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

    [Theory]
    [InlineData("pack(3)")]
    [InlineData("pack(32)")]
    [InlineData("pack(1,)")]
    [InlineData("pack(push,1,2)")]
    [InlineData("pack(push")]
    public void Malformed_or_unsupported_pack_directives_fail_explicitly(string pragma)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-pack-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "main.c");
        File.WriteAllText(path, "#pragma " + pragma + "\nint main(void) { return 0; }\n");
        try
        {
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path })).Message.ShouldContain("#pragma pack");
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
