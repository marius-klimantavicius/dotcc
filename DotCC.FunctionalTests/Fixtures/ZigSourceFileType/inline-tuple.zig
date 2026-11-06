fn Key(comptime text: []const u8) type {
    return struct { pub fn first() u8 { return text[0]; } };
}
fn limits(input: u8) u8 {
    var total: u8 = 0;
    inline for (.{ .{ "a", 1 }, .{ "long", 2 }, .{ "xyz", 3 } }) |limit| {
        if (input != Key(limit[0]).first()) total += limit[1];
    }
    return total;
}
fn counted(calls: *u8, value: u8) u8 {
    calls.* += 1;
    return value;
}
fn effects(calls: *u8) u8 {
    var total: u8 = 0;
    inline for (.{ counted(calls, 10), counted(calls, 20) }) |value| {
        total += value;
    }
    return total;
}
pub fn main() u8 {
    if (limits('a') != 5 or limits('l') != 4 or limits('x') != 3) return 1;
    var visits: u8 = 0;
    inline for (.{ 1, 2, 3 }) |value| {
        if (value == 2) break;
        visits += value;
    }
    if (visits != 1) return 2;
    var calls: u8 = 0;
    if (effects(&calls) != 30 or calls != 2) return 3;
    return 42;
}
