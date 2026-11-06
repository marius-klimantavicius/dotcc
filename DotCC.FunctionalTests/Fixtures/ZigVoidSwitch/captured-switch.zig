const Direction = enum { up, down, none };
fn append(trace: *u32, digit: u32) void { trace.* = trace.* * 10 + digit; }
fn select(trace: *u32, direction: Direction) Direction { append(trace, 1); return direction; }
fn optional(trace: *u32, present: bool) ?u32 { append(trace, 2); return if (present) 3 else null; }
fn adjust(trace: *u32, direction: Direction, present: bool) void {
    switch (select(trace, direction)) {
        .up => if (optional(trace, present)) |value| {
            append(trace, value);
        } else {
            append(trace, 4);
        },
        .down => {
            var index: u32 = 0;
            while (index < 2) : (index += 1) append(trace, 5);
        },
        .none => {},
    }
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var trace: u32 = 0;
    adjust(&trace, .up, true);
    if (trace != 123) return 1;
    trace = 0;
    adjust(&trace, .up, false);
    if (trace != 124) return 2;
    trace = 0;
    adjust(&trace, .down, true);
    if (trace != 155) return 3;
    trace = 0;
    adjust(&trace, .none, true);
    if (trace != 1) return 4;
    return 42;
}
