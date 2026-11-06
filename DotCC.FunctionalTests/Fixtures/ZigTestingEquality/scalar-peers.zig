const std = @import("std");
pub const std_options_debug_io: std.Io = .failing;
var order: u32 = 0;
fn number(marker: u32) usize { order = order * 10 + marker; return 4; }
fn flag(marker: u32) bool { order = order * 10 + marker; return false; }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    order = 0;
    std.testing.expectEqual(4, number(1)) catch return 1;
    std.testing.expectEqual(number(2), 4) catch return 2;
    if (order != 12) return 3;
    order = 0;
    std.testing.expectEqual(!flag(1), !flag(2)) catch return 4;
    if (order != 12) return 5;
    std.testing.expectEqual(!false, true) catch return 6;
    var left: usize = 4;
    std.testing.expectEqual(left, blk: { left = 5; break :blk @as(usize, 4); }) catch return 7;
    if (left != 5) return 8;
    const narrow: u8 = 7;
    var wide: u16 = 7;
    wide += 0;
    std.testing.expectEqual(narrow, wide) catch return 9;
    return 42;
}
