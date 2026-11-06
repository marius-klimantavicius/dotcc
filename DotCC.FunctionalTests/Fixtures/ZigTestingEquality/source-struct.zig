const std = @import("std");
pub const std_options_debug_io: std.Io = .failing;
const Row = struct { len: usize, width: u8 };
const Nested = struct { row: Row, weight: f64 };

fn row(counter: *u32, marker: u32) Row {
    counter.* = counter.* * 10 + marker;
    return .{ .len = 3, .width = 2 };
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var counter: u32 = 0;
    std.testing.expectEqual(row(&counter, 1), row(&counter, 2)) catch return 1;
    if (counter != 12) return 2;
    const expected: Nested = .{ .row = .{ .len = 3, .width = 2 }, .weight = 0.0 };
    const actual: Nested = .{ .row = .{ .len = 3, .width = 2 }, .weight = -0.0 };
    std.testing.expectEqual(expected, actual) catch return 3;
    return 42;
}
