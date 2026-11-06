pub threadlocal var value: u32 = 17;
pub fn add() void { value += 8; }
pub fn read() u32 { return value; }
