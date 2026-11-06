const Tag = enum { payload, empty };
const Value = union(Tag) { payload: u32, empty };
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const invalid: Value = Value.payload;
    _ = invalid;
    return 42;
}
