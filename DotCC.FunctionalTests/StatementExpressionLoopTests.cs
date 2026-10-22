using System;
using System.IO;
using System.Linq;
using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class StatementExpressionLoopTests
{
    [Theory]
    [InlineData("gnu-statement-expression-loops")]
    [InlineData("for-comma-condition")]
    [InlineData("multidimensional-designators")]
    [InlineData("global-pointer-assignment")]
    [InlineData("integer-constant-cast-width")]
    public void Object_link_keeps_loop_evaluation(string name)
    {
        var fixture = FixtureRunner.Discover().Single(row => row.name == name);
        string directory = Path.Combine(Path.GetTempPath(), "dotcc-statement-loops-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string fragment = Path.Combine(directory, "loop.cs");
            File.WriteAllText(fragment, Compiler.EmitObject(fixture.sources.Single()));
            var result = FixtureRunner.CompileAndRunCapturingExit(Compiler.LinkObjects(new[] { fragment }), Array.Empty<string>());
            result.exit.ShouldBe(0);
            result.stdout.Trim().ShouldBe(fixture.expectedStdout.Trim());
        }
        finally { Directory.Delete(directory, true); }
    }
}
