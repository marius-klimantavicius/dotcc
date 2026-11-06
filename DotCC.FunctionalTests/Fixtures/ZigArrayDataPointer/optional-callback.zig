var order: u32 = 0;
fn target(a: u8, b: u8) u8 { return if (order == 123) a + b else 1; }
var callback: ?*const fn (u8, u8) u8 = &target;
fn get() ?*const fn (u8, u8) u8 { order = order * 10 + 1; return callback; }
fn first() u8 { order = order * 10 + 2; return 40; }
fn last() u8 { order = order * 10 + 3; return 2; }
fn missing() ?u8 { order = order * 10 + 4; return null; }
fn lazy() u8 { return get().?(first(), missing() orelse return 9); }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (get().?(first(), last()) != 42 or order != 123) return 1;
    order = 0;
    if (lazy() != 9 or order != 124) return 2;
    return 42;
}
