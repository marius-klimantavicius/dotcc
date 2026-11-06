var value: u32 = 42;
fn check(expected: anytype, actual: anytype) u8 {
    const T = @TypeOf(expected, actual);
    const Alias = T;
    if (@typeInfo(Alias).pointer.size != .one) return 1;
    return 42;
}
fn forward(expected: anytype, actual: anytype) u8 { return check(expected, actual); }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const first: *anyopaque = @ptrCast(&value);
    const second: *anyopaque = @ptrCast(&value);
    return forward(first, second);
}
