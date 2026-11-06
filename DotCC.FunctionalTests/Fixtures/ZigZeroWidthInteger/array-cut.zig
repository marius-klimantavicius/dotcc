comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 { var zeros: [2]u0 = .{ 0, 0 }; zeros[0] = 0; _ = &zeros; return 42; }
