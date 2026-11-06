const Event = enum { registration, acceptance };
fn condition(calls: *u8, value: bool) bool { calls.* += 1; return value; }
fn event(calls: *u8, more: bool) error{Fail}!?Event {
    return if (condition(calls, more)) null else .registration;
}
fn reversed(calls: *u8, present: bool) error{Fail}!?Event {
    return if (condition(calls, present)) .acceptance else null;
}
fn fallible(calls: *u8, cleaned: *u8, fail: bool) error{Fail}!?Event {
    errdefer cleaned.* += 1;
    return if (condition(calls, fail)) error.Fail else .acceptance;
}
pub fn main() u8 {
    var calls: u8 = 0;
    const missing = event(&calls, true) catch return 1;
    if (missing != null or calls != 1) return 2;
    const present = (event(&calls, false) catch return 3) orelse return 4;
    if (present != .registration or calls != 2) return 5;
    const accepted = (reversed(&calls, true) catch return 6) orelse return 7;
    if (accepted != .acceptance or calls != 3) return 7;
    if ((reversed(&calls, false) catch return 8) != null or calls != 4) return 9;
    var cleaned: u8 = 0;
    const live = (fallible(&calls, &cleaned, false) catch return 10) orelse return 11;
    if (live != .acceptance) return 11;
    const failed = fallible(&calls, &cleaned, true) catch null;
    if (failed != null or cleaned != 1 or calls != 6) return 12;
    return 42;
}
