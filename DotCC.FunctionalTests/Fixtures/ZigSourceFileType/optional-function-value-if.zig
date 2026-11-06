const State = struct { enabled: bool, trace: u32 = 0, callback: ?*const fn (*@This()) []const u8 = callback };
const Callback = *const fn (*State) []const u8;
fn callback(state: *State) []const u8 {
    state.trace = state.trace * 10 + 2;
    return "managed";
}
fn fetch(state: *State) ?Callback {
    state.trace = state.trace * 10 + 1;
    return if (state.enabled) state.callback else null;
}
fn fallback(state: *State) []const u8 {
    state.trace = state.trace * 10 + 3;
    return "";
}
fn read(state: *State) []const u8 {
    return if (fetch(state)) |func| func(state) else fallback(state);
}
pub fn main() u8 {
    var state: State = .{ .enabled = true };
    const present = read(&state);
    if (state.trace != 12 or present.len != 7 or present[0] != 'm' or present[6] != 'd') return 1;
    state.enabled = false;
    state.trace = 0;
    const absent = read(&state);
    if (state.trace != 13 or absent.len != 0) return 2;
    state.enabled = true;
    state.callback = null;
    state.trace = 0;
    if (read(&state).len != 0 or state.trace != 13) return 3;
    return 42;
}
