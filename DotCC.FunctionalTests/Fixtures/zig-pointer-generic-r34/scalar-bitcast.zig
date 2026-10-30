pub fn main() u8 { const bits: u32 = 1065353216; const value: f32 = @bitCast(bits); return @intFromFloat(value * 42); }
