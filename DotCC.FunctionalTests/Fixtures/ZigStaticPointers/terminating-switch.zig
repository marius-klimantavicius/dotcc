const Mode = enum { good, bad, other };
var calls: u8 = 0;
fn next() Mode { calls += 1; return .good; }
fn stop() noreturn { @trap(); }
fn select(mode: Mode, flag: bool) u8 {
    switch (mode) {
        .good => return 41,
        .bad => stop(),
        .other => { if (flag) stop() else stop(); },
    }
    comptime unreachable;
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 { return select(next(), false) + calls; }
