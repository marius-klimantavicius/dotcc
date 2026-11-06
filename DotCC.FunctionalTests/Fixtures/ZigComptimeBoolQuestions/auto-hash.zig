const std = @import("std");
const Coordinate = extern struct { x: u16, y: u32 };
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var first = std.hash.Wyhash.init(0);
    std.hash.autoHash(&first, Coordinate{ .x = 17, .y = 42 });
    var second = std.hash.Wyhash.init(0);
    std.hash.autoHash(&second, @as(u16, 17));
    std.hash.autoHash(&second, @as(u32, 42));
    return if (first.final() == second.final()) 42 else 1;
}
