fn increment(x: u8) u8 { return x + 1; }
fn consume(p: anytype) u8 {
    const pointer = p.@"7";
    const nested = p.@"8";
    pointer.* += nested.@"0".*;
    const callback = p.@"9";
    if (callback != &increment) return 1;
    if (p.@"10" != null) return 2;
    if (p.@"12") |present| { if (present != pointer) return 3; } else return 4;
    return callback(pointer.*) + @as(u8, @intCast(nested.@"1".len + p.@"11".len));
}
pub fn main() u8 {
    var first: u8 = 20;
    var second: u8 = 7;
    const absent: ?*u8 = null;
    const present: ?*u8 = &first;
    const values = .{ 1, 2, 3, 4, 5, 6, 7, &first, .{ &second, "abc" }, &increment, absent, "xyz", present };
    return consume(values);
}
