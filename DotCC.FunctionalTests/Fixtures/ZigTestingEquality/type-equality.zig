const std = @import("std");
const Four = enum { a, b, c, d };
const Packed = packed struct { low: u2, high: u3 };
const Explicit = packed struct(u16) { low: u5, high: u11 };
const Plain = struct { value: u8 };
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const info = @typeInfo(Four).@"enum";
    std.testing.expectEqual(u2, info.tag_type) catch return 1;
    std.testing.expectEqual(c_int, c_int) catch return 2;
    var mismatched = false;
    std.testing.expectEqual(u3, info.tag_type) catch |err| {
        if (err != error.TestExpectedEqual) return 3;
        mismatched = true;
    };
    if (!mismatched) return 4;
    if (@typeInfo(Plain).@"struct".backing_integer != null) return 5;
    if (@typeInfo(Packed).@"struct".backing_integer) |Backing| {
        std.testing.expectEqual(u5, Backing) catch return 6;
        if (@typeInfo(Backing).int.bits != 5) return 7;
    } else return 8;
    std.testing.expectEqual(u16, @typeInfo(Explicit).@"struct".backing_integer.?) catch return 9;
    return 42;
}
