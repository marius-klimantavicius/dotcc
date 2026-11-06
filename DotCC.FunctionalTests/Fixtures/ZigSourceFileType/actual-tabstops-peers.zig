const Tabstops = @import("tabstops").Tabstops;
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var dynamic: [80]u8 = @splat(0);
    var stops: Tabstops = .{ .cols = 1152, .dynamic_stops = &dynamic };
    const columns = [_]usize{ 0, 7, 8, 503, 504, 511, 512, 513, 519, 520, 1023, 1151 };
    for (columns) |column| {
        if (stops.get(column)) return 1;
        stops.set(column);
        if (!stops.get(column)) return 2;
    }
    for (columns) |column| {
        if (!stops.get(column)) return 3;
        stops.unset(column);
        if (stops.get(column)) return 4;
    }
    for (dynamic) |unit| if (unit != 0) return 5;
    return 42;
}
