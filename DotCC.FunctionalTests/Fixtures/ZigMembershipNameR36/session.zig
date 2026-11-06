const Target = struct { value: u8, pub const answer: u8 = 42; };
comptime {
    const name: []const u8 = "value";
    if (!@hasField(Target, name)) @compileError("R36_SESSION_STRING");
    const bytes = [_]u8{ 'x', 'v', 'a', 'l', 'u', 'e', 'x' };
    const part = bytes[1..6];
    if (!@hasField(Target, part)) @compileError("R36_SESSION_SLICE");
}
pub fn main() u8 { return Target.answer; }
