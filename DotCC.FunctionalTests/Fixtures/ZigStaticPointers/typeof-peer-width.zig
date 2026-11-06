var calls: u8 = 0;
fn next() u7 { calls += 1; return 65; }
fn read(args: anytype) u8 {
    return @intCast(@typeInfo(@TypeOf(args[0])).int.bits + @typeInfo(@TypeOf(args[1])).int.bits);
}
fn forward(args: anytype) u8 { return read(args); }
fn inner(comptime T: type, expected: T, actual: T) u8 {
    return forward(.{ expected, actual });
}
fn outer(expected: anytype, actual: anytype) u8 {
    const T = @TypeOf(expected, actual);
    return inner(T, expected, actual);
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const Unevaluated = @TypeOf(next(), @as(u4, 3));
    if (@typeInfo(Unevaluated).int.bits != 7 or calls != 0) return 1;
    var narrow: u2 = 2;
    _ = &narrow;
    if (outer(1, narrow) != 4) return 2;
    if (outer(@as(u4, 3), @as(u7, 65)) != 14) return 3;
    return 42;
}
