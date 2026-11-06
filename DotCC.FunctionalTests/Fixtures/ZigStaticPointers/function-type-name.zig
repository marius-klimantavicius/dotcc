const std = @import("std");
const library = @import("EnumOwner/values.zig");
const Wrapper = struct { value: u8 };
const Handle = ?*Wrapper;
const Narrow = *const fn (u3) u8;
const Byte = *const fn (u8) u8;
const Native = *const fn (?*anyopaque, [*]const u8, usize) callconv((.c)) void;
const Aliased = *const fn (Handle, ?*anyopaque) callconv(library.calling_conv) void;
fn narrow(_: u3) u8 { return 42; }
fn byte(_: u8) u8 { return 42; }
fn native(_: ?*anyopaque, _: [*]const u8, _: usize) callconv(.c) void {}
fn aliased(_: Handle, _: ?*anyopaque) callconv(.c) void {}
fn named(value: anytype, expected: []const u8) bool {
    const Child = @typeInfo(@TypeOf(value)).pointer.child;
    return std.mem.eql(u8, @typeName(Child), expected);
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const small: Narrow = &narrow;
    const wide: Byte = &byte;
    const callback: Native = &native;
    const via_alias: Aliased = &aliased;
    if (!named(small, "fn (u3) u8") or !named(wide, "fn (u8) u8")) return 1;
    if (!named(callback, "fn (?*anyopaque, [*]const u8, usize) callconv(.c) void")) return 2;
    if (!std.mem.eql(u8, @typeName(Native), "*const fn (?*anyopaque, [*]const u8, usize) callconv(.c) void")) return 3;
    if (!std.mem.eql(u8, @typeName(?Native), "?*const fn (?*anyopaque, [*]const u8, usize) callconv(.c) void")) return 4;
    if (small(7) != 42 or wide(255) != 42) return 5;
    if (!named(via_alias, "fn (?*" ++ @typeName(Wrapper) ++ ", ?*anyopaque) callconv(.c) void")) return 6;
    return 42;
}
