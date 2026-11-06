const host = @import("host-prototypes.zig");
const again = @import("host-prototypes.zig");
fn counted(calls: *u8) u8 { calls.* += 1; return 1; }
pub fn main() u8 {
    const fd_t = u64;
    const local: fd_t = 42;
    if (host.write(1, "", 0) != 0 or again.emptyWrite() != 0) return 1;
    if (host.write(-1, "bad", 3) != -1) return 2;
    if (host.abs(-5) != 5 or again.abs(0) != 0 or host.query(-7) != 7) return 3;
    if (@sizeOf(host.Chosen) != 16 or @alignOf(host.Chosen) != 8) return 4;
    if (@sizeOf(c_longdouble) != 16 or @alignOf(c_longdouble) != 16) return 5;
    if (@bitSizeOf(c_longdouble) != 80 or @typeInfo(c_longdouble).float.bits != 80) return 6;
    if (@sizeOf(host.MaximumAlignment) != 32 or @alignOf(host.MaximumAlignment) != 16) return 7;
    if (@offsetOf(host.MaximumAlignment, "b") != 16) return 8;
    var calls: u8 = 0;
    const selected = switch (counted(&calls)) { 1 => @as(u8, 42), else => @as(u8, 1) };
    if (calls != 1 or selected != 42) return 9;
    if (@TypeOf(host.unavailable) != void) return 10;
    return @intCast(local);
}
