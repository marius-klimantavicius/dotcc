using Xunit;

namespace DotCC.Tests;

[Collection("ZigFrontend")]
public sealed class ZigPointerCasTests
{
    [Theory]
    [InlineData("var n: u7 = 1; var p: ?*u8 = null; _ = @cmpxchgStrong(?*u8, &p, null, &n, .seq_cst, .monotonic);", "atomic source pointee type")]
    [InlineData("var n: u7 = 1; var p: *u7 = &n; var m: u8 = 1; _ = @cmpxchgStrong(*u8, &p, &m, &m, .seq_cst, .monotonic);", "atomic source pointee type")]
    [InlineData("var n: u7 = 1; var p: ?*u8 = null; _ = @atomicRmw(?*u8, &p, .Xchg, &n, .monotonic);", "atomic source pointee type")]
    [InlineData("var p: ?*u8 = null; var n: u16 = 1; _ = @cmpxchgStrong(?*u8, &p, null, &n, .seq_cst, .monotonic);", "atomic pointer type")]
    [InlineData("const p: ?*u8 = null; _ = @cmpxchgWeak(?*u8, &p, null, null, .release, .monotonic);", "const pointer")]
    [InlineData("var n: u8 = 1; var p: *u8 = &n; _ = @cmpxchgStrong(*u8, &p, null, &n, .seq_cst, .monotonic);", "null operand requires")]
    [InlineData("const n: u8 = 1; var p: ?*u8 = null; _ = @cmpxchgStrong(?*u8, &p, null, &n, .seq_cst, .monotonic);", "atomic pointer type")]
    [InlineData("var n: u8 = 1; var p: *u8 = &n; const maybe: ?*u8 = &n; _ = @cmpxchgStrong(*u8, &p, &n, maybe, .seq_cst, .monotonic);", "atomic pointer type")]
    [InlineData("var p: ?*u8 = null; const slot: *align(1) ?*u8 = @ptrCast(&p); _ = @cmpxchgStrong(?*u8, slot, null, null, .seq_cst, .monotonic);", "alignment is insufficient")]
    [InlineData("var p: ?*u8 = null; _ = @atomicRmw(?*u8, &p, .Add, null, .monotonic);", "only supports Xchg")]
    [InlineData("const p: ?*u8 = null; _ = @atomicRmw(?*u8, &p, .Xchg, null, .monotonic);", "const pointer")]
    [InlineData("const S = packed struct(u7) { bits: u7 }; var s: S = .{ .bits = 0 }; _ = @atomicLoad(S, &s, .monotonic);", "exact 8, 16, 32 or 64 bit source layout")]
    [InlineData("const S = packed struct(u64) { bits: u64 }; var s: S = .{ .bits = 0 }; _ = @atomicRmw(S, &s, .Add, .{ .bits = 1 }, .monotonic);", "only supports bitwise")]
    [InlineData("const S = packed struct(u64) { bits: u64 }; var s: S = .{ .bits = 0 }; _ = @atomicRmw(S, &s, .Or, @as(u64, 1), .monotonic);", "requested packed source type")]
    public void Unsupported_pointer_and_packed_atomic_contracts_are_rejected(string body, string diagnostic)
        => new ZigAtomicAccessTests().Invalid_or_unsupported_atomic_accesses_fail_before_runtime(body, diagnostic);
}
