var calls: u32 = 0;
fn signedOperand() @Vector(8, i64) {
    calls += 1;
    return .{ 0, 0, 0, 0, 0, 0, 3, -9223372036854775808 };
}
pub fn main() u8 {
    // Each family: the only nonzero lane is the last one.
    const a: @Vector(8, u8) = .{ 0, 0, 0, 0, 0, 0, 0, 128 };
    const b: @Vector(8, u16) = .{ 0, 0, 0, 0, 0, 0, 0, 32768 };
    const c: @Vector(8, u32) = .{ 0, 0, 0, 0, 0, 0, 0, 2147483648 };
    const d: @Vector(8, u64) = .{ 0, 0, 0, 0, 0, 0, 0, 9223372036854775808 };
    if (@reduce(.Or, a) != 128 or @reduce(.Or, b) != 32768) return 1;
    if (@reduce(.Or, c) != 2147483648 or @reduce(.Or, d) != 9223372036854775808) return 2;
    const signed = @reduce(.Or, signedOperand());
    if (signed != -9223372036854775805 or calls != 1) return 3;
    const sa: @Vector(8, i8) = .{ 1, 0, 0, 0, 0, 0, 0, -128 };
    const sb: @Vector(8, i16) = .{ 1, 0, 0, 0, 0, 0, 0, -32768 };
    const sc: @Vector(8, i32) = .{ 1, 0, 0, 0, 0, 0, 0, -2147483647 };
    if (@reduce(.Or, sa) != -127 or @reduce(.Or, sb) != -32767 or @reduce(.Or, sc) != -2147483647) return 4;
    const zero: @Vector(8, u64) = @splat(0);
    if (@reduce(.Or, zero) != 0) return 5;
    // Bool reductions and ordinary numeric SIMD keep their existing behavior.
    if (!@reduce(.Or, a != @as(@Vector(8, u8), @splat(0)))) return 6;
    if (@reduce(.And, a != @as(@Vector(8, u8), @splat(0)))) return 7;
    if (!@reduce(.Xor, a != @as(@Vector(8, u8), @splat(0)))) return 8;
    if (@reduce(.Add, @as(@Vector(8, u8), @splat(2))) != 16) return 9;
    return 42;
}
