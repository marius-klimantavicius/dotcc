fn reject(comptime fail: bool) u32 {
    if (@inComptime()) {
        if (fail) @compileError("selected failure");
        return 2;
    } else {
        return 42;
    }
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u32 { return comptime reject(true); }
pub fn main() u8 { return @intCast(answer()); }
