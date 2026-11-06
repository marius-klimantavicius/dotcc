const std = @import("std");
const Table = struct { run: *const fn (?*anyopaque) void };
pub fn main() u8 {
    const table: Table = .{ .run = std.Io.noCrashHandler };
    const alias = &std.Io.noCrashHandler;
    if (alias != table.run) return 1;
    table.run(null);
    alias(null);
    return 42;
}
