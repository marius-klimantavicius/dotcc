const provider = @import("provider.zig");
pub const dotcc_test_service: u8 = 42;
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (!@hasDecl(@import("root"), "dotcc_test_service")) return 1;
    if (!@hasDecl(@import("provider.zig"), "public_value")) return 2;
    if (@hasDecl(@import("provider.zig"), "missing")) return 3;
    if (!@hasDecl(@import("provider.zig"), "unused_generic")) return 4;
    if (@hasDecl(@import("provider.zig"), "unused_generic") != @hasDecl(provider, "unused_generic")) return 5;
    if (!@hasDecl(@import("std"), "testing")) return 6;
    return provider.selected();
}
