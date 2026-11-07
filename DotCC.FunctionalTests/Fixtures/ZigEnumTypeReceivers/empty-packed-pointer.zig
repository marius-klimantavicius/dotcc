const Pair = packed struct(u32) { first: u16 = 17, second: u16 = 25 };
const explicit: Pair = .{ .first = 3, .second = 4 };
fn choose(use_explicit: bool) *const Pair {
    return if (use_explicit) &explicit else &.{};
}
export fn answer() c_int {
    const empty = choose(false);
    if (empty.first != 17 or empty.second != 25) return 1;
    if (empty != choose(false)) return 2;
    const value = choose(true);
    if (value.first != 3 or value.second != 4) return 3;
    return 42;
}
