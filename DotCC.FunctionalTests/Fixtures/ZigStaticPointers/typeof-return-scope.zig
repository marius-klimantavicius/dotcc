var calls: u8 = 0;
fn next() i4 { calls += 1; return -3; }
fn bounded(val: anytype, lower: anytype, upper: anytype) @TypeOf(val, lower, upper) {
    return @max(lower, @min(val, upper));
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const value = bounded(@abs(next()), @as(u4, 1), @as(u4, 5));
    return if (value == 3 and calls == 1) 42 else 1;
}
