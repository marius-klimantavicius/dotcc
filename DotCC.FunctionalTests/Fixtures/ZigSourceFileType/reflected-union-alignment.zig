const Tag = enum(u8) { small = 13, wide = 93 };
const Value = @Union(.auto, Tag, &.{ "small", "wide" }, &.{ u8, u64 }, &@splat(.{}));
const Overlay = @Union(.@"extern", null, &.{ "small", "wide" }, &.{ u8, u32 }, &.{ .{}, .{ .@"align" = 16 } });
pub fn main() u8 {
    if (@sizeOf(Value) != 16 or @alignOf(Value) != 8) return 1;
    if (@sizeOf(Overlay) != 16 or @alignOf(Overlay) != 16) return 2;
    if (@typeInfo(Value).@"union".field_names.len != 2) return 3;
    const info = @typeInfo(Overlay).@"union";
    if (info.field_attrs[1].@"align".? != 16) return 4;
    var values: [2]Value = .{ @unionInit(Value, "small", 42), @unionInit(Value, "wide", 0x12345678) };
    var overlays: [2]Overlay = .{ @unionInit(Overlay, "small", 42), @unionInit(Overlay, "wide", 0xabcdef01) };
    if (@intFromPtr(&values[1]) - @intFromPtr(&values[0]) != 16) return 5;
    if (@intFromPtr(&overlays[1]) - @intFromPtr(&overlays[0]) != 16) return 6;
    if (@intFromPtr(&values[0]) % 8 != 0 or @intFromPtr(&overlays[0]) % 16 != 0) return 7;
    if (values[0].small != 42 or values[1].wide != 0x12345678 or overlays[1].wide != 0xabcdef01) return 8;
    return values[0].small;
}
