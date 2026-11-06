const std = @import("std");

fn table(index: usize, value: u32, store: bool) u32 {
    const empty = std.math.maxInt(u32);
    const Static = struct {
        var seen: [16]u32 = @splat(empty);
        var pointer: *[16]u32 = &seen;
    };
    if (store) Static.pointer[index] = value;
    return Static.seen[index];
}

fn wide(comptime initial: u128) *u128 {
    const seed: u128 = initial;
    const Static = struct {
        var value: u128 = seed;
        var pointer: *u128 = &value;
    };
    return Static.pointer;
}

comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    for (0..16) |index| {
        if (table(index, 0, false) != std.math.maxInt(u32)) return 1;
    }
    if (table(5, 42, true) != 42) return 2;
    if (table(5, 0, false) != 42) return 3;
    if (table(4, 0, false) != std.math.maxInt(u32)) return 5;
    const first = wide(41);
    const second = wide(84);
    if (first == second or first.* != 41 or second.* != 84) return 6;
    if (@intFromPtr(first) & 15 != 0 or @intFromPtr(second) & 15 != 0) return 7;
    first.* += 1;
    if (wide(41) != first or wide(41).* != 42 or wide(84).* != 84) return 8;
    return @intCast(first.*);
}
