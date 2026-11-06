const State = enum(u16) { idle = 1, busy = 2, _ };
const Other = enum(u16) { busy = 2, _ };
var state: State = .idle;
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    _ = @atomicRmw(State, &state, .Xchg, Other.busy, .monotonic);
    return 42;
}
