using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigActualModeReportExecutionTests
{
    [Theory]
    [InlineData(false, "actual-mode-report.zig")]
    [InlineData(true, "actual-mode-report.zig")]
    [InlineData(false, "actual-format-concat.zig")]
    [InlineData(true, "actual-format-concat.zig")]
    public void Actual_mode_report_bound_preserves_encoding_and_short_buffer_errors(bool objects, string fixture)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        var source = Environment.GetEnvironmentVariable("DOTCC_GHOSTTY_MODES");
        if (string.IsNullOrEmpty(library) || string.IsNullOrEmpty(source))
            Assert.Skip("Set DOTCC_TARGET_ZIG_LIB and DOTCC_GHOSTTY_MODES to the pinned Zig library and terminal/modes.zig.");
        var path = Fixture(fixture);
        var options = new ZigCompilationOptions("root", [new("root", path, [new("modes", "modes")]), new("modes", source, [])],
            outputMode: ZigSourceOutputMode.Lib, optimizeMode: ZigSourceOptimizeMode.Fast,
            zigLibDir: library, stdlibTypes: ZigStdlibTypeModel.Source, functionPointerAbi: ZigFunctionPointerAbi.Managed,
            managedRoots: ["answer"]);
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-mode-report-" + Guid.NewGuid().ToString("N"));
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
            emitted += "\npublic static class ModeReportHost { public static int Main() => DotCcLib.answer(); }";
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    private static string Fixture(string fixture, [CallerFilePath] string file = "")
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigSourceFileType", fixture);
}
