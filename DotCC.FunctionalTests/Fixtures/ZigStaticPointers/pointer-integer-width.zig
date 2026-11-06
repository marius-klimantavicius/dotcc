const std = @import("std");
var byte: u8 = 42;
var calls: u8 = 0;
fn address() *u8 { calls += 1; return &byte; }
fn width(value: anytype) u8 { return @typeInfo(@TypeOf(value)).int.bits; }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const pointer = @intFromPtr(address());
    if (calls != 1 or width(pointer) != 64) return 1;
    const mixed = @as(u16, 123) ^ pointer;
    if (width(mixed) != 64) return 2;
    const typed = std.hash.int(@as(usize, mixed));
    const inferred = std.hash.int(@as(u16, 123) ^ @intFromPtr(&byte));
    if (typed != inferred) return 3;
    // The pinned std/hash.zig test vector for uint64(1), evaluated at runtime.
    if (std.hash.int(@as(u64, calls)) != 0x071894de00d9981f) return 5;
    const a: u4 = 3;
    const b: u7 = 64;
    const combined = a ^ b;
    if (width(combined) != 7 or combined != 67) return 4;
    return 42;
}
