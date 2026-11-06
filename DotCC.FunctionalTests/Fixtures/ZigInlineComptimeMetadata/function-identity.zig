const Config = struct {
    callback: fn (comptime flag: bool, value: anytype) u8 = initial,
};
fn initial(comptime flag: bool, value: anytype) u8 { return if (flag) @intCast(value) else 0; }
const options: Config = .{};

inline fn selected(value: u8) u8 {
    if (options.callback == initial) return value + 2;
    return 0;
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 { return selected(40); }
