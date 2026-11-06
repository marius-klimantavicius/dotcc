const Bad = packed union(u16) { byte: u8, signed: i8 };
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const Backing = @typeInfo(Bad).@"union".backing_integer.?;
    return @bitSizeOf(Backing);
}
