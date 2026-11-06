fn question(comptime T: type) bool {
    return switch (@typeInfo(T)) {
        .@"struct" => {
            @compileError("selected type question validation");
        },
        else => false,
    };
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (comptime question(struct { value: u8 })) return 1;
    return 42;
}
