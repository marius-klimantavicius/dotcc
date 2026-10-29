using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class AttributedTagTypedefExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Attributed_forward_typedef_keeps_physical_packed_storage(bool objects)
    {
        var fixture = Path.Combine(AppContext.BaseDirectory, "Fixtures", "attributed-tag-typedef");
        var source = Path.Combine(fixture, "main.c");
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-attributed-tag-run-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.cs");
                File.WriteAllText(fragment, Compiler.EmitObject(source));
                emitted = Compiler.LinkObjects(new[] { fragment });
            }
            else emitted = Compiler.EmitCSharp(new[] { source });
            var result = FixtureRunner.CompileAndRunCapturingExit(emitted, Array.Empty<string>());
            result.exit.ShouldBe(0);
            result.stdout.ReplaceLineEndings("\n").ShouldBe(File.ReadAllText(Path.Combine(fixture, "expected-stdout.txt")).ReplaceLineEndings("\n"));
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
