using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigTypedFunctionConstantExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Imported_nested_method_alias_retains_its_function_address(bool objects)
    {
        var result = Emit(Fixture("nested-alias.zig"), objects);
        var execution = FixtureRunner.CompileAndRunCapturingStreams(result
            + "\npublic static class NestedMethodHost { public static int Main() => DotCcLib.answer(); }", [], TestContext.Current.CancellationToken);
        execution.exit.ShouldBe(42);
        execution.stdout.ShouldBeEmpty(); execution.stderr.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Imported_body_can_demand_a_root_generic_after_the_root_pass(bool objects)
    {
        var result = Emit(Fixture("late-root.zig"), objects);
        var execution = FixtureRunner.CompileAndRunCapturingStreams(result
            + "\npublic static class LateRootGenericHost { public static int Main() => DotCcLib.answer(); }", [], TestContext.Current.CancellationToken);
        execution.exit.ShouldBe(42);
        execution.stdout.ShouldBeEmpty(); execution.stderr.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Typed_function_constants_keep_definition_scope_and_module_aliases(bool objects)
    {
        var source = Fixture("aliases.zig");
        var result = Emit(source, objects);
        var execution = FixtureRunner.CompileAndRunCapturingStreams(result
            + "\npublic static class TypedFunctionConstantHost { public static int Main() => DotCcLib.answer(); }", [], TestContext.Current.CancellationToken);
        execution.exit.ShouldBe(42);
        execution.stdout.ShouldBeEmpty(); execution.stderr.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Typed_function_constants_reject_a_different_parameter_type(bool objects)
    {
        var source = Fixture("mismatch.zig");
        var options = Options(source);
        Should.Throw<CompileException>(() => Emit(source, objects, options))
            .Message.ShouldContain("incompatible declared signature");
    }

    private static string Emit(string source, bool objects, ZigCompilationOptions? options = null)
    {
        options ??= Options(source);
        if (!objects) return Compiler.EmitCSharp([source], emit: EmitMode.ManagedLib, zigOptions: options);
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-typed-function-const-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var fragment = Path.Combine(directory, "main.csobj");
            File.WriteAllText(fragment, Compiler.EmitObject(source, zigOptions: options));
            return Compiler.LinkObjects([fragment], emit: EmitMode.ManagedLib);
        }
        finally { Directory.Delete(directory, true); }
    }

    private static ZigCompilationOptions Options(string source)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        return new("root", [new("root", source, [])],
            outputMode: ZigSourceOutputMode.Lib, optimizeMode: ZigSourceOptimizeMode.Fast, zigLibDir: library,
            stdlibTypes: ZigStdlibTypeModel.Source, functionPointerAbi: ZigFunctionPointerAbi.Managed, managedRoots: ["answer"]);
    }

    private static string Fixture(string file, [CallerFilePath] string caller = "")
        => Path.Combine(Path.GetDirectoryName(caller)!, "Fixtures", "ZigTypedFunctionConstants", file);
}
