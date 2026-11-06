fn note(trace: *u64, digit: u64) void { trace.* = trace.* * 10 + digit; }
fn mark(trace: *u64, digit: u64, value: u8) u8 { note(trace, digit); return value; }
fn optional(trace: *u64, fail: bool) ?u8 { note(trace, 2); return if (fail) null else 20; }
fn risky(trace: *u64, fail: bool) !u8 { note(trace, 2); return if (fail) error.Argument else 20; }
fn pair(a: u8, b: u8) u8 { return a + b; }
fn finish(trace: *u64, a: u8, b: u8, fail: bool) !u8 {
    note(trace, 4);
    return if (fail) error.Callee else a + b;
}
fn outer(trace: *u64, fail_arg: bool, fail_call: bool) !u8 {
    defer note(trace, 9);
    errdefer note(trace, 8);
    const result = finish(trace, mark(trace, 1, 10), optional(trace, fail_arg) orelse return error.Missing, fail_call)
        catch return error.Call;
    note(trace, 3);
    return result;
}
fn catchArgument(trace: *u64, fail_arg: bool) !u8 {
    defer note(trace, 9);
    errdefer note(trace, 8);
    return finish(trace, mark(trace, 1, 10), risky(trace, fail_arg) catch return error.Arg, false)
        catch return error.Call;
}
const State = struct {
    trace: u64 = 0,
    value: u8 = 10,
    fn append(self: *State, a: u8, b: u8) !u8 {
        note(&self.trace, 4);
        self.value = a + b;
        return self.value;
    }
    fn byValue(self: State, a: u8) u8 { return self.value + a; }
    fn generic(self: *State, a: u8, b: anytype) !u8 { return self.append(a, b); }
};
fn receiver(s: *State) *State { note(&s.trace, 1); return s; }
fn mutate(s: *State, fail: bool) ?u8 {
    note(&s.trace, 2);
    s.value = 20;
    return if (fail) null else 2;
}
fn withReceiver(s: *State, fail: bool) !u8 {
    defer note(&s.trace, 9);
    errdefer note(&s.trace, 8);
    return receiver(s).append(s.value, mutate(s, fail) orelse return error.Missing) catch return error.Call;
}
fn valueReceiver(s: *State) !u8 {
    return s.byValue(mutate(s, false) orelse return error.Missing);
}
fn firstPointer(s: *State) *u8 { note(&s.trace, 1); return &s.value; }
fn usePointer(p: *u8, v: u8) u8 { p.* += v; return p.*; }
fn pointerArgument(s: *State, fail: bool) !u8 {
    return usePointer(firstPointer(s), mutate(s, fail) orelse return error.Missing);
}
fn nested(trace: *u64, fail: bool) !u8 {
    return pair(mark(trace, 1, 10), pair(mark(trace, 3, 1), optional(trace, fail) orelse return error.Missing));
}
fn lazy(trace: *u64, take: bool, fail: bool) !u8 {
    return pair(mark(trace, 1, 10), if (take) optional(trace, fail) orelse return error.Missing else mark(trace, 3, 30));
}
fn managedCallback(op: *const fn(u8, u8) u8, trace: *u64, fail: bool) !u8 {
    return op(mark(trace, 1, 10), optional(trace, fail) orelse return error.Missing);
}
const PairFields = struct { a: u8, b: u8 };
fn takeFields(a: u8, fields: PairFields) u8 { return a + fields.a + fields.b; }
fn nestedLiteral(trace: *u64, fail: bool) !u8 {
    return takeFields(mark(trace, 1, 10), .{ .a = mark(trace, 3, 1), .b = optional(trace, fail) orelse return error.Missing });
}
fn nestedReturn(trace: *u64, fail: bool) !u8 {
    defer note(trace, 9);
    errdefer note(trace, 8);
    return finish(trace, mark(trace, 1, 10), optional(trace, true) orelse
        return pair(mark(trace, 3, 1), optional(trace, fail) orelse return error.Missing), false) catch return error.Call;
}
const Callbacks = struct { op: *const fn(u8, u8) u8 };
fn callbackField(callbacks: *Callbacks, trace: *u64, fail: bool) !u8 {
    return callbacks.op(mark(trace, 1, 10), optional(trace, fail) orelse return error.Missing);
}
fn genericReceiver(s: *State, fail: bool) !u8 {
    return receiver(s).generic(s.value, mutate(s, fail) orelse return error.Missing) catch return error.Call;
}
fn subtract(a: u8, b: u8) u8 { return b - a; }
fn changeCallback(callbacks: *Callbacks) ?u8 { callbacks.op = &subtract; return 20; }
fn originalCallback(callbacks: *Callbacks) !u8 {
    return callbacks.op(10, changeCallback(callbacks) orelse return error.Missing);
}
fn voidArgument(value: void, tail: u8) u8 { _ = value; return tail; }
fn withVoid(trace: *u64, fail: bool) !u8 {
    return voidArgument({}, optional(trace, fail) orelse return error.Missing);
}
fn appendVoid(trace: *u64, a: u8, b: u8, fail: bool) !void {
    _ = a;
    _ = b;
    note(trace, 4);
    if (fail) return error.Callee;
}
fn wholeStatement(trace: *u64, fail_arg: bool, fail_call: bool) !void {
    defer note(trace, 9);
    errdefer note(trace, 8);
    appendVoid(trace, mark(trace, 1, 10), optional(trace, fail_arg) orelse return error.Missing, fail_call)
        catch return error.Call;
    note(trace, 3);
}
pub fn main() u8 {
    var t: u64 = 0;
    if (pair(mark(&t, 1, 10), mark(&t, 2, 20)) != 30 or t != 12) return 1;
    t = 0;
    if ((outer(&t, false, false) catch 0) != 30 or t != 12439) return 2;
    t = 0;
    if (outer(&t, true, false)) |_| { return 3; } else |err| { if (err != error.Missing or t != 1289) return 4; }
    t = 0;
    if (outer(&t, false, true)) |_| { return 5; } else |err| { if (err != error.Call or t != 12489) return 6; }
    t = 0;
    if ((catchArgument(&t, false) catch 0) != 30 or t != 1249) return 7;
    t = 0;
    if (catchArgument(&t, true)) |_| { return 8; } else |err| { if (err != error.Arg or t != 1289) return 9; }
    var s: State = .{};
    if ((withReceiver(&s, false) catch 0) != 12 or s.value != 12 or s.trace != 1249) return 10;
    s = .{};
    if (withReceiver(&s, true)) |_| { return 11; } else |err| { if (err != error.Missing or s.value != 20 or s.trace != 1289) return 12; }
    s = .{};
    if ((valueReceiver(&s) catch 0) != 12 or s.value != 20 or s.trace != 2) return 13;
    s = .{};
    if ((pointerArgument(&s, false) catch 0) != 22 or s.value != 22 or s.trace != 12) return 14;
    s = .{};
    if (pointerArgument(&s, true)) |_| { return 15; } else |err| { if (err != error.Missing or s.value != 20 or s.trace != 12) return 16; }
    t = 0;
    if ((nested(&t, false) catch 0) != 31 or t != 132) return 17;
    t = 0;
    if (nested(&t, true)) |_| { return 18; } else |err| { if (err != error.Missing or t != 132) return 19; }
    t = 0;
    if ((lazy(&t, false, true) catch 0) != 40 or t != 13) return 20;
    t = 0;
    if ((lazy(&t, true, false) catch 0) != 30 or t != 12) return 21;
    t = 0;
    if (lazy(&t, true, true)) |_| { return 22; } else |err| { if (err != error.Missing or t != 12) return 23; }
    t = 0;
    if ((managedCallback(&pair, &t, false) catch 0) != 30 or t != 12) return 24;
    t = 0;
    if (managedCallback(&pair, &t, true)) |_| { return 25; } else |err| { if (err != error.Missing or t != 12) return 26; }
    t = 0;
    if ((nestedLiteral(&t, false) catch 0) != 31 or t != 132) return 27;
    t = 0;
    if (nestedLiteral(&t, true)) |_| { return 28; } else |err| { if (err != error.Missing or t != 132) return 29; }
    t = 0;
    if ((nestedReturn(&t, false) catch 0) != 21 or t != 12329) return 30;
    t = 0;
    if (nestedReturn(&t, true)) |_| { return 31; } else |err| { if (err != error.Missing or t != 123289) return 32; }
    var callbacks: Callbacks = .{ .op = &pair };
    t = 0;
    if ((callbackField(&callbacks, &t, false) catch 0) != 30 or t != 12) return 33;
    t = 0;
    if (callbackField(&callbacks, &t, true)) |_| { return 34; } else |err| { if (err != error.Missing or t != 12) return 35; }
    s = .{};
    if ((genericReceiver(&s, false) catch 0) != 12 or s.value != 12 or s.trace != 124) return 36;
    s = .{};
    if (genericReceiver(&s, true)) |_| { return 37; } else |err| { if (err != error.Missing or s.value != 20 or s.trace != 12) return 38; }
    if ((originalCallback(&callbacks) catch 0) != 30 or callbacks.op(10, 20) != 10) return 39;
    t = 0;
    if ((withVoid(&t, false) catch 0) != 20 or t != 2) return 40;
    t = 0;
    wholeStatement(&t, false, false) catch return 41;
    if (t != 12439) return 43;
    t = 0;
    if (wholeStatement(&t, true, false)) { return 44; } else |err| { if (err != error.Missing or t != 1289) return 45; }
    t = 0;
    if (wholeStatement(&t, false, true)) { return 46; } else |err| { if (err != error.Call or t != 12489) return 47; }
    return 42;
}
