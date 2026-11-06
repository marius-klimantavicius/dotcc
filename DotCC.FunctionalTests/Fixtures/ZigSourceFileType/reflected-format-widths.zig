const Count = u16;
const Source = struct { one: u1, seven: u7, byte: u8, small: u17, wide: u32, count: Count, signed: i7 };
fn Copy(comptime T: type) type {
    const info = @typeInfo(T).@"struct";
    var types: [info.field_names.len]type = undefined;
    inline for (info.field_types, &types) |Type, *out| { out.* = Type; }
    return @Struct(.auto, null, info.field_names, &types, &@splat(.{}));
}
const Copied = Copy(Copy(Source));
fn bits(value: anytype) u8 { return @typeInfo(@TypeOf(value)).int.bits; }
fn inspect(args: anytype) u8 { const saved = args; return bits(@field(saved, "0")); }
fn Limit(comptime n: u17) type { return struct { pub const length: u17 = n; }; }
fn Limits(comptime n: u32) type { return struct { pub const small: u17 = n; pub const wide: u32 = n; }; }
fn switched(value: Copied, flag: bool) u8 {
    const n = switch (flag) { true => value.small - 1, false => value.small };
    return inspect(.{n + 1});
}
var effects: u8 = 0;
fn counted(value: u17) u17 { effects += 1; return value; }
pub fn main() u8 {
    const value: Copied = .{ .one = 1, .seven = 7, .byte = 8, .small = 17, .wide = 32, .count = 16, .signed = -1 };
    if (inspect(.{value.one}) != 1 or inspect(.{value.seven}) != 7 or inspect(.{value.byte}) != 8) return 1;
    if (inspect(.{value.small}) != 17 or inspect(.{value.wide}) != 32 or inspect(.{value.count}) != 16) return 2;
    if (inspect(.{value.signed}) != 7) return 3;
    if (bits(counted(value.small)) != 17 or effects != 1) return 4;
    const type_list = @typeInfo(Copied).@"struct".field_types;
    if (@typeInfo(type_list[3]).int.bits != 17 or @typeInfo(type_list[4]).int.bits != 32) return 5;
    if (inspect(.{value.small + 1}) != 17 or inspect(.{value.byte + 1}) != 8) return 6;
    if (switched(value, true) != 17 or switched(value, false) != 17) return 7;
    if (inspect(.{Limit(17).length}) != 17) return 8;
    if (inspect(.{Limits(17).small}) != 17 or inspect(.{Limits(17).wide}) != 32 or inspect(.{Limits(17).small}) != 17) return 9;
    return 42;
}
