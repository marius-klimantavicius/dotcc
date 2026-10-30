using System;
using System.IO;
using DotCC.Frontends;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("ZigFrontend")]
public sealed class ZigSourceOptimizeModeTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "dotcc-optimize-mode-" + Guid.NewGuid().ToString("N"));
    public ZigSourceOptimizeModeTests() => Directory.CreateDirectory(_directory);
    public void Dispose() => Directory.Delete(_directory, true);

    private string Source(string name, string text)
    {
        var path = Path.Combine(_directory, name);
        File.WriteAllText(path, text);
        return path;
    }

    private ZigCompilationOptions Options(string source, ZigSourceOptimizeMode? mode = null) =>
        new("root", new[] { new ZigModuleDefinition("root", source, Array.Empty<ZigModuleDependency>()) },
            outputMode: ZigSourceOutputMode.Lib, optimizeMode: mode);

    [Theory]
    [InlineData("debug", ZigSourceOptimizeMode.Debug)]
    [InlineData("safe", ZigSourceOptimizeMode.Safe)]
    [InlineData("fast", ZigSourceOptimizeMode.Fast)]
    [InlineData("small", ZigSourceOptimizeMode.Small)]
    public void Strict_graph_loads_exact_native_tags_and_preserves_output_mode(string tag, ZigSourceOptimizeMode mode)
    {
        var graph = Source("modules.json", "{\"schema_version\":1,\"root_module\":\"root\",\"modules\":[{\"name\":\"root\",\"root\":\"main.zig\"}],\"output_mode\":\"Lib\",\"optimize_mode\":\"" + tag + "\"}");
        var options = ZigCompilationOptions.Load(graph);
        options.OptimizeMode.ShouldBe(mode);
        options.OutputMode.ShouldBe(ZigSourceOutputMode.Lib);
        foreach (var withStd in new[] { false, true })
        {
            var source = ZigSyntheticModules.SourceForPath(ZigSyntheticModules.BuiltinPath, withStd, options.OutputMode, options.OptimizeMode);
            source.ShouldContain("pub const mode = ." + tag + ";");
            source.ShouldContain("pub const output_mode = .Lib;");
        }
    }

    [Theory]
    [InlineData("null")]
    [InlineData("false")]
    [InlineData("1")]
    [InlineData("{}")]
    [InlineData("[]")]
    [InlineData("\"\"")]
    [InlineData("\"Fast\"")]
    [InlineData("\"ReleaseFast\"")]
    [InlineData("\" fast\"")]
    [InlineData("\"-Ofast\"")]
    [InlineData("\"unknown\"")]
    [InlineData("\"fast\",\"optimize_mode\":\"debug\"")]
    public void Strict_graph_rejects_invalid_or_ambiguous_optimize_context(string value)
    {
        var graph = Source("modules.json", "{\"schema_version\":1,\"root_module\":\"root\",\"modules\":[{\"name\":\"root\",\"root\":\"main.zig\"}],\"optimize_mode\":" + value + "}");
        Should.Throw<CompileException>(() => ZigCompilationOptions.Load(graph));
    }

    [Fact]
    public void Invalid_enum_is_rejected_before_compilation()
    {
        Should.Throw<CompileException>(() => Options("main.zig", (ZigSourceOptimizeMode)99)).Message.ShouldContain("optimize_mode");
    }

    [Theory]
    [InlineData("debug", ZigSourceOptimizeMode.Debug)]
    [InlineData("safe", ZigSourceOptimizeMode.Safe)]
    [InlineData("fast", ZigSourceOptimizeMode.Fast)]
    [InlineData("small", ZigSourceOptimizeMode.Small)]
    public void Mode_guards_are_invocation_local_for_source_and_object_emission(string tag, ZigSourceOptimizeMode mode)
    {
        const string marker = "DOTCC_WRONG_NATIVE_OPTIMIZE_MODE";
        var source = Source("main.zig", "const builtin = @import(\"builtin\"); comptime { if (builtin.mode != ." + tag + ") @compileError(\"" + marker + "\"); if (builtin.output_mode != .Lib) @compileError(\"wrong output mode\"); } pub fn main() u8 { return 42; }");
        var options = Options(source, mode);
        Compiler.EmitCSharp(new[] { source }, zigOptions: options).ShouldContain("42");
        Compiler.EmitObject(source, zigOptions: options).ShouldContain("42");
        var wrong = Options(source, mode == ZigSourceOptimizeMode.Debug ? ZigSourceOptimizeMode.Fast : ZigSourceOptimizeMode.Debug);
        Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { source }, zigOptions: wrong)).Message.ShouldContain(marker);
        Should.Throw<CompileException>(() => Compiler.EmitObject(source, zigOptions: wrong)).Message.ShouldContain(marker);
        Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { source }, zigOptions: Options(source))).Message.ShouldContain(marker);
        // Reusing the immutable request after another request must still select its original tag.
        Compiler.EmitObject(source, zigOptions: options).ShouldContain("42");
    }

    [Fact]
    public void Omitted_context_preserves_legacy_ReleaseFast_with_or_without_a_graph()
    {
        var graph = Source("modules.json", "{\"schema_version\":1,\"root_module\":\"root\",\"modules\":[{\"name\":\"root\",\"root\":\"main.zig\"}]}");
        var options = ZigCompilationOptions.Load(graph);
        options.OptimizeMode.ShouldBeNull();
        var source = Source("main.zig", "const builtin = @import(\"builtin\"); comptime { if (builtin.mode != .ReleaseFast) @compileError(\"legacy mode changed\"); } pub fn main() u8 { return 42; }");
        Compiler.EmitCSharp(new[] { source }).ShouldContain("42");
        Compiler.EmitObject(source).ShouldContain("42");
        Compiler.EmitCSharp(new[] { source }, zigOptions: options).ShouldContain("42");
        Compiler.EmitObject(source, zigOptions: options).ShouldContain("42");
    }
}
