var values: [3]i32 = .{ 1, 2, 3 };
var order: u32 = 0;
var completions: u8 = 0;
fn supply(marker: u8) []const i32 { order = order * 10 + marker; return &values; }
fn complete() bool { completions += 1; return true; }
fn fallback() bool { completions += 1; return false; }
fn compare(left: [3]i32, right: [3]i32) bool {
    switch (@typeInfo(@TypeOf(left))) {
        .array => for (left, right) |a, b| { if (a != b) return false; } else return complete(),
        else => unreachable,
    }
}
fn headers(mode: u8) bool {
    switch (mode) {
        1 => for (supply(1), supply(2)) |a, b| { if (a != b) return false; } else return complete(),
        else => return false,
    }
}
fn breaking(mode: u8) u8 {
    switch (mode) {
        1 => for (values) |value| {
            switch (value) { 2 => break, else => {} }
        } else return 1,
        else => return 2,
    }
    return 42;
}
fn find(needle: i32) bool {
    return for (values) |value| { if (value == needle) break true; } else fallback();
}
fn findReturn(needle: i32) bool {
    return for (values) |value| { if (value == needle) break true; } else return false;
}
fn whileReturn(mode: u8, stop: bool) u8 {
    var index: u8 = 0;
    switch (mode) {
        1 => while (index < 3) : (index += 1) {
            if (index == 1) continue;
            switch (index) { 2 => if (stop) break, else => {} }
        } else return index + 40,
        else => return 0,
    }
    return index + 40;
}
fn next(index: *u8) ?u8 {
    index.* += 1;
    if (index.* < 3) return index.*;
    return null;
}
fn captureReturn(mode: u8, stop: bool) u8 {
    var index: u8 = 0;
    switch (mode) {
        1 => while (next(&index)) |value| {
            if (stop and value == 2) break;
        } else return index + 40,
        else => return 0,
    }
    return index + 40;
}
fn nextError(index: *u8) error{Done}!u8 {
    index.* += 1;
    if (index.* < 3) return index.*;
    return error.Done;
}
fn errorReturn(mode: u8) u8 {
    var index: u8 = 0;
    switch (mode) {
        1 => while (nextError(&index)) |_| {} else |err| return if (err == error.Done) index + 39 else 0,
        else => return 0,
    }
}
fn conditionalReturn(mode: u8, flag: bool) u8 {
    switch (mode) {
        1 => for (values) |_| {} else if (flag) return 41 else return 42,
        else => return 0,
    }
}
fn unreachableElse(mode: u8) u8 {
    switch (mode) {
        1 => for (values) |value| { if (value == 1) break; } else unreachable,
        else => return 0,
    }
    return 42;
}
fn labeledReturn(mode: u8) u8 {
    var index: u8 = 0;
    switch (mode) {
        1 => outer: while (index < 4) : (index += 1) {
            for (values) |value| {
                if (value == 2) continue :outer;
            } else return 1;
        } else return index + 38,
        else => return 0,
    }
}
fn emptyElse(mode: u8) u8 {
    switch (mode) {
        1 => for (values) |_| {} else {},
        else => return 0,
    }
    return 42;
}
fn inlineReturn(mode: u8) u8 {
    switch (mode) {
        1 => inline for (@typeInfo(enum { first, second }).@"enum".field_values) |_| {} else return 42,
        else => return 0,
    }
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    completions = 0;
    if (!compare(.{ 1, 2, 3 }, .{ 1, 2, 3 }) or completions != 1) return 1;
    if (compare(.{ 1, 2, 3 }, .{ 1, 4, 3 }) or completions != 1) return 2;
    order = 0;
    if (!headers(1) or order != 12 or completions != 2) return 3;
    if (breaking(1) != 42) return 4;
    if (!find(2) or completions != 2) return 5;
    if (find(9) or completions != 3) return 6;
    if (!findReturn(2) or findReturn(9)) return 7;
    if (whileReturn(1, false) != 43 or whileReturn(1, true) != 42) return 8;
    if (captureReturn(1, false) != 43 or captureReturn(1, true) != 42) return 9;
    if (errorReturn(1) != 42) return 10;
    if (conditionalReturn(1, true) != 41 or conditionalReturn(1, false) != 42) return 11;
    if (unreachableElse(1) != 42) return 12;
    if (labeledReturn(1) != 42) return 13;
    if (emptyElse(1) != 42) return 14;
    if (inlineReturn(1) != 42) return 15;
    return 42;
}
