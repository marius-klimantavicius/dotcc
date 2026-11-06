using Xunit;

namespace DotCC.Tests;

[Collection("ZigFrontend")]
public sealed class ZigAtomicCompareExchangeTests
{
    [Theory]
    [InlineData("var n: u32 = 0; _ = @cmpxchgStrong(u32, &n, 0, 1, .unordered, .monotonic);", "invalid memory order")]
    [InlineData("var n: u32 = 0; _ = @cmpxchgStrong(u32, &n, 0, 1, .seq_cst, .unordered);", "invalid memory order")]
    [InlineData("var n: u32 = 0; _ = @cmpxchgStrong(u32, &n, 0, 1, .seq_cst, .release);", "invalid memory order")]
    [InlineData("var n: u32 = 0; _ = @cmpxchgStrong(u32, &n, 0, 1, .seq_cst, .acq_rel);", "invalid memory order")]
    [InlineData("var n: u32 = 0; _ = @cmpxchgStrong(u32, &n, 0, 1, .monotonic, .acquire);", "stronger than success")]
    [InlineData("var n: u32 = 0; _ = @cmpxchgStrong(u32, &n, 0, 1, .acq_rel, .seq_cst);", "stronger than success")]
    [InlineData("const n: u32 = 0; _ = @cmpxchgStrong(u32, &n, 0, 1, .acq_rel, .acquire);", "const pointer")]
    [InlineData("var n: u64 = 0; _ = @cmpxchgStrong(u32, &n, 0, 1, .acq_rel, .acquire);", "pointer does not address")]
    [InlineData("var n: u31 = 0; _ = @cmpxchgStrong(u32, &n, 0, 1, .acq_rel, .acquire);", "atomic source type")]
    [InlineData("var b: [4]u8 = undefined; const p: *align(1) u32 = @ptrCast(&b); _ = @cmpxchgStrong(u32, p, 0, 1, .acq_rel, .acquire);", "alignment is insufficient")]
    [InlineData("var n: u3 = 0; _ = @cmpxchgStrong(u3, &n, 0, 1, .acq_rel, .acquire);", "exact 8, 16, 32 or 64 bit")]
    [InlineData("var n: bool = false; _ = @cmpxchgStrong(bool, &n, false, true, .acq_rel, .acquire);", "storage type is not modeled")]
    [InlineData("var n: u32 = 0; _ = @cmpxchgStrong(u32, &n, false, 1, .acq_rel, .acquire);", "operand must coerce")]
    [InlineData("var n: u8 = 0; _ = @cmpxchgStrong(u8, &n, 0, 256, .acq_rel, .acquire);", "operand does not fit")]
    [InlineData("var big: u64 = 0; var n: u32 = 0; _ = @cmpxchgStrong(u32, &n, @atomicLoad(u64, &big, .monotonic), 1, .acq_rel, .acquire);", "operand must coerce")]
    [InlineData("var n: u128 = 0; _ = @cmpxchgStrong(u128, &n, 0, 1, .acq_rel, .acquire);", "storage type is not modeled")]
    [InlineData("const Order = enum { monotonic, acquire, release, acq_rel, seq_cst }; var n: u32 = 0; _ = @cmpxchgStrong(u32, &n, 0, 1, Order.seq_cst, Order.acquire);", "requires std.lang.AtomicOrder")]
    [InlineData("var n: u32 = 0; _ = @cmpxchgStrong(u32, &n, 0, 1, @atomicLoad(u32, &n, .monotonic), .monotonic);", "comptime-known AtomicOrder")]
    public void Invalid_orders_and_storage_fail_before_runtime(string body, string diagnostic)
        => new ZigAtomicAccessTests().Invalid_or_unsupported_atomic_accesses_fail_before_runtime(body, diagnostic);
}
