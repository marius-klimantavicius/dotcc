fn typeBits(comptime T: type) u8 { return @typeInfo(T).int.bits; }
fn elements(values: anytype) u8 {
    for (values) |value| return typeBits(@TypeOf(value));
    return 0;
}
fn typed(comptime T: type, values: T) u8 { return elements(values); }
fn forward(values: anytype) u8 { return typed(@TypeOf(values), values); }
fn reverse(values: anytype) u8 { return typed(@TypeOf(values), values); }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const unicode = [_]u21{65};
    const codes = [_]u32{65};
    const narrow: []const u21 = &unicode;
    const wide: []const u32 = &codes;
    if (forward(narrow) != 21 or forward(wide) != 32 or forward(narrow) != 21) return 1;
    if (reverse(wide) != 32 or reverse(narrow) != 21 or reverse(wide) != 32) return 2;
    return 42;
}
