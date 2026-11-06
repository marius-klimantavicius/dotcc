const t = @import("types.zig");
pub fn main() u8 {
    // This checks physical sizes independently of logical reflection.
    if (@sizeOf(t.Flags8) != 1) return 1;
    if (@sizeOf(t.Mixed8) != 1) return 2;
    if (@sizeOf(t.NonByte9) != 2) return 3;
    if (@sizeOf(t.Backed16) != 2) return 4;
    return 0;
}
