fn inspect(value: anytype) u8 {
    const info = @typeInfo(@TypeOf(value)).pointer;
    const sentinel = info.sentinel_ptr orelse return 100;
    const scalar: *const u8 = @ptrCast(@alignCast(sentinel));
    return scalar.*;
}
fn own(comptime sentinel: u8, value: [:sentinel]u8) error{}![:sentinel]u8 {
    return value;
}
fn plain(comptime ignored: u8, value: []u8) error{}![]u8 {
    _ = ignored;
    return value;
}
pub fn main() u8 {
    const sentinel: u8 = 99;
    var zero: [2:0]u8 = .{ 11, 13 };
    var seven: [2:7]u8 = .{ 17, 19 };
    const a = own(0, &zero) catch return 1;
    const b = own(7, &seven) catch return 2;
    const c = plain(sentinel, &seven) catch return 3;
    const d = own(0, &zero) catch return 4;
    if (inspect(a) != 0 or inspect(b) != 7 or inspect(c) != 100 or inspect(d) != 0) return 5;
    if (@intFromPtr(a.ptr) != @intFromPtr(&zero[0]) or @intFromPtr(b.ptr) != @intFromPtr(&seven[0]) or @intFromPtr(c.ptr) != @intFromPtr(&seven[0])) return 6;
    if (a.len != 2 or b.len != 2 or b[2] != 7 or a[2] != 0) return 7;
    return 42;
}
