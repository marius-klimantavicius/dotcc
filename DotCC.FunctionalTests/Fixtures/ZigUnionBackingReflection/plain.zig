const Tag = enum(u8) { none, byte };
const Color = union(Tag) { none: void, byte: u8 };
const Plain = union { byte: u8, word: u16 };
const Extern = extern union { byte: u8, word: u16 };
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (@typeInfo(Color).@"union".backing_integer != null) return 1;
    if (@typeInfo(Plain).@"union".backing_integer != null) return 2;
    if (@typeInfo(Extern).@"union".backing_integer != null) return 3;
    if (@typeInfo(Color).@"union".backing_integer) |_| {
        @compileError("ordinary tagged union has no integer backing");
    } else switch (@typeInfo(Color).@"union".layout) {
        .auto => return 42,
        else => @compileError("ordinary tagged union layout changed"),
    }
}
