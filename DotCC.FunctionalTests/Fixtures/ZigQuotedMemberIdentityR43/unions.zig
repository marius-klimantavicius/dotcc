const same = @import("names.zig").same;
const E = @import("provider.zig").Key;
const U = union(E) {
    _256: u8, @"256": u8, @"a-b": u8, @"a?b": u8,
    __zig_member_612D62: u8, @"café": u8,
};
const Auto = union(enum(u8)) { @"a-b": u8, @"a?b": u8, a_b: u8 };
pub fn main() u8 {
    const x: U = .{ .@"256" = 20 };
    const y: U = .{ ._256 = 10 };
    const a: Auto = .{ .@"a-b" = 5 };
    const b: Auto = .{ .@"a?b" = 7 };
    if (!same(@tagName(E.@"256"), "256") or !same(@tagName(E._256), "_256")) return 1;
    const names = @typeInfo(U).@"union".field_names;
    if (!same(names[2], "a-b") or !same(names[3], "a?b")) return 2;
    if (@hasField(U, "a_b") or !@hasField(Auto, "a_b")) return 3;
    const selected = switch (x) {
        .@"256" => |v| v, ._256 => 0, .@"a-b" => 0, .@"a?b" => 0,
        .__zig_member_612D62 => 0, .@"café" => 0,
    };
    return selected + y._256 + a.@"a-b" + b.@"a?b";
}
