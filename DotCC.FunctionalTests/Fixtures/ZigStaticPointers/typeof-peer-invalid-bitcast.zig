comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const Narrow = @TypeOf(@as(u4, 1), @as(u7, 2));
    const value: u8 = 42;
    return @as(Narrow, @bitCast(value));
}
