const std = @import("std");
var calls: u8 = 0;
var rows = [_][2]u21{ .{ 1, 2 }, .{ 3, 4 }, .{ 5, 6 }, .{ 7, 8 } };
fn source() []const [2]u21 { calls = calls * 10 + 2; return rows[0..2]; }
fn destination() [][2]u21 { calls = calls * 10 + 1; return rows[1..3]; }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    std.mem.copyForwards([2]u21, destination(), source());
    if (calls != 12) return 1;
    // Forward overlap intentionally reads the first copied row again.
    if (rows[1][0] != 1 or rows[1][1] != 2 or rows[2][0] != 1 or rows[2][1] != 2) return 2;
    if (rows[0][0] != 1 or rows[3][0] != 7 or rows[3][1] != 8) return 3;
    const empty = rows[0..0];
    std.mem.copyForwards([2]u21, empty, empty);
    return 42;
}
