const builtin = @import("builtin");
fn answer(comptime text: []const u8) u8 {
    if (text.len != 6 or text[0] != 'c') return 1;
    return 42;
}
fn terminated(comptime text: []const u8) bool { return text.len == 7 and text[6] == 0; }
pub fn main() u8 {
    const text = if (builtin.target.os.tag == .illumos) "other" else "chosen";
    if (!terminated(text ++ [1]u8{0})) return 2;
    return answer(text);
}
