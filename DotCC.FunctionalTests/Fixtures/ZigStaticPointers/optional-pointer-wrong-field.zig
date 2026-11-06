comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    return if (@typeInfo(?*u8).pointer.size == .one) 42 else 1;
}
