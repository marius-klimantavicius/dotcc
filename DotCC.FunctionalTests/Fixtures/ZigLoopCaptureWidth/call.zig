const support = @import("support.zig");
const data = [_]u21{65};
var calls: u8 = 0;
fn values() []const u21 { calls += 1; return &data; }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    for (values()) |cp| {
        if (calls != 1) return 3;
        return support.through(.{cp});
    }
    return 4;
}
