using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigSourceOptimizeModeExecutionTests
{
    [Theory]
    [InlineData(ZigSourceOptimizeMode.Debug, "debug", "debug", false)]
    [InlineData(ZigSourceOptimizeMode.Safe, "safe", "release_safe", false)]
    [InlineData(ZigSourceOptimizeMode.Fast, "fast", "release_fast", false)]
    [InlineData(ZigSourceOptimizeMode.Small, "small", "release_small", false)]
    [InlineData(ZigSourceOptimizeMode.Debug, "debug", "debug", true)]
    [InlineData(ZigSourceOptimizeMode.Safe, "safe", "release_safe", true)]
    [InlineData(ZigSourceOptimizeMode.Fast, "fast", "release_fast", true)]
    [InlineData(ZigSourceOptimizeMode.Small, "small", "release_small", true)]
    public void RootModeComparison_and_build_info_mapping_execute_in_source_and_objects(
        ZigSourceOptimizeMode mode, string tag, string expected, bool objects)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-native-optimize-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var source = Path.Combine(directory, "main.zig");
            // Same four-way mapping as pinned src/terminal/c/build_info.zig:72.
            File.WriteAllText(Path.Combine(directory, "build_info.zig"), """
                const builtin = @import("builtin");
                pub const Optimize = enum(c_int) { debug = 0, release_safe = 1, release_small = 2, release_fast = 3 };
                pub fn get(out: *Optimize) void {
                    out.* = switch (builtin.mode) {
                        .debug => .debug,
                        .safe => .release_safe,
                        .small => .release_small,
                        .fast => .release_fast,
                    };
                }
                pub fn matches(expected: Optimize) bool {
                    var mode: Optimize = undefined;
                    get(&mode);
                    return mode == expected;
                }
                """);
            File.WriteAllText(source, "const builtin = @import(\"builtin\"); const root = @import(\"root\"); const info = @import(\"build_info.zig\"); pub const expected_mode = ." + tag + "; comptime { if (builtin.mode != root.expected_mode) @compileError(\"DOTCC_WRONG_NATIVE_OPTIMIZE_MODE\"); if (builtin.output_mode != .Lib) @compileError(\"wrong output mode\"); } pub fn main() u8 { return if (info.matches(." + expected + ")) 42 else 1; }");
            var options = new ZigCompilationOptions("root", new[] { new ZigModuleDefinition("root", source, Array.Empty<ZigModuleDependency>()) },
                outputMode: ZigSourceOutputMode.Lib, optimizeMode: mode);
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.object.cs");
                File.WriteAllText(fragment, Compiler.EmitObject(source, zigOptions: options));
                emitted = Compiler.LinkObjects(new[] { fragment }, emit: EmitMode.Csproj);
            }
            else emitted = Compiler.EmitCSharp(new[] { source }, emit: EmitMode.Csproj, zigOptions: options);
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, Array.Empty<string>());
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }
}
