const api = @import("type-namespace.zig");
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 { return api.Namespace.call(41); }
