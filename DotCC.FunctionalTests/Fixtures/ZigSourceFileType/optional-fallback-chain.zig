var trace: u32 = 0;
fn pick(id: u32, value: ?u8) ?u8 {
    trace = trace * 10 + id;
    return value;
}
fn chain(a: ?u8, b: ?u8) u8 {
    return pick(1, a) orelse pick(2, b) orelse return 7;
}
fn hoisted(a: ?u8, b: ?u8, c: ?u8) u8 {
    // The fallback has statements from its nested control-flow orelse.
    const value = pick(1, a) orelse pick(pick(2, 3) orelse return 8, b);
    return value orelse c orelse return 9;
}
fn search() u8 {
    var i: u8 = 0;
    var sum: u8 = 0;
    search: while (i < 3) : (i += 1) {
        const a: ?u8 = if (i == 0) 0 else null;
        const b: ?u8 = if (i == 1) 5 else null;
        const value = a orelse b orelse { continue :search; };
        sum += value;
    }
    return sum;
}
fn retainNone(value: ?u8) ?u8 { return value orelse null; }
fn constants() u8 {
    const absent: ?u8 = null;
    const value = absent orelse pick(4, 0);
    if ((value orelse return 2) != 0) return 3;
    const present: ?u8 = 0;
    const zero = present orelse pick(5, 99);
    if (zero != 0) return 4;
    const dead = @as(?u8, 0) orelse @compileError("dead fallback");
    if (dead != 0) return 4;
    const still_absent = absent orelse @as(?u8, null);
    if (still_absent != null) return 6;
    return 0;
}
pub fn main() u8 {
    trace = 0;
    if (chain(0, 5) != 0 or trace != 1) return 1;
    trace = 0;
    if (chain(null, 5) != 5 or trace != 12) return 2;
    trace = 0;
    if (chain(null, null) != 7 or trace != 12) return 3;
    trace = 0;
    if (hoisted(0, 5, 6) != 0 or trace != 1) return 4;
    trace = 0;
    if (hoisted(null, 0, 6) != 0 or trace != 123) return 5;
    trace = 0;
    if (hoisted(null, null, 6) != 6 or trace != 123) return 6;
    trace = 0;
    if (hoisted(null, null, null) != 9 or trace != 123) return 7;
    if (search() != 5) return 8;
    if (retainNone(null) != null) return 10;
    if ((retainNone(0) orelse return 11) != 0) return 12;
    trace = 0;
    if (constants() != 0 or trace != 4) return 9;
    return 42;
}
