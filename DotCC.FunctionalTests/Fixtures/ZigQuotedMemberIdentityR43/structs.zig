const same = @import("names.zig").same;
const S = struct {
    _256: u8, @"256": u8, @"a-b": u8, @"a?b": u8,
    __zig_member_612D62: u8,
};
const Single = struct { @"a-b": u8 };
pub fn main() u8 {
    const x = S{ ._256 = 2, .@"256" = 5, .@"a-b" = 9, .@"a?b" = 11, .__zig_member_612D62 = 15 };
    if (!@hasField(S, "256") or !@hasField(S, "_256") or @hasField(S, "a_b")) return 1;
    if (@hasField(Single, "a?b") or @hasField(Single, "a_b")) return 2;
    const names = @typeInfo(S).@"struct".field_names;
    if (!same(names[0], "_256") or !same(names[1], "256") or !same(names[2], "a-b")) return 3;
    return @field(x, "_256") + @field(x, "256") + @field(x, "a-b") + @field(x, "a?b") + x.__zig_member_612D62;
}
