var calls: u8 = 0;
var values = [_]u16{ 41, 42 };
fn view() *[2]u16 { calls += 1; return &values; }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const input = "ABC";
    const bytes = input.ptr;
    const sentinel: [*:0]const u8 = bytes;
    if (sentinel[0] != 'A' or sentinel[3] != 0) return 1;
    if (@typeInfo(@TypeOf(bytes)).pointer.size != .many) return 2;
    const elements = view().ptr;
    elements[0] += 1;
    if (calls != 1 or values[0] != 42) return 3;
    if (@typeInfo(@TypeOf(elements)).pointer.size != .many) return 4;
    if (@bitSizeOf(@typeInfo(@TypeOf(elements)).pointer.child) != 16) return 5;
    return 42;
}
