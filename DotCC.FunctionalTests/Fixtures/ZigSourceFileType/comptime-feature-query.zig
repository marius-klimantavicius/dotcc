const provider = @import("comptime-feature-provider.zig");
pub fn main() u8 {
    if (@TypeOf(provider.present) == void) return 1;
    if (@TypeOf(provider.absent) != void) return 2;
    if (provider.scalar != 42) return 3;
    if (provider.select(true) + provider.select(false) != 42) return 4;
    const callback: *const fn (u8) u8 = provider.present;
    return callback(40);
}
