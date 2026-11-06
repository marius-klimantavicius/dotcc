fn find(bytes: []const u8, value: u8) ?usize {
    for (bytes, 0..) |byte, i| if (byte == value) return i;
    return null;
}
fn trim(bytes: []const u8, strip: []const u8) []const u8 {
    var begin: usize = 0;
    var end: usize = bytes.len;
    while (begin < end and find(strip, bytes[begin]) != null) : (begin += 1) {}
    while (end > begin and null != find(strip, bytes[end - 1])) : (end -= 1) {}
    return bytes[begin..end];
}
fn check() u8 {
    const zero = find(" ", ' ');
    const absent = find(" ", 'x');
    if (zero == null or null == zero) return 1;
    if (absent != null or null != absent) return 2;
    if (zero.? != 0) return 3;
    const bytes = trim(" \t  0 \t", " \t");
    if (bytes.len != 1 or bytes[0] != '0') return 4;
    const flag: ?bool = false;
    if (flag == null or null == flag or flag.?) return 5;
    return 42;
}
pub fn main() u8 {
    const answer = comptime check();
    return answer;
}
