pub fn same(a: []const u8, b: []const u8) bool {
    if (a.len != b.len) return false;
    for (a, 0..) |byte, i| { if (byte != b[i]) return false; }
    return true;
}
