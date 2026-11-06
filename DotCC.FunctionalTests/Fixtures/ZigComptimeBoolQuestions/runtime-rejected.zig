var enabled: bool = false;
fn question(comptime T: type) bool {
    return switch (@typeInfo(T)) { .int => enabled, else => false };
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (comptime question(u8)) return 1;
    return 42;
}
