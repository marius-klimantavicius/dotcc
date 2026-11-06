const Config = struct {
    callback: fn (comptime flag: bool, value: anytype) u8 = initial,
    answer: u8 = 0,
};
fn initial(comptime flag: bool, value: anytype) u8 { return if (flag) @intCast(value) else 0; }
fn custom(comptime flag: bool, value: anytype) u8 { return if (flag) @intCast(value + 2) else 0; }
const options: Config = blk: {
    var value: Config = .{ .answer = 40 };
    value.callback = custom;
    value.answer += 2;
    break :blk value;
};
pub fn main() u8 {
    if (options.answer != 42) return 1;
    return options.callback(true, 40);
}
