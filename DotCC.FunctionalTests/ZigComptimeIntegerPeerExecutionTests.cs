using System;
using System.IO;
using System.Runtime.CompilerServices;
using DotCC.Frontends;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigComptimeIntegerPeerExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Named_comptime_integer_peers_keep_source_widths_and_runtime_evaluation(bool objects)
        => new ZigSourceFileTypeExecutionTests()
            .Empty_file_structs_and_generic_module_reexports_keep_their_source_identity(objects, "comptime-integer-peers.zig");

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Actual_tabstops_accesses_both_sides_of_the_preallocation_boundary(bool objects)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-actual-tabstops-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var source = Path.Combine(Fixtures(), "actual-tabstops-peers.zig");
            var upstream = Path.GetFullPath(Path.Combine(Fixtures(), "../../../libghostty/ref/ghostty-93967b7f9478e94dcbb42125c8a90f34feb40d75/src"));
            var mirror = Path.Combine(directory, "upstream");
            Directory.CreateDirectory(mirror);
            foreach (var entry in Directory.EnumerateFileSystemEntries(upstream))
            {
                var target = Path.Combine(mirror, Path.GetFileName(entry));
                if (Directory.Exists(entry)) Directory.CreateSymbolicLink(target, entry);
                else File.CreateSymbolicLink(target, entry);
            }
            var bridge = Path.Combine(mirror, "dotcc-tabstops.zig");
            File.WriteAllText(bridge, "pub const Tabstops = @import(\"terminal/Tabstops.zig\");");
            var options = new ZigCompilationOptions("root", [new("root", source, [new("tabstops", "tabstops")]), new("tabstops", bridge, [])],
                outputMode: ZigSourceOutputMode.Lib, optimizeMode: ZigSourceOptimizeMode.Fast,
                zigLibDir: library, stdlibTypes: ZigStdlibTypeModel.Source, functionPointerAbi: ZigFunctionPointerAbi.Managed,
                managedRoots: ["answer"]);
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "tabstops.csobj");
                File.WriteAllText(fragment, Compiler.EmitObject(source, zigOptions: options));
                emitted = Compiler.LinkObjects([fragment], emit: EmitMode.ManagedLib);
            }
            else emitted = Compiler.EmitCSharp([source], emit: EmitMode.ManagedLib, zigOptions: options);
            emitted += "\npublic static class TabstopsHost { public static int Main() => DotCcLib.answer(); }";
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42); result.stdout.ShouldBeEmpty(); result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    [Theory]
    [InlineData("const big=100+200; fn f(x:u8)u8{return x+big;} pub fn main()u8{return f(2);}", "type 'u8' cannot represent integer value '300'")]
    [InlineData("const negative=0-1; fn f(x:usize)usize{return x+negative;} pub fn main()u8{return @intCast(f(2));}", "type 'u64' cannot represent integer value '-1'")]
    [InlineData("fn f(x:bool)u8{return @intFromBool(x)*10;} pub fn main()u8{return f(true);}", "type 'u1' cannot represent integer value '10'")]
    [InlineData("fn f(x:u16)u8{return x;} pub fn main()u8{return f(42);}", "expected type 'u8', found 'u16'")]
    [InlineData("const divisor=1+1; fn f(x:i5)i5{return x/divisor;} pub fn main()u8{return @intCast(f(4));}", "signed integers must use @divTrunc")]
    public void Unrepresentable_arithmetic_and_typed_runtime_narrowing_still_reject(string program, string diagnostic)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-peer-invalid-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(directory, "lib", "std"));
        try
        {
            var source = Path.Combine(directory, "main.zig");
            File.WriteAllText(source, program);
            File.WriteAllText(Path.Combine(directory, "lib", "std", "std.zig"), "");
            var options = new ZigCompilationOptions("root", [new("root", source, [])], zigLibDir: Path.Combine(directory, "lib"),
                stdlibTypes: ZigStdlibTypeModel.Source, functionPointerAbi: ZigFunctionPointerAbi.Managed);
            Should.Throw<CompileException>(() => Compiler.EmitObject(source, zigOptions: options)).Message.ShouldContain(diagnostic);
        }
        finally { Directory.Delete(directory, true); }
    }

    private static string Fixtures([CallerFilePath] string file = "") =>
        Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigSourceFileType");
}
