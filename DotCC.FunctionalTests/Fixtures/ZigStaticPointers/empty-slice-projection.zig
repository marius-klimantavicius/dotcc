const Entry = struct { value: u8 };
const safety = @import("builtin").mode.runtimeSafety() and true;
const Options = struct { callback: *const fn () u8, filter: []const Entry = &.{}, flag: ?u8 = null, enabled: bool = safety, allow: bool = false };
fn expected() u8 { return 42; }
const options: Options = .{ .callback = expected };
var calls: u8 = 0;
fn runtimeFlag() bool { calls += 1; return false; }
fn noTrace() u8 {
    if (!options.allow) return options.callback();
    @compileError("code after constant guard return must remain unanalyzed");
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    inline for (options.filter) |entry| {
        _ = entry;
        @compileError("empty constant slice body must remain unanalyzed");
    }
    if (options.filter.len != 0 or options.flag != null) return 1;
    if (options.enabled != @import("builtin").mode.runtimeSafety()) return 2;
    if (runtimeFlag()) return 3;
    if (calls != 1) return 4;
    return noTrace();
}
