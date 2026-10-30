using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigModuleTypeEqualityExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Module_identity_is_a_comptime_boolean_value_without_confusing_nested_containers(bool objects)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-module-identity-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var source = Path.Combine(directory, "main.zig");
            File.WriteAllText(Path.Combine(directory, "api.zig"), "const self = @This(); fn ensure(ok: bool) void { if (!ok) unreachable; } comptime { ensure(@import(\"api.zig\") == @This()); } pub fn value() u8 { return if (self == @import(\"api.zig\")) 42 else 7; }");
            File.WriteAllText(Path.Combine(directory, "other.zig"), "pub const value: u8 = 1;");
            File.WriteAllText(source, "const api = @import(\"api.zig\"); const other = @import(\"other.zig\"); const Nested = struct { pub fn different() bool { return @This() != @import(\"root\"); } }; pub fn main() u8 { if (api == other) return 1; if (!Nested.different()) return 2; return api.value(); }");
            var options = new ZigCompilationOptions("root", new[] {new ZigModuleDefinition("root", source, Array.Empty<ZigModuleDependency>())});
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.object.cs");
                File.WriteAllText(fragment, Compiler.EmitObject(source, zigOptions: options));
                emitted = Compiler.LinkObjects(new[] {fragment}, emit: EmitMode.Csproj);
            }
            else emitted = Compiler.EmitCSharp(new[] {source}, emit: EmitMode.Csproj, zigOptions: options);
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, Array.Empty<string>());
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }
}
