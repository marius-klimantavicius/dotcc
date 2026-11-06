var effects: u32 = 0;
fn acquire(buffer: *[3:0]u8) [:0]u8 { effects = effects * 10 + 1; return buffer; }
fn mutate(buffer: *[3:0]u8) u32 { effects = effects * 10 + 2; buffer[0] = 19; return 1; }
fn read(pointer: [*:0]const u8, extra: u32) u32 {
    if (pointer[3] != 0) return 0;
    return pointer[0] + @as(u32, pointer[1]) + pointer[2] + extra;
}
fn provide(buffer: *[3:0]u8, fail: bool) error{Failed}![:0]u8 {
    if (fail) return error.Failed;
    return buffer;
}
fn control(buffer: *[3:0]u8) u32 {
    const value = provide(buffer, false) catch return 0;
    return read(value, 1);
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    effects = 0;
    var buffer: [3:0]u8 = .{ 20, 21, 1 };
    if (read(acquire(&buffer), mutate(&buffer)) != 42 or effects != 12) return 1;
    const caught = provide(&buffer, false) catch unreachable;
    if (read(caught, 1) != 42 or control(&buffer) != 42) return 2;
    return 42;
}
