const Value = union(enum) { live: u8, absent: noreturn };
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const value: Value = .{ .absent = 0 };
    return switch (value) { .live => |number| number, .absent => unreachable };
}
