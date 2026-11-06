const std = @import("std");
const schema = @import("metadata.zig");
const First = packed struct(u16) { low: u5, high: u11 };
const Second = packed struct(u8) { solo: u8 };

fn Options(comptime T: type) type {
    const info = schema.describe(T);
    const default_value: u8 = 7;
    var names: [info.field_names.len][]const u8 = undefined;
    var types: [info.field_names.len]type = undefined;
    var attrs: [info.field_names.len]std.lang.Type.Struct.FieldAttributes = undefined;
    inline for (info.field_names, 0..) |name, i| {
        names[i] = name;
        types[i] = u8;
        attrs[i] = .{ .default_value_ptr = &default_value };
    }
    const Fields = @Struct(.auto, null, &names, &types, &attrs);
    return struct { fields: Fields = .{} };
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var first: Options(First) = .{};
    const second: Options(Second) = .{};
    if (first.fields.low != 7 or first.fields.high != 7 or second.fields.solo != 7) return 1;
    first.fields.low = 9;
    if (first.fields.low != 9 or first.fields.high != 7 or second.fields.solo != 7) return 2;
    return 42;
}
