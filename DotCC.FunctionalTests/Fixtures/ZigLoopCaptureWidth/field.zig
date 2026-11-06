const support = @import("support.zig");
const Row = struct { values: []const u21 };
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const values = [_]u21{65};
    const row: Row = .{ .values = &values };
    for (row.values) |cp| return support.through(.{cp});
    return 3;
}
