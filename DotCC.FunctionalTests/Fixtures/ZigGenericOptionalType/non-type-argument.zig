fn accepts(comptime candidate: ?type) u8 { _ = candidate; return 42; }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 { return accepts(7); }
