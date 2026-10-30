using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class ZigSourceOutputModeTests
{
    [Theory]
    [InlineData(ZigSourceOutputMode.Exe)]
    [InlineData(ZigSourceOutputMode.Lib)]
    [InlineData(ZigSourceOutputMode.Obj)]
    public void Source_output_mode_is_invocation_local_and_independent_of_object_emission(ZigSourceOutputMode mode)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-source-mode-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var source = Path.Combine(directory, "main.zig");
            File.WriteAllText(source, "const builtin = @import(\"builtin\"); comptime { if (builtin.output_mode != ." + mode + ") @compileError(\"wrong source mode\"); if (@import(\"root\") != @This()) @compileError(\"wrong root identity\"); } pub fn answer() u8 { return 42; }");
            var options = new ZigCompilationOptions("root", new[] { new ZigModuleDefinition("root", source, Array.Empty<ZigModuleDependency>()) }, outputMode: mode);
            Compiler.EmitObject(source, zigOptions: options).ShouldContain("answer");
            Compiler.EmitCSharp(new[] { source }, emit: EmitMode.ManagedLib, zigOptions: options).ShouldContain("answer");
            if (mode != ZigSourceOutputMode.Exe)
                Should.Throw<Exception>(() => Compiler.EmitCSharp(new[] {source}, emit: EmitMode.ManagedLib)).Message.ShouldContain("wrong source mode");
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public void Unknown_output_mode_is_rejected()
    {
        Should.Throw<CompileException>(() => new ZigCompilationOptions("root", Array.Empty<ZigModuleDefinition>(), outputMode: (ZigSourceOutputMode)99)).Message.ShouldContain("output_mode");
    }
}
