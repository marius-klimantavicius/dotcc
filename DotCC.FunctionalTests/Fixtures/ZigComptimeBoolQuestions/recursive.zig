const provider = @import("provider.zig");
const Coordinate = extern struct { x: u16, y: u32 };
const Nested = struct { coordinate: Coordinate, text: []const u8 };
const Tagged = union(enum) { coordinate: Coordinate, text: []const u8 };
const Plain = struct { coordinate: Coordinate, pointer: [*]u8 };
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (comptime provider.containsSlice(Coordinate)) @compileError("coordinate has no slice");
    if (comptime !provider.containsSlice(Nested)) @compileError("nested slice disappeared");
    if (comptime !provider.containsSlice(Tagged)) @compileError("union slice disappeared");
    if (comptime provider.containsSlice(Plain)) @compileError("many pointer is not a slice");
    return 42;
}
