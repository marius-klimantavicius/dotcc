fn check(flag: bool, value: u21) u8 {
    const leading = if (flag) 0 else value;
    const trailing = if (flag) value else 0;
    if (@bitSizeOf(@TypeOf(leading)) != 21 or @bitSizeOf(@TypeOf(trailing)) != 21) return 1;
    return if (leading + trailing == 65) 42 else 2;
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 { return if (check(true, 65) == 42) check(false, 65) else 3; }
