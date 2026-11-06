const State = enum(u8) { ready = 40, done = 41 };
fn select(comptime until: bool, state: State) u8 {
    const local = until;
    if (@TypeOf(local) != bool) return 1;
    const previous = if (comptime until) state else {};
    if (comptime until) return @intFromEnum(previous) + 2;
    return 42;
}
fn boolean(comptime flag: bool) bool { return flag; }
fn called(comptime flag: bool) u8 {
    const value = if (comptime boolean(flag)) @as(u8, 42) else {};
    if (comptime flag) return value;
    return 42;
}
fn demand(comptime flag: bool, payload: anytype) u8 {
    const value = if (comptime flag) payload.value else {};
    if (comptime flag) return value;
    return 42;
}
pub fn main() u8 {
    if (select(true, .ready) != 42 or select(false, .done) != 42) return 1;
    if (called(true) != 42 or called(false) != 42) return 2;
    if (demand(false, @as(u8, 0)) != 42) return 3;
    return demand(true, .{ .value = @as(u8, 42) });
}
