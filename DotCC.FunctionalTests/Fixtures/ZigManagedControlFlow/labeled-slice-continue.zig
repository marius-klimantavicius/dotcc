pub fn main() u8 {
    const values = [_]u8{ 1, 2, 3, 4 };
    const skipped = [_]u8{ 2, 4 };
    var total: u64 = 0;
    var else_runs: u32 = 0;
    outer: for (values[0..], 0..) |value, index| {
        for (skipped) |skip| {
            if (value == skip) continue :outer;
        }
        total += @as(u64, value) + index;
    } else {
        else_runs += 1;
    }
    if (total != 6 or else_runs != 1) return 1;
    total = 0;
    else_runs = 0;
    search: for (values) |value| {
        for (skipped) |skip| {
            if (value == skip) continue :search;
        }
        if (value == 3) break :search;
        total += value;
    } else {
        else_runs += 1;
    }
    if (total != 1 or else_runs != 0) return 2;
    var index: u64 = 0;
    total = 0;
    next: while (index < 5) : (index += 1) {
        if (index == 2) continue :next;
        total += index;
    }
    if (index != 5 or total != 8) return 3;
    return 42;
}
