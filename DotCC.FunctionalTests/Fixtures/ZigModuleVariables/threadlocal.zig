const state = @import("thread-state.zig");
pub fn main() u8 {
    if (state.read() != 17) return 1;
    state.add();
    return @intCast(state.value + 17);
}
