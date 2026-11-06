fn kind(pointer: anytype) u8 {
    return switch (@typeInfo(@TypeOf(pointer)).pointer.size) { .one => 1, .many => 2, .c => 3, else => 0 };
}
fn alignment(pointer: anytype) usize {
    return @typeInfo(@TypeOf(pointer)).pointer.attrs.@"align" orelse 0;
}
fn inspect(value: *?[*]align(8) const u8) u8 {
    const slot = if (value.*) |*v| v else return 0;
    const payload = slot.*;
    if (kind(slot) != 1 or kind(payload) != 2 or alignment(payload) != 8) return 1;
    return payload[0];
}
pub fn main() u8 {
    var value: u64 = 42;
    const bytes: [*]align(8) const u8 = @ptrCast(&value);
    var optional: ?[*]align(8) const u8 = bytes;
    if (inspect(&optional) != 42) return 1;
    optional = null;
    if (inspect(&optional) != 0) return 2;
    return 42;
}
