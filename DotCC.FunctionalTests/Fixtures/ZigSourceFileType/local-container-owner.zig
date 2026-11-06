const provider = @import("local-owner-provider.zig");
pub fn main() u8 {
    const narrow = provider.Stats(u8).info;
    const wide = provider.Stats(u64).info;
    if (narrow.value != 42 or wide.value != 42) return 1;
    if (@sizeOf(@TypeOf(narrow)) != 1 or @sizeOf(@TypeOf(wide)) != 8) return 2;
    if (@TypeOf(narrow) == @TypeOf(wide)) return 3;
    if (provider.Stats(u8).stamp != 42 or provider.Stats(u64).stamp != 42) return 4;
    const repeated = provider.Stats(u8).info;
    if (@TypeOf(repeated) != @TypeOf(narrow) or repeated.value != 42) return 5;
    return repeated.value;
}
