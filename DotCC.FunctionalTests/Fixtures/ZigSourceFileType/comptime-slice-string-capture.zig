const names: []const []const u8 = &.{ "outside", "\xFF\x00a", "end" };
fn check(comptime text: []const u8) u8 {
    if (text.len != 3 or text[0] != 255 or text[1] != 0 or text[2] != 'a') return 1;
    return 42;
}
pub fn main() u8 {
    inline for (names[1..2]) |name| {
        const window = name[0..name.len];
        return check(window);
    }
    return 2;
}
