using System;
using System.IO;
using System.Linq;
using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class ParenthesizedStringArrayExecutionTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Grouping_keeps_array_bytes_extent_identity_and_static_lifetime(bool objects, bool reverse)
    {
        var fixture = Path.Combine(AppContext.BaseDirectory, "Fixtures", "parenthesized-string-arrays");
        var sources = Directory.GetFiles(fixture, "*.c").OrderBy(path => path).ToArray();
        if (reverse) Array.Reverse(sources);
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-paren-arrays-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var options = new CSharpOutputOptions(Runtime: RuntimeProfile.C, LiteralPool: true);
            string generated;
            if (objects)
            {
                var inputs = sources.Select((path, index) =>
                {
                    var output = Path.Combine(directory, "unit" + index + ".o");
                    File.WriteAllText(output, Compiler.EmitObject(path));
                    return output;
                }).ToArray();
                generated = Compiler.LinkObjects(inputs, outputOptions: options);
            }
            else generated = Compiler.EmitCSharp(sources, outputOptions: options);
            var observed = FixtureRunner.CompileAndRunCapturingExit(generated, Array.Empty<string>());
            observed.exit.ShouldBe(0);
            observed.stdout.Trim().ShouldBe(File.ReadAllText(Path.Combine(fixture, "expected-stdout.txt")).Trim());
        }
        finally { Directory.Delete(directory, true); }
    }
}
