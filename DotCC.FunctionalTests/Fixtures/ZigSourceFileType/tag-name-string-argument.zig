const Option = enum { id, app, @"my-key" };
fn key(comptime name: []const u8) u8 {
    if (name.len == 2 and name[0] == 'i' and name[1] == 'd') return 40;
    if (name.len == 3 and name[0] == 'a' and name[1] == 'p' and name[2] == 'p') return 41;
    if (name.len == 6 and name[0] == 'm' and name[2] == '-' and name[5] == 'y') return 42;
    return 1;
}
fn read(comptime option: Option) u8 { return key(@tagName(option)); }
fn length(comptime name: []const u8) usize { return name.len; }
pub fn main() u8 {
    if (read(.id) != 40) return 1;
    if (read(.app) != 41) return 2;
    if (length("@" ++ @tagName(Option.app)) != 4) return 3;
    return read(.@"my-key");
}
