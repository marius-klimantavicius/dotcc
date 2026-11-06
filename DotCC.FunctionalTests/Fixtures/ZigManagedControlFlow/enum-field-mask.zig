const std = @import("std");
const Kind = enum(u2) { plain = 0, group = 1, palette = 2, rgb = 3 };
const Wide = enum(u2) { narrow, wide, tail, head };
const Cell = packed struct(u64) {
    content_tag: Kind = .plain,
    data: u24 = 0,
    style_id: u16 = 0,
    wide: Wide = .narrow,
    protected: bool = false,
    hyperlink: bool = false,
    semantic: u2 = 0,
    padding: u16 = 0,
};
fn fieldMask(comptime T: type, comptime field: []const u8) u64 {
    const Field = @FieldType(T, field);
    const Ones = @Int(.unsigned, @bitSizeOf(Field));
    return @as(u64, std.math.maxInt(Ones)) << @bitOffsetOf(T, field);
}
fn checkType(comptime T: type, comptime bits: comptime_int) bool {
    return @bitSizeOf(T) == bits;
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const Field = @FieldType(Cell, "content_tag");
    const Alias = Field;
    if (@bitSizeOf(Kind) != 2 or @bitSizeOf(Field) != 2 or @bitSizeOf(Alias) != 2) return 1;
    if (@sizeOf(Field) != 1 or @bitSizeOf(@typeInfo(Field).@"enum".tag_type) != 2) return 2;
    if (!checkType(Alias, 2)) return 3;
    const mask = comptime fieldMask(Cell, "content_tag") | fieldMask(Cell, "style_id");
    if (mask != ((@as(u64, 65535) << 26) | 3)) return 4;
    const plain: Cell = .{ .data = 'N' };
    const styled: Cell = .{ .data = 'R', .style_id = 1 };
    const bits_plain: u64 = @bitCast(plain);
    const bits_styled: u64 = @bitCast(styled);
    if (bits_plain & mask != 0 or bits_styled & mask == 0) return 5;
    return 42;
}
