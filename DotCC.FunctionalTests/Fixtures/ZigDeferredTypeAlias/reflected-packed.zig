const std = @import("std");
const Entry = struct { name: []const u8, default: bool };
const entries = [_]Entry{
    .{ .name = "first", .default = false },
    .{ .name = "answer", .default = true },
};
const P = packed_type: {
    var names: [entries.len][]const u8 = undefined;
    var types: [entries.len]type = @splat(bool);
    var attrs: [entries.len]std.lang.Type.Struct.FieldAttributes = undefined;
    for (entries, &names, &attrs) |entry, *name, *attr| {
        name.* = entry.name;
        attr.* = .{ .default_value_ptr = &entry.default };
    }
    break :packed_type @Struct(.@"packed", null, &names, &types, &attrs);
};
comptime {
    if (@bitSizeOf(P) != 2 or @sizeOf(P) != 1) @compileError("BAD_REIFIED_PACKED_LAYOUT");
}
pub fn main() u8 {
    var value: P = .{};
    if (value.first or !value.answer) return 1;
    value.first = true;
    const bits: u2 = @bitCast(value);
    return if (bits == 3) 42 else 2;
}
