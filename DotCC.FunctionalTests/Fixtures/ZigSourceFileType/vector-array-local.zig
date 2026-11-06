comptime { @export(&answer, .{ .name = "answer" }); }
const Cell = packed struct(u64) { code: u21, wide: u2, rest: u41 };
var calls: u8 = 0;
fn value() @Vector(16, u64) { calls += 1; return @splat(0x123456789abcdef0); }
fn answer() callconv(.c) u8 {
    const narrow: @Vector(16, u32) = @splat(0xfedcba98);
    const wide: @Vector(16, u64) = @intCast(narrow);
    const widened: [16]u64 = wide;
    if (widened[0] != 0xfedcba98 or widened[15] != 0xfedcba98) return 6;
    const counts: @Vector(16, u6) = @splat(3);
    const shifted = wide << counts;
    const uniform = wide << @splat(2);
    if (shifted[15] != 0x7f6e5d4c0 or uniform[0] != 0x3fb72ea60) return 8;
    const signed: @Vector(32, i32) = @splat(-128);
    const signed_counts: @Vector(32, u5) = @splat(3);
    const signed_result = signed >> signed_counts;
    if (signed_result[31] != -16) return 9;
    const implicit: @Vector(16, u64) = narrow;
    if (implicit[0] != 0xfedcba98 or implicit[15] != 0xfedcba98) return 7;
    var v: @Vector(16, u64) = @splat(42);
    v[15] = 99;
    const inter: [16]u64 = v;
    v[0] = 7;
    if (inter[0] != 42 or inter[15] != 99 or v[0] != 7) return 1;
    var cells: [18]Cell = undefined;
    cells[0] = .{ .code = 7, .wide = 1, .rest = 8 };
    cells[17] = .{ .code = 9, .wide = 2, .rest = 10 };
    cells[1..][0..16].* = @bitCast(inter);
    if (cells[1].code != 42 or cells[16].code != 99 or cells[1].wide != 0 or cells[16].rest != 0) return 4;
    if (cells[0].code != 7 or cells[17].rest != 10) return 5;
    var out: [16]u64 = value();
    if (calls != 1 or out[15] != 0x123456789abcdef0) return 2;
    out[0] = 11;
    if (out[1] != 0x123456789abcdef0 or inter[0] != 42) return 3;
    return 42;
}
