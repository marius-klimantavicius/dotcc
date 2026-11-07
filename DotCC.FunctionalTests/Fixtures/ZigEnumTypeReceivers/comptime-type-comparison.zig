fn classify(comptime Cp: type) bool {
    if (comptime Cp == u8) return true;
    return false;
}
export fn answer() c_int {
    if (!classify(u8) or classify(u32)) return 1;
    return 42;
}
