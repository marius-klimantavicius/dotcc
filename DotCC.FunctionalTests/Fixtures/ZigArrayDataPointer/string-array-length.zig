comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const wrong: [5]u8 = "ABCD".*;
    return wrong[0];
}
