const same = @import("names.zig").same;
const E = @import("provider.zig").Key;
pub fn main() u8 {
    if (@intFromEnum(E._256) != 2 or @intFromEnum(E.@"256") != 5) return 1;
    if (@intFromEnum(@field(E, "a-b")) != 9 or @intFromEnum(@field(E, "a?b")) != 11) return 2;
    if (@intFromEnum(E.__zig_member_612D62) != 13 or @intFromEnum(E.@"café") != 17) return 3;
    if (!same(@tagName(E.@"caf\u{e9}"), "caf\xc3\xa9")) return 4;
    const names = @typeInfo(E).@"enum".field_names;
    if (!same(names[2], "a-b") or !same(names[3], "a?b") or !same(names[5], "café")) return 5;
    if (@hasField(E, "a_b") or @hasField(E, "caf_")) return 6;
    return 42;
}
