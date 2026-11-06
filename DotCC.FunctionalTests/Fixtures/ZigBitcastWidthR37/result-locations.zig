const Narrow = u24;
const Signed = i24;
const Holder = struct { value: Narrow };
fn negative() Signed { return -1; }
fn returning(x: Signed) Narrow { return @bitCast(x); }
fn consume(x: Narrow) u8 { return if (x == 0xffffff) 0 else 1; }
pub fn main() u8 {
    const local: Narrow = @bitCast(negative());
    const grouped: Narrow = (@bitCast(negative()));
    const explicit = @as(Narrow, @bitCast(negative()));
    const widened: u32 = @as(Narrow, @bitCast(negative()));
    const holder: Holder = .{ .value = @bitCast(negative()) };
    const elements: [1]Narrow = .{ @bitCast(negative()) };
    var assigned: Narrow = 0;
    assigned = @bitCast(negative());
    if (local != 0xffffff or grouped != local or explicit != local or widened != local) return 1;
    if (holder.value != local or elements[0] != local or assigned != local) return 2;
    if (returning(-1) != local) return 3;
    return consume(@bitCast(negative()));
}
