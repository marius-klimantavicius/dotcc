fn inspect(values: []const u8) void {
    inline for (values) |value| { _ = value; }
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const bytes = [_]u8{42};
    inspect(&bytes);
    return 42;
}
