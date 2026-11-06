fn alignment(pointer: anytype) usize {
    return @typeInfo(@TypeOf(pointer)).pointer.attrs.@"align" orelse 0;
}
fn inspect(storage: *align(1) ?u32) u32 {
    const cap = if (storage.*) |*c| c else return 0;
    if (alignment(cap) != 1) return 1;
    cap.* += 2;
    return cap.*;
}
pub fn main() u8 {
    var value: ?u32 = 40;
    if (inspect(&value) != 42 or value.? != 42) return 1;
    return 42;
}
