const std = @import("std");
const Posix = struct {
    const c = std.c;
    pub const Handle = c.pthread_t;
    handle: Handle,
};
const Second = struct {
    const c = std.c;
    pub const Handle = c.pthread_t;
    handle: Handle,
};
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (Posix.Handle != std.c.pthread_t or Second.Handle != Posix.Handle) return 1;
    if (@sizeOf(Posix) != @sizeOf(std.c.pthread_t)) return 2;
    if (@alignOf(Posix) != @alignOf(std.c.pthread_t)) return 3;
    return 42;
}
