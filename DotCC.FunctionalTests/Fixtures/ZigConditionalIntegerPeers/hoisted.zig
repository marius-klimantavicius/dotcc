var conditions: usize = 0;
var reads: usize = 0;
fn condition(flag: bool) bool { conditions += 1; return flag; }
fn read(value: ?u21) ?u21 { reads += 1; return value; }
fn choose(flag: bool, value: ?u21) u8 {
    const result = if (condition(flag)) 0 else read(value) orelse return 2;
    if (@bitSizeOf(@TypeOf(result)) != 21) return 3;
    return if (result == 65) 42 else 0;
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (choose(true, null) != 0 or conditions != 1 or reads != 0) return 4;
    if (choose(false, 65) != 42 or conditions != 2 or reads != 1) return 5;
    return 42;
}
