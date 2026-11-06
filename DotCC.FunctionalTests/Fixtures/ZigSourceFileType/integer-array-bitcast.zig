var calls: u8 = 0;
fn value() u128 { calls += 1; return 0xfedcba98765432108070605040302010; }
fn store(buffer: *[16]u8, number: u128, swap: bool) void {
    buffer.* = switch (swap) { false => @bitCast(number), true => @bitCast(@byteSwap(number)) };
}
fn choose(flag: bool) u32 { return switch (flag) { true => @bitCast(@as(i32, -1)), false => @bitCast(@as(i32, 0)) }; }
pub fn main() u8 {
    if (choose(true) != 0xffffffff or choose(false) != 0) return 9;
    const small: [2]u8 = @bitCast(@as(u16, 0xab12));
    if (small[0] != 0x12 or small[1] != 0xab) return 1;
    const signed: [2]u8 = @bitCast(@as(i16, -257));
    if (signed[0] != 0xff or signed[1] != 0xfe) return 2;
    const words: [2]u16 = @bitCast(@as(u32, 0xabcd1234));
    if (words[0] != 0x1234 or words[1] != 0xabcd) return 3;
    const narrow: [2]u9 = @bitCast(@as(u18, 0x3ffff));
    if (narrow[0] != 511 or narrow[1] != 511) return 7;
    const narrow_signed: [2]i9 = @bitCast(@as(u18, 0x3ffff));
    if (narrow_signed[0] != -1 or narrow_signed[1] != -1) return 8;
    const bytes: [16]u8 = @bitCast(value());
    if (calls != 1 or bytes[0] != 0x10 or bytes[15] != 0xfe or bytes[8] != 0x10) return 4;
    var output: [16]u8 = undefined;
    store(&output, 0xfedcba98765432108070605040302010, false);
    for (bytes, output) |a, b| if (a != b) return 5;
    store(&output, 0xfedcba98765432108070605040302010, true);
    for (bytes, 0..) |a, i| if (a != output[15 - i]) return 6;
    return 42;
}
