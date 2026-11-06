const std = @import("std");
const Set = std.bit_set.Integer(1);
fn index(counter: *u32) usize { counter.* += 1; return 0; }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (Set.ShiftInt != u0 or @bitSizeOf(Set.ShiftInt) != 0) return 1;
    var counter: u32 = 0;
    var bits: Set = .empty;
    if (bits.isSet(index(&counter))) return 2;
    bits.set(index(&counter));
    var copy = bits;
    bits.unset(index(&counter));
    if (bits.isSet(index(&counter)) or !copy.isSet(index(&counter))) return 3;
    copy.toggle(index(&counter));
    if (copy.isSet(index(&counter)) or counter != 7) return 4;
    return 42;
}
