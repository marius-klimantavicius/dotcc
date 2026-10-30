var calls: u8 = 0;
fn bytes(p: *[5]u8) *const [4]u8 { calls += 1; return p[1..5]; }
fn view(p: *[5]u8) []const u8 { calls += 1; return p; }
fn sliceWord(data: *[5]u8) u32 { return @bitCast((view(data)[1..5].*)); }
fn arrayWord(data: *const [4]u8) u32 { return @bitCast(data.*); }
pub fn main() u8 {
    var data = [_]u8{ 99, 3, 17, 29, 41 };
    const before = sliceWord(&data);
    data[1] += 2;
    const after = arrayWord(bytes(&data));
    const literal = arrayWord("abcd");
    return @intCast((before % 251 + after % 251 + literal % 251 + calls) % 251);
}
