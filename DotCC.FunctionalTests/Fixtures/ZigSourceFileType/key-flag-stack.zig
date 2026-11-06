const key = @import("key");
comptime { @export(&answer, .{ .name = "answer" }); }
fn incoming(trace: *u32, value: u5) key.Flags {
    trace.* = trace.* * 10 + 2;
    return @bitCast(value);
}
fn receiver(trace: *u32, stack: *key.FlagStack) *key.FlagStack {
    trace.* = trace.* * 10 + 1;
    return stack;
}
pub fn answer() callconv(.c) u8 {
    var stack: key.FlagStack = .{};
    var slot: usize = 0;
    while (slot < 8) : (slot += 1) {
        stack.idx = @intCast(slot);
        var a: u8 = 0;
        while (a < 32) : (a += 1) {
            var b: u8 = 0;
            while (b < 32) : (b += 1) {
                const first: u5 = @intCast(a);
                const second: u5 = @intCast(b);
                stack.flags[slot] = @bitCast(first);
                stack.set(.not, @bitCast(second));
                if (stack.current().int() != (first & ~second)) return 1;
                stack.set(.@"or", @bitCast(second));
                if (stack.current().int() != ((first & ~second) | second)) return 2;
                stack.set(.set, @bitCast(second));
                if (stack.current().int() != second) return 3;
            }
        }
    }
    var trace: u32 = 0;
    receiver(&trace, &stack).set(.not, incoming(&trace, 31));
    if (trace != 12 or stack.current().int() != 0) return 4;
    stack.pop(100);
    var i: usize = 0;
    while (i < 24) : (i += 1) {
        stack.push(@bitCast(@as(u5, @truncate(i))));
        if (stack.idx != (i + 1) % 8 or stack.current().int() != i % 32) return 5;
    }
    stack.pop(100);
    if (stack.idx != 0) return 6;
    for (stack.flags) |flag| if (flag.int() != 0) return 7;
    return 42;
}
