var calls: u8 = 0;
fn size(fail: bool) error{Absent}!usize {
    calls += 1;
    if (fail) return error.Absent;
    return 11;
}
fn integer(fail: bool) bool {
    const amount, const exact = if (size(fail)) |value| .{ value - 2, true } else |_| .{ 128, false };
    if (@typeInfo(@TypeOf(amount)).int.bits != 64) return false;
    return amount == (if (fail) @as(usize, 128) else @as(usize, 9)) and exact == !fail;
}
fn slices(present: bool, empty: bool) bool {
    var bytes: [3]u8 = .{ 11, 13, 17 };
    const data: []u8 = &bytes;
    const prefix, const suffix = blk: {
        if (!present) break :blk .{ data, null };
        const offset: usize = if (empty) 3 else 1;
        break :blk .{ data[0..offset], data[offset..] };
    };
    const readonly: ?[]const u8 = suffix;
    if (!present) return readonly == null and prefix.len == 3;
    const actual = readonly orelse return false;
    if (empty) return actual.len == 0 and actual.ptr == data.ptr + 3 and prefix.len == 3;
    return actual.len == 2 and actual[0] == 13 and actual[1] == 17 and actual.ptr == data.ptr + 1 and prefix.len == 1;
}
pub fn main() u8 {
    if (!integer(false) or !integer(true) or calls != 2) return 1;
    if (!slices(false, false) or !slices(true, false) or !slices(true, true)) return 2;
    return 42;
}
