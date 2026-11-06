var calls: u8 = 0;
var total: u8 = 0;
fn operation(fail: bool) error{Small}!void {
    calls += 1;
    if (fail) return error.Small;
    total += 3;
}
fn selected(fail: bool) void {
    const unit: void = operation(fail) catch |err| switch (err) {
        error.Small => { total += 7; },
    };
    _ = unit;
}
fn terminating(fail: bool) u8 {
    operation(fail) catch |err| switch (err) {
        error.Small => return 11,
    };
    return 13;
}
fn repeated() error{Small}!void {
    for (0..3) |_| try operation(false);
}
pub fn main() u8 {
    selected(false);
    selected(true);
    if (calls != 2 or total != 10) return 1;
    if (terminating(true) != 11 or terminating(false) != 13) return 2;
    if (calls != 4 or total != 13) return 3;
    repeated() catch return 4;
    if (calls != 7 or total != 22) return 5;
    return 42;
}
