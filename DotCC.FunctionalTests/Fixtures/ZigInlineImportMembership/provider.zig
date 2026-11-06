pub const public_value: u8 = 13;
pub fn unused_generic(comptime T: type) type {
    _ = T;
    @compileError("membership must not demand this function body");
}
pub fn selected() u8 {
    return if (@hasDecl(@import("root"), "dotcc_test_service"))
        @import("root").dotcc_test_service
    else
        7;
}
