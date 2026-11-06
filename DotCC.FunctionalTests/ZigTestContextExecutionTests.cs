using System;
using System.IO;
using System.Text.Json;
using DotCC.Frontends;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigTestContextExecutionTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Explicit_source_test_context_and_product_default_survive_object_emission(bool objects, bool enabled)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-test-context-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(directory, "lib", "std"));
        try
        {
            var source = Path.Combine(directory, "main.zig");
            File.WriteAllText(source, "const builtin=@import(\"builtin\"); pub fn main() u8 { return if(builtin.is_test == "
                + (enabled ? "true" : "false") + ") 42 else 1; }");
            File.WriteAllText(Path.Combine(directory, "lib", "std", "std.zig"), "");
            var graph = Path.Combine(directory, "graph.json");
            File.WriteAllText(graph, JsonSerializer.Serialize(new
            {
                schema_version = 1, root_module = "root", is_test = enabled,
                zig_lib_dir = Path.Combine(directory, "lib"), stdlib_types = "source", function_pointer_abi = "managed",
                modules = new[] { new { name = "root", root = source, dependencies = Array.Empty<object>() } },
            }));
            var options = ZigCompilationOptions.Load(graph);
            options.IsTest.ShouldBe(enabled);
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "context.csobj");
                File.WriteAllText(fragment, Compiler.EmitObject(source, zigOptions: options));
                emitted = Compiler.LinkObjects([fragment], emit: EmitMode.Csproj);
            }
            else emitted = Compiler.EmitCSharp([source], emit: EmitMode.Csproj, zigOptions: options);
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42); result.stdout.ShouldBeEmpty(); result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public void Compiler_test_mode_sets_the_same_builtin_test_context()
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-test-mode-context-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var source = Path.Combine(directory, "main.zig");
            File.WriteAllText(source, "const builtin=@import(\"builtin\"); test \"coherent test context\" { if(!builtin.is_test) return error.NotTest; }");
            var emitted = Compiler.EmitCSharp([source], emit: EmitMode.Csproj, testMode: true);
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(0); result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }
}
