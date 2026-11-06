const Ptr = ?*u8;
const Result = ?Ptr;
fn wrap(p: Ptr) Result { return p; }
fn step(trace: *u32, digit: u32) void { trace.* = trace.* * 10 + digit; }
fn address(p: *Ptr, trace: *u32) *Ptr { step(trace, 1); return p; }
fn expected(p: Ptr, trace: *u32) Ptr { step(trace, 2); return p; }
fn desired(p: Ptr, trace: *u32) Ptr { step(trace, 3); return p; }
fn lazy(p: *Ptr, value: Ptr, enabled: bool) bool {
    return enabled and @cmpxchgStrong(Ptr, p, null, value, .acq_rel, .acquire) == null;
}
fn retry(p: *Ptr, value: Ptr) u8 {
    var retries: u8 = 0;
    while (@cmpxchgWeak(Ptr, p, null, value, .release, .monotonic)) |observed| {
        if (observed == null) return 99;
        retries += 1;
        p.* = null;
    }
    return retries;
}
fn redirect(slot: **Ptr, replacement: *Ptr, comparand: *Ptr, value: Ptr) Result {
    slot.* = replacement;
    comparand.* = null;
    return value;
}
pub fn main() u8 {
    var first: u8 = 1;
    var second: u8 = 2;
    var head: Ptr = null;
    if (@cmpxchgStrong(Ptr, &head, &first, &second, .release, .monotonic)) |observed| {
        if (observed != null) return 1;
    } else return 2;
    if (@cmpxchgStrong(Ptr, &head, null, &first, .acq_rel, .acquire) != null) return 3;
    const observed = @cmpxchgWeak(Ptr, &head, null, &second, .release, .monotonic) orelse return 4;
    if (observed != &first or head != &first) return 5;
    if (@TypeOf(@cmpxchgStrong(Ptr, &head, null, &second, .seq_cst, .seq_cst)) != Result) return 6;
    if (@TypeOf(head) != Ptr or @TypeOf(&first) == Ptr) return 7;
    var outer: Result = wrap(null);
    if (outer == null or outer.? != null) return 8;
    outer = null;
    if (outer != null) return 9;
    outer = wrap(&first);
    if (outer.? != &first or (outer orelse &second) != &first) return 10;
    outer = wrap(null);
    if ((outer orelse &second) != null) return 11;
    outer = null;
    if ((outer orelse &second) != &second) return 12;
    if (@sizeOf(Result) != 16 or @alignOf(Result) != 8) return 13;
    var trace: u32 = 0;
    if (@cmpxchgStrong(Ptr, address(&head, &trace), expected(&first, &trace), desired(&second, &trace), .seq_cst, .acquire) != null) return 14;
    if (head != &second or trace != 123) return 15;
    trace = 0;
    const failed = @cmpxchgStrong(Ptr, address(&head, &trace), expected(&first, &trace), desired(null, &trace), .acquire, .monotonic) orelse return 16;
    if (failed != &second or trace != 123 or head != &second) return 17;
    head = null;
    if (lazy(&head, &first, false) or head != null) return 18;
    if (!lazy(&head, &first, true) or head != &first) return 19;
    if (retry(&head, &second) != 1 or head != &second) return 20;
    var other: Ptr = null;
    var slot: *Ptr = &head;
    var comparand: Ptr = &second;
    if (@cmpxchgStrong(Ptr, slot, comparand, redirect(&slot, &other, &comparand, &first) orelse return 21, .seq_cst, .seq_cst) != null) return 22;
    if (head != &first or other != null or slot != &other or comparand != null) return 23;
    var nonnull: *u8 = &first;
    if (@cmpxchgStrong(*u8, &nonnull, &first, &second, .seq_cst, .monotonic) != null or nonnull != &second) return 24;
    const nonnullFailed = @cmpxchgStrong(*u8, &nonnull, &first, &first, .seq_cst, .monotonic) orelse return 25;
    if (nonnullFailed != &second) return 26;
    trace = 0;
    if (@atomicRmw(Ptr, address(&head, &trace), .Xchg, desired(null, &trace), .acquire) != &first) return 27;
    if (head != null or trace != 13) return 28;
    if (@atomicRmw(Ptr, &head, .Xchg, &second, .release) != null or head != &second) return 29;
    if (@atomicRmw(*u8, &nonnull, .Xchg, &first, .seq_cst) != &second or nonnull != &first) return 30;
    if (@TypeOf(head.?) != *u8 or @TypeOf(head orelse &first) != *u8) return 31;
    if (head) |payload| {
        if (@TypeOf(payload) != *u8) return 32;
    }
    while (head) |payload| {
        if (@TypeOf(payload) != *u8) return 33;
        break;
    }
    outer = wrap(null);
    if (outer) |*payload| { payload.* = &first; }
    if (outer.? != &first) return 34;
    outer = null;
    if (outer) |*payload| { payload.* = &second; }
    if (outer != null) return 35;
    var slots: [2]Result = .{ wrap(null), null };
    if (@intFromPtr(&slots[1]) - @intFromPtr(&slots[0]) != @sizeOf(Result)) return 36;
    if (slots[0] == null or slots[0].? != null or slots[1] != null) return 37;
    slots[1] = wrap(&first);
    if (slots[0] == null or slots[0].? != null or slots[1].? != &first) return 38;
    return 42;
}
