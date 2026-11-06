using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigCaptureIfExecutionTests
{
    [Theory]
    [InlineData(false, "member.zig")]
    [InlineData(true, "member.zig")]
    [InlineData(false, "contexts.zig")]
    [InlineData(true, "contexts.zig")]
    [InlineData(false, "pointer-slot.zig")]
    [InlineData(true, "pointer-slot.zig")]
    [InlineData(false, "metadata.zig")]
    [InlineData(true, "metadata.zig")]
    [InlineData(false, "aligned-storage.zig")]
    [InlineData(true, "aligned-storage.zig")]
    public void Value_captures_preserve_storage_and_branch_control_flow(bool objects, string entry)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-capture-if-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(Fixtures(), entry);
            var options = Options(path, directory);
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.csobj");
                File.WriteAllText(fragment, Compiler.EmitObject(path, zigOptions: options));
                emitted = Compiler.LinkObjects([fragment], emit: EmitMode.Csproj);
            }
            else emitted = Compiler.EmitCSharp([path], emit: EmitMode.Csproj, zigOptions: options);
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    [Theory]
    [InlineData("fn change(p: *const ?u32) void { const v = if (p.*) |*v| v else { return; }; v.* = 3; }")]
    [InlineData("const Box = struct { value: ?u32 }; fn box(p: *const Box) *const Box { return p; } fn change(p: *const Box) void { const v = if (box(p).value) |*v| v else { return; }; v.* = 3; }")]
    [InlineData("fn change(p: *?u32) void { const cap = if (p.*) |*v| blk: { v = v; break :blk v; } else { return; }; _ = cap; }")]
    public void Readonly_payload_captures_reject_stores(string source)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-capture-if-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "readonly.zig");
            File.WriteAllText(path, source);
            Should.Throw<CompileException>(() => Compiler.EmitCSharp([path], zigOptions: Options(path, directory)))
                .Message.ShouldContain("cannot assign to constant");
        }
        finally { Directory.Delete(directory, true); }
    }

    [Theory]
    [InlineData("fn make() ?u32 { return 1; } fn read() void { const v = if (make()) |*v| v else { return; }; _ = v; }", "rvalue optional captures are not modeled")]
    [InlineData("fn read(opt: ?u32) u32 { return if (opt) |v| v else { var side: u32 = 1; side += 1; }; }", "falling-through block")]
    public void Unmodeled_storage_and_nonterminating_value_blocks_fail_explicitly(string source, string diagnostic)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-capture-if-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "unsupported.zig");
            File.WriteAllText(path, source);
            Should.Throw<CompileException>(() => Compiler.EmitCSharp([path], zigOptions: Options(path, directory)))
                .Message.ShouldContain(diagnostic);
        }
        finally { Directory.Delete(directory, true); }
    }

    private static ZigCompilationOptions Options(string path, string directory)
    {
        var library = Path.Combine(directory, "lib");
        Directory.CreateDirectory(Path.Combine(library, "std"));
        File.WriteAllText(Path.Combine(library, "std", "std.zig"), "");
        return new ZigCompilationOptions("root", [new("root", path, [])], zigLibDir: library,
            stdlibTypes: ZigStdlibTypeModel.Source, functionPointerAbi: ZigFunctionPointerAbi.Managed);
    }
    private static string Fixtures([CallerFilePath] string file = "")
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigCaptureIf");
}
