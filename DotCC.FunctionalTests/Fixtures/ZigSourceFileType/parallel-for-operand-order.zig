fn mutatePointer(p: *[*]u8, next: [*]u8, trace: *u32) []const u8 {
    trace.* = trace.* * 10 + 2;
    p.* = next;
    return next[0..2];
}
fn first(p: [*]u8, trace: *u32) [*]u8 { trace.* = trace.* * 10 + 1; return p; }
pub fn main() u8 {
    var a = [_]u8{ 3, 4, 5 };
    var b = [_]u8{ 10, 20, 30 };
    var p: [*]u8 = &a;
    var trace: u32 = 0;
    var sum: usize = 0;
    for (first(p, &trace), mutatePointer(&p, &b, &trace), tail: {
        trace = trace * 10 + 3;
        break :tail 5;
    }..7) |v, w, i| { sum += v + w + i; }
    if (trace != 123 or sum != 48 or p[0] != 10) return 1;
    p = &a;
    sum = 0;
    for (p, mutatePointer(&p, &b, &trace)) |v, _| sum += v;
    if (sum != 7) return 2;
    var s: []u8 = a[0..2];
    sum = 0;
    for (s, changed: { s = b[0..2]; break :changed s; }) |v, w| { sum += v + w; }
    if (sum != 37) return 3;
    var lo: usize = 1;
    var hi: usize = 3;
    sum = 0;
    for (lo..hi, changed: { lo = 10; hi = 11; break :changed p; }) |i, _| sum += i;
    if (sum != 3 or lo != 10 or hi != 11) return 4;
    return 42;
}
