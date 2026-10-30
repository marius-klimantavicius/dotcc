using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigUnionForwardAliasExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Union_payload_resolves_its_later_type_alias_before_field_layout(bool objects)
    {
        const string source = """
            pub const Command = union(enum) {
                event: Event,
                pub const Event = Payload;
            };
            pub const Payload = struct { value: u8, };
            pub fn main() u8 {
                const command: Command = .{ .event = .{ .value = 42 } };
                return command.event.value;
            }
            """;
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-zig-union-alias-" + Guid.NewGuid().ToString("N"));
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
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }
}
