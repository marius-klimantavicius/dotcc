const provider = @import("late-root-provider.zig");
pub fn callback(comptime increment: u8, value: u8) u8 { return value + increment; }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 { return provider.invoke(40); }
