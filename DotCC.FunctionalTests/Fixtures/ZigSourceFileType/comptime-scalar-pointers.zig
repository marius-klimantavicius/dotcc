fn bump(pointer: *usize) void { pointer.* += 1; }
fn nested(pointer: *usize) void {
    const alias = pointer;
    bump(alias);
    alias.* *= 3;
}
fn calculate() u8 {
    var first: usize = 12;
    var second: usize = 0;
    const saved = first;
    const first_ptr = &first;
    const second_ptr = &second;
    const missing: ?*usize = null;
    if (first_ptr == second_ptr or first_ptr == missing or first_ptr != &first) return 2;
    nested(&first);
    bump(&second);
    var bytes = [_]u8{ 1, 2 };
    const p = &bytes[1];
    var other = [_]u8{ 1, 2 };
    if (p == &bytes[0] or p == &other[1] or p != &bytes[1]) return 3;
    p.* += 1;
    if (first != 39 or second != 1 or saved != 12 or bytes[1] != 3) return 1;
    return @intCast(first + second + bytes[0] + 1);
}
pub fn main() u8 {
    const answer = comptime calculate();
    return answer;
}
