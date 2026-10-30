using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigTypeReturningValueExecutionTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Type_returning_calls_bind_typed_values_at_compile_time(bool objects, bool taggedUnion)
    {
        var source = taggedUnion ? """
            const Target = union(enum) { c, zig };
            fn Select(target: Target) type {
                return switch (target) { .c => c_int, .zig => u8 };
            }
            const Value = Select(.c);
            pub fn main() u8 { const value: Value = 42; return @intCast(value); }
            """ : """
            fn Select(enabled: bool, keys: []const u8) type {
                return if (enabled and keys.len == 2) c_int else u8;
            }
            const Value = Select(true, "ab");
            pub fn main() u8 { const value: Value = 42; return @intCast(value); }
            """;
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-zig-type-values-" + Guid.NewGuid().ToString("N"));
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
                emitted = Compiler.LinkObjects(new[] {fragment}, emit: EmitMode.Csproj);
            }
            else emitted = Compiler.EmitCSharp(new[] {path}, emit: EmitMode.Csproj);
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, Array.Empty<string>());
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public void A_runtime_argument_cannot_be_used_to_compute_a_type()
    {
        const string source = """
            fn Select(enabled: bool) type { return if (enabled) c_int else u8; }
            fn runtime(enabled: bool) u8 {
                const Value = Select(enabled);
                const value: Value = 42;
                return @intCast(value);
            }
            pub fn main() u8 { return runtime(true); }
            """;
        var path = Path.Combine(Path.GetTempPath(), "dotcc-runtime-type-" + Guid.NewGuid().ToString("N") + ".zig");
        File.WriteAllText(path, source);
        try { Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] {path})); }
        finally { File.Delete(path); }
    }
}
