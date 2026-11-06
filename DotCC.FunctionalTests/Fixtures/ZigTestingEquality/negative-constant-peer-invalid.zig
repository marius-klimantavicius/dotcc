const std = @import("std");
pub const std_options_debug_io: std.Io = .failing;
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer(actual: u64) callconv(.c) u8 {
    std.testing.expectEqual(@as(i32, -1), actual) catch return 1;
    return 42;
}
