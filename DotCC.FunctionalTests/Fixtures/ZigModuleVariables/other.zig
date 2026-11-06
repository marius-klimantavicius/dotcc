pub var global: u32 = 11;
pub fn add(n: u32) void { global += n; }
pub fn read() u32 { return global; }
