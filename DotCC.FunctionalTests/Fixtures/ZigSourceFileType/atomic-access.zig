fn address(p: *u64, count: *u8) *u64 { count.* += 1; return p; }
fn change(p: **u64, replacement: *u64) ?u64 { p.* = replacement; return 42; }
pub fn main() u8 {
    var count: u8 = 0;
    var word: u64 = 0;
    const pattern: u64 = 0xfedcba9876543210;
    @atomicStore(u64, address(&word, &count), pattern, .release);
    const value = @atomicLoad(u64, address(&word, &count), .acquire);
    if (value != pattern or count != 2) return 1;
    var small: u8 = 0;
    @atomicStore(u8, &small, 42, .unordered);
    if (@atomicLoad(u8, &small, .unordered) != 42) return 2;
    var half: u16 = 0;
    @atomicStore(u16, &half, 0xfedc, .monotonic);
    if (@atomicLoad(u16, &half, .monotonic) != 0xfedc) return 3;
    var signed: i32 = 0;
    @atomicStore(i32, &signed, -42, .seq_cst);
    if (@atomicLoad(i32, &signed, .seq_cst) != -42) return 4;
    var other: u64 = 42;
    var pointer: *u64 = &word;
    @atomicStore(*u64, &pointer, &other, .seq_cst);
    if (@atomicLoad(*u64, &pointer, .monotonic) != &other) return 5;
    var before: u64 = 0;
    var after: u64 = 1;
    var target: *u64 = &before;
    @atomicStore(u64, target, change(&target, &after) orelse return 6, .seq_cst);
    if (before != 42 or after != 1 or target != &after) return 7;
    return @intCast(@atomicLoad(u64, &other, .seq_cst));
}
