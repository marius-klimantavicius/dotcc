var subjects: u32 = 0;
var arms: u32 = 0;
var trace: u32 = 0;
fn subject(value: u8) u8 {
    subjects += 1;
    trace = trace * 10 + 1;
    return value;
}
fn arm(value: u8) u8 {
    arms += 1;
    trace = trace * 10 + 2;
    return value;
}
fn first(value: u8) u8 {
    return switch (subject(value)) {
        else => arm(9),
        0 => arm(11),
        1, 2 => arm(13),
    };
}
fn middle(value: u8) u8 {
    return switch (subject(value)) {
        0 => arm(11),
        else => arm(9),
        1...3 => arm(13),
    };
}
pub fn main() u8 {
    if (first(0) != 11 or subjects != 1 or arms != 1 or trace != 12) return 1;
    trace = 0;
    if (first(2) != 13 or subjects != 2 or arms != 2 or trace != 12) return 2;
    trace = 0;
    if (first(8) != 9 or subjects != 3 or arms != 3 or trace != 12) return 3;
    trace = 0;
    if (middle(0) != 11 or subjects != 4 or arms != 4 or trace != 12) return 4;
    trace = 0;
    if (middle(3) != 13 or subjects != 5 or arms != 5 or trace != 12) return 5;
    trace = 0;
    if (middle(8) != 9 or subjects != 6 or arms != 6 or trace != 12) return 6;
    return 42;
}
