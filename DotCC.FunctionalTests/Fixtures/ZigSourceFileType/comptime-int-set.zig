const std = @import("std");
const Set = @import("intset").ComptimeIntSet;
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const Many = Set(u16, &.{2026, 25, 1049, 0, 65535, 8, 100});
    const Other = Set(u16, &.{91, 2, 900, 8, 512, 0, 25});
    const Signed = Set(i16, &.{-32768, 42, -1, 32767, 0});
    if (@typeInfo(Many.Index).int.bits != 3) return 1;
    var key: u32 = 0;
    while (key <= 65535) : (key += 1) {
        const found = Many.indexOf(@intCast(key));
        const expected: ?u8 = switch (key) {2026 => 0, 25 => 1, 1049 => 2, 0 => 3, 65535 => 4, 8 => 5, 100 => 6, else => null};
        if ((found != null) != (expected != null)) return 6;
        if (found) |index| { if (index != expected.?) return 7; }
        const other = Other.indexOf(@intCast(key));
        const other_expected: ?u8 = switch (key) {91 => 0, 2 => 1, 900 => 2, 8 => 3, 512 => 4, 0 => 5, 25 => 6, else => null};
        if ((other != null) != (other_expected != null)) return 18;
        if (other) |index| { if (index != other_expected.?) return 19; }
    }
    if ((Signed.indexOf(-32768) orelse return 8) != 0) return 9;
    if ((Signed.indexOf(42) orelse return 10) != 1) return 11;
    if ((Signed.indexOf(-1) orelse return 12) != 2) return 13;
    if ((Signed.indexOf(32767) orelse return 14) != 3) return 15;
    if ((Signed.indexOf(0) orelse return 16) != 4 or Signed.indexOf(1) != null) return 17;
    return 42;
}
