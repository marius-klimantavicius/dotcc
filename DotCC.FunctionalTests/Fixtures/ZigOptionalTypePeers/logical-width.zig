fn bits(left: u2, right: u3) u8 {
    const Peer = @TypeOf(left, right);
    return @bitSizeOf(Peer);
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 { return if (bits(1, 3) == 3) 42 else 1; }
