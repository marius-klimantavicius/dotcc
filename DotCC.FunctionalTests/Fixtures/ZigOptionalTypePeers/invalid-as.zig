comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const Peer = @TypeOf(@as(u8, 256), @as(u8, 42));
    return @bitSizeOf(Peer);
}
