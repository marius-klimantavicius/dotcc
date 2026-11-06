using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigEnumFieldMaskExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Enum_fields_aliases_and_generic_type_arguments_keep_their_source_tag_width(bool objects)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        var path = Fixture();
        var options = new ZigCompilationOptions("root", [new("root", path, [])], zigLibDir: library,
            outputMode: ZigSourceOutputMode.Lib, optimizeMode: ZigSourceOptimizeMode.Fast,
            stdlibTypes: ZigStdlibTypeModel.Source, functionPointerAbi: ZigFunctionPointerAbi.Managed, managedRoots: ["answer"]);
        var directory = Path.Combine(Path.GetTempPath(), "enum-field-mask-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string source;
            if (objects)
            {
                var fragment = Path.Combine(directory, "mask.csobj");
                File.WriteAllText(fragment, Compiler.EmitObject(path, zigOptions: options));
                source = Compiler.LinkObjects([fragment], emit: EmitMode.ManagedLib);
            }
            else source = Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib, zigOptions: options);
            var result = FixtureRunner.CompileAndRunCapturingStreams(source
                + "\npublic static class EnumMaskHost { public static int Main() => DotCcLib.answer(); }", [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty(); result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }
    private static string Fixture([CallerFilePath] string caller = "")
        => Path.Combine(Path.GetDirectoryName(caller)!, "Fixtures", "ZigManagedControlFlow", "enum-field-mask.zig");
}
