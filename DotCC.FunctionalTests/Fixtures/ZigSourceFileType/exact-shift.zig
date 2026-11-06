const Unit = u8;
const unit_bits = @bitSizeOf(Unit);
const masks = blk: {
    var res: [unit_bits]Unit = undefined;
    for (res, 0..) |_, i| {
        res[i] = @shlExact(@as(Unit, 1), @as(u3, @intCast(i)));
    }
    break :blk res;
};

fn unsigned(value: u8, shift: u3) u8 { return @shlExact(value, shift); }
fn signed(value: i8, shift: u3) i8 { return @shlExact(value, shift); }
fn narrow(value: u3, shift: u2) u3 { return @shlExact(value, shift); }
fn narrowSigned(value: i3, shift: u2) i3 { return @shlExact(value, shift); }
fn wide(value: u128, shift: u7) u128 { return @shlExact(value, shift); }
fn wideSigned(value: i128, shift: u7) i128 { return @shlExact(value, shift); }
fn generic(comptime T: type, value: T, shift: u3) T { return @shlExact(value, shift); }
fn boundary(comptime T: type, value: T, comptime shift: u7) T { return @shlExact(value, shift); }
fn mark(trace: *u32) u8 { trace.* = trace.* * 10 + 1; return 3; }
fn amount(trace: *u32) u3 { trace.* = trace.* * 10 + 2; return 2; }
fn optional(trace: *u32) ?u3 { return amount(trace); }
fn hoisted(trace: *u32) u8 {
    return @shlExact(mark(trace), optional(trace) orelse return 0);
}
pub fn main() u8 {
    for (masks, 0..) |mask, i| {
        if (mask != unsigned(1, @intCast(i))) return 1;
    }
    if (masks[7] != 128 or unsigned(255, 0) != 255) return 2;
    if (signed(-1, 7) != -128 or signed(63, 1) != 126) return 3;
    if (narrow(1, 2) != 4 or narrowSigned(-1, 2) != -4) return 4;
    if (@shlExact(narrow(1, 1), 1) != 4) return 5;
    if (generic(u9, 255, 1) != 510 or generic(i9, -128, 1) != -256) return 6;
    if (@shlExact(@as(u21, 1), 20) != 1048576) return 7;
    if (@shlExact(@as(u32, 1), 31) != 2147483648) return 8;
    if (@shlExact(@as(u64, 1), 63) != 9223372036854775808) return 9;
    if (wide(1, 127) != 170141183460469231731687303715884105728) return 10;
    if (wideSigned(-1, 127) != -170141183460469231731687303715884105728) return 11;
    if (wide(340282366920938463463374607431768211455, 0) != 340282366920938463463374607431768211455) return 12;
    var trace: u32 = 0;
    if (@shlExact(mark(&trace), amount(&trace)) != 12 or trace != 12) return 13;
    trace = 0;
    if (hoisted(&trace) != 12 or trace != 12) return 14;
    if (boundary(u16, 1, 15) != 32768 or boundary(i16, -1, 15) != -32768) return 15;
    if (boundary(u32, 1, 31) != 2147483648 or boundary(i32, -1, 31) != -2147483648) return 16;
    if (boundary(u64, 1, 63) != 9223372036854775808 or boundary(i64, -1, 63) != -9223372036854775808) return 17;
    if (boundary(u1, 1, 0) != 1 or boundary(i1, -1, 0) != -1) return 18;
    return 42;
}
