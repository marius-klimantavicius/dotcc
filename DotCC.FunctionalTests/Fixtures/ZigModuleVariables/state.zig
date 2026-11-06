const initial: u32 = 5;
pub var global: u32 = initial;
pub var values: [2]u32 = .{ 3, 7 };
pub var unused: u32 = @compileError("UNUSED_GLOBAL_REACHED");
pub fn add(n: u32) void { global += n; }
pub fn read() u32 { return global; }
pub fn pointer() *u32 { return &global; }
