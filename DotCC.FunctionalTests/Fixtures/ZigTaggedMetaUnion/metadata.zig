const Tag = enum(u8) { number = 7, flag = 19 };

fn MetaArms() type {
    return @Union(.auto, Tag, &.{ "number", "flag" }, &.{ type, type },
        &.{ .{ .@"align" = 1 }, .{ .@"align" = 1 } });
}

comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const U = MetaArms();
    const info = @typeInfo(U).@"union";
    if (info.tag_type.? != Tag) return 1;
    if (@intFromEnum(Tag.number) != 7 or @intFromEnum(Tag.flag) != 19) return 2;
    if (info.field_names.len != 2) return 3;
    if (@FieldType(U, "number") != type or @FieldType(U, "flag") != type) return 4;
    if (info.field_attrs[0].@"align" != 1 or info.field_attrs[1].@"align" != 1) return 5;
    return 42;
}
