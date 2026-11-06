const Value = union(enum) { live: u8, other: struct { present: u8 } };
fn read(value: Value) u8 {
    return switch (value) {
        .live => |number| number,
        .other => |payload| payload.missing,
    };
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 { return read(.{ .live = 42 }); }
