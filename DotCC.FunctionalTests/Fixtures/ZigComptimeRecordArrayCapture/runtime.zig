const Entry = struct { name: []const u8, T: type, marker: u8 };
var input: u8 = 1;
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const entries = [_]Entry{ .{ .name = "runtime", .T = u8, .marker = input } };
    inline for (entries) |entry| { return entry.marker; }
    return 42;
}
