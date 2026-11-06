const State = struct { serial: u48 };
fn copy(state: *const State) u48 {
    const serial: u48 = state.serial;
    return serial;
}
pub fn main() u8 {
    var state: State = .{ .serial = 0x123456789ABC };
    if (copy(&state) != 0x123456789ABC) return 1;
    state.serial = 0xFFFFFFFFFFFF;
    if (copy(&state) != 0xFFFFFFFFFFFF) return 2;
    return 42;
}
