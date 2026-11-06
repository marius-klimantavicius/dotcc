const enabled = false;
pub fn main() u8 {
    if (comptime enabled) @compileError("UNTAKEN_EXPLICIT_COMPTIME_ARM");
    return 42;
}
