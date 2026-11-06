const std = @import("std");
pub const std_options_debug_io: std.Io = .failing;
pub const default_limit: usize = 0;
var calls: u32 = 0;
fn actual(value: usize) usize { calls += 1; return value; }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    std.testing.expectEqual(default_limit, actual(0)) catch return 1;
    std.testing.expectEqual(default_limit, actual(0)) catch return 2;
    const custom: usize = 1234;
    std.testing.expectEqual(custom, actual(1234)) catch return 3;
    std.testing.expectEqual(@as(i32, 1), actual(1)) catch return 4;
    std.testing.expectEqual(actual(1), @as(i32, 1)) catch return 5;
    return if (calls == 5) 42 else 6;
}
