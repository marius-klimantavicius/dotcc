const Cell = packed struct(u64) { code: u21, wide: u2, rest: u41 };
fn bits(wide: bool) u64 {
    const template = Cell{ .code = 42, .wide = 0, .rest = 0x12345 };
    const result: u64 = if (wide) wb: {
        var w = template;
        w.wide = 2;
        break :wb @bitCast(w);
    } else @bitCast(template);
    return result;
}
const Small = packed struct(u9) { a: u5, b: u4 };
fn narrow(wide: bool) u9 {
    const template = Small{ .a = 17, .b = 3 };
    const result: u9 = if (wide) wb: { break :wb @bitCast(template); } else 0;
    return result;
}
fn known(value: u9) u9 { return value; }
fn selected() u9 {
    const value = switch (@as(u8, 64)) {
        64 => known(321),
        else => fallback: { break :fallback @as(u9, 1); },
    };
    return @bitCast(value);
}
pub fn main() u8 {
    if (bits(false) != 42 + (@as(u64, 0x12345) << 23)) return 1;
    if (bits(true) != 42 + (@as(u64, 2) << 21) + (@as(u64, 0x12345) << 23)) return 2;
    if (narrow(true) != 113 or narrow(false) != 0) return 3;
    if (selected() != 321) return 4;
    return 42;
}
