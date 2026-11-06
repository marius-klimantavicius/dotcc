const Code = enum(u16) { okay = 0, retry = 5, denied = 37, _ };
var reads: u8 = 0;
fn get(value: u16) Code {
    reads += 1;
    return @enumFromInt(value);
}
fn score(value: u16) u16 {
    switch (get(value)) {
        .okay => |code| return @intFromEnum(code),
        .retry, .denied => |code| {
            if (@TypeOf(code) != Code) return 999;
            return @intFromEnum(code);
        },
        else => |code| return @intFromEnum(code),
    }
}
pub fn main() u8 {
    if (score(0) != 0 or score(5) != 5 or score(37) != 37 or score(100) != 100) return 1;
    if (reads != 4) return 2;
    return 42;
}
