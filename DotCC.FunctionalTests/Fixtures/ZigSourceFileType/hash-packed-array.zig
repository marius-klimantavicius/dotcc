const std = @import("std");
const Packed = packed struct(u128) { low: u64, high: u64 };
fn select(comptime T: type, input: T) T {
    const bits = @bitSizeOf(T);
    const value = switch (bits) {
        16 => @as(u16, input),
        32 => @as(u32, input),
        64 => @as(u64, input),
        else => blk: { @compileError("unsupported test width"); break :blk input; },
    };
    return @bitCast(value);
}
fn bits(value: anytype) u8 { return @typeInfo(@TypeOf(value)).int.bits; }
fn reference(input: u64) u64 {
    var x = input;
    x = (x ^ (x >> 32)) *% 0xbea225f9eb34556d;
    x = (x ^ (x >> 29)) *% 0xbea225f9eb34556d;
    x = (x ^ (x >> 32)) *% 0xbea225f9eb34556d;
    return x ^ (x >> 29);
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var index: u64 = 0;
    while (index < 100) : (index += 1) {
        const packed_value: Packed = .{ .low = index * 123, .high = index * 987 };
        const wide: [2]u64 = @bitCast(packed_value);
        const input = wide[0] ^ wide[1];
        if (bits(wide[0] ^ wide[1]) != 64) return 1;
        if (std.hash.int(input) != reference(input)) return 2;
        if (select(u64, input) != input) return 3;
    }
    if (select(u16, 17) != 17 or select(u32, 23) != 23) return 4;
    const narrow: [2]u21 = .{ 1, 2 };
    if (bits(narrow[0] ^ narrow[1]) != 21) return 5;
    return 42;
}
