const Tag = enum(u8) { number = 9, empty = 21 };
fn consume(value: union(Tag) { number: u16, empty }) u16 {
    if (@typeInfo(@TypeOf(value)).@"union".tag_type.? != Tag) return 1;
    return switch (value) {
        .number => |number| if (@intFromEnum(@as(Tag, value)) == 9) number else 2,
        .empty => if (@intFromEnum(@as(Tag, value)) == 21) 0 else 3,
    };
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (consume(.{ .number = 42 }) != 42 or consume(.empty) != 0) return 1;
    return 42;
}
