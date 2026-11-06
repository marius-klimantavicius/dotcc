using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("ZigFrontend")]
public sealed class ZigAtomicAccessTests
{
    [Theory]
    [InlineData("var n: u64 = 0; _ = @atomicRmw(u64, &n, .Add, 1, .unordered);", "invalid memory order")]
    [InlineData("var n: u7 = 0; _ = @atomicRmw(u7, &n, .Add, 1, .monotonic);", "exact 8, 16, 32 or 64 bit")]
    [InlineData("var n: u17 = 0; _ = @atomicRmw(u32, &n, .Add, 1, .monotonic);", "requested atomic source type")]
    [InlineData("var n: u8 = 0; _ = @atomicRmw(u8, &n, .Add, 256, .monotonic);", "operand does not fit")]
    [InlineData("const n: u32 = 0; _ = @atomicRmw(u32, &n, .Add, 1, .monotonic);", "const pointer")]
    [InlineData("var bytes: [8]u8 = undefined; const p: *align(1) u64 = @ptrCast(&bytes); _ = @atomicRmw(u64, p, .Add, 1, .monotonic);", "alignment is insufficient")]

    [InlineData("var n: u64 = 0; _ = @atomicLoad(u64, &n, .release);", "invalid memory order")]
    [InlineData("var n: u64 = 0; _ = @atomicLoad(u64, &n, .acq_rel);", "invalid memory order")]
    [InlineData("var n: u64 = 0; @atomicStore(u64, &n, 42, .acquire);", "invalid memory order")]
    [InlineData("var n: u64 = 0; @atomicStore(u64, &n, 42, .acq_rel);", "invalid memory order")]
    [InlineData("var n: u32 = 0; _ = @atomicLoad(u64, &n, .seq_cst);", "pointer does not address")]
    [InlineData("const n: u64 = 0; @atomicStore(u64, &n, 42, .seq_cst);", "const pointer")]
    [InlineData("var n: u3 = 0; _ = @atomicLoad(u3, &n, .seq_cst);", "exact 8, 16, 32 or 64 bit")]
    [InlineData("var bytes: [8]u8 = undefined; const p: *align(1) u64 = @ptrCast(&bytes); _ = @atomicLoad(u64, p, .seq_cst);", "alignment is insufficient")]
    public void Invalid_or_unsupported_atomic_accesses_fail_before_runtime(string body, string diagnostic)
    {
        var dir = Path.Combine(Path.GetTempPath(), "dotcc-zig-atomic-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(dir, "lib", "std"));
        try
        {
            File.WriteAllText(Path.Combine(dir, "lib", "std", "std.zig"), "");
            var path = Path.Combine(dir, "main.zig");
            File.WriteAllText(path, "pub fn main() u8 { " + body + " return 42; }");
            var options = new ZigCompilationOptions("root", [new("root", path, [])], zigLibDir: Path.Combine(dir, "lib"),
                stdlibTypes: ZigStdlibTypeModel.Source, functionPointerAbi: ZigFunctionPointerAbi.Managed);
            Should.Throw<CompileException>(() => Compiler.EmitObject(path, zigOptions: options)).Message.ShouldContain(diagnostic);
        }
        finally { Directory.Delete(dir, true); }
    }
}
