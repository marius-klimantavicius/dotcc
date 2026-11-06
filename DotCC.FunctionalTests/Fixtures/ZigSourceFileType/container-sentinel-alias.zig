fn Factory(comptime alignment: ?usize) type {
    return struct {
        const Slice = if (alignment) |_| []const u8 else []const u8;
        fn identity(value: Slice) Slice { return value; }
    };
}
const Plain = Factory(null);
const Marked = struct {
    const Slice = [:7]const u8;
    fn identity(value: Slice) Slice { return value; }
};
fn inspect(value: anytype) u8 {
    const info = @typeInfo(@TypeOf(value)).pointer;
    const sentinel = info.sentinel_ptr orelse return 10;
    const scalar: *const u8 = @ptrCast(@alignCast(sentinel));
    return scalar.*;
}
pub fn main() u8 {
    const buffer: [2:7]u8 = .{ 11, 13 };
    const plain = Plain.identity(&buffer);
    const marked = Marked.identity(&buffer);
    if (inspect(plain) != 10 or inspect(marked) != 7) return 1;
    if (plain.len != 2 or marked.len != 2 or marked[2] != 7) return 2;
    if (plain.ptr != marked.ptr or plain[0] + marked[1] != 24) return 3;
    return 42;
}
