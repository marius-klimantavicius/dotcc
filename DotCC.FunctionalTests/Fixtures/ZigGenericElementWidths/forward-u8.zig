const std = @import("std");
pub const std_options_debug_io: std.Io = .failing;
const Page = struct { memory: []align(4096) u8 };
var calls: u32 = 0;
const bytes = [_]u8{42};
fn input() []const u8 { calls += 1; return &bytes; }
fn width(comptime T: type) u8 { return @typeInfo(T).int.bits; }
fn array(values: anytype) u8 {
    for (values) |value| return width(@TypeOf(value));
    return 0;
}
fn pointer(values: anytype) u8 { return array(values); }
fn hash(values: anytype) u8 { return pointer(values); }
fn manyPointer(values: anytype) u8 {
    const Child = @typeInfo(@TypeOf(values)).pointer.child;
    if (!std.mem.eql(u8, @typeName(Child), "u8")) return 0;
    return @typeInfo(Child).int.bits;
}
fn forwardPointer(values: anytype) u8 { return manyPointer(values); }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (hash(input()) != 8 or calls != 1) return 1;
    var aligned: [1]u8 align(4096) = .{42};
    const many: [*]align(4096)u8 = &aligned;
    if (forwardPointer(many) != 8) return 2;
    const page: Page = .{ .memory = &aligned };
    const retained = page.memory.ptr;
    if (forwardPointer(retained) != 8) return 3;
    std.testing.expectEqual(retained, page.memory.ptr) catch return 4;
    return 42;
}
