const Count = packed struct(u32) { n: u31, filling: bool };
var counter: Count = .{ .n = 0x7fffffff, .filling = false };
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const previous = @atomicRmw(Count, &counter, .Add, .{ .n = 1, .filling = false }, .acq_rel);
    if (previous.n != 0x7fffffff or previous.filling) return 1;
    if (counter.n != 0 or !counter.filling) return 2;
    const restored = @atomicRmw(Count, &counter, .Sub, .{ .n = 1, .filling = false }, .acq_rel);
    if (restored.n != 0 or !restored.filling) return 3;
    if (counter.n != 0x7fffffff or counter.filling) return 4;
    _ = @atomicRmw(Count, &counter, .Sub, .{ .n = 0, .filling = true }, .monotonic);
    if (counter.n != 0x7fffffff or !counter.filling) return 5;
    return 42;
}
