const checked = false;
const Safety = enum { checked, assumed };
const safety: Safety = if (checked) .checked else .assumed;
const Empty = struct {
    field: switch (safety) {
        .checked => @compileError("unselected checked type"),
        .assumed => void,
    } = {},
};
const wide = true;
const Width = enum { wide, narrow };
const width: Width = if (wide) .wide else .narrow;
const Value = struct {
    field: switch (width) {
        .wide => u16,
        .narrow => @compileError("unselected narrow type"),
    },
};
const Owner = struct {
    pub const Hash = u64;
    pub const Narrow = @Int(.unsigned, 21);
    const Choice = union(enum) { checked: usize, assumed };
    const enabled = false;
    const selected: Choice = if (enabled) .{ .checked = 8 } else .assumed;
    empty: switch (selected) {
        .checked => @compileError("unselected owner type"),
        .assumed => void,
    } = {},
    hash: Hash,
    narrow: Narrow,
};
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const value: Value = .{ .field = 0x1234 };
    if (@FieldType(Empty, "field") != void) return 1;
    if (@sizeOf(Value) != 2 or value.field != 0x1234) return 2;
    const owner: Owner = .{ .hash = 9, .narrow = 17 };
    if (@bitSizeOf(Owner.Hash) != 64 or @bitSizeOf(Owner.Narrow) != 21) return 3;
    if (owner.hash != 9 or owner.narrow != 17) return 4;
    return 42;
}
