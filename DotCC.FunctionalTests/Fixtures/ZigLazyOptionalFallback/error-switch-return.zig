const Request = union(enum) { support, query: u8 };
fn other(fail: bool) error{Invalid}!Request {
    if (fail) return error.Invalid;
    return .{ .query = 42 };
}
fn parse(kind: u8, trace: *u32) error{Invalid}!Request {
    defer trace.* = trace.* * 10 + 9;
    errdefer trace.* = trace.* * 10 + 8;
    return switch (kind) {
        0 => { trace.* = 1; return .support; },
        1 => .{ .query = 42 },
        2 => other(false),
        3 => other(true),
        else => error.Invalid,
    };
}
fn mapped(fail: bool, large: bool, trace: *u32) error{Invalid,TooBig}!Request {
    defer trace.* = trace.* * 10 + 9;
    errdefer trace.* = trace.* * 10 + 8;
    return other(fail) catch |err| switch (err) {
        error.Invalid => if (large) error.TooBig else error.Invalid,
    };
}
fn labeled(kind: u8, trace: *u32) error{Invalid}!Request {
    defer trace.* = trace.* * 10 + 9;
    errdefer trace.* = trace.* * 10 + 8;
    return result: {
        defer trace.* = trace.* * 10 + 7;
        switch (kind) {
            0 => break :result .{ .query = 42 },
            1 => break :result .support,
            2 => break :result other(false),
            3 => break :result error.Invalid,
            else => break :result other(true),
        }
    };
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var trace: u32 = 0;
    const support = parse(0, &trace) catch return 1;
    if (support != .support or trace != 19) return 2;
    for ([_]u8{ 1, 2 }) |kind| {
        trace = 0;
        const query = parse(kind, &trace) catch return 3;
        if (query.query != 42 or trace != 9) return 4;
    }
    for ([_]u8{ 3, 4 }) |kind| {
        trace = 0;
        if (parse(kind, &trace)) |_| return 5 else |err| {
            if (err != error.Invalid or trace != 89) return 6;
        }
    }
    trace = 0;
    const success = mapped(false, true, &trace) catch return 7;
    if (success.query != 42 or trace != 9) return 8;
    for ([_]bool{ false, true }) |large| {
        trace = 0;
        if (mapped(true, large, &trace)) |_| return 9 else |err| {
            if (err != (if (large) error.TooBig else error.Invalid) or trace != 89) return 10;
        }
    }
    for ([_]u8{ 0, 1, 2 }) |kind| {
        trace = 0;
        const value = labeled(kind, &trace) catch return 11;
        if (trace != 79) return 12;
        if (kind == 1) {
            if (value != .support) return 13;
        } else if (value.query != 42) return 14;
    }
    for ([_]u8{ 3, 4 }) |kind| {
        trace = 0;
        if (labeled(kind, &trace)) |_| return 15 else |err| {
            if (err != error.Invalid or trace != 789) return 16;
        }
    }
    return 42;
}
