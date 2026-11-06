const std = @import("std");
comptime { @export(&answer, .{ .name = "answer" }); }
fn answer() callconv(.c) u8 {
    comptime {
        const max_rows = std.math.maxInt(u16);
        _ = std.math.divCeil(usize, max_rows, 1) catch unreachable;
    }
    return 42;
}
