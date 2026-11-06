var calls: u32 = 0;
fn fallback() u32 { calls += 1; return 5; }
fn fromValue(value: ?u32) u32 { return if (value) |v| v else fallback(); }
fn terminatingThen(value: ?u32, state: *u32) u32 {
    const result = if (value) |_| { state.* += 2; return 12; } else @as(u32, 10);
    return result;
}
fn both(value: ?u32) u32 {
    const unused = if (value) |v| { return v; } else { return 9; };
    return unused;
}
fn nested(opt: *?u32, active: bool) u32 {
    return if (active) (if (opt.*) |*v| blk: { v.* += 1; break :blk v.*; } else 8) else 6;
}
pub fn main() u8 {
    var opt: ?u32 = 20;
    var state: u32 = 0;
    if (nested(&opt, false) != 6 or opt.? != 20) return 1;
    if (nested(&opt, true) != 21 or opt.? != 21) return 2;
    if (fromValue(30) != 30 or calls != 0) return 3;
    if (fromValue(null) != 5 or calls != 1) return 4;
    if (terminatingThen(3, &state) != 12 or state != 2) return 5;
    if (terminatingThen(null, &state) != 10 or state != 2) return 6;
    if (both(33) != 33 or both(null) != 9) return 7;
    if (direct(17) != 17 or direct(null) != 4 or jumps() != 23) return 8;
    return 42;
}
fn direct(value: ?u32) u32 { return if (value) |v| v else return 4; }
fn jumps() u32 {
    var total: u32 = 0;
    var i: u32 = 0;
    while (i < 3) : (i += 1) {
        defer total += 1;
        const v: ?u32 = if (i == 1) null else 10;
        const n = if (v) |n| n else continue;
        total += n;
    }
    return total;
}
