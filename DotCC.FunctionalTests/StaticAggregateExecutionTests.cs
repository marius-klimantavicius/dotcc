using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class StaticAggregateExecutionTests
{
    [Theory]
    [InlineData("array_global", false)]
    [InlineData("array_global", true)]
    [InlineData("custom_allocator", false)]
    [InlineData("custom_allocator", true)]
    public void Zig_static_aggregates_and_vtables_execute_from_source_and_objects(string name, bool objects)
    {
        // Reuse the oracle's fixed programs and expected observations without
        // making emitted C# execution depend on an installed Zig toolchain.
        var program = ZigOracleTests.Programs.Single(row => (string)row[0] == name);
        string directory = Path.Combine(Path.GetTempPath(), "dotcc-static-zig-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string input = Path.Combine(directory, "input.zig");
            File.WriteAllText(input, (string)program[1]);
            string emitted;
            if (objects)
            {
                string obj = Path.Combine(directory, "input.o");
                File.WriteAllText(obj, Compiler.EmitObject(input));
                emitted = Compiler.LinkObjects([obj], emit: EmitMode.Csproj);
            }
            else emitted = Compiler.EmitCSharp([input], emit: EmitMode.Csproj);
            var result = FixtureRunner.CompileAndRunCapturingExit(emitted, []);
            result.exit.ShouldBe((int)program[2]);
            result.stdout.ReplaceLineEndings("\n").TrimEnd('\n').ShouldBe(((string)program[3]).ReplaceLineEndings("\n").TrimEnd('\n'));
        }
        finally { Directory.Delete(directory, true); }
    }
}
