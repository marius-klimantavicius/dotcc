fn check(comptime T: type, comptime N: usize, comptime C: type) bool {
    const V = @Vector(N, T); const Counts = @Vector(N, C);
    var a: [N]T = undefined; var counts: [N]C = undefined;
    for (0..N) |i| { a[i] = @intCast(i + 1); counts[i] = @intCast(i % @bitSizeOf(T)); }
    const v: V = a; const c: Counts = counts;
    const left = v << c; const right = v >> c;
    inline for (0..N) |i| {
        if (left[i] != @as(T, a[i] << counts[i]) or right[i] != @as(T, a[i] >> counts[i])) return false;
    }
    return true;
}
var calls: u8 = 0;
fn getCounts() @Vector(8, u6) { calls += 1; return .{ 0, 1, 2, 3, 31, 32, 62, 63 }; }
pub fn main() u8 {
    if (!check(u8, 8, u3) or !check(u16, 8, u4) or !check(u32, 8, u5) or !check(u64, 8, u6)) return 1;
    const v: @Vector(8, u64) = @splat(1);
    const shifted = v << block: { break :block getCounts(); };
    if (calls != 1 or shifted[7] != 0x8000000000000000 or shifted[0] != 1 or shifted[5] != 0x100000000) return 2;
    const signed: @Vector(8, i32) = @splat(-128);
    const c: @Vector(8, u5) = @splat(3);
    const sr = signed >> c;
    if (sr[0] != -16 or sr[7] != -16) return 3;
    return 42;
}
