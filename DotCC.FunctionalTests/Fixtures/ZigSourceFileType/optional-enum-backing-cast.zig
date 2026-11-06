const Mode = enum(u8) { right = 0, left = 1, all = 2 };
var calls: u8 = 0;
fn value(x: u16) u16 {
    calls += 1;
    return x;
}
fn parse(input: []const u16) ?Mode {
    return switch (input.len) {
        0 => .right,
        1 => if (input[0] < 3) @fromBackingInt(@truncate(value(input[0]))) else null,
        else => null,
    };
}
fn present(x: u16) ?Mode { return @fromBackingInt(@truncate(value(x))); }
fn assigned(x: u16) u8 {
    var result: ?Mode = null;
    result = if (x < 3) @fromBackingInt(@truncate(value(x))) else null;
    return if (result) |mode| @intFromEnum(mode) else 99;
}
pub fn main() u8 {
    const empty = [_]u16{};
    const valid = [_]u16{2};
    const invalid = [_]u16{3};
    const multiple = [_]u16{0, 1};
    if (parse(&empty).? != .right) return 1;
    if (parse(&valid).? != .all) return 2;
    if (parse(&invalid) != null) return 3;
    if (parse(&multiple) != null) return 4;
    if (calls != 1) return 5;
    if (present(257).? != .left) return 6;
    if (assigned(2) != 2) return 7;
    if (assigned(3) != 99) return 8;
    if (calls != 3) return 9;
    return 42;
}
