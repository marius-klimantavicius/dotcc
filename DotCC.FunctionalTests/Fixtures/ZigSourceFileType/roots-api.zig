pub fn used() callconv(.c) u8 { return 42; }
pub fn ffi_only() callconv(.c) u8 { asm volatile ("nop"); return 7; }
