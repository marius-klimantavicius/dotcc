const std = @import("std");
fn raw() [3]u8 { return .{ 'A', 0, 255 }; }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const left = "X" ++ raw();
    const right = raw() ++ "Y";
    if (left.len != 4 or left[0] != 'X' or left[2] != 0 or left[3] != 255) return 1;
    if (right.len != 4 or right[0] != 'A' or right[1] != 0 or right[2] != 255 or right[3] != 'Y') return 2;
    const terminated: [*:0]const u8 = left;
    if (terminated[4] != 0) return 3;
    const query = "\x1BP+q" ++ std.fmt.bytesToHex("TN", .upper) ++ "\x1B\\";
    if (query.len != 10 or query[4] != '5' or query[5] != '4' or query[6] != '4' or query[7] != 'E') return 4;
    return 42;
}
