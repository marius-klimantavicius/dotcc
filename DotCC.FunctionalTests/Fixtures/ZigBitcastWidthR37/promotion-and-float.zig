fn narrow(a: u3, b: u3) i3 { return @bitCast((a + b)); }
fn grouped(a: u8, b: u8) u8 { return @bitCast((a + b)); }
fn floatBits(x: f32) u32 { return @bitCast(x); }
fn fromBits(x: u32) f32 { return @bitCast(x); }
pub fn main() u8 {
    if (narrow(1,2) != 3 or grouped(20,22) != 42) return 1;
    if (floatBits(1.0) != 0x3f800000 or fromBits(0x3f800000) != 1.0) return 2;
    return 0;
}
