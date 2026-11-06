var calls: u8 = 0;
fn ordinary(x: u8) u8 { calls += 1; return x; }
pub fn main() u8 { const result = ordinary(41); return result + calls; }
