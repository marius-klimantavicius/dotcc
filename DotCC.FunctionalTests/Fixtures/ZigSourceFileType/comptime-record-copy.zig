const Inner = struct { value: u8 };
const Record = struct { nested: Inner, values: [2]u8, pointer: *u8 };
fn change(value: Record) u8 {
    var copy = value;
    copy.nested.value = 3;
    copy.values[0] = 4;
    copy.pointer.* += 1;
    return copy.nested.value + copy.values[0];
}
fn check() u8 {
    var byte: u8 = 31;
    const original: Record = .{ .nested = .{ .value = 5 }, .values = .{ 6, 7 }, .pointer = &byte };
    var copied = original;
    copied.nested.value = 8;
    copied.values[0] = 9;
    if (original.nested.value != 5 or original.values[0] != 6) return 1;
    if (change(copied) != 7) return 2;
    if (copied.nested.value != 8 or copied.values[0] != 9 or byte != 32) return 3;
    return original.nested.value + original.values[0] + byte - 1;
}
pub fn main() u8 { return comptime check(); }
