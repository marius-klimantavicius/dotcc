var effects: u8 = 0;
var value: usize = 10;
fn address() *usize { effects = effects * 10 + 1; return &value; }
fn operand() usize { effects = effects * 10 + 2; return 7; }
fn optional() ?usize { return 7; }
fn absent() ?usize { return null; }
fn specialize(comptime n: ?usize) u8 { return if (n) |some| @intCast(some) else 35; }
pub fn main() u8 {
    if (specialize(optional()) + specialize(absent()) != 42) return 1;
    const old = @atomicRmw(usize, address(), .Add, optional() orelse return 2, .acquire);
    if (old != 10 or value != 17 or effects != 1) return 3;
    effects = 0;
    if (@atomicRmw(usize, address(), .Add, operand(), .monotonic) != 17 or value != 24 or effects != 12) return 4;
    if (@atomicRmw(usize, &value, .Sub, 4, .release) != 24 or value != 20) return 5;
    if (@atomicRmw(usize, &value, .And, 15, .acq_rel) != 20 or value != 4) return 6;
    if (@atomicRmw(usize, &value, .Or, 3, .seq_cst) != 4 or value != 7) return 7;
    if (@atomicRmw(usize, &value, .Xor, 2, .monotonic) != 7 or value != 5) return 8;
    if (@atomicRmw(usize, &value, .Xchg, 42, .acquire) != 5 or value != 42) return 9;
    var small: u8 = 255;
    if (@atomicRmw(u8, &small, .Add, 1, .monotonic) != 255 or small != 0) return 10;
    var signed: i16 = -2;
    if (@atomicRmw(i16, &signed, .Add, 3, .seq_cst) != -2 or signed != 1) return 11;
    return 42;
}
