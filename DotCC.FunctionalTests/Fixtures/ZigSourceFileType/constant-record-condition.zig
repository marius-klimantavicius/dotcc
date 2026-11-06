const Callback = *const fn (u8) u8;
const Settings = struct { enabled: bool, callback: Callback };
fn increment(value: u8) u8 { return value + 1; }
const settings: Settings = config: {
    var value: Settings = .{ .enabled = true, .callback = increment };
    value.enabled = false;
    break :config value;
};
var mutable: Settings = .{ .enabled = false, .callback = increment };
var calls: u8 = 0;
fn query() bool { calls += 1; return false; }
fn read() u8 {
    if (settings.enabled) @compileError("immutable condition must prune this branch");
    if (query()) return 1;
    if (calls != 1) return 2;
    mutable.enabled = true;
    if (mutable.enabled) calls += 1;
    if (calls != 2) return 3;
    return settings.callback(41);
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    calls = 0;
    return read();
}
