const same = @import("names.zig").same;
const S = struct { @"caf\xc3\xa9": u8, @"cafe\u{301}": u8, @"a\"b": u8, @"a\\b": u8 };
pub fn main() u8 {
    const x = S{ .@"café" = 10, .@"cafe\u{301}" = 20, .@"a\"b" = 5, .@"a\\b" = 7 };
    if (!@hasField(S, "café") or !@hasField(S, "caf\u{e9}") or !@hasField(S, "cafe\u{301}")) return 1;
    if (@hasField(S, "caf_") or @hasField(S, "a_b")) return 2;
    const names = @typeInfo(S).@"struct".field_names;
    if (!same(names[0], "café") or !same(names[1], "cafe\u{301}") or !same(names[2], "a\"b") or !same(names[3], "a\\b")) return 3;
    const selected = "xcaf\xc3\xa9x"[1..6];
    return @field(x, selected) + @field(x, "cafe\u{301}") + @field(x, "a\"b") + @field(x, "a\\b");
}
