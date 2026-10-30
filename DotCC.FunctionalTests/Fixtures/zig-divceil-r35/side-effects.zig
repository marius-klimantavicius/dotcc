var trace: u32 = 0;
fn numerator() i16 {
    trace = trace * 10 + 1;
    return -7;
}
fn denominator() i16 {
    trace = trace * 10 + 2;
    return -3;
}
pub fn main() u8 {
    const result = @divCeil(numerator(), denominator());
    if (trace != 12 or result != 3) return 1;
    return 42;
}
