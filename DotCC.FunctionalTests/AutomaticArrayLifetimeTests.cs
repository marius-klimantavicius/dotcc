using System;
using System.IO;
using System.Linq;
using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class AutomaticArrayLifetimeTests
{
    [Fact]
    public void Reentered_array_initializers_and_live_recursive_storage_survive_object_linking()
    {
        var fixture = FixtureRunner.Discover().Single(row => row.name == "automatic-array-lifetime");
        var temporary = Path.Combine(Path.GetTempPath(), "dotcc-array-lifetime-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporary);
        try
        {
            var fragment = Path.Combine(temporary, "main.o.cs");
            File.WriteAllText(fragment, Compiler.EmitObject(fixture.sources.Single()));
            var generated = Compiler.LinkObjects(new[] { fragment });
            var result = FixtureRunner.CompileAndRunCapturingExit(generated, Array.Empty<string>());
            result.exit.ShouldBe(0);
            result.stdout.Trim().ShouldBe(fixture.expectedStdout.Trim());
        }
        finally { Directory.Delete(temporary, true); }
    }
}
