const std = @import("std");
pub const std_options_debug_io: std.Io = .failing;
var calls: u32 = 0;
fn none() ?[2]u16 { calls += 1; return null; }
fn isAbsent(value: ?[2]u16) bool { return value == null; }
fn forward(value: anytype) bool { return isAbsent(value); }
fn compare(expected: anytype, actual: anytype) !void {
    try std.testing.expectEqual(expected, actual);
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    compare(null, none()) catch return 1;
    if (calls != 1 or !forward(null)) return 2;
    const typed: ?[2]u16 = null;
    compare(typed, none()) catch return 3;
    if (calls != 2) return 4;
    return 42;
}
