const names = @import("names.zig");
const Target = struct {
    value: u8,
    @"café": u8,
    @"a-b": u8,
    pub const answer: u8 = 42;
    pub const unused = @compileError("R36_UNUSED_DECL_POISON");
};
const selected = if (false) "missing" else "answer";
const raw = [_]u8{ 'x', 'v', 'a', 'l', 'u', 'e', 'x' };
const sliced: []const u8 = raw[1..6];
const array = [_]u8{ 'v', 'a', 'l', 'u', 'e' };
const pointer = &array;
pub fn main() u8 {
    const local = if (true) "value" else "missing";
    const value = Target{ .value = 40, .@"café" = 2, .@"a-b" = 1 };
    if (!@hasDecl(Target, selected)) return 1;
    if (!@hasDecl(Target, names.chosen)) return 2;
    if (@hasDecl(Target, "absent")) return 3;
    if (!@hasField(Target, local)) return 4;
    if (!@hasField(Target, sliced)) return 5;
    if (!@hasField(Target, pointer)) return 6;
    if (!@hasField(Target, "caf\xc3\xa9")) return 7;
    if (!@hasField(Target, "a-b")) return 8;
    if (@hasField(Target, "unknown?field")) return 9;
    if (@field(.{ @as(u8, 42) }, "0") != 42) return 10;
    return @field(value, sliced) + @field(value, "caf\u{e9}");
}
