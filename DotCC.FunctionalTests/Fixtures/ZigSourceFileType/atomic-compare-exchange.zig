const AtomicOrder = @import("std").lang.AtomicOrder;
fn namedOrder(p: *u32, comptime success: AtomicOrder, comptime failure: AtomicOrder) ?u32 {
    return @cmpxchgStrong(u32, p, 0, 42, success, failure);
}
fn step(trace: *u32, digit: u32) void { trace.* = trace.* * 10 + digit; }
fn address(p: *u32, trace: *u32) *u32 { step(trace, 1); return p; }
fn expected(trace: *u32) u32 { step(trace, 2); return 0; }
fn desired(trace: *u32) u32 { step(trace, 3); return 42; }
fn redirect(p: **u32, replacement: *u32, expected_value: *u32) ?u32 {
    p.* = replacement;
    expected_value.* = 99;
    return 42;
}
fn observedWidth(value: anytype) u8 {
    if (@bitSizeOf(@TypeOf(value)) != 32) @compileError("lost observed width");
    return @intCast(value);
}
fn lazy(p: *u32, enabled: bool) bool {
    return enabled and @cmpxchgStrong(u32, p, 1, 9, .acq_rel, .acquire) == null;
}
fn retryLoop(p: *u32) u8 {
    var retries: u8 = 0;
    while (@cmpxchgStrong(u32, p, 0, 42, .acq_rel, .acquire) != null) {
        retries += 1;
        p.* = 0;
    }
    return retries;
}
pub fn main() u8 {
    var word: u32 = 0;
    var trace: u32 = 0;
    if (@cmpxchgStrong(u32, address(&word, &trace), expected(&trace), desired(&trace), .acq_rel, .acquire) != null) return 1;
    if (word != 42 or trace != 123) return 2;
    trace = 0;
    const ordered_failure = @cmpxchgStrong(u32, address(&word, &trace), expected(&trace), desired(&trace), .acq_rel, .acquire) orelse return 26;
    if (ordered_failure != 42 or trace != 123 or word != 42) return 27;
    const failed = @cmpxchgStrong(u32, &word, 0, 99, .seq_cst, .seq_cst) orelse return 3;
    if (failed != 42 or word != 42 or observedWidth(failed) != 42) return 4;
    word = 0;
    const zero = @cmpxchgStrong(u32, &word, 1, 99, .acquire, .acquire) orelse return 5;
    if (zero != 0 or word != 0) return 6;
    var byte: u8 = 255;
    if (@cmpxchgStrong(u8, &byte, 255, 42, .monotonic, .monotonic) != null or byte != 42) return 7;
    var half: u16 = 0xfedc;
    const half_failure = @cmpxchgStrong(u16, &half, 1, 2, .release, .monotonic) orelse return 8;
    if (half_failure != 0xfedc) return 9;
    var signed: i32 = -42;
    if (@cmpxchgStrong(i32, &signed, -42, 1, .release, .acquire) != null or signed != 1) return 10;
    var wide: u64 = 0xfedcba9876543210;
    const wide_failure = @cmpxchgStrong(u64, &wide, 0, 1, .seq_cst, .acquire) orelse return 11;
    if (wide_failure != 0xfedcba9876543210) return 12;
    if (@cmpxchgWeak(u64, &wide, 0xfedcba9876543210, 42, .seq_cst, .monotonic) != null or wide != 42) return 13;
    var other: u32 = 1;
    var old: u32 = 7;
    var new: u32 = 8;
    var target: *u32 = &old;
    var comparand: u32 = 7;
    if (@cmpxchgStrong(u32, target, comparand, redirect(&target, &new, &comparand) orelse return 17, .seq_cst, .seq_cst) != null) return 18;
    if (old != 42 or new != 8 or target != &new or comparand != 99) return 19;
    if (lazy(&other, false) or other != 1) return 20;
    if (!lazy(&other, true) or other != 9) return 21;
    word = 1;
    if (retryLoop(&word) != 1 or word != 42) return 28;
    word = 0;
    if (namedOrder(&word, .acq_rel, .acquire) != null or word != 42) return 25;
    // Actual stream slot-claim shape: null breaks, failure carries the old key.
    var slots: [2]u32 = .{ 0xffffffff, 0xffffffff };
    for (&slots) |*slot| {
        const cur = @atomicLoad(u32, slot, .acquire);
        if (cur != 0xffffffff) continue;
        const actual = @cmpxchgStrong(u32, slot, 0xffffffff, 42, .acq_rel, .acquire) orelse break;
        if (actual == 42) return 22;
    } else return 23;
    if (slots[0] != 42 or slots[1] != 0xffffffff) return 24;
    return 42;
}
