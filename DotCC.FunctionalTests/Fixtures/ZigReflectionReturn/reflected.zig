const schema = @import("metadata.zig");
const std = @import("std");
const Layout = packed struct(u16) { low: u5, high: u11 };
fn forwarded(comptime T: type) std.lang.Type.Struct { return schema.describe(T); }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const info = @typeInfo(u32).int;
    const reflected = forwarded(Layout);
    if (reflected.layout != .@"packed" or reflected.field_names.len != 2) return 1;
    const Backing = reflected.backing_integer orelse @compileError("backing disappeared");
    const First = reflected.field_types[0];
    const Second = reflected.field_types[1];
    if (@bitSizeOf(Backing) != 16 or @bitSizeOf(First) != 5 or @bitSizeOf(Second) != 11) return 2;
    if (!std.mem.eql(u8, reflected.field_names[0], "low") or !std.mem.eql(u8, reflected.field_names[1], "high")) return 3;
    if (info.bits != 32) return 4;
    return 42;
}
