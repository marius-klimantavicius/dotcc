fn counts(x: u64) u8 {
    return @intCast(@clz(x) / 8 + @ctz(x) / 8 + @popCount(x) / 8);
}
fn remainder(x: u64) u8 { return @intCast(@clz(x) % 8); }
fn narrow(x: u5) u8 { return @intCast(@clz(x) / 2); }
pub fn main() u8 {
    if (counts(0) != 16) return 1;
    if (counts(1) != 7) return 2;
    if (counts(0xffffffffffffffff) != 8) return 3;
    if (remainder(1) != 7) return 4;
    if (narrow(0) != 2 or narrow(1) != 2 or narrow(16) != 0) return 5;
    return 42;
}
