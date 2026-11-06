const std = @import("std");
fn Convert(comptime T: type) type {
    return copy: {
        const info = @typeInfo(T).@"struct";
        var names: [info.field_names.len][]const u8 = undefined;
        var types: [info.field_names.len]type = undefined;
        var attrs: [info.field_names.len]std.lang.Type.Struct.FieldAttributes = undefined;
        inline for (info.field_names, info.field_types, info.field_attrs, &names, &types, &attrs) |field_name, field_type, field_attr, *name, *ty, *attr| {
            name.* = field_name;
            ty.* = field_type;
            attr.* = field_attr;
        }
        break :copy @Struct(.@"extern", null, &names, &types, &attrs);
    };
}
const Original = struct { answer: u16 = 42, flag: bool = false };
const C = Convert(Original);
comptime {
    if (@sizeOf(C) != 4 or @alignOf(C) != 2) @compileError("BAD_REFLECTED_LAYOUT");
}
pub fn main() u8 {
    const value: C = .{};
    return if (!value.flag) @intCast(value.answer) else 0;
}
