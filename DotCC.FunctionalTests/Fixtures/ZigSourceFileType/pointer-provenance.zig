const Optional = ?*u32;
fn makePointer(pointer: *u32, fail: bool) error{Failed}!*u32 {
    if (fail) return error.Failed;
    return pointer;
}
fn tryPointer(pointer: *u32) error{Failed}!*u32 { return try makePointer(pointer, false); }
const Box = struct { pointer: *u32 };
var address_calls: u8 = 0;
fn address(pointer: *u32) usize { address_calls += 1; return @intFromPtr(pointer); }
fn kind(pointer: anytype) u8 {
    return switch (@typeInfo(@TypeOf(pointer)).pointer.size) {
        .one => 1,
        .many => 2,
        .c => 3,
        else => 0,
    };
}
fn read(optional: Optional) u8 {
    const pointer = optional orelse return 0;
    return kind(pointer);
}
pub fn main() u8 {
    var values = [_]u32{ 20, 22 };
    const many: [*]u32 = &values;
    const c: [*c]u32 = @ptrCast(&values);
    if (read(null) != 0 or read(&values[0]) != 1) return 1;
    if (kind(many) != 2 or kind(c) != 3 or kind(&values[0]) != 1) return 2;
    const returned = makePointer(&values[0], false) catch return 3;
    const box: Box = .{ .pointer = returned };
    if (kind(returned) != 1 or kind(box.pointer) != 1) return 4;
    const recovered = makePointer(&values[0], true) catch &values[1];
    if (recovered.* != 22) return 5;
    const tried = tryPointer(returned) catch return 6;
    if (tried.* != 20) return 7;
    address_calls = 0;
    const roundtrip: *u32 = @ptrFromInt(address(&values[0]));
    if (address_calls != 1 or roundtrip != &values[0] or roundtrip.* != 20) return 8;
    const sentinel = comptime @as(usize, 0x1000);
    const sentinel_pointer: [*]u8 = @ptrFromInt(sentinel);
    if (@intFromPtr(sentinel_pointer) != 0x1000) return 9;
    const zero = comptime @as(usize, 0);
    const null_pointer: ?*u8 = @ptrFromInt(zero);
    if (null_pointer != null) return 10;
    return @intCast(values[0] + values[1]);
}
