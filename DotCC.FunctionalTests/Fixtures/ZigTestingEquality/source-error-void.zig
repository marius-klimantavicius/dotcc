const std = @import("std");
pub const std_options_debug_io: std.Io = .failing;
fn result(counter: *u32, fail: bool) error{Expected}!void {
    counter.* += 1;
    if (fail) return error.Expected;
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var count: u32 = 0;
    std.testing.expectError(error.Expected, result(&count, true)) catch return 1;
    if (count != 1) return 2;
    result(&count, false) catch return 3;
    return if (count == 2) 42 else 4;
}
