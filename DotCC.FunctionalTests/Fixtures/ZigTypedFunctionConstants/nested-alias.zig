const provider = @import("nested-provider.zig");
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const callback: *const fn () u8 = provider.invoke;
    return callback();
}
