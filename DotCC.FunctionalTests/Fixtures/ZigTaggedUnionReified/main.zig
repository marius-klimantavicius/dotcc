const LibEnum = @import("upstream/enum.zig").Enum;
const aliases = @import("aliases.zig");

const Command = union(Key) {
    invalid,
    value: u32,
    pub const Key = LibEnum(.c, &.{ "invalid", null, "value" });
};
const Imported = union(aliases.Reexport) { invalid, value: u32 };
const Alias = aliases.ReifiedAlias;
const BareAlias = union(Alias) { invalid, value: u32 };
const Holder = struct {
    pub const Nested = union(Key) {
        invalid,
        value: u32,
        pub const Key = aliases.ReifiedAlias;
    };
    pub const Small = union(Key) {
        empty,
        byte: u8,
        pub const Key = aliases.Other;
    };
};

pub fn main() u8 {
    if (@sizeOf(Command.Key) != 4 or @sizeOf(Command) != 8 or @alignOf(Command) != 4) return 1;
    if (@sizeOf(Imported) != 8 or @sizeOf(BareAlias) != 8 or @sizeOf(Holder.Nested) != 8) return 2;
    if (@sizeOf(Holder.Small.Key) != 1 or @sizeOf(Holder.Small) != 2) return 3;
    const key: Command.Key = .value;
    const nested_key: Holder.Nested.Key = .value;
    const small_key: Holder.Small.Key = .byte;
    if (@intFromEnum(key) != 2 or @intFromEnum(nested_key) != 2 or @intFromEnum(small_key) != 5) return 4;
    const command: Command = .{ .value = 30 };
    const imported: Imported = .{ .value = 3 };
    const bare: BareAlias = .{ .value = 2 };
    const nested: Holder.Nested = .{ .value = 2 };
    const small: Holder.Small = .{ .byte = 3 };
    const selected = switch (command) { .invalid => 0, .value => |value| value };
    return @intCast(selected + imported.value + bare.value + nested.value + small.byte + @intFromEnum(key));
}
