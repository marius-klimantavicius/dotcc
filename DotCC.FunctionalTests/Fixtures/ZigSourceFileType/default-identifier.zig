const Value = struct { number: u8 };
fn choose(default: u8, value: u8) u8 { return if (value == 0) default else value; }
pub fn main() u8 {
    const default: Value = .{ .number = 42 };
    if (choose(default.number, 0) != 42 or choose(7, 42) != 42) return 1;
    return 42;
}
