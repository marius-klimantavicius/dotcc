const Result = struct { value: u8, nested: struct { number: u8 } };
fn success() error{Fail}!Result {
    const object = struct {}{};
    const value: u8 = 39;
    errdefer object.nonexistentCleanup();
    return .{ .value = value, .nested = .{ .number = 3 } };
}
fn maybe(fail: bool) error{Fail}!u8 {
    if (fail) return error.Fail;
    return 42;
}
fn live(calls: *u8, fail: bool) error{Fail}!Result {
    errdefer calls.* += 1;
    return .{ .value = try maybe(fail), .nested = .{ .number = 0 } };
}
pub fn main() u8 {
    const result = success() catch return 1;
    if (result.value + result.nested.number != 42) return 2;
    var calls: u8 = 0;
    const ok = live(&calls, false) catch return 3;
    if (ok.value != 42 or calls != 0) return 4;
    const failed: Result = live(&calls, true) catch .{ .value = 7, .nested = .{ .number = 0 } };
    if (failed.value != 7 or calls != 1) return 5;
    return 42;
}
