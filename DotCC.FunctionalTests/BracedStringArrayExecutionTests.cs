using System;
using System.IO;
using System.Linq;
using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class BracedStringArrayExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Inferred_braced_strings_own_all_code_units_and_terminators(bool objects)
    {
        var fixture = FixtureRunner.Discover().Single(row => row.name == "braced-string-arrays");
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-braced-strings-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string generated;
            if (objects)
            {
                var path = Path.Combine(directory, "main.o");
                File.WriteAllText(path, Compiler.EmitObject(fixture.sources.Single()));
                generated = Compiler.LinkObjects(new[] { path });
            }
            else generated = Compiler.EmitCSharp(fixture.sources);
            var observed = FixtureRunner.CompileAndRunCapturingExit(generated, Array.Empty<string>());
            observed.exit.ShouldBe(0);
            observed.stdout.Trim().ShouldBe(fixture.expectedStdout.Trim());
        }
        finally { Directory.Delete(directory, true); }
    }
}
