const Entry = struct { value: comptime_int, selected: bool = false };
const entries: []const Entry = &.{
    .{ .value = 99 }, .{ .value = 11 }, .{ .value = 20, .selected = true }, .{ .value = 31 }, .{ .value = 98 },
};
const window = entries[1..4];
fn find(input: u8) u8 {
    inline for (window) |entry| {
        if (entry.selected) continue;
        if (input == entry.value) return entry.value;
    }
    return 0;
}
pub fn main() u8 {
    if (find(99) != 0 or find(98) != 0 or find(20) != 0) return 1;
    var visits: u8 = 0;
    inline for (window) |entry| {
        if (entry.selected) break;
        visits += 1;
    }
    if (visits != 1) return 2;
    inline for (entries[2..2]) |_| { return 3; }
    return find(11) + find(31);
}
