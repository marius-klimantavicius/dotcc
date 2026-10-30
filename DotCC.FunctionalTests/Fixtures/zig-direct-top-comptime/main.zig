comptime {
    @compileError("DOTCC_ZIG_DIRECT_ROOT_COMPTIME_MUST_FAIL");
}

pub fn main() u8 {
    return 42;
}
