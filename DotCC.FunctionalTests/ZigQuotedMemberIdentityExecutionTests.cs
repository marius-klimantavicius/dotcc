using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigQuotedMemberIdentityExecutionTests
{
    [Theory]
    [InlineData("declared", false)]
    [InlineData("declared", true)]
    [InlineData("reified", false)]
    [InlineData("reified", true)]
    [InlineData("unions", false)]
    [InlineData("unions", true)]
    [InlineData("structs", false)]
    [InlineData("structs", true)]
    [InlineData("unicode", false)]
    [InlineData("unicode", true)]
    [InlineData("collision", false)]
    [InlineData("collision", true)]
    public void Native_backed_member_identities_execute_in_both_output_paths(string fixture, bool objects)
    {
        var directory = Path.Combine(Path.GetTempPath(), "zig-quoted-member-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            foreach (var file in Directory.GetFiles(FixtureDirectory(), "*.zig"))
                File.Copy(file, Path.Combine(directory, Path.GetFileName(file)));
            var path = Path.Combine(directory, fixture + ".zig");
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.object.cs");
                File.WriteAllText(fragment, Compiler.EmitObject(path));
                emitted = Compiler.LinkObjects(new[] { fragment }, emit: EmitMode.Csproj);
            }
            else emitted = Compiler.EmitCSharp(new[] { path }, emit: EmitMode.Csproj);
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, Array.Empty<string>());
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    private static string FixtureDirectory([CallerFilePath] string file = "")
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigQuotedMemberIdentityR43");
}
