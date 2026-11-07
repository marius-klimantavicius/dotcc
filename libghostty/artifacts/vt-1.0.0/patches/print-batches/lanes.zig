const std = @import("std");
const simd = @import("simd");
pub fn main() void {
    const byte_lanes = simd.lanes(u8) orelse 0;
    const short_lanes = simd.lanes(u16) orelse 0;
    const cp_lanes = simd.lanes(u32) orelse 0;
    if (byte_lanes <= 64) @panic("control requires the actual 128-lane byte frontier");
    if (@min(64, short_lanes) != short_lanes or @min(64, cp_lanes) != cp_lanes)
        @panic("typed lane identity changed");
    std.debug.print("u8={d} capped={d} u16={d} u32={d}\n", .{ byte_lanes, @min(64, byte_lanes), short_lanes, cp_lanes });
}
