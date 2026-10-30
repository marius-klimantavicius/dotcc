comptime {
    @compileError("DOTCC_ZIG_DIRECT_IMPORTED_COMPTIME_MUST_FAIL");
}

pub fn answer() u8 {
    return 42;
}
