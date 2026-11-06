fn choose(calls: *u8, flag: bool) bool { calls.* += 1; return flag; }
fn longer(calls: *u8) *const [3:0]u8 { calls.* += 10; return "yes"; }
fn shorter(calls: *u8) *const [0:0]u8 { calls.* += 20; return ""; }
fn alternate(calls: *u8) *const [3:0]u8 { calls.* += 30; return "alt"; }
pub fn main() u8 {
    var calls: u8 = 0;
    const first = if (choose(&calls, true)) longer(&calls) else shorter(&calls);
    if (@typeInfo(@TypeOf(first)).pointer.size != .slice or @typeInfo(@TypeOf(first)).pointer.child != u8) return 1;
    if (first.len != 3 or first[0] != 'y' or first[2] != 's' or calls != 11) return 2;
    const second = if (choose(&calls, false)) longer(&calls) else shorter(&calls);
    if (second.len != 0 or calls != 32) return 3;
    const same = if (choose(&calls, true)) longer(&calls) else alternate(&calls);
    if (@typeInfo(@TypeOf(same)).pointer.size != .one or same.len != 3 or calls != 43) return 4;
    const literal = if (choose(&calls, false)) "prefix" else "";
    if (@typeInfo(@TypeOf(literal)).pointer.size != .slice or literal.len != 0 or calls != 44) return 5;
    return 42;
}
