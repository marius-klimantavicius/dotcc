fn sequenceLength(cp: u21) error{Invalid}!u3 {
    if (cp < 0x80) return 1;
    if (cp < 0x800) return 2;
    if (cp < 0x10000) return 3;
    if (cp <= 0x10ffff) return 4;
    return error.Invalid;
}
fn encoded(comptime cp: u21) [sequenceLength(cp) catch |err| @compileError(@errorName(err))]u8 {
    return @splat(42);
}
pub fn main() u8 {
    if (encoded('$').len != 1 or encoded(0xa2).len != 2) return 1;
    if (encoded(0x20ac).len != 3 or encoded(0x10348).len != 4) return 2;
    const bytes = encoded(0x20ac);
    if (bytes[0] != 42 or bytes[2] != 42) return 3;
    return encoded('$')[0];
}
