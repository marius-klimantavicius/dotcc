const values = @import("EnumOwner/values.zig");
const Packed = packed struct(u16) { value: u15, flag: bool = false };
fn check(value: anytype, expected: u16) bool {
    const integer = @backingInt(value);
    if (@TypeOf(integer) != u16 or @bitSizeOf(@TypeOf(integer)) != 16) return false;
    const tag: Packed = @bitCast(integer);
    return tag.value == expected and !tag.flag;
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (!check(@as(values.Small, .low), 6) or !check(@as(values.Small, .high), 1025)) return 1;
    if (!check(@as(values.Reified, .low), 6) or !check(@as(values.Reified, .high), 1025)) return 2;
    const Backing = @typeInfo(values.Small).@"enum".tag_type;
    if (Backing != u16) return 3;
    const negative = @backingInt(@as(values.Signed, .negative));
    if (@typeInfo(@TypeOf(negative)).int.signedness != .signed or @bitSizeOf(@TypeOf(negative)) != 7 or negative != -12) return 4;
    return 42;
}
