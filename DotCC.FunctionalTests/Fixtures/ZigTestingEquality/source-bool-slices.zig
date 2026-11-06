const std = @import("std");
pub const std_options_debug_io: std.Io = .failing;
var order: u32 = 0;
fn slice(values: []bool, marker: u32) []const bool { order = order * 10 + marker; return values; }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var a = [_]bool{ true, false, true };
    var b = [_]bool{ true, false, true };
    std.testing.expectEqualSlices(bool, slice(&a, 1), slice(&b, 2)) catch return 1;
    if (order != 12) return 2;
    std.testing.expectEqualSlices(bool, a[0..2], b[0..2]) catch return 3;
    return 42;
}
