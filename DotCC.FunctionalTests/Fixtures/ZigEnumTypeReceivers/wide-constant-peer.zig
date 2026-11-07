const std = @import("std");
const Bits = packed struct(u128) { low: u64, high: u64 };
fn mix(value: Bits) u64 {
    const fibonacci_multiplier = 0x9e3779b97f4a7c15;
    const wide: [2]u64 = @bitCast(value);
    return @call(.always_inline, std.hash.int, .{wide[0] ^ (wide[1] *% fibonacci_multiplier)});
}
fn identity(input: anytype) @TypeOf(input) {
    if (@typeInfo(@TypeOf(input)).int.bits != 64) unreachable;
    return input;
}
export fn answer() c_int {
    const large = 0x9e3779b97f4a7c15;
    var value: u64 = 3;
    value = identity(value *% large);
    if (value != 0xdaa66d2c7ddf743f) return 1;
    const got = mix(.{ .low = 7, .high = 3 });
    const want = std.hash.int(@as(u64, 7) ^ (@as(u64, 3) *% @as(u64, 0x9e3779b97f4a7c15)));
    if (got != want) return 2;
    const vector: @Vector(2, u64) = .{ 9, 11 };
    const copied: [2]u64 = @bitCast(vector);
    if (identity(copied[1]) != 11) return 3;
    return 42;
}
