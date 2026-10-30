fn left(a: u8) u8 { return a; }
fn right(b: u8) u8 { return b; }
fn mix(a: u8, b: u8) i8 { return @bitCast((left(a) | right(b))); }
pub fn main() u8 { return @intCast(mix(1, 2) + 39); }
