comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const values = [_]u8{ 41, 42 };
    return values.ptr[0];
}
