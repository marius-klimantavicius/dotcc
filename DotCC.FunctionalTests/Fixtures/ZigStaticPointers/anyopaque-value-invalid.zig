const Erased = anyopaque;
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const values: [1]Erased = undefined;
    return @intCast(values.len);
}
