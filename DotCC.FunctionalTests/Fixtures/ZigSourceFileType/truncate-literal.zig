pub fn main() u8 {
    const wide: u64 = @truncate(0x123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef0);
    if (wide != 0x123456789abcdef0) return 1;
    const full: u128 = @truncate(0x123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef0);
    if (full != 0x123456789abcdef0123456789abcdef0) return 2;
    const narrow: u5 = @truncate(0x12345);
    if (narrow != 5) return 3;
    const signed: i7 = @truncate(127);
    if (signed != -1) return 4;
    const negative: u8 = @truncate(-0x100000000000000000000000000000001);
    if (negative != 255) return 5;
    const binary: u8 = @truncate(0b1000000000000000000000000000000010101010);
    const octal: u8 = @truncate(0o1234567123456712345671234567123456712345671);
    const decimal: u8 = @truncate(340282366920938463463374607431768211461);
    if (binary != 170 or octal != 185 or decimal != 5) return 6;
    return 42;
}
