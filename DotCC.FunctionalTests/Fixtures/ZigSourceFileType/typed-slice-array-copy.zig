fn check(source: []u8, offset: usize) bool {
    var copy: [3]u8 = source[offset..][0..3].*;
    if (copy[0] != 11 or copy[1] != 12 or copy[2] != 13) return false;
    copy[0] = 42;
    source[offset + 1] = 99;
    return source[offset] == 11 and copy[0] == 42 and copy[1] == 12;
}
pub fn main() u8 {
    var bytes: [5]u8 = .{ 10, 11, 12, 13, 14 };
    if (!check(&bytes, 1)) return 1;
    const array: [3]u8 = bytes[1..4].*;
    if (array[0] != 11 or array[1] != 99 or array[2] != 13) return 2;
    bytes[1] = 77;
    if (array[0] != 11) return 3;
    return 42;
}
