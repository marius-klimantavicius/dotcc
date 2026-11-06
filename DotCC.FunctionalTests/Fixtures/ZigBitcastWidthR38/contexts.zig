const defs = @import("defs.zig");
const Word = u32;
fn negative() i24 { return -1; }
fn pointed(p: *const [3]u8) u24 { return @bitCast(p.*); }
pub fn main() u8 {
    var h: defs.Holder = .{ .lanes = .{1, 2, 3} };
    if (h.value != 0xffffff) return 1;
    h.value = @bitCast(negative());
    if (defs.take(@bitCast(negative())) != 0) return 2;
    if (h.take(@bitCast(negative())) != 0) return 3;
    const bits: u9 = @bitCast(h.lanes);
    if (bits != 209 or defs.lanes(.{1, 2, 3}) != 209) return 4;
    var a: [1]defs.Word = .{ @bitCast(negative()) };
    a[0] = @bitCast(negative());
    if (a[0] != 0xffffff) return 5;
    if (defs.generic(u24, @bitCast(negative())) != 0xffffff) return 6;
    if (defs.recode(u24, negative()) != 0xffffff) return 7;
    const bytes = [3]u8{0x12, 0x34, 0x56};
    if (pointed(&bytes) != 0x563412) return 8;
    const widened: Word = @as(defs.Word, @bitCast(negative()));
    return if (widened == 0xffffff) 0 else 9;
}
