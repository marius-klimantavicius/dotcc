fn choose(flag: bool, value: u8) u8 {
    const result = if (flag) 256 else value;
    return result;
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 { return choose(false, 42); }
