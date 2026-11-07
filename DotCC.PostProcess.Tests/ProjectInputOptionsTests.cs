using DotCC.PostProcess;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace DotCC.PostProcess.Tests;

public sealed class ProjectInputOptionsTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void External_sdk_config_requires_the_exact_evaluated_generated_path(bool sdkGenerated)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-external-config-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var projectDirectory = Path.Combine(directory, "input");
            Directory.CreateDirectory(projectDirectory);
            var config = Path.Combine(directory, "artifacts", "Fixture.GeneratedMSBuildEditorConfig.editorconfig");
            Directory.CreateDirectory(Path.GetDirectoryName(config)!);
            File.WriteAllText(config, "is_global = true\nbuild_property.RootNamespace = Fixture\n");
            var args = new[] { "/target:library", "/analyzerconfig:" + config };
            var parsed = CSharpCommandLineParser.Default.Parse(args, projectDirectory, sdkDirectory: null);
            using var input = new ProjectInput(Path.Combine(projectDirectory, "Fixture.csproj"), "Fixture", "net10.0",
                "Release", parsed, args, [])
            {
                SdkGeneratedAnalyzerConfigPath = (sdkGenerated ? config : config + ".other").Replace('\\', '/'),
            };
            if (sdkGenerated)
                input.CreateCompilation(TestContext.Current.CancellationToken).AssemblyName.ShouldBe("Fixture");
            else
                Should.Throw<InvalidOperationException>(() => input.CreateCompilation(TestContext.Current.CancellationToken))
                    .Message.ShouldContain("Custom analyzer configuration is unsupported");
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
