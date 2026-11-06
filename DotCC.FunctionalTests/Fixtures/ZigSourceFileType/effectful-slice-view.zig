var calls: u8 = 0;
var words: [2]u16 = .{ 0x1234, 0x5678 };
fn source() []const u16 { calls += 1; return &words; }
fn choose(present: bool, bytes: []u8) []const u8 {
    return if (present) bytes else "";
}
fn optionalView(present: bool, bytes: []u8) ?[]u16 {
    if (!present) return null;
    return @ptrCast(@alignCast(bytes));
}
fn inferred(present: bool, bytes: []u8) bool {
    const data = blk: {
        if (!present) break :blk "";
        break :blk bytes;
    };
    const pointer: ?*u8 = if (present) &bytes[0] else null;
    const captured = if (pointer) |_| bytes else "";
    return data.len == (if (present) bytes.len else @as(usize, 0))
        and captured.len == data.len and (!present or (data.ptr == bytes.ptr and captured.ptr == bytes.ptr));
}
fn emptyWords(present: bool, values: []const u16) bool {
    const right = if (present) values else &.{};
    const left = if (present) &.{} else values;
    return right.len == (if (present) values.len else @as(usize, 0))
        and left.len == (if (present) @as(usize, 0) else values.len);
}
pub fn main() u8 {
    const bytes: []u8 = @ptrCast(@constCast(source()));
    if (calls != 1 or bytes.len != 4) return 1;
    if (bytes[0] != 0x34 or bytes[1] != 0x12 or bytes[2] != 0x78 or bytes[3] != 0x56) return 2;
    bytes[1] = 0x55;
    if (words[0] != 0x5534 or words[1] != 0x5678) return 3;
    const a = choose(true, bytes);
    const b = choose(false, bytes);
    if (a.len != 4 or a.ptr != bytes.ptr or b.len != 0) return 4;
    if (!inferred(false, bytes) or !inferred(true, bytes)) return 5;
    if (optionalView(false, bytes) != null) return 6;
    const viewed = optionalView(true, bytes) orelse return 7;
    if (viewed.len != 2 or viewed[0] != 0x5534 or viewed[1] != 0x5678) return 8;
    if (!emptyWords(true, &words) or !emptyWords(false, &words)) return 9;
    return 42;
}
