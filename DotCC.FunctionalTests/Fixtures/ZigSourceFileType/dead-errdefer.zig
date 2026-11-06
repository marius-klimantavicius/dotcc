fn successful() error{Fail}!u8 {
    const result: u8 = 42;
    const object = struct {}{};
    errdefer object.nonexistentCleanup();
    return result;
}
fn fallible(calls: *u8, fail: bool) error{Fail}!u8 {
    errdefer calls.* += 1;
    if (fail) return error.Fail;
    return 42;
}
pub fn main() u8 {
    const success = successful() catch return 1;
    if (success != 42) return 2;
    var calls: u8 = 0;
    const live = fallible(&calls, false) catch return 3;
    if (live != 42 or calls != 0) return 4;
    const failed = fallible(&calls, true) catch 17;
    if (failed != 17 or calls != 1) return 5;
    return 42;
}
