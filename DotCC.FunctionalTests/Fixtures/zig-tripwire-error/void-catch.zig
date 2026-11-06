var effects: u32 = 0;
fn attempt(fail: bool) error{Denied}!void {
    effects = effects * 10 + 1;
    if (fail) return error.Denied;
}
fn consume(value: void, after: u8) void { _ = value; _ = after; }
fn last() u8 { effects = effects * 10 + 3; return 0; }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    effects = 0;
    consume(attempt(true) catch |err| {
        if (err != error.Denied) return 1;
        defer { effects = effects * 10 + 2; }
        effects = effects * 10 + 4;
    }, last());
    if (effects != 1423) return 2;
    consume(attempt(false) catch { effects = 9999; }, last());
    if (effects != 142313) return 3;
    return 42;
}
