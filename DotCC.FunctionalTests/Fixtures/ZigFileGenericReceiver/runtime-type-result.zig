const Value = @import("Value.zig");
fn invalid(input: u8) u8 {
    const receiver: Value = .{ .value = input };
    const T = receiver.Result();
    const result: T = 42;
    return @intCast(result);
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 { return invalid(17); }
