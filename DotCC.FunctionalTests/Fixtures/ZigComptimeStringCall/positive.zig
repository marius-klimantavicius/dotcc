const Option = enum {
    cp, fmt,
    fn key(comptime self: Option) []const u8 { return @tagName(self); }
};
fn keyLength(comptime key: []const u8) usize { return key.len; }
fn get(comptime option: Option) usize { return keyLength(option.key()); }
fn zeroKey() []const u8 { return "a\x00b"; }
fn checkZero(comptime key: []const u8) bool { return key.len == 3 and key[0] == 'a' and key[1] == 0 and key[2] == 'b'; }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (get(.cp) != 2 or get(.fmt) != 3 or !checkZero(zeroKey())) return 1;
    return 42;
}
