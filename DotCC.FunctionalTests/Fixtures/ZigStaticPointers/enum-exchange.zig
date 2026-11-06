const State = enum(u16) { idle = 0x2962, busy = 0x0030, _ };
var state: State = .idle;
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (@atomicLoad(State, &state, .acquire) != .idle) return 3;
    const previous = @atomicRmw(State, &state, .Xchg, .busy, .monotonic);
    if (previous != .idle or state != .busy) return 1;
    if (@atomicLoad(State, &state, .acquire) != .busy) return 4;
    const restored = @atomicRmw(State, &state, .Xchg, previous, .monotonic);
    if (restored != .busy or state != .idle) return 2;
    return 42;
}
