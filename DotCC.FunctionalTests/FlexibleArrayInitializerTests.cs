#nullable enable
using System;
using System.IO;
using System.Linq;
using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class FlexibleArrayInitializerTests
{
    [Fact]
    public void Static_flexible_tail_survives_separate_object_linking()
    {
        var fixture = FixtureRunner.Discover().Single(f => f.name == "static-flexible-initializer");
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-flexlink-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var objects = fixture.sources.Select((source, index) =>
            {
                var path = Path.Combine(directory, index + ".cs");
                File.WriteAllText(path, Compiler.EmitObject(source, includeDirs: new[] { fixture.dir }));
                return path;
            }).ToArray();
            var emitted = Compiler.LinkObjects(objects, emit: EmitMode.Csproj);
            FixtureRunner.CompileAndRun(emitted, Array.Empty<string>()).Trim()
                .ShouldBe(fixture.expectedStdout.Trim());
        }
        finally { Directory.Delete(directory, true); }
    }
}
