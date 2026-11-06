var calls: u8 = 0;
fn counted(value: u64) u64 { calls += 1; return value; }
fn verify(value: u64, leading: u8, trailing: u8, population: u8) bool {
    const prefix = @clz(counted(value));
    const suffix = @ctz(counted(value));
    const occupied = @popCount(counted(value));
    if (@typeInfo(@TypeOf(prefix)).int.bits != 7 or @typeInfo(@TypeOf(prefix)).int.signedness != .unsigned) return false;
    if (prefix != leading or suffix != trailing or occupied != population) return false;
    const start_bit = 64 - prefix;
    const remaining: usize = 65 - prefix;
    return start_bit == 64 - @as(usize, leading) and remaining == 65 - @as(usize, leading);
}
pub fn main() u8 {
    if (!verify(0, 64, 64, 0)) return 1;
    if (!verify(0xffffffffffffffff, 0, 0, 64)) return 2;
    if (!verify(1, 63, 0, 1)) return 3;
    if (calls != 9) return 4;
    var index: usize = 0;
    while (index < 64) : (index += 1) {
        const value = @as(u64, 1) << @intCast(index);
        const leading = @clz(value);
        const trailing = @ctz(value);
        if (leading + trailing != 63 or @popCount(value) != 1) return 5;
    }
    var small: u3 = 0;
    const short_count = @clz(small);
    if (@typeInfo(@TypeOf(short_count)).int.bits != 2 or @typeInfo(@TypeOf(short_count)).int.signedness != .unsigned) return 6;
    if (short_count != 3) return 7;
    small = 5;
    if (@popCount(small) != 2 or @clz(small) != 0 or @ctz(small) != 0) return 8;
    return 42;
}
