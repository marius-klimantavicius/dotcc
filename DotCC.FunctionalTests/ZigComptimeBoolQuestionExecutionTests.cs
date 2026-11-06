using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigComptimeBoolQuestionExecutionTests
{
    [Theory]
    [InlineData("recursive.zig", false)]
    [InlineData("recursive.zig", true)]
    [InlineData("auto-hash.zig", false)]
    [InlineData("auto-hash.zig", true)]
    public void Type_only_boolean_switch_blocks_execute_their_source_field_walk(string fixture, bool objects)
    {
        var path = Path.Combine(FixtureDirectory(), fixture);
        var options = Options(path);
        var directory = Path.Combine(Path.GetTempPath(), "comptime-bool-question-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.csobj");
                File.WriteAllText(fragment, Compiler.EmitObject(path, zigOptions: options));
                emitted = Compiler.LinkObjects([fragment], emit: EmitMode.ManagedLib);
            }
            else emitted = Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib, zigOptions: options);
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted
                + "\npublic static class ComptimeBoolQuestionHost { public static int Main() => DotCcLib.answer(); }", [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty(); result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    [Theory]
    [InlineData("runtime-rejected.zig", "explicit comptime condition must evaluate", false)]
    [InlineData("runtime-rejected.zig", "explicit comptime condition must evaluate", true)]
    [InlineData("invalid-block.zig", "selected type question validation", false)]
    [InlineData("invalid-block.zig", "selected type question validation", true)]
    public void Unknown_runtime_state_and_selected_validation_errors_are_not_replaced_by_false(string fixture, string diagnostic, bool objects)
    {
        var path = Path.Combine(FixtureDirectory(), fixture);
        var options = Options(path);
        var error = Should.Throw<CompileException>(() =>
        {
            if (objects) Compiler.EmitObject(path, zigOptions: options);
            else Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib, zigOptions: options);
        });
        error.Message.ShouldContain(diagnostic);
    }

    private static ZigCompilationOptions Options(string path)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        return new ZigCompilationOptions("root", [new("root", path, [])], zigLibDir: library,
            outputMode: ZigSourceOutputMode.Lib, optimizeMode: ZigSourceOptimizeMode.Debug,
            stdlibTypes: ZigStdlibTypeModel.Source, functionPointerAbi: ZigFunctionPointerAbi.Managed,
            managedRoots: ["answer"], isTest: true);
    }
    private static string FixtureDirectory([CallerFilePath] string file = "")
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigComptimeBoolQuestions");
}
