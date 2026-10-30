using System;
using System.IO;
using System.Linq;
using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class DeclaratorInitializerExecutionTests
{
    [Theory]
    [InlineData("mixed-declarator-initializers")]
    [InlineData("block-function-declarations")]
    [InlineData("return-comma-expression")]
    [InlineData("typedef-alias-list")]
    [InlineData("nested-switch-entry")]
    [InlineData("grouped-object-declarators")]
    [InlineData("designated-inline-buffers")]
    [InlineData("unary-token-boundaries")]
    [InlineData("self-linked-static-storage")]
    [InlineData("static-flexible-initializer")]
    [InlineData("callback-value-dereference")]
    [InlineData("qualified-aggregate-projection")]
    [InlineData("goto-guarded-initializers")]
    [InlineData("lazy-comma-local-address")]
    [InlineData("local-callback-arrays")]
    [InlineData("partitioned-static-initializers")]
    [InlineData("large-static-aggregate")]
    [InlineData("large-sparse-static-aggregate")]
    [InlineData("multiline-comment-directives")]
    [InlineData("scoped-global-addresses")]
    [InlineData("constant-byte-payloads")]
    public void Object_link_preserves_declaration_values_and_scope(string name)
    {
        var fixture = FixtureRunner.Discover().Single(f => f.name == name);
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-mixed-declarators-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var objects = fixture.sources.Select((source, index) =>
            {
                var path = Path.Combine(directory, index + ".cs");
                File.WriteAllText(path, Compiler.EmitObject(source, includeDirs: new[] { fixture.dir }));
                return path;
            }).ToArray();
            var result = FixtureRunner.CompileAndRunCapturingExit(Compiler.LinkObjects(objects), Array.Empty<string>());
            result.exit.ShouldBe(0);
            result.stdout.Trim().ShouldBe(fixture.expectedStdout.Trim());
        }
        finally { Directory.Delete(directory, true); }
    }
}
