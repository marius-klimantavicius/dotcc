const Fill = packed struct(u32) { at: u31, extended: bool };
const Other = packed struct(u32) { at: u31, extended: bool };
var fill: Fill = .{ .at = 7, .extended = true };
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    _ = @cmpxchgWeak(Fill, &fill, Other{ .at = 7, .extended = true }, .{ .at = 8, .extended = false }, .monotonic, .monotonic);
    return 42;
}
