const Cell = packed struct(u64) { codepoint: u32, flags: u32 };
const CCell = u64;
const View = extern struct { ptr: ?[*]const CCell, len: usize };
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var cells = [_]CCell{42, 0};
    const view: View = .{ .ptr = &cells, .len = 2 };
    const ptr = view.ptr.?;
    const first: Cell = @bitCast(ptr[0]);
    return @intCast(first.codepoint);
}
