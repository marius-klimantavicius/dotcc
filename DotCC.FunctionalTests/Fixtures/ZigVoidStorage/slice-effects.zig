fn record(trace: *u32, digit: u32) void { trace.* = trace.* * 10 + digit; }
fn elements(trace: *u32) []void {
    record(trace, 1);
    const pointer: [*]void = @ptrFromInt(1);
    return pointer[0..3];
}
fn index(trace: *u32) usize { record(trace, 2); return 1; }
fn value(trace: *u32) void { record(trace, 3); }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var trace: u32 = 0;
    elements(&trace)[index(&trace)] = value(&trace);
    if (trace != 123) return 1;
    trace = 0;
    const read = elements(&trace)[index(&trace)];
    _ = read;
    if (trace != 12) return 2;
    trace = 0;
    elements(&trace)[index(&trace)] = undefined;
    if (trace != 12) return 3;
    trace = 0;
    const pointer: *void = @ptrFromInt(1);
    pointer.* = value(&trace);
    if (trace != 3) return 4;
    return 42;
}
