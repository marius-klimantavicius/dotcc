fn choose(left: ?u16, right: ?u16) ?u16 {
    const Peer = @TypeOf(left, right);
    const value: Peer = left;
    return value;
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const Peer = @TypeOf(@as(?u16, 17), @as(?u16, null));
    if (Peer != ?u16) return 1;
    return @intCast(choose(42, null).?);
}
