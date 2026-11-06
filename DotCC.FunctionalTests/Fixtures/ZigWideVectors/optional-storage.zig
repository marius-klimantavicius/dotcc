const V = @Vector(32, u32);
fn make(value: u32) ?V { return @as(V, @splat(value)); }
fn pointer(value: *V, trace: *u8) *V { trace.* = trace.* * 10 + 1; return value; }
fn operand(trace: *u8) u32 { trace.* = trace.* * 10 + 2; return 3; }
pub fn main() u8 {
    if (@sizeOf(V) != 128 or @alignOf(V) != 128) return 1;
    if (@sizeOf(?V) != 256 or @alignOf(?V) != 128) return 2;
    var values: [2]?V = .{ null, make(17) };
    if (@intFromPtr(&values) % 128 != 0 or @intFromPtr(&values[1]) % 128 != 0) return 3;
    if (values[0] != null) return 4;
    if (values[1]) |*payload| { payload[31] = 42; } else return 5;
    const saved = values[1];
    values[1] = null;
    if (values[1] != null or saved == null) return 6;
    const payload = saved orelse return 7;
    if (payload[0] != 17 or payload[31] != 42) return 8;
    values[0] = make(9);
    const first = values[0] orelse return 9;
    if (first[0] != 9 or first[31] != 9) return 10;
    var trace: u8 = 0;
    var other: V = @splat(7);
    pointer(&other, &trace)[31] += operand(&trace);
    if (trace != 12 or other[0] != 7 or other[31] != 10) return 11;
    return @intCast(payload[31]);
}
