const std = @import("std");
pub fn describe(comptime T: type) std.lang.Type.Struct {
    const info = @typeInfo(T).@"struct";
    if (info.layout != .@"packed") @compileError("metadata requires a packed struct");
    if (info.backing_integer == null) @compileError("metadata requires integer backing");
    return info;
}
