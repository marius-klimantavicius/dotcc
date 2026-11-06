fn signed(x: u128) i128 { return @bitCast(x); }
fn unsigned(x: i128) u128 { return @bitCast(x); }
fn doubleBits(x: f64) u64 { return @bitCast(x); }
fn doubleValue(x: u64) f64 { return @bitCast(x); }
pub fn main() u8 {
    const sign = @as(u128, 1) << 127;
    const negative = signed(sign);
    if (negative >= 0 or unsigned(negative) != sign) return 1;
    if (unsigned(-1) != ~@as(u128, 0)) return 2;
    if (doubleBits(1.0) != 0x3ff0000000000000) return 3;
    if (doubleValue(0x3ff0000000000000) != 1.0) return 4;
    return 0;
}
