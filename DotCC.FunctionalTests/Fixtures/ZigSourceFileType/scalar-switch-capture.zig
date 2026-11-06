var calls: u8 = 0;
fn next() u15 { calls += 1; return 2; }
fn chosen() u8 {
    switch (next()) {
        1...3 => |value| {
            if (@typeInfo(@TypeOf(value)).int.bits != 15) return 1;
            return @intCast(value + 40);
        },
        else => |value| { return if (value == 0) 3 else 4; },
    }
}
fn signed(value: i7) bool {
    switch (value) {
        -3...-1 => |captured| {
            if (@typeInfo(@TypeOf(captured)).int.bits != 7 or @typeInfo(@TypeOf(captured)).int.signedness != .signed) return false;
            return captured == value;
        },
        else => return false,
    }
}
pub fn main() u8 {
    if (chosen() != 42 or calls != 1) return 5;
    if (!signed(-2) or signed(2)) return 6;
    return 42;
}
