using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigBitcastWidthGuardPageTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Three_byte_read_ends_at_an_inaccessible_page_in_a_child_process(bool objects)
    {
        if (!OperatingSystem.IsLinux() || !BitConverter.IsLittleEndian) return;
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-width-guard-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(FixtureDirectory(), "guard-page.zig");
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "guard.object.cs");
                File.WriteAllText(fragment, Compiler.EmitObject(path));
                emitted = Compiler.LinkObjects(new[] { fragment }, emit: EmitMode.ManagedLib);
            }
            else emitted = Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib);
            var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
                .Select(p => MetadataReference.CreateFromFile(p));
            var compilation = CSharpCompilation.Create("WidthGuard", new[] {
                CSharpSyntaxTree.ParseText(emitted + Host, new CSharpParseOptions(LanguageVersion.Preview)) }, references,
                new CSharpCompilationOptions(OutputKind.ConsoleApplication, allowUnsafe: true,
                    optimizationLevel: OptimizationLevel.Release));
            var dll = Path.Combine(directory, "guard.dll");
            var result = compilation.Emit(dll);
            result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            File.WriteAllText(Path.Combine(directory, "guard.runtimeconfig.json"),
                "{\"runtimeOptions\":{\"tfm\":\"net10.0\",\"framework\":{\"name\":\"Microsoft.NETCore.App\",\"version\":\"10.0.0\"}}}");
            var good = Run(dll, false);
            if (Environment.GetEnvironmentVariable("DOTCC_WIDTH_EVIDENCE") is { Length: > 0 } evidence)
            {
                var saved = Path.Combine(evidence, objects ? "object" : "source");
                Directory.CreateDirectory(saved);
                File.WriteAllText(Path.Combine(saved, "emitted.cs"), emitted + Host);
                File.Copy(dll, Path.Combine(saved, "guard.dll"), true);
                File.Copy(Path.Combine(directory, "guard.runtimeconfig.json"), Path.Combine(saved, "guard.runtimeconfig.json"), true);
                File.WriteAllText(Path.Combine(saved, "good.json"), System.Text.Json.JsonSerializer.Serialize(good, new System.Text.Json.JsonSerializerOptions { IncludeFields = true }));
            }
            good.exit.ShouldBe(0, good.stdout + good.stderr);
            good.stdout.Trim().ShouldBe("value=563412 allocated=0");
            var bad = Run(dll, true);
            bad.exit.ShouldNotBe(0);
            bad.stderr.ShouldContain("AccessViolationException");
        }
        finally { Directory.Delete(directory, true); }
    }
    private static (int exit, string stdout, string stderr) Run(string dll, bool bad)
    {
        var info = new ProcessStartInfo("dotnet") { RedirectStandardOutput = true, RedirectStandardError = true };
        info.ArgumentList.Add(dll);
        if (bad) info.ArgumentList.Add("bad");
        info.Environment["DOTNET_DbgEnableMiniDump"] = "0";
        using var process = Process.Start(info)!;
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        if (!process.WaitForExit(10000)) { process.Kill(entireProcessTree: true); throw new TimeoutException("guard child"); }
        return (process.ExitCode, stdout.GetAwaiter().GetResult(), stderr.GetAwaiter().GetResult());
    }
    private static string FixtureDirectory([CallerFilePath] string file = "")
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigBitcastWidthR38");

    private const string Host = """

public static unsafe class WidthGuardHost
{
    [System.Runtime.InteropServices.DllImport("libc")] private static extern void* mmap(void* p, nuint n, int prot, int flags, int fd, nint offset);
    [System.Runtime.InteropServices.DllImport("libc")] private static extern int mprotect(void* p, nuint n, int prot);
    [System.Runtime.InteropServices.DllImport("libc")] private static extern int munmap(void* p, nuint n);
    [System.Runtime.InteropServices.DllImport("libc")] private static extern int setrlimit(int resource, ref Limit value);
    private struct Limit { public ulong Current; public ulong Maximum; }
    public static int Main(string[] args)
    {
        var limit = new Limit(); setrlimit(4, ref limit);
        var page = (nuint)System.Environment.SystemPageSize;
        var memory = (byte*)mmap(null, page * 2, 3, 0x22, -1, 0);
        if (memory == (void*)(-1)) return 10;
        try
        {
            var p = memory + page - 3;
            p[0] = 0x12; p[1] = 0x34; p[2] = 0x56;
            if (mprotect(memory + page, page, 0) != 0) return 11;
            if (args.Length != 0) { System.Console.WriteLine(System.Runtime.CompilerServices.Unsafe.ReadUnaligned<uint>(p)); return 12; }
            // Call the emitted method directly so delegate profiling is outside the meter.
            var source = new ConstSlice<byte>(p, 3);
            for (var i = 0; i < 2048; i++) DotCcLib.width_probe(source);
            System.GC.Collect(2, System.GCCollectionMode.Forced, blocking: true, compacting: true);
            var before = System.GC.GetAllocatedBytesForCurrentThread();
            uint value = 0;
            for (var i = 0; i < 10000; i++) value = DotCcLib.width_probe(source);
            var allocated = System.GC.GetAllocatedBytesForCurrentThread() - before;
            System.Console.WriteLine($"value={value:x} allocated={allocated}");
            return value == 0x563412 && allocated == 0 ? 0 : 13;
        }
        finally { munmap(memory, page * 2); }
    }
}
""";
}
