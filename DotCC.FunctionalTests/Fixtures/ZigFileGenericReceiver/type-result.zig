const Value = @import("Value.zig");
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const receiver = comptime Value{ .value = 17 };
    const T = receiver.Result();
    if (T != u8) return 1;
    const result: T = 42;
    return result;
}
