fn unavailable() noreturn { @panic("unavailable branch executed"); }
fn choose(present: bool) u8 {
    const optional: ?u8 = if (present) 42 else null;
    const value = optional orelse { unavailable(); };
    return value;
}
var cleanup_count: u8 = 0;
fn guarded(present: bool) error{Missing}!u8 {
    errdefer { cleanup_count += 1; }
    const optional: ?u8 = if (present) 42 else null;
    return optional orelse return error.Missing;
}
pub fn main() u8 {
    _ = guarded(false) catch {};
    if (cleanup_count != 1) return 1;
    const result = guarded(true) catch return 2;
    if (cleanup_count != 1 or result != 42) return 3;
    return choose(true);
}
