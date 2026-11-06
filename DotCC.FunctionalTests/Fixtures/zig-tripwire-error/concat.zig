const status = @import("status.zig");
var visits: u8 = 0;
fn check(t: status.Terminator) bool {
    visits += 1;
    switch (t) {
        inline else => |terminator| {
            const response = "\x1b]7501;" ++ status.query_reply ++ comptime terminator.string();
            if (response.len != 7 + 21 + (if (t == .bell) @as(usize, 1) else 2)) return false;
            if (response[0] != 0x1b or response[7] != '?' or response[27] != 'g') return false;
            if (response[28] != (if (t == .bell) @as(u8, 7) else 27)) return false;
            return true;
        },
    }
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    visits = 0;
    if (!check(.bell) or !check(.st) or visits != 2) return 1;
    const Record = struct { child: u8 };
    const value: Record = .{ .child = 42 };
    const scalar = result: { break :result value.child; };
    if (scalar != 42) return 2;
    return 42;
}
