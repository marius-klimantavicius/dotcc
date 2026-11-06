const std = @import("std");
const Three = struct { alpha: u8, beta: u16, gamma: u32 };
const Eight = struct { a: u8, b: u8, c: u8, d: u8, e: u8, f: u8, g: u8, h: u8 };
const Plain = union { low: u8, high: u16 };
const Tagged = union(enum) { first: u8, second: bool, empty: void };

comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const Small = std.meta.FieldEnum(Three);
    const Full = std.meta.FieldEnum(Eight);
    const Untagged = std.meta.FieldEnum(Plain);
    const Existing = std.meta.FieldEnum(Tagged);
    if (@intFromEnum(Small.alpha) != 0 or @intFromEnum(Small.gamma) != 2) return 1;
    if (@intFromEnum(Full.a) != 0 or @intFromEnum(Full.h) != 7) return 2;
    if (@intFromEnum(Untagged.low) != 0 or @intFromEnum(Untagged.high) != 1) return 3;
    if (Existing != @typeInfo(Tagged).@"union".tag_type.?) return 4;
    return 42;
}
