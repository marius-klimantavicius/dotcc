var values: [2]u16 = .{ 17, 42 };
fn classify(value: anytype) u8 {
    return switch (@typeInfo(@TypeOf(value))) {
        .optional => 17,
        .pointer => 42,
        else => 1,
    };
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const raw: [*]const u16 = @ptrCast(&values);
    const nullable: ?[*]const u16 = raw;
    if (classify(raw) != 42 or classify(nullable) != 17 or classify(raw) != 42) return 1;
    const P = ?[*]align(2) const u16;
    const Child = @typeInfo(P).optional.child;
    if (@typeInfo(Child).pointer.size != .many) return 2;
    if (!@typeInfo(Child).pointer.attrs.@"const") return 3;
    if (@typeInfo(Child).pointer.attrs.@"align".? != 2) return 4;
    return 42;
}
