const builtin = @import("builtin");
fn selected() c_int {
    if (comptime builtin.mode == .debug) @compileError("debug branch in fast source");
    if (comptime builtin.mode != .fast) @compileError("wrong source optimization mode");
    return 42;
}
export fn answer() c_int {
    return selected();
}
