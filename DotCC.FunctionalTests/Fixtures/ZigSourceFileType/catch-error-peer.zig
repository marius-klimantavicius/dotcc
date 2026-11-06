const Errors = error{ Original, Mapped, Other };
const mapped = Errors.Mapped;
fn source(calls: *u8, fail: bool) Errors![]const u8 {
    calls.* += 1;
    if (fail) return error.Original;
    return "*";
}
fn remap(calls: *u8, fallback: *u8, fail: bool) Errors![]const u8 {
    return source(calls, fail) catch map(fallback);
}
fn map(effects: *u8) Errors {
    effects.* += 1;
    return mapped;
}
fn captured(calls: *u8, fail: bool) Errors![]const u8 {
    return source(calls, fail) catch |err| err;
}
fn qualified(calls: *u8, fail: bool) Errors![]const u8 {
    return source(calls, fail) catch Errors.Mapped;
}
fn literal(calls: *u8, fail: bool) Errors![]const u8 {
    return source(calls, fail) catch error.Mapped;
}
fn alias(calls: *u8, fail: bool) Errors![]const u8 {
    return source(calls, fail) catch (mapped);
}
fn clean(calls: *u8, fallback: *u8, trace: *u32, fail: bool) Errors![]const u8 {
    defer trace.* = trace.* * 10 + 1;
    errdefer trace.* = trace.* * 10 + 2;
    defer trace.* = trace.* * 10 + 3;
    return source(calls, fail) catch map(fallback);
}
fn conditional(calls: *u8, effects: *u8, fail: bool, choose: bool) Errors![]const u8 {
    return source(calls, fail) catch if (choose) map(effects) else "!";
}
fn fallbackUnion(calls: *u8, fallback: *u8, fail: bool, again: bool) Errors![]const u8 {
    return source(calls, fail) catch source(fallback, again);
}
fn nested(calls: *u8, effects: *u8, fail: bool) Errors![]const u8 {
    return source(calls, fail) catch (source(effects, true) catch |err| err);
}
fn pointer(value: *u8, fail: bool) Errors!*u8 {
    if (fail) return error.Original;
    return value;
}
fn narrow(fail: bool) Errors!u3 {
    if (fail) return error.Original;
    return 5;
}
fn scalar(fail: bool) Errors!u8 {
    if (fail) return error.Original;
    return 42;
}
fn voidSource(fail: bool) Errors!void {
    if (fail) return error.Original;
}
fn voidMapped(trace: *u8, fail: bool) Errors!void {
    defer trace.* += 1;
    errdefer trace.* += 2;
    return voidSource(fail) catch mapped;
}
pub fn main() u8 {
    var calls: u8 = 0;
    var effects: u8 = 0;
    const ok = remap(&calls, &effects, false) catch return 1;
    if (ok.len != 1 or ok[0] != 42 or calls != 1 or effects != 0) return 2;
    if (remap(&calls, &effects, true)) |_| return 3 else |err| {
        if (err != error.Mapped or calls != 2 or effects != 1) return 4;
    }
    const saved = source(&calls, true) catch mapped;
    comptime { if (@TypeOf(saved) != Errors![]const u8) @compileError("CATCH_LOST_ERROR_UNION_TYPE"); }
    if (saved) |_| return 5 else |err| { if (err != error.Mapped) return 6; }
    if (captured(&calls, true)) |_| return 7 else |err| { if (err != error.Original) return 8; }
    if (qualified(&calls, true)) |_| return 9 else |err| { if (err != error.Mapped) return 10; }
    if (literal(&calls, true)) |_| return 11 else |err| { if (err != error.Mapped) return 12; }
    if (alias(&calls, true)) |_| return 13 else |err| { if (err != error.Mapped) return 14; }
    var trace: u32 = 0;
    _ = clean(&calls, &effects, &trace, false) catch return 15;
    if (trace != 31 or effects != 1) return 16;
    trace = 0;
    if (clean(&calls, &effects, &trace, true)) |_| return 17 else |err| {
        if (err != error.Mapped or trace != 321 or effects != 2) return 18;
    }
    const cond = conditional(&calls, &effects, true, false) catch return 19;
    if (cond.len != 1 or cond[0] != 33 or effects != 2) return 20;
    if (conditional(&calls, &effects, true, true)) |_| return 21 else |err| {
        if (err != error.Mapped or effects != 3) return 22;
    }
    _ = conditional(&calls, &effects, false, true) catch return 23;
    if (effects != 3) return 24;
    const alternate = fallbackUnion(&calls, &effects, true, false) catch return 25;
    if (alternate[0] != 42 or effects != 4) return 26;
    if (fallbackUnion(&calls, &effects, true, true)) |_| return 27 else |err| {
        if (err != error.Original or effects != 5) return 28;
    }
    const number = scalar(true) catch mapped;
    if (number) |_| return 29 else |err| { if (err != error.Mapped) return 30; }
    const value = scalar(false) catch mapped;
    const scalarOk = value catch return 31;
    if (scalarOk != 42) return 32;
    var voidTrace: u8 = 0;
    voidMapped(&voidTrace, false) catch return 33;
    if (voidTrace != 1) return 34;
    if (voidMapped(&voidTrace, true)) |_| return 35 else |err| {
        if (err != error.Mapped or voidTrace != 4) return 36;
    }
    const beforeNested = calls;
    _ = nested(&calls, &effects, false) catch return 37;
    if (calls != beforeNested + 1 or effects != 5) return 38;
    if (nested(&calls, &effects, true)) |_| return 39 else |err| {
        if (err != error.Original or calls != beforeNested + 2 or effects != 6) return 40;
    }
    const recovered = source(&calls, true) catch "?";
    if (recovered.len != 1 or recovered[0] != 63) return 41;
    var target: u8 = 42;
    const pointerOk = pointer(&target, false) catch mapped;
    const ptr = pointerOk catch return 43;
    if (ptr != &target or ptr.* != 42) return 44;
    const pointerErr = pointer(&target, true) catch mapped;
    if (pointerErr) |_| return 45 else |err| { if (err != error.Mapped) return 46; }
    const small = narrow(false) catch mapped;
    const smallOk = small catch return 47;
    if (@typeInfo(@TypeOf(smallOk)).int.bits != 3 or smallOk != 5) return 48;
    return 42;
}
