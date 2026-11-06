const std = @import("std");
const Mode = enum { first, second };
const Nested = struct { value: u16 = 7 };
const Payload = struct {
    nested: Nested = .{ .value = 42 },
    text: []const u8 = "a\x00z",
    mode: Mode = .second,
    fraction: f32 = 1.5,
    narrow: u5 = 23,
    absent: ?u8 = null,
    counter: u16,
};
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var first = std.mem.zeroInit(Payload, .{});
    const second = std.mem.zeroInit(Payload, .{ .narrow = @as(u5, 17) });
    if (first.nested.value != 42 or first.text.len != 3 or first.text[1] != 0 or first.text[2] != 'z') return 1;
    if (first.mode != .second or first.fraction != 1.5 or first.narrow != 23 or first.absent != null or first.counter != 0) return 2;
    first.nested.value = 9;
    if (second.nested.value != 42 or second.narrow != 17) return 3;
    return 42;
}
