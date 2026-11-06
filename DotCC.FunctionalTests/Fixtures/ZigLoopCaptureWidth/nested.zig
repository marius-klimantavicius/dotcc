const support = @import("support.zig");
const Row = struct { groups: []const []const u21 };
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const values = [_]u21{65};
    const groups = [_][]const u21{&values};
    const row: Row = .{ .groups = &groups };
    for (row.groups) |group| {
        for (group) |cp| return support.through(.{cp});
    }
    return 3;
}
