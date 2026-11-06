fn ignore(_: u8, _: u16, __zig_discard_param0: u8) u8 { return __zig_discard_param0; }
fn step(trace: *u8, digit: u8) u8 { trace.* = trace.* * 10 + digit; return digit; }
pub fn main() u8 {
    var trace: u8 = 0;
    if (ignore(step(&trace, 1), step(&trace, 2), 42) != 42 or trace != 12) return 1;
    return 42;
}
