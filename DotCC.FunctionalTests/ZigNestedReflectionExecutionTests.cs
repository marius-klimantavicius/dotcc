using System;
using System.IO;
using Shouldly;
using Xunit;
namespace DotCC.FunctionalTests;
[Collection("ZigFrontend")]
public sealed class ZigNestedReflectionExecutionTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Parent_layout_can_demand_nested_fields_before_the_body_loop(bool objects, bool imported)
    {
        const string provider = """
            pub const Parent = struct {
                data: [@typeInfo(Row).@"struct".field_names.len]u8,
                pub const Row = struct {
                    a: u8,
                    b: u8,
                    pub fn tally(self: Row) u8 { return self.a + self.b; }
                };
            };
            """;
        const string body = """
            pub fn main() u8 {
                const parent: Parent = .{ .data = .{ 40, 2 } };
                const row: Parent.Row = .{ .a = parent.data[0], .b = parent.data[1] };
                return row.tally();
            }
            """;
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-nested-reflection-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "main.zig");
            File.WriteAllText(Path.Combine(directory, "provider.zig"), provider);
            File.WriteAllText(path, imported ? "const provider = @import(\"provider.zig\"); const Parent = provider.Parent;\n" + body : provider + body);
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
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_global_type_initializer_can_demand_a_declared_function_body(bool objects)
    {
        const string source = """
            fn names() []const []const u8 { return &.{ "a", "b" }; }
            fn Make() type { return @Struct(.auto,null,names(),&.{u8,u8},&@splat(.{})); }
            const S = Make();
            pub fn main() u8 { const x:S = .{ .a=20,.b=22 }; return x.a+x.b; }
            """;
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-global-reflection-" + Guid.NewGuid().ToString("N"));
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
