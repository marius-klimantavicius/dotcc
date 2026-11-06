comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 { const zero: u0 = 1; _ = zero; return 42; }
