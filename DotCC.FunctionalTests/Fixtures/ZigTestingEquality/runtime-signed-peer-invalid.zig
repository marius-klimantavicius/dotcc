const std = @import("std");
pub const std_options_debug_io: std.Io = .failing;
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer(expected: i32, actual: u64) callconv(.c) u8 {
    std.testing.expectEqual(expected, actual) catch return 1;
    return 42;
}
