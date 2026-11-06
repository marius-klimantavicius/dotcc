const std = @import("std");
fn check(comptime pair_lanes: usize, seed: u64) bool {
    const Vp = @Vector(pair_lanes, u64);
    var wide: Vp = @splat(seed);
    var spacer: Vp = @splat(~seed);
    inline for (0..pair_lanes) |pair| {
        wide[pair] = seed ^ (@as(u64, pair) * 17);
        spacer[pair] = ~seed ^ (@as(u64, pair) * 9);
    }
    var inter: [2 * pair_lanes]u64 = undefined;
    inline for (0..pair_lanes) |pair| {
        inter[2 * pair] = wide[pair];
        inter[2 * pair + 1] = spacer[pair];
    }
    inline for (0..pair_lanes) |pair| {
        if (inter[2 * pair] != seed ^ (@as(u64, pair) * 17)) return false;
        if (inter[2 * pair + 1] != ~seed ^ (@as(u64, pair) * 9)) return false;
    }
    return true;
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    inline for (.{ 1, 2, 4, 8 }) |count| {
        if (!check(count, 0)) return 1;
        if (!check(count, 0x0123456789abcdef)) return 2;
        if (!check(count, 0xffffffffffffffff)) return 3;
    }
    return 42;
}
