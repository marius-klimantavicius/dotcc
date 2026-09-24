using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class QualifiedAssignmentValueTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Qualified_values_preserve_side_effects_in_source_objects_and_instances(bool objects, bool instances)
    {
        var fixture = FixtureRunner.Discover().Single(f => f.name == "qualified-assignment-values");
        string directory = Path.Combine(Path.GetTempPath(), "dotcc-qualified-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var options = instances ? new CSharpOutputOptions(Runtime: RuntimeProfile.C, InstanceMethods: true) : null;
            var mode = instances ? EmitMode.ManagedLib : EmitMode.Csproj;
            string emitted;
            if (objects)
            {
                string fragment = Path.Combine(directory, "main.o.cs");
                File.WriteAllText(fragment, Compiler.EmitObject(fixture.sources.Single(), outputOptions: instances ? new(InstanceMethods: true) : null));
                emitted = Compiler.LinkObjects([fragment], emit: mode, className: instances ? "QualifiedApi" : null, outputOptions: options);
            }
            else emitted = Compiler.EmitCSharp(fixture.sources, emit: mode, className: instances ? "QualifiedApi" : null, outputOptions: options);
            if (instances) emitted += """

                public static class QualifiedProbe {
                    public static int Main() {
                        using var first = new QualifiedApi(); using var second = new QualifiedApi();
                        using (first.__DotCcEnter()) if (first.main() != 0) return 1;
                        using (second.__DotCcEnter()) return second.main();
                    }
                }
                """;
            string expected = fixture.expectedStdout.ReplaceLineEndings("\n");
            if (instances) expected += expected;
            FixtureRunner.CompileAndRun(emitted, []).ReplaceLineEndings("\n").TrimEnd('\n').ShouldBe(expected.TrimEnd('\n'));
        }
        finally { Directory.Delete(directory, true); }
    }
}
