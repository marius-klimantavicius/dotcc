fn ordinary(stop: bool, trace: *u8) void {
    const amount: u8 = value: { break :value if (stop) return else 3; };
    trace.* += amount;
}
fn fallible(stop: bool, trace: *u8) !void {
    const amount: u8 = value: { break :value if (stop) return else 3; };
    trace.* += amount;
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var trace: u8 = 0;
    ordinary(true, &trace);
    if (trace != 0) return 1;
    ordinary(false, &trace);
    if (trace != 3) return 2;
    fallible(true, &trace) catch return 3;
    if (trace != 3) return 4;
    fallible(false, &trace) catch return 5;
    if (trace != 6) return 6;
    return 42;
}
