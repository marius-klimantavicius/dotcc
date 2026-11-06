fn callback(_: u8) u8 { return 42; }
const Pointer = *const fn (u8) u8;
fn classify(value: anytype) u8 {
    return switch (@typeInfo(@TypeOf(value))) {
        .pointer => 1, .optional => 2, .@"fn" => 3, else => 4,
    };
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const pointer: Pointer = &callback;
    const optional: ?Pointer = pointer;
    if (classify(pointer) != 1 or classify(optional) != 2 or classify(callback) != 3) return 1;
    if (@typeInfo(Pointer).pointer.size != .one) return 2;
    if (!@typeInfo(Pointer).pointer.attrs.@"const") return 3;
    if (@typeInfo(@typeInfo(Pointer).pointer.child) != .@"fn") return 4;
    if (@typeInfo(@typeInfo(?Pointer).optional.child) != .pointer) return 5;
    const payload = optional orelse return 6;
    if (classify(payload) != 1 or payload(17) != 42) return 7;
    return 42;
}
