const Row = struct { values: []const u32 };
fn take(value: u21) u8 { return @intCast(value); }
fn invalid(values: []const u32) u8 {
    const row: Row = .{ .values = values };
    for (row.values) |cp| return take(cp);
    return 1;
}
var data = [_]u32{65};
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 { return invalid(&data); }
