fn count(a: u128, b: u128) u128 {
    return @divCeil(a, b);
}
const from_high_unsigned = @divCeil(@as(u128, 340282366920938463463374607431768211455), @as(u128, 340282366920938463463374607431768211454));
const from_negative = @divCeil(@as(i128, -7), 3) + 5;
const from_wrapping = @divCeil(@as(u8, @truncate(@as(u16, 257))), 2);
const from_wrapping_high = @divCeil(@as(u8, @truncate(@as(u128, 340282366920938463463374607431768211455))), 128);
pub fn main() u8 {
    const from_function = comptime count(18446744073709551617, 18446744073709551616);
    const a: [from_function]u8 = .{ 40, 2 };
    const b: [from_high_unsigned]u8 = .{ 40, 2 };
    const c: [from_negative]u8 = .{ 10, 20, 12 };
    const d: [from_wrapping]u8 = .{ 42 };
    const e: [from_wrapping_high]u8 = .{ 40, 2 };
    if (@sizeOf(@TypeOf(a)) != 2 or @sizeOf(@TypeOf(b)) != 2 or @sizeOf(@TypeOf(c)) != 3) return 1;
    if (a[0] + a[1] != 42 or b[0] + b[1] != 42 or c[0] + c[1] + c[2] != 42) return 2;
    if (d[0] != 42 or e[0] + e[1] != 42) return 3;
    return 42;
}
