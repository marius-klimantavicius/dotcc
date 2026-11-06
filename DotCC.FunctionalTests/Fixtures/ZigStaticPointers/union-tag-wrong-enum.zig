const Tag = enum(u16) { payload = 128, empty = 65280 };
const Other = enum(u16) { payload = 128, empty = 65280 };
const Value = union(Tag) { payload: u32, empty };
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const a: Value = .{ .payload = 17 };
    return if (a == Other.payload) 42 else 1;
}
