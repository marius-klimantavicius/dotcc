const Plain = union { first: u32, second: u32 };

comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var effects: u32 = 0;
    if (@typeInfo(Plain).@"union".tag_type) |Tag| {
        _ = Tag;
        @compileError("untagged union must have no tag");
    } else effects = effects * 10 + 1;
    const known: ?type = @typeInfo(Plain).@"union".tag_type;
    if (known) |Tag| {
        _ = Tag;
        @compileError("stored optional type must be null");
    } else effects = effects * 10 + 2;
    const selected: u8 = if (@typeInfo(Plain).@"union".tag_type) |_| 0 else 3;
    effects = effects * 10 + selected;
    const Fallback = @typeInfo(Plain).@"union".tag_type orelse u8;
    if (Fallback != u8 or effects != 123) return 1;
    return 42;
}
