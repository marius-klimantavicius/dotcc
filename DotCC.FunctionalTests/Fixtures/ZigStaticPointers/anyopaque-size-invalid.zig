const Erased = anyopaque;
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    return @intCast(@sizeOf(Erased));
}
