fn reject() u32 {
    @compileError("unconditional failure");
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u32 { return reject(); }
pub fn main() u8 { return @intCast(answer()); }
