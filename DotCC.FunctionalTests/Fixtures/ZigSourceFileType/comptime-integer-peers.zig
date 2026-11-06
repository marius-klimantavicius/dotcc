const unit_bits = @bitSizeOf(u8);
const prealloc_columns = 512;
const prealloc_count = prealloc_columns / unit_bits;
var effects: u8 = 0;
fn once(value: usize) usize { effects += 1; return value; }
fn offset(value: usize) usize {
    const dynamic_i = once(value) - prealloc_count;
    if (@TypeOf(dynamic_i) != usize) return 99;
    return dynamic_i;
}
fn narrow(value: u5) u8 {
    const amount = 1 + 2;
    const result = value + amount;
    if (@typeInfo(@TypeOf(result)).int.bits != 5) return 99;
    if (@typeInfo(@TypeOf(result)).int.signedness != .unsigned) return 98;
    return @as(u8, result);
}
fn signed(value: i5) i8 {
    const amount = 2 - 5;
    const result = value + amount;
    if (@typeInfo(@TypeOf(result)).int.bits != 5) return 99;
    if (@typeInfo(@TypeOf(result)).int.signedness != .signed) return 98;
    return @as(i8, result);
}
fn typed() u8 { effects += 1; return 3; }
fn typedPeer(value: u1) u8 { return value + typed(); }
fn castPeer(value: u1) u8 { return value + @as(u8, 3); }
fn boolPeer(value: bool) u8 {
    const unit = 1 + 0;
    const result = @intFromBool(value) * unit;
    if (@typeInfo(@TypeOf(result)).int.bits != 1) return 99;
    if (@typeInfo(@TypeOf(result)).int.signedness != .unsigned) return 98;
    return @as(u8, result);
}
fn generic(comptime count: comptime_int, value: usize) usize {
    const named = count / unit_bits;
    return value - named;
}
fn comparisons(value: u8) bool {
    const large = 100 + 200;
    const negative = 1 - 2;
    return value < large and value > negative;
}
const Capacity = struct { rows: u16 };
fn divide(capacity: Capacity) u8 {
    const half = capacity.rows / 2;
    if (@TypeOf(half) != u16) return 91;
    if (@typeInfo(@TypeOf(half)).int.bits != 16) return 92;
    const conditional = if (half >= capacity.rows) half - capacity.rows else 0;
    if (@TypeOf(conditional) != u16) return 93;
    return if (half == 21 and conditional == 0) 42 else 94;
}
pub fn main() u8 {
    if (offset(64) != 0 or offset(65) != 1 or offset(0xffffffffffffffff) != 0xffffffffffffffbf) return 1;
    if (effects != 3) return 2;
    if (generic(512, 66) != 2) return 3;
    if (narrow(0) != 3 or narrow(28) != 31) return 4;
    if (signed(-13) != -16 or signed(15) != 12) return 5;
    if (typedPeer(1) != 4 or castPeer(1) != 4 or effects != 4) return 6;
    if (!comparisons(0) or !comparisons(255)) return 7;
    if (boolPeer(false) != 0 or boolPeer(true) != 1) return 8;
    const divided = divide(.{ .rows = 42 });
    if (divided != 42) return divided;
    return 42;
}
