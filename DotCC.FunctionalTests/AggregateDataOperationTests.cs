using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class AggregateDataOperationTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Constant_operations_match_native_bytes_from_source_and_objects(bool objects, bool aggregate)
    {
        string fixture = Path.Combine(AppContext.BaseDirectory, "Fixtures", "aggregate-byte-data-operations");
        string directory = Path.Combine(Path.GetTempPath(), "dotcc-aggregate-operations-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string[] paths = Directory.GetFiles(fixture, "*.c").Order(StringComparer.Ordinal).ToArray();
            if (objects)
            {
                paths = paths.Select((path, index) =>
                {
                    string obj = Path.Combine(directory, "unit" + index + ".o");
                    File.WriteAllText(obj, Compiler.EmitObject(path, includeDirs: [fixture],
                        outputOptions: new(AggregateData: aggregate)));
                    return obj;
                }).ToArray();
            }
            var options = new CSharpOutputOptions(Runtime: RuntimeProfile.C, AggregateData: aggregate && !objects);
            string source = objects
                ? Compiler.LinkObjects(paths, emit: EmitMode.Csproj, outputOptions: options)
                : Compiler.EmitCSharp(paths, includeDirs: [fixture], emit: EmitMode.Csproj, outputOptions: options);

            // There is exactly one eligible table. Pin its entire extent, so
            // an unrelated blob or a fallback for any row cannot satisfy this.
            string[] markers = source.Split('\n').Select(line => line.Trim())
                .Where(line => line.StartsWith("// aggregate-data:", StringComparison.Ordinal)).ToArray();
            string[] expectedMarkers = aggregate ? ["// aggregate-data: 1984 bytes"] : [];
            markers.ShouldBe(expectedMarkers);

            var result = FixtureRunner.CompileAndRunCapturingExit(source, []);
            result.exit.ShouldBe(0);
            result.stdout.ReplaceLineEndings("\n").ShouldBe(
                File.ReadAllText(Path.Combine(fixture, "expected-stdout.txt")).ReplaceLineEndings("\n"));
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }
}
