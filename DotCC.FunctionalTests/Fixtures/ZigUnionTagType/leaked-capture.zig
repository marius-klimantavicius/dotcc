const U = union(enum) { first: u8, second: u8 };
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (@typeInfo(U).@"union".tag_type) |Captured| {
        _ = Captured;
    }
    const value: Captured = .first;
    return @intCast(@intFromEnum(value));
}
