const provider = @import("nonexhaustive-provider.zig");
const CStyle = provider.CStyle;
fn reply(raw: CStyle, count: *u8) u8 {
    switch (raw) {
        .bar => { count.* += 1; return 10; },
        .block => { count.* += 2; return 20; },
        .underline => { count.* += 3; return 30; },
        .block_hollow, _ => { count.* += 4; return 40; },
    }
}
pub fn main() u8 {
    if (@typeInfo(CStyle).@"enum".mode != .nonexhaustive) return 6;
    for (0..4) |i| {
        const raw: CStyle = @enumFromInt(i);
        if (@intFromEnum(raw.convert().?) != i) return 1;
    }
    const negatives = [_]c_int{ -1, 4, 2147483647, (-2147483647 - 1) };
    for (negatives) |i| {
        const raw: CStyle = @enumFromInt(i);
        if (raw.convert() != null) return 2;
    }
    var count: u8 = 0;
    if (reply(.bar, &count) != 10 or count != 1) return 3;
    if (reply(.block_hollow, &count) != 40 or count != 5) return 4;
    if (reply(@enumFromInt(100), &count) != 40 or count != 9) return 5;
    return 42;
}
