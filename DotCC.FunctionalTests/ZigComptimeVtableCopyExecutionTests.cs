using System;
using System.IO;
using System.Runtime.CompilerServices;
using DotCC.Ir;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigComptimeVtableCopyExecutionTests
{
    [Theory]
    [InlineData("anonymous.zig", false)]
    [InlineData("anonymous.zig", true)]
    [InlineData("named.zig", false)]
    [InlineData("named.zig", true)]
    public void Immutable_vtable_copy_keeps_callbacks_and_independent_value_storage(string fixture, bool objects)
    {
        var path = Path.Combine(FixtureDirectory(), fixture);
        var options = Options(path);
        var directory = Path.Combine(Path.GetTempPath(), "comptime-vtable-copy-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.csobj");
                File.WriteAllText(fragment, Compiler.EmitObject(path, zigOptions: options));
                emitted = Compiler.LinkObjects([fragment], emit: EmitMode.ManagedLib);
            }
            else emitted = Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib, zigOptions: options);
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted
                + "\npublic static class VtableCopyHost { public static int Main() => DotCcLib.answer(); }", [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty(); result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Copying_an_immutable_pointer_field_does_not_make_its_mutable_pointee_comptime(bool objects)
    {
        var path = Path.Combine(FixtureDirectory(), "mutable-pointee.zig");
        var options = Options(path);
        var error = Should.Throw<IrUnsupportedException>(() =>
        {
            if (objects) Compiler.EmitObject(path, zigOptions: options);
            else Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib, zigOptions: options);
        });
        error.Message.ShouldContain("forbidden");
        error.Message.ShouldContain("did not evaluate at compile time");
    }

    private static ZigCompilationOptions Options(string path)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        return new ZigCompilationOptions("root", [new("root", path, [])], zigLibDir: library,
            outputMode: ZigSourceOutputMode.Lib, optimizeMode: ZigSourceOptimizeMode.Debug,
            stdlibTypes: ZigStdlibTypeModel.Source, functionPointerAbi: ZigFunctionPointerAbi.Managed,
            managedRoots: ["answer"], isTest: true);
    }
    private static string FixtureDirectory([CallerFilePath] string file = "")
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigComptimeVtableCopy");
}
