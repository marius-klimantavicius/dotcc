const Count = packed struct(u32) { n: u31, filling: bool };
const Other = packed struct(u32) { n: u31, filling: bool };
var counter: Count = .{ .n = 0, .filling = false };
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    _ = @atomicRmw(Count, &counter, .Add, Other{ .n = 1, .filling = false }, .monotonic);
    return 42;
}
