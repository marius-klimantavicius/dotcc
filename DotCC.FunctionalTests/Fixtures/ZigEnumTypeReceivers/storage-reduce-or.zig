const Wide = @Vector(16, u64);
var calls: u32 = 0;
fn produce() Wide {
    calls += 1;
    var value: Wide = @splat(0);
    value[15] = @as(u64, 1) << 63;
    return value;
}
export fn answer() c_int {
    const original = produce();
    var copy = original;
    copy[15] = 0;
    if (@reduce(.Or, original) != @as(u64, 1) << 63 or @reduce(.Or, copy) != 0) return 1;
    if (@reduce(.Or, produce()) != @as(u64, 1) << 63 or calls != 2) return 2;
    var signed: @Vector(16, i64) = @splat(0);
    signed[15] = -1;
    if (@reduce(.Or, signed) != -1) return 3;
    var small: @Vector(4, u8) = @splat(0);
    small[3] = 128;
    if (@reduce(.Or, small) != 128) return 4;
    return 42;
}
