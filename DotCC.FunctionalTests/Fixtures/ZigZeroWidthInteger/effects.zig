fn produce(trace: *u32, digit: u32, result: u64) u64 {
    trace.* = trace.* * 10 + digit;
    return result;
}
fn checked(trace: *u32) u0 { return @intCast(produce(trace, 1, 0)); }
fn truncated(trace: *u32) u0 { return @truncate(produce(trace, 2, 99)); }
pub fn rejects(value: u64) callconv(.c) u8 { return @as(u8, @as(u0, @intCast(value))); }
comptime { @export(&answer, .{ .name = "answer" }); @export(&rejects, .{ .name = "rejects" }); }
pub fn answer() callconv(.c) u8 {
    var trace: u32 = 0;
    var zero: u0 = checked(&trace);
    zero = truncated(&trace);
    if (trace != 12 or @as(u64, zero) != 0) return 1;
    const shifted = @as(u1, 1) << checked(&trace);
    if (trace != 121 or shifted != 1) return 2;
    _ = @as(u0, @intCast(produce(&trace, 3, 0)));
    if (trace != 1213 or rejects(0) != 0) return 3;
    return 42;
}
