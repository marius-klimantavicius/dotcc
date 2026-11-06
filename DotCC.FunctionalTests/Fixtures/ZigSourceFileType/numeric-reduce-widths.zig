fn unsignedOperand() @Vector(8, u5) { return .{ 1, 2, 4, 8, 0, 0, 0, 16 }; }
fn signedOperand() @Vector(8, i5) { return .{ 1, 2, 0, 0, 0, 0, 0, -16 }; }
pub fn main() u8 {
    const unsigned = @reduce(.Or, unsignedOperand());
    const signed = @reduce(.Or, signedOperand());
    if (unsigned != 31 or signed != -13) return 1;
    if (@typeInfo(@TypeOf(unsigned)).int.bits != 5 or @typeInfo(@TypeOf(signed)).int.bits != 5) return 2;
    if (unsigned +% 1 != 0) return 3;
    if (@as(u5, @bitCast(signed)) != 19) return 5;
    const direct = @reduce(.Or, @as(@Vector(8, u5), @splat(31)));
    if (@typeInfo(@TypeOf(direct)).int.bits != 5 or direct +% 1 != 0) return 4;
    const combined = @reduce(.Or, unsignedOperand() | unsignedOperand());
    if (@typeInfo(@TypeOf(combined)).int.bits != 5 or combined +% 1 != 0) return 6;
    return 42;
}
