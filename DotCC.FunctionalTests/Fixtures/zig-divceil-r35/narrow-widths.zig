fn unsigned(a: u3, b: u3) u3 { return @divCeil(a, b); }
fn signed(a: i3, b: i3) i3 { return @divCeil(a, b); }
fn resultWidth(a: u3, b: u3) u8 {
    const value = @divCeil(a, b);
    if (@bitSizeOf(@TypeOf(value)) != 3) return 0;
    return @bitSizeOf(@TypeOf(@divCeil(a, b)));
}
fn literalPeer(a: i8) u8 {
    if (@sizeOf(@TypeOf(@divCeil(a, -3))) != 1) return 0;
    if (@sizeOf(@TypeOf(@divCeil(-3, a))) != 1) return 0;
    const quotient = @divCeil(a, -3);
    if (@bitSizeOf(@TypeOf(quotient)) != 8 or quotient != -2) return 0;
    return 42;
}
pub fn main() u8 {
    const result: u3 = unsigned(7, 2);
    const wrapped: u3 = result +% 5;
    if (wrapped != 1) return 1;
    if (unsigned(7, 3) != 3 or unsigned(0, 7) != 0) return 2;
    if (signed(-4, 3) != -1 or signed(-4, -2) != 2 or signed(3, -2) != -1) return 3;
    if (resultWidth(7, 2) != 3) return 4;
    if (literalPeer(7) != 42) return 5;
    return 42;
}
