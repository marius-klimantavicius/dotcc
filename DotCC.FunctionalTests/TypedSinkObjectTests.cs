using System;
using System.IO;
using System.Linq;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class TypedSinkObjectTests
{
    [Theory]
    [InlineData("bool-numeric-sinks", false)]
    [InlineData("bool-numeric-sinks", true)]
    [InlineData("array-arrow-member", false)]
    [InlineData("array-arrow-member", true)]
    [InlineData("fractionless-floats", false)]
    [InlineData("fractionless-floats", true)]
    public void Native_checked_typing_cases_execute_after_object_link(string name, bool nested)
    {
        var fixture = FixtureRunner.Discover().Single(row => row.name == name);
        var fragment = Path.Combine(Path.GetTempPath(), "dotcc-typed-sink-" + Guid.NewGuid().ToString("N") + ".obj.cs");
        try
        {
            File.WriteAllText(fragment, Compiler.EmitObject(fixture.sources.Single()));
            var emitted = nested
                ? Compiler.LinkObjects(new[] { fragment }, emit:EmitMode.ManagedLib, className:"Probe", namespaceName:"Typed.Sinks",
                    outputOptions:new CSharpOutputOptions(NestTypes:true, Runtime:RuntimeProfile.C))
                    + "\npublic static class EntryPoint { public static int Main() => Probe.main(); }\n"
                : Compiler.LinkObjects(new[] { fragment });
            var result = FixtureRunner.CompileAndRunCapturingExit(emitted, Array.Empty<string>());
            result.exit.ShouldBe(0);
            result.stdout.ShouldBe(fixture.expectedStdout);
        }
        finally { File.Delete(fragment); }
    }
}
