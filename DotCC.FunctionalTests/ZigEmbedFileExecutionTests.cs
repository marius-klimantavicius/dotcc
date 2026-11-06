using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigEmbedFileExecutionTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Exact_resource_bytes_and_source_types_survive_emission_and_resource_removal(bool objects, bool aggregate)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-zig-embed-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var source = Path.Combine(directory, "source");
            Directory.CreateDirectory(Path.Combine(source, "embed", "res"));
            var fixtures = Fixtures();
            foreach (var name in new[] { "embed-file.zig", "embed/provider.zig", "embed/res/bytes.bin", "embed/res/empty.bin", "embed/res/bom.bin" })
                File.Copy(Path.Combine(fixtures, name), Path.Combine(source, name));
            var root = Path.Combine(source, "embed-file.zig");
            var library = Path.Combine(directory, "lib");
            Directory.CreateDirectory(Path.Combine(library, "std"));
            File.WriteAllText(Path.Combine(library, "std", "std.zig"), "");
            var zig = new ZigCompilationOptions("root", [new("root", root, [])], zigLibDir: library,
                stdlibTypes: ZigStdlibTypeModel.Source, functionPointerAbi: ZigFunctionPointerAbi.Managed);
            var output = new CSharpOutputOptions(AggregateData: aggregate, LiteralPool: true);
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.csobj");
                File.WriteAllText(fragment, Compiler.EmitObject(root, zigOptions: zig, outputOptions: new(AggregateData: aggregate)));
                Directory.Delete(source, true);
                emitted = Compiler.LinkObjects([fragment], emit: EmitMode.Csproj, outputOptions: new(LiteralPool: true));
            }
            else
            {
                emitted = Compiler.EmitCSharp([root], emit: EmitMode.Csproj, zigOptions: zig, outputOptions: output);
                Directory.Delete(source, true);
            }
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    [Theory]
    [InlineData("pub fn main() u8 { return @embedFile(\"missing.bin\")[0]; }", "cannot read")]
    [InlineData("pub fn main() u8 { return @embedFile(\"bytes.bin/no-child\")[0]; }", "cannot read")]
    [InlineData("fn f(path: []const u8) u8 { return @embedFile(path)[0]; } pub fn main() u8 { return f(\"bytes.bin\"); }", "comptime")]
    [InlineData("pub fn main() u8 { @embedFile(\"bytes.bin\")[0] = 7; return 42; }", "cannot assign to constant")]
    [InlineData("pub fn main() u8 { const a: []u8 = @embedFile(\"bytes.bin\"); return a[0]; }", "mutable slice")]
    [InlineData("pub fn main() u8 { return @embedFile(\"bytes.bin\\x00suffix\")[0]; }", "zero bytes")]
    public void Invalid_paths_and_mutable_uses_are_compile_errors(string source, string diagnostic)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-zig-embed-invalid-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            File.WriteAllBytes(Path.Combine(directory, "bytes.bin"), [42]);
            var path = Path.Combine(directory, "main.zig");
            File.WriteAllText(path, source);
            Should.Throw<CompileException>(() => Compiler.EmitObject(path)).Message.ShouldContain(diagnostic);
        }
        finally { Directory.Delete(directory, true); }
    }

    private static string Fixtures([CallerFilePath] string file = "")
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigSourceFileType");
}
