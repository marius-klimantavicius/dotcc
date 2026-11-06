const E = error{Failed};

fn payload(counter: *u32) void {
    counter.* = counter.* * 10 + 1;
}
fn attempt(counter: *u32, fail: bool) E!void {
    if (fail) return error.Failed;
    return payload(counter);
}
fn forward(counter: *u32, fail: bool) E!void {
    try attempt(counter, fail);
    counter.* = counter.* * 10 + 2;
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var counter: u32 = 0;
    forward(&counter, false) catch return 1;
    if (counter != 12) return 2;
    forward(&counter, true) catch |err| {
        if (err != error.Failed) return 3;
        counter = counter * 10 + 3;
    };
    if (counter != 123) return 4;
    attempt(&counter, false) catch {};
    if (counter != 1231) return 5;
    attempt(&counter, true) catch {};
    if (counter != 1231) return 6;
    return 42;
}
