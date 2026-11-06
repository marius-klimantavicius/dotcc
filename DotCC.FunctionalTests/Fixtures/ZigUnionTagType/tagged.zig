const Inferred = union(enum) { first: u8, second: bool, empty: void };
const ExplicitTag = enum(u16) { small = 3, large = 500 };
const Explicit = union(ExplicitTag) { small: u8, large: u64 };

comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var effects: u32 = 0;
    if (@typeInfo(Inferred).@"union".tag_type) |Tag| {
        if (Tag != @typeInfo(Inferred).@"union".tag_type.?) return 1;
        if (@intFromEnum(Tag.first) != 0 or @intFromEnum(Tag.second) != 1 or @intFromEnum(Tag.empty) != 2) return 2;
        effects = effects * 10 + 1;
    } else @compileError("inferred union must have a tag");
    const known: ?type = @typeInfo(Explicit).@"union".tag_type;
    if (known) |Tag| {
        if (Tag != ExplicitTag) return 3;
        const value: Tag = .large;
        if (@intFromEnum(value) != 500 or @bitSizeOf(Tag) != 16) return 4;
        effects = effects * 10 + 2;
    } else @compileError("explicit union must have its declared enum");
    const selected: u8 = if (@typeInfo(Explicit).@"union".tag_type) |Tag|
        if (Tag == ExplicitTag) 3 else 0
    else @compileError("expression capture must be present");
    effects = effects * 10 + selected;
    if (effects != 123) return 5;
    return 42;
}
