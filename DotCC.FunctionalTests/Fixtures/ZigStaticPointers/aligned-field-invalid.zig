const Invalid = struct { value: u8 align(3) };
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 { return @intCast(@sizeOf(Invalid)); }
