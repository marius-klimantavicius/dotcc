const Color = union(enum) { none: void, byte: u8 };
comptime {
    const Backing = @typeInfo(Color).@"union".backing_integer.?;
    _ = @bitSizeOf(Backing);
    @export(&answer, .{ .name = "answer" });
}
pub fn answer() callconv(.c) u8 { return 42; }
