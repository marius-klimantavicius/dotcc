const std = @import("std");
const provider = @import("integer-provider.zig");
const Namespace = struct { pub const count = 73; pub const typed: u16 = 73; };
fn firstIs(comptime T: type, args: anytype) bool { return @TypeOf(args[0]) == T; }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (!firstIs(comptime_int, .{Namespace.count}) or !firstIs(u16, .{Namespace.typed})) return 1;
    if (!firstIs(comptime_int, .{provider.count})) return 2;
    if (!firstIs(usize, .{provider.typed})) return 5;
    var buffer: [16]u8 = undefined;
    const text = std.mem.print(&buffer, "n{}", .{Namespace.count}) catch return 3;
    if (!std.mem.eql(u8, text, "n73")) return 4;
    return 42;
}
