const Narrow = packed struct(u15) { value: u15 };
fn copyNarrow(dst: [*]u15, src: []const u15) void {
    for (dst[1..], src) |*d, s| d.* = @bitCast(Narrow{ .value = s });
}
fn readNarrow(src: [*]const u15, dst: []u15) void {
    for (dst, src[1..]) |*d, s| d.* = @as(Narrow, @bitCast(s)).value;
}
pub fn main() u8 {
    var a = [_]u8{ 1, 0, 0, 0, 9 };
    const p: [*]u8 = &a;
    // Overlapping aliases: each source capture sees the previous iteration's write.
    for (p[1..], a[0..3]) |*d, *s| d.* = s.* + 1;
    if (a[1] != 2 or a[2] != 3 or a[3] != 4 or a[4] != 9) return 1;
    var n = [_]u15{ 0, 0, 0 };
    const values = [_]u15{ 11, 31 };
    copyNarrow(&n, &values);
    var out = [_]u15{ 0, 0 };
    readNarrow(&n, &out);
    if (out[0] != 11 or out[1] != 31) return 2;
    var b = [_]u8{ 0, 0, 0, 0 };
    const q: [*]u8 = &b;
    for (3..6, q) |i, *d| d.* = @intCast(i);
    for (q[1..], 7..9) |*d, i| d.* += @intCast(i);
    if (b[0] != 3 or b[1] != 11 or b[2] != 13 or b[3] != 0) return 3;
    var empty_count: u8 = 0;
    for (q, b[0..0]) |_, _| { empty_count += 1; } else { empty_count += 10; }
    for (0..0, q) |_, _| { empty_count += 1; } else { empty_count += 10; }
    if (empty_count != 20) return 4;
    var total: u8 = 0;
    for (q, 0..3) |v, i| { if (i == 1) continue; total += v; } else { total += 1; }
    if (total != 17) return 5;
    for (0..3, q) |i, _| { if (i == 1) break; } else { return 6; }
    const c: [*c]const u8 = &b;
    var csum: u8 = 0;
    for (c, 0..2) |v, _| csum += v;
    if (csum != 14) return 7;
    return 42;
}
