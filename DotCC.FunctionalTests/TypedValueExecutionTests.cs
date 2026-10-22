using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class TypedValueExecutionTests
{
    [Theory]
    [InlineData("typed-value-loads", false, false)]
    [InlineData("typed-value-loads", true, false)]
    [InlineData("typed-value-loads", false, true)]
    [InlineData("typed-value-loads", true, true)]
    [InlineData("typed-value-scalars", false, false)]
    [InlineData("typed-value-scalars", true, false)]
    [InlineData("typed-value-scalars", false, true)]
    [InlineData("typed-value-scalars", true, true)]
    [InlineData("typed-value-array-address", false, false)]
    [InlineData("typed-value-array-address", true, false)]
    [InlineData("typed-value-array-address", false, true)]
    [InlineData("typed-value-array-address", true, true)]
    [InlineData("canonical-allocation-boundaries", false, false)]
    [InlineData("canonical-allocation-boundaries", true, false)]
    [InlineData("canonical-allocation-boundaries", false, true)]
    [InlineData("canonical-allocation-boundaries", true, true)]
    public void Actual_typed_values_execute_in_source_objects_and_owners(string name, bool objects, bool owners)
    {
        var fixture = FixtureRunner.Discover().Single(f => f.name == name);
        string directory = Path.Combine(Path.GetTempPath(), "dotcc-typed-values-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var options = owners ? new CSharpOutputOptions(Runtime: RuntimeProfile.C, InstanceMethods: true) : null;
            var mode = owners ? EmitMode.ManagedLib : EmitMode.Csproj;
            string emitted;
            if (objects)
            {
                var fragments = fixture.sources.Select((source, index) =>
                {
                    string fragment = Path.Combine(directory, index + ".o.cs");
                    File.WriteAllText(fragment, Compiler.EmitObject(source, outputOptions: owners ? new(InstanceMethods: true) : null));
                    return fragment;
                }).ToArray();
                emitted = Compiler.LinkObjects(fragments, emit: mode, className: owners ? "TypedApi" : null, outputOptions: options);
            }
            else emitted = Compiler.EmitCSharp(fixture.sources, emit: mode, className: owners ? "TypedApi" : null, outputOptions: options);
            if (owners) emitted += """

                public static class TypedValueProbe {
                    public static int Main() {
                        using var first = new TypedApi(); using var second = new TypedApi();
                        using (first.__DotCcEnter()) { if (first.main() != 0) return 101; }
                        using (second.__DotCcEnter()) { if (second.main() != 0) return 102; }
                        return 0;
                    }
                }
                """;
            var result = FixtureRunner.CompileAndRunCapturingExit(emitted, []);
            result.exit.ShouldBe(0);
            string expected = fixture.expectedStdout.ReplaceLineEndings("\n");
            if (owners) expected += expected;
            result.stdout.ReplaceLineEndings("\n").ShouldBe(expected);
        }
        finally { Directory.Delete(directory, true); }
    }
}
