const count = 128;
const Unused = @Int(.unsigned, @compileError("UNUSED_TYPE_CONSTRUCTOR_ANALYZED"));
fn category(x: anytype) u8 {
    return switch (@typeInfo(@TypeOf(x))) {
        .comptime_int => 10,
        .int => |info| if (info.bits == 64 and info.signedness == .unsigned) 32 else 1,
        else => 0,
    };
}
fn bits(x: anytype) u8 {
    return @typeInfo(@TypeOf(x)).int.bits;
}
pub fn main() u8 {
    const names = @typeInfo(struct { x: u8, y: u21 }).@"struct".field_names;
    if (bits(@as(u21, 5)) != 21 or bits(@as(u32, 5)) != 32) return 3;
    return category(count * 8) + category(names.len);
}
