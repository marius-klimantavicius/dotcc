const Parent = struct {
    state: State = .unlocked,
    pub const State = if (true) enum(usize) {
        unlocked = 0,
        shared = 42,
        fn isShared(state: State) bool { return state == .shared; }
    } else enum { unknown };
};
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const lock: Parent = .{ .state = .shared };
    return if (lock.state.isShared()) 42 else 1;
}
