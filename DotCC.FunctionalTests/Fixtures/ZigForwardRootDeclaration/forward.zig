const inspector = @import("inspect-root.zig");
const Choice = enum(u8) { selected = inspector.marker };
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (@intFromEnum(Choice.selected) != 1 or later(41) != 42) return 1;
    return 42;
}
pub fn later(value: u16) u16 { return value + 1; }
