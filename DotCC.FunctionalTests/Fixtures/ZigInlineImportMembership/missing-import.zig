comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (@hasDecl(@import("does-not-exist.zig"), "member")) return 1;
    return 42;
}
