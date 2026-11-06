const Result = union(enum) { read: error{IO}!usize, write: error{IO}!usize };
fn operation(value: u8) error{Cancel}!Result {
    if (value == 2) return error.Cancel;
    return switch (value) {
        0 => .{ .read = error.IO },
        else => .{ .write = 42 },
    };
}
fn pair(value: u8) bool {
    const lower: u8, const upper: u8 = switch (value) {
        0 => .{ 160, 191 },
        1 => .{ 128, 159 },
        else => .{ 128, 191 },
    };
    if (@typeInfo(@TypeOf(lower)).int.bits != 8 or @typeInfo(@TypeOf(upper)).int.bits != 8) return false;
    return if (value == 0) lower == 160 and upper == 191 else if (value == 1) lower == 128 and upper == 159 else lower == 128 and upper == 191;
}
pub fn main() u8 {
    if (!pair(0) or !pair(1) or !pair(2)) return 1;
    const first = operation(0) catch return 2;
    switch (first) {
        .read => |value| { if (value) |_| return 3 else |err| { if (err != error.IO) return 4; } },
        .write => return 5,
    }
    const second = operation(1) catch return 6;
    switch (second) { .read => return 7, .write => |value| { if ((value catch return 8) != 42) return 9; } }
    _ = operation(2) catch |err| return if (err == error.Cancel) 42 else 10;
    return 11;
}
