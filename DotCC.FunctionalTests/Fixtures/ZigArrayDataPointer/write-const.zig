comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const input = "ABC";
    input.ptr[0] = 'X';
    return 42;
}
