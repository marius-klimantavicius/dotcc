const std = @import("std");
const bytes = [_]u8{ 17, 25, 42 };
var calls: u32 = 0;
fn input() []const u8 { calls += 1; return &bytes; }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var actual = std.hash.Wyhash.init(0);
    std.hash.autoHashStrat(&actual, input(), .Deep);
    if (calls != 1) return 1;
    var expected = std.hash.Wyhash.init(0);
    expected.update(&bytes);
    std.hash.autoHash(&expected, @as(usize, bytes.len));
    return if (actual.final() == expected.final()) 42 else 2;
}
