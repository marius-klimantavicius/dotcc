const same = @import("names.zig").same;
const E = enum(u8) {
    _256 = 2, @"256" = 5, @"a-b" = 9, @"a?b" = 11,
    __zig_member_612D62 = 13, @"café" = 17, @"cafe\u{301}" = 19,
};
const Under = enum(u8) { @"_", next };
const Open = enum(u8) { named, _ };
pub fn main() u8 {
    if (@intFromEnum(E._256) != 2 or @intFromEnum(E.@"256") != 5) return 1;
    if (@intFromEnum(E.@"a-b") != 9 or @intFromEnum(E.@"a?b") != 11) return 2;
    if (!@hasField(E, "a-b") or !@hasField(E, "a?b") or @hasField(E, "a_b")) return 3;
    if (@intFromEnum(@field(E, "__zig_member_612D62")) != 13) return 4;
    if (!same(@tagName(E.@"a-b"), "a-b") or !same(@tagName(E.@"256"), "256")) return 5;
    const names = @typeInfo(E).@"enum".field_names;
    if (!same(names[0], "_256") or !same(names[1], "256") or !same(names[4], "__zig_member_612D62")) return 6;
    if (!same(names[5], "caf\xc3\xa9") or !same(names[6], "cafe\u{301}")) return 7;
    if (@intFromEnum(Under.@"_") != 0 or !@hasField(Under, "_") or @hasField(Open, "_")) return 8;
    if (!same(@typeInfo(Under).@"enum".field_names[0], "_")) return 9;
    return 42;
}
