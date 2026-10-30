using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigMembershipNameExecutionTests
{
    [Theory]
    [InlineData("ascii", false)]
    [InlineData("ascii", true)]
    [InlineData("captured", false)]
    [InlineData("captured", true)]
    [InlineData("ownership", false)]
    [InlineData("ownership", true)]
    [InlineData("session", false)]
    [InlineData("session", true)]
    public void Native_backed_names_resolve_and_execute(string fixture, bool objects)
        => RunFixture(fixture, objects);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Quoted_name_identity_is_a_pending_native_valid_boundary(bool objects)
        => RunFixture("computed", objects);

    private static void RunFixture(string fixture, bool objects)
    {
        var directory = Path.Combine(Path.GetTempPath(), "zig-membership-name-run-" + Guid.NewGuid().ToString("N"));
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
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigMembershipNameR36");
}
