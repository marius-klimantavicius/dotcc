var trace: u32 = 0;
var value: u8 = 10;
var enabled: bool = false;
fn first() u8 { trace = trace * 10 + 1; return 3; }
fn empty() void { trace = trace * 10 + 2; }
fn last() u8 { trace = trace * 10 + 3; return 4; }
fn consume(left: u8, _: void, right: u8) u8 { return left + right; }
fn mutate() void { value = 99; }
fn tupleValue(args: anytype) u8 {
    if (@typeInfo(@TypeOf(args[1])) != .void) return 0;
    return consume(args[0], args[1], 1);
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (consume(first(), empty(), last()) != 7 or trace != 123) return 1;
    if (consume(value, mutate(), 1) != 11 or value != 99) return 2;
    trace = 0;
    if (enabled and consume(first(), empty(), last()) == 7) return 3;
    if (trace != 0) return 4;
    enabled = true;
    if (!enabled or consume(first(), empty(), last()) != 7 or trace != 123) return 5;
    if (tupleValue(.{ @as(u8, 41), @as(void, {}) }) != 42) return 6;
    return 42;
}
