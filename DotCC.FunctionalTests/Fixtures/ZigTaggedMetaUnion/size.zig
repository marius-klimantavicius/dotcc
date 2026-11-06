const Tag = enum { number, flag };
const U = @Union(.auto, Tag, &.{ "number", "flag" }, &.{ type, type },
    &.{ .{ .@"align" = 1 }, .{ .@"align" = 1 } });
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 { return @intCast(@sizeOf(U)); }
