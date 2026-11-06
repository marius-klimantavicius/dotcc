const t = @import("types.zig");
const first = @import("first.zig");
const second = @import("second.zig");
fn width(comptime T: type) u32 { return @bitSizeOf(T); }
pub fn main() u8 {
    if (@bitSizeOf(t.Flags8) != 8) return 1;
    if (@bitSizeOf(t.Mixed8) != 8) return 2;
    if (@bitSizeOf(t.NonByte9) != 9) return 3;
    if (@bitSizeOf(t.Backed16) != 16) return 4;
    if (@bitSizeOf(t.Nested16) != 16) return 5;
    if (@bitSizeOf(t.Empty) != 0) return 7;
    if (width(t.NonByte9) != 9) return 8;
    if (@bitSizeOf(first.Bits) != 4 or @bitSizeOf(second.Bits) != 6) return 10;
    // Existing scalar answers remain unchanged.
    if (@bitSizeOf(u3) != 3 or @bitSizeOf(bool) != 1) return 9;
    return 0;
}
