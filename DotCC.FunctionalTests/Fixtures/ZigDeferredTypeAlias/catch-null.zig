var calls: u8 = 0;
fn text(fail: bool, empty: bool) error{Failure}![]const u8 {
    calls += 1;
    if (fail) return error.Failure;
    return if (empty) "" else "forty-two";
}
fn number(fail: bool) error{Failure}!u16 {
    calls += 1;
    if (fail) return error.Failure;
    return 42;
}
pub fn main() u8 {
    const present: ?[]const u8 = text(false, false) catch null;
    const absent: ?[]const u8 = text(true, false) catch null;
    const empty = text(false, true) catch null;
    const value = number(false) catch null;
    const missing = number(true) catch null;
    if (present == null or absent != null or empty == null or value == null or missing != null) return 1;
    if (present.?.len != 9 or empty.?.len != 0 or calls != 5) return 2;
    return @intCast(value.?);
}
