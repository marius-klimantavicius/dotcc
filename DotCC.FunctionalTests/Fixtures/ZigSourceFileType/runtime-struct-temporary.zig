const Packet = struct { value: u8, trace: *u32 };
fn take(trace: *u32, digit: u32, value: u8) u8 {
    trace.* = trace.* * 10 + digit;
    return value;
}
fn consume(packet: *const Packet, amount: u8) u8 {
    packet.trace.* = packet.trace.* * 10 + 3;
    return packet.value + amount;
}
fn run(trace: *u32, value: u8) u8 {
    return consume(&.{ .value = take(trace, 1, value), .trace = trace }, take(trace, 2, 2));
}
fn compare(first: *const Packet, second: *const Packet) bool {
    return first != second and first.value == 40 and second.value == 41;
}
fn nested(packet: *const Packet, depth: u8) u8 {
    const before = packet.value;
    if (depth == 0) return before;
    const answer = nested(&.{ .value = before + 1, .trace = packet.trace }, depth - 1);
    if (packet.value != before) return 0;
    return answer;
}
pub fn main() u8 {
    var trace: u32 = 0;
    var value: u8 = 39;
    value += 1;
    if (run(&trace, value) != 42 or trace != 123) return 1;
    if (!compare(&.{ .value = value, .trace = &trace }, &.{ .value = value + 1, .trace = &trace })) return 2;
    if (nested(&.{ .value = value, .trace = &trace }, 2) != 42) return 3;
    trace = 0;
    if (trace != 0 and consume(&.{ .value = take(&trace, 1, value), .trace = &trace }, 2) == 42) return 4;
    if (trace != 0) return 5;
    return 42;
}
