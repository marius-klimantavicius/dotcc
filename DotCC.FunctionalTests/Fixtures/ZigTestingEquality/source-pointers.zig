const std = @import("std");
pub const std_options_debug_io: std.Io = .failing;
const Item = struct { value: u8 };
var order: u32 = 0;
fn pointer(value: *Item, marker: u32) *Item { order = order * 10 + marker; return value; }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var a: Item = .{ .value = 1 };
    std.testing.expectEqual(pointer(&a, 1), pointer(&a, 2)) catch return 1;
    if (order != 12) return 2;
    const opaque_a: *anyopaque = @ptrCast(&a);
    std.testing.expectEqual(opaque_a, @as(*anyopaque, @ptrCast(&a))) catch return 3;
    return 42;
}
