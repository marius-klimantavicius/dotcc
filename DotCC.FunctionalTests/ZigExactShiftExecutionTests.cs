using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigExactShiftExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Exact_shifts_execute_from_source_and_objects(bool objects)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-exact-shift-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(directory, "lib", "std"));
        try
        {
            File.WriteAllText(Path.Combine(directory, "lib", "std", "std.zig"), "");
            var path = Fixture();
            var options = new ZigCompilationOptions("root", [new("root", path, [])],
                zigLibDir: Path.Combine(directory, "lib"), stdlibTypes: ZigStdlibTypeModel.Source,
                functionPointerAbi: ZigFunctionPointerAbi.Managed);
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.csobj");
                File.WriteAllText(fragment, Compiler.EmitObject(path, zigOptions: options));
                emitted = Compiler.LinkObjects([fragment], emit: EmitMode.Csproj);
            }
            else emitted = Compiler.EmitCSharp([path], emit: EmitMode.Csproj, zigOptions: options);
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    private static string Fixture([CallerFilePath] string file = "")
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigSourceFileType", "exact-shift.zig");
}
