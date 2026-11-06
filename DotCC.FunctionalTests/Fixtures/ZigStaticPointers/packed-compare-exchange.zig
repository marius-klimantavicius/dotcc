const Fill = packed struct(u32) { at: u31, extended: bool };
var fill: Fill = .{ .at = 7, .extended = true };
var sequence: u8 = 0;
fn expected() Fill { sequence = sequence * 10 + 1; return .{ .at = 7, .extended = true }; }
fn desired() Fill { sequence = sequence * 10 + 2; return .{ .at = 8, .extended = false }; }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (@cmpxchgStrong(Fill, &fill, expected(), desired(), .acq_rel, .acquire) != null) return 1;
    if (sequence != 12 or fill.at != 8 or fill.extended) return 2;
    const observed = @cmpxchgWeak(Fill, &fill, .{ .at = 7, .extended = true }, .{ .at = 9, .extended = true }, .monotonic, .monotonic);
    if (observed == null) return 3;
    if (observed.?.at != 8 or observed.?.extended) return 4;
    if (fill.at != 8 or fill.extended) return 5;
    fill = .{ .at = 0, .extended = false };
    const zero = @cmpxchgStrong(Fill, &fill, .{ .at = 1, .extended = false }, .{ .at = 2, .extended = true }, .monotonic, .monotonic);
    if (zero == null) return 6;
    if (zero.?.at != 0 or zero.?.extended or fill.at != 0 or fill.extended) return 7;
    return 42;
}
