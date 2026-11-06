using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using DotCC.Frontends;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigHostExternExecutionTests
{
    [Fact]
    public void Same_named_user_extern_does_not_acquire_a_source_stdlib_binding()
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-user-host-extern-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "main.zig");
            File.WriteAllText(path, "extern fn openat64(value: u8) u8; pub fn main() u8 { return openat64(42); }");
            var library = Path.Combine(directory, "lib");
            Directory.CreateDirectory(Path.Combine(library, "std"));
            File.WriteAllText(Path.Combine(library, "std", "std.zig"), "");
            var options = new ZigCompilationOptions("root", [new("root", path, [])], zigLibDir: library,
                stdlibTypes: ZigStdlibTypeModel.Source, functionPointerAbi: ZigFunctionPointerAbi.Managed);
            var fragment = Compiler.EmitObject(path, zigOptions: options);
            fragment.ShouldContain("openat64(");
            fragment.ShouldNotContain("__dotcc_zig_c_openat64(");
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public void Libc_host_services_preserve_descriptors_errno_memory_and_ownership()
    {
        if (!OperatingSystem.IsLinux() || System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture
            != System.Runtime.InteropServices.Architecture.X64) Assert.Skip("Linux x64 host control");
        ZigHostExternRuntimeControl.Run(Path.GetTempPath()).ShouldBe(42);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Declared_source_libc_calls_bind_to_managed_host_services(bool objects)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-source-host-extern-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Fixture();
            var options = new ZigCompilationOptions("root", [new("root", path, [])],
                outputMode: ZigSourceOutputMode.Lib, optimizeMode: ZigSourceOptimizeMode.Fast,
                zigLibDir: library, stdlibTypes: ZigStdlibTypeModel.Source,
                functionPointerAbi: ZigFunctionPointerAbi.Managed, managedRoots: ["answer"]);
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "host.csobj");
                File.WriteAllText(fragment, Compiler.EmitObject(path, zigOptions: options));
                emitted = Compiler.LinkObjects([fragment], emit: EmitMode.ManagedLib);
            }
            else emitted = Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib, zigOptions: options);
            var filename = JsonSerializer.Serialize(Path.Combine(directory, "file"));
            emitted += "\npublic static unsafe class SourceHost { public static int Main() { var p = global::System.Runtime.InteropServices.Marshal.StringToCoTaskMemUTF8("
                + filename + "); try { return DotCcLib.answer((byte*)p); } finally { global::System.Runtime.InteropServices.Marshal.FreeCoTaskMem(p); } } }";
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42); result.stdout.ShouldBeEmpty(); result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    private static string Fixture([CallerFilePath] string file = "") =>
        Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigSourceFileType", "source-host-externs.zig");
}
