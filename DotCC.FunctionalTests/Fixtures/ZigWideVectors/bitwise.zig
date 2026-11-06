fn check(comptime T: type, comptime N: usize) bool {
    const LaneVector = @Vector(N, T);
    const bytes = N * @sizeOf(T);
    var raw: [bytes * 4 + 3]u8 = @splat(0xa5);
    const a = @as(*align(1) [N]T, @ptrCast(&raw[1]));
    const b = @as(*align(1) [N]T, @ptrCast(&raw[bytes + 1]));
    const out = @as(*align(1) [N]T, @ptrCast(&raw[bytes * 2 + 1]));
    var i: usize = 0;
    while (i < N) : (i += 1) {
        a[i] = ~@as(T, @intCast(i % 61));
        b[i] = @as(T, @intCast((i * 7 + 19) % 113));
    }
    const av: LaneVector = a.*;
    const bv: LaneVector = b.*;
    const saved = av;
    var changed = av & bv;
    out[0..N].* = changed;
    i = 0;
    while (i < N) : (i += 1) if (out[i] != a[i] & b[i]) return false;
    changed[N - 1] = 0;
    if (saved[N - 1] != a[N - 1] or av[N - 1] != a[N - 1]) return false;
    out[0..N].* = av | bv;
    i = 0;
    while (i < N) : (i += 1) if (out[i] != a[i] | b[i]) return false;
    out[0..N].* = av ^ bv;
    i = 0;
    while (i < N) : (i += 1) if (out[i] != a[i] ^ b[i]) return false;
    out[0..N].* = ~av;
    i = 0;
    while (i < N) : (i += 1) if (out[i] != @as(T, ~a[i])) return false;
    // Check signed high-bit masks and both upper and lower comparison lanes.
    if (N <= 64) {
        var different = bv;
        different[0] = a[0];
        different[N - 1] = a[N - 1];
        const eq = av == different;
        const ne = av != different;
        inline for (0..N) |lane| {
            if (eq[lane]) { if (a[lane] != different[lane]) return false; }
            else if (a[lane] == different[lane]) return false;
            if (ne[lane]) { if (a[lane] == different[lane]) return false; }
            else if (a[lane] != different[lane]) return false;
        }
        if (!@reduce(.And, av == saved)) return false;
    }
    return raw[0] == 0xa5 and raw[bytes * 3 + 1] == 0xa5 and raw[bytes * 4 + 2] == 0xa5;
}

const V = @Vector(32, u32);
fn next(trace: *u32, digit: u32, value: u32) V {
    trace.* = trace.* * 10 + digit;
    return @splat(value);
}
fn maybeNext(trace: *u32, fail: bool) ?V {
    trace.* = trace.* * 10 + 2;
    return if (fail) null else @as(V, @splat(0x0f0f0f0f));
}
fn effects(trace: *u32, fail: bool) bool {
    const anded = next(trace, 1, 0xfedcba98) & (maybeNext(trace, fail) orelse return false);
    if (anded[31] != 0x0e0c0a08) return false;
    const ored = next(trace, 3, 0x80000000) | next(trace, 4, 1);
    if (ored[31] != 0x80000001) return false;
    const xored = next(trace, 5, 0x80000000) ^ next(trace, 6, 0xffffffff);
    if (xored[31] != 0x7fffffff) return false;
    const inverted = ~next(trace, 7, 0xff00ff00);
    return inverted[31] == 0x00ff00ff;
}
fn mutate(value: *V) ?V {
    value.* = @splat(0xffffffff);
    return @as(V, @splat(0xff));
}
fn readBeforeMutation() bool {
    var value: V = @splat(0x12345678);
    const result = value & (mutate(&value) orelse return false);
    return result[31] == 0x78 and value[31] == 0xffffffff;
}
fn compareBeforeMutation() bool {
    var value: V = @splat(0xff);
    const equal = value == (mutate(&value) orelse return false);
    if (!@reduce(.And, equal)) return false;
    value = @splat(0xff);
    const different = value != (mutate(&value) orelse return false);
    return !@reduce(.Or, different);
}
pub fn main() u8 {
    if (!check(u8, 128) or !check(i8, 128)) return 1;
    if (!check(u16, 64) or !check(i16, 64)) return 2;
    if (!check(u32, 32) or !check(i32, 32)) return 3;
    if (!check(u64, 16) or !check(i64, 16)) return 4;
    if (!check(u8, 4) or !check(i8, 4)) return 5;
    if (!check(u16, 2) or !check(i16, 2)) return 6;
    if (!check(u32, 1) or !check(i32, 1)) return 7;
    var trace: u32 = 0;
    if (!effects(&trace, false) or trace != 1234567) return 8;
    trace = 0;
    if (effects(&trace, true) or trace != 12) return 9;
    if (!readBeforeMutation()) return 10;
    if (!compareBeforeMutation()) return 14;
    var codepoints: V = @splat(0x100);
    codepoints[0] = 0;
    codepoints[16] = 0x80;
    codepoints[31] = 0x1f;
    const stop = (codepoints & @as(V, @splat(~@as(u32, 0x9f)))) == @as(V, @splat(0));
    const stop_bits: u32 = @bitCast(stop);
    if (stop_bits != 0x80010001) return 12;
    const mask_back: @Vector(32, bool) = @bitCast(stop_bits);
    if (!mask_back[31] or mask_back[30] or !mask_back[16] or !mask_back[0]) return 13;
    // Ordinary SIMD bitwise operations and the bool-mask path remain usable.
    const normal: @Vector(4, u32) = @splat(0x12345678);
    const mask: @Vector(4, bool) = normal == normal;
    const other: @Vector(4, u32) = @splat(0x0f0f0f0f);
    const bits = (normal & other) | (normal ^ other);
    if (bits[3] != 0x1f3f5f7f or !@reduce(.And, mask)) return 11;
    return 42;
}
