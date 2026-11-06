const Enum = @import("upstream/enum.zig").Enum;

pub const Original = enum(u32) { invalid = 0, value = 2 };
pub const Reexport = Original;
pub const Reified = Enum(.c, &.{ "invalid", null, "value" });
pub const ReifiedAlias = Reified;
pub const Other = enum(u8) { empty = 0, byte = 5 };
