const std = @import("std");
const Cell = struct { raw: u64, grapheme: []const u21 };
fn take(value: u21) u8 { return if (value == 65) 42 else 1; }
fn through(args: anytype) u8 { return take(args[0]); }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const List = std.MultiArrayList(Cell);
    const values = [_]u21{65};
    var raw = [_]u64{0};
    var columns = [_][]const u21{&values};
    const view: List.Slice = .{
        .ptrs = .{ @ptrCast(&raw), @ptrCast(&columns) },
        .len = 1,
        .capacity = 1,
    };
    for (view.items(.grapheme)) |graphemes| {
        for (graphemes) |cp| return through(.{cp});
    }
    return 3;
}
