const std = @import("std");
const E = error{Failed};
const Future = std.Io.Future(E!void);

comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (@sizeOf(E!void) != 2 or @alignOf(E!void) != 2) return 1;
    var current: ?Future = null;
    if (current != null) return 2;
    current = .{ .any_future = null, .result = {} };
    if (current) |future| {
        if (future.any_future != null) return 3;
        future.result catch return 3;
    } else return 4;
    current = .{ .any_future = null, .result = error.Failed };
    const copy = current;
    current = null;
    if (copy) |future| {
        future.result catch |err| {
            if (err != error.Failed) return 5;
            current = null;
        };
    } else return 6;
    if (current != null) return 7;
    return 42;
}
