const U = @Vector(32, u32);
const S = @Vector(32, i32);
fn checkUnsigned(a: U, b: U) bool {
    const ge = a >= b; const le = a <= b; const gt = a > b; const lt = a < b;
    const eq = a == b; const ne = a != b;
    inline for (0..32) |i| {
        if (@intFromBool(ge[i]) != @intFromBool(a[i] >= b[i]) or @intFromBool(le[i]) != @intFromBool(a[i] <= b[i])) return false;
        if (@intFromBool(gt[i]) != @intFromBool(a[i] > b[i]) or @intFromBool(lt[i]) != @intFromBool(a[i] < b[i])) return false;
        if (@intFromBool(eq[i]) != @intFromBool(a[i] == b[i]) or @intFromBool(ne[i]) != @intFromBool(a[i] != b[i])) return false;
    }
    return true;
}
fn checkSigned(a: S, b: S) bool {
    const ge = a >= b; const le = a <= b; const gt = a > b; const lt = a < b;
    inline for (0..32) |i| {
        if (@intFromBool(ge[i]) != @intFromBool(a[i] >= b[i]) or @intFromBool(le[i]) != @intFromBool(a[i] <= b[i])) return false;
        if (@intFromBool(gt[i]) != @intFromBool(a[i] > b[i]) or @intFromBool(lt[i]) != @intFromBool(a[i] < b[i])) return false;
    }
    return true;
}
var calls: u8 = 0;
var live: U = @splat(5);
fn next() U { calls += 1; live = @splat(0); return @splat(3); }
pub fn main() u8 {
    var a: [32]u32 = undefined; var b: [32]u32 = undefined;
    var sa: [32]i32 = undefined; var sb: [32]i32 = undefined;
    for (0..32) |i| { a[i] = @intCast(i); b[i] = 16; sa[i] = @as(i32, @intCast(i)) - 16; sb[i] = 0; }
    a[0] = 0xffffffff; a[31] = 0x80000000;
    sa[0] = -2147483647; sa[0] -= 1; sb[0] = 2147483647;
    if (!checkUnsigned(a, b) or !checkSigned(sa, sb)) return 1;
    const lo: U = @splat(0x10); const hi: U = @splat(0xff);
    const v: U = a;
    const in_range = (v >= lo) & (v <= hi);
    inline for (0..32) |i| if (@intFromBool(in_range[i]) != @intFromBool(a[i] >= 0x10 and a[i] <= 0xff)) return 2;
    const mask = live >= changed: { break :changed next(); };
    if (calls != 1 or !@reduce(.And, mask) or live[0] != 0) return 3;
    const small_values = [_]i8{ -128, -1, 0, 127 };
    const small_a: @Vector(4, i8) = small_values;
    const small_b: @Vector(4, i8) = @splat(0);
    const small_lt = small_a < small_b;
    const small_ge = small_a >= small_b;
    if (!small_lt[0] or !small_lt[1] or small_lt[2] or small_lt[3]) return 4;
    if (small_ge[0] or small_ge[1] or !small_ge[2] or !small_ge[3]) return 5;
    return 42;
}
