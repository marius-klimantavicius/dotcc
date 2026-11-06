var calls: u8 = 0;
fn operand(value: u9, marker: u8) u9 {
    calls = calls * 10 + marker;
    return value;
}
fn checkUnsigned(a: u9, b: u9) bool {
    const add = @addWithOverflow(a, b);
    const sub = @subWithOverflow(a, b);
    const mul = @mulWithOverflow(a, b);
    const av: u9 = add[0];
    const mv: u9 = mul[0];
    const flag: u1 = mul[1];
    if (@typeInfo(@TypeOf(mv)).int.bits != 9 or @typeInfo(@TypeOf(flag)).int.bits != 1) return false;
    const ae = @as(u32, a) + b;
    const me = @as(u32, a) * b;
    const se = @as(i32, a) - b;
    return av == @as(u9, @intCast(ae & 511)) and add[1] == @intFromBool(ae > 511)
        and sub[0] == @as(u9, @intCast(se & 511)) and sub[1] == @intFromBool(se < 0)
        and mv == @as(u9, @intCast(me & 511)) and flag == @intFromBool(me > 511);
}
fn checkSigned(a: i9, b: i9) bool {
    const add = @addWithOverflow(a, b);
    const sub = @subWithOverflow(a, b);
    const mul = @mulWithOverflow(a, b);
    const ae = @as(i32, a) + b;
    const se = @as(i32, a) - b;
    const me = @as(i32, a) * b;
    return add[0] == @as(i9, @intCast(@mod(ae + 256, 512) - 256)) and add[1] == @intFromBool(ae < -256 or ae > 255)
        and sub[0] == @as(i9, @intCast(@mod(se + 256, 512) - 256)) and sub[1] == @intFromBool(se < -256 or se > 255)
        and mul[0] == @as(i9, @intCast(@mod(me + 256, 512) - 256)) and mul[1] == @intFromBool(me < -256 or me > 255);
}
pub fn main() u8 {
    const unsigned = [_]u9{ 0, 1, 2, 15, 255, 256, 510, 511 };
    for (unsigned) |a| for (unsigned) |b| if (!checkUnsigned(a, b)) return 1;
    const signed = [_]i9{ -256, -255, -17, -1, 0, 1, 17, 254, 255 };
    for (signed) |a| for (signed) |b| if (!checkSigned(a, b)) return 2;
    const product = @mulWithOverflow(operand(30, 1), operand(20, 2));
    if (calls != 12 or product[0] != 88 or product[1] != 1) return 3;
    const shifted = @shlWithOverflow(@as(u9, 256), 1);
    if (shifted[0] != 0 or shifted[1] != 1) return 4;
    return 42;
}
