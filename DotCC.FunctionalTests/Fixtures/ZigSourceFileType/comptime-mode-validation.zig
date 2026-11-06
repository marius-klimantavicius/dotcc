const modes = @import("modes");
fn accumulated() u8 {
    comptime var total: u8 = 0;
    comptime for ([_]u8{ 11, 13, 18 }) |value| { total = total + value; };
    return total;
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (accumulated() != 42) return 1;
    const state: modes.ModeState = .{};
    const fixed = [_]u16{ 1, 5, 7, 10, 11, 13, 14, 15, 16, 17, 18, 19 };
    for (fixed) |value| {
        const report = state.getReport(.{ .ansi = true, .value = value });
        if (report.state != .permanently_reset or !report.tag.ansi or report.tag.value != value) return 2;
    }
    const erase = state.getReport(.{ .ansi = false, .value = 117 });
    if (erase.state != .permanently_reset) return 3;
    const unknown = state.getReport(.{ .ansi = true, .value = 65535 });
    if (unknown.state != .not_recognized) return 4;
    return 42;
}
