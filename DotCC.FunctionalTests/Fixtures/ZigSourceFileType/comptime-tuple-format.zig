const std = @import("std");
const limit = 64;
fn copied(args: anytype) @TypeOf(args) { return args; }
fn forward(w: *std.Io.Writer, args: anytype) !void {
    const saved = copied(args);
    try w.print("{d}/{d};", saved);
}
var calls: u8 = 0;
fn next() usize { calls += 1; return calls + 6; }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var buffer: [128]u8 = undefined;
    var w: std.Io.Writer = .fixed(&buffer);
    forward(&w, .{ next(), limit }) catch return 1;
    forward(&w, .{ next(), limit + 1 }) catch return 2;
    forward(&w, .{ next(), -13 }) catch return 3;
    forward(&w, .{ next(), 4294967295 }) catch return 4;
    w.print("{d}/{d};", .{ 17, next() }) catch return 5;
    w.print("{d}/{d};", .{ -18, next() }) catch return 6;
    if (calls != 6) return 7;
    const expected = "7/64;8/65;9/-13;10/4294967295;17/11;-18/12;";
    const actual = w.buffered();
    if (actual.len != expected.len) return 8;
    for (actual, expected) |a, b| { if (a != b) return 9; }
    return 42;
}
