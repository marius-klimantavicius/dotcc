fn bytes(a: []const u8) u24 { return @bitCast(a[0..3].*); }
fn offset(a: []const u8) u24 { return @bitCast(a[1..4].*); }
fn threes(a: [3]u3) u9 { return @bitCast(a); }
fn words(a: [2]u24) u48 { return @bitCast(a); }
pub fn main() u8 {
    const exact = [3]u8{0x12, 0x34, 0x56};
    const trailing = [5]u8{0xdd, 0x12, 0x34, 0x56, 0xa5};
    if (bytes(&exact) != 0x563412) return 1;
    if (offset(&trailing) != 0x563412) return 2;
    if (threes([3]u3{1,2,3}) != 209) return 3;
    if (words([2]u24{0x563412,0xabcdef}) != 0xabcdef563412) return 4;
    return 0;
}
