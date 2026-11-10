const builtin = @import("builtin");
const V = @Vector(32, u32);
const vector_alignment: usize = if (builtin.cpu.has(.x86, .avx512f)) 64 else if (builtin.cpu.has(.x86, .avx)) 32 else 16;
fn make(value: u32) ?V { return @as(V, @splat(value)); }
fn pointer(value: *V, trace: *u8) *V { trace.* = trace.* * 10 + 1; return value; }
fn operand(trace: *u8) u32 { trace.* = trace.* * 10 + 2; return 3; }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (@sizeOf(V) != 128 or @alignOf(V) != vector_alignment) return 1;
    if (@sizeOf(?V) != 128 + vector_alignment or @alignOf(?V) != vector_alignment) return 2;
    var values: [2]?V = .{ null, make(17) };
    if (@intFromPtr(&values) % vector_alignment != 0 or @intFromPtr(&values[1]) % vector_alignment != 0) return 3;
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
