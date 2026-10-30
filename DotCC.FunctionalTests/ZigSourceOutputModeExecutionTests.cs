using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigSourceOutputModeExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Library_context_preserves_root_identity_and_exported_body(bool objects)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-library-context-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var source = Path.Combine(directory, "main.zig");
            File.WriteAllText(source, "const builtin = @import(\"builtin\"); comptime { if (builtin.output_mode != .Lib) @compileError(\"executable path must stay inactive\"); if (@import(\"root\") == @This()) @export(&answer, .{ .name = \"library_context_answer\" }); } fn answer() callconv(.c) u8 { return 42; } pub fn main() u8 { return answer(); }");
            var options = new ZigCompilationOptions("root", new[] {new ZigModuleDefinition("root", source, Array.Empty<ZigModuleDependency>())}, outputMode: ZigSourceOutputMode.Lib);
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.object.cs");
                File.WriteAllText(fragment, Compiler.EmitObject(source, zigOptions: options));
                emitted = Compiler.LinkObjects(new[] {fragment}, emit: EmitMode.Csproj);
            }
            else emitted = Compiler.EmitCSharp(new[] {source}, emit: EmitMode.Csproj, zigOptions: options);
            emitted.ShouldContain("library_context_answer");
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, Array.Empty<string>());
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }
}
