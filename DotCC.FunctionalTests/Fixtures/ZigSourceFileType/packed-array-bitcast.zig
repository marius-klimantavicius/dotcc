const Packed = packed struct(u128) { a: u61, b: u11, c: u53, d: bool, e: u2 };
var calls: u8 = 0;
fn make() Packed {
    calls += 1;
    return .{ .a = 0x123456789abcdef, .b = 0x3ff, .c = 0x123456789abcd, .d = true, .e = 2 };
}
pub fn main() u8 {
    const wide: [2]u64 = @bitCast(make());
    if (calls != 1) return 1;
    const expected = @as(u128, 0x123456789abcdef) | (@as(u128, 0x3ff) << 61) |
        (@as(u128, 0x123456789abcd) << 72) | (@as(u128, 1) << 125) | (@as(u128, 2) << 126);
    if (wide[0] != @as(u64, @truncate(expected)) or wide[1] != @as(u64, @truncate(expected >> 64))) return 2;
    var copy = wide;
    copy[0] ^= 1;
    if (copy[0] == wide[0] or copy[1] != wide[1]) return 3;
    const Short = packed struct(u16) { x: u5, y: i7, z: u4 };
    const short: Short = .{ .x = 31, .y = -7, .z = 9 };
    const bytes: [2]u8 = @bitCast(short);
    if (bytes[0] != 0x3f or bytes[1] != 0x9f) return 4;
    const Narrow = packed struct(u24) { low: u13, high: u11 };
    const narrow: Narrow = .{ .low = 0x1234, .high = 0x5ab };
    const compact: [3]u8 = @bitCast(narrow);
    const expected_narrow = @as(u32, 0x1234) | (@as(u32, 0x5ab) << 13);
    if (compact[0] != @as(u8, @truncate(expected_narrow)) or compact[1] != @as(u8, @truncate(expected_narrow >> 8)) or compact[2] != @as(u8, @truncate(expected_narrow >> 16))) return 5;
    return 42;
}
