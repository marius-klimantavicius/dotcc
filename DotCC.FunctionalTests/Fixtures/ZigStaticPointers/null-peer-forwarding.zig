var calls: u32 = 0;
fn array() ?[2]u16 { calls += 1; return .{ 17, 42 }; }
fn peer(expected: anytype, actual: anytype, comptime Wanted: type) bool {
    return @TypeOf(expected, actual) == Wanted;
}
fn forward(expected: anytype, actual: anytype, comptime Wanted: type) bool {
    return peer(expected, actual, Wanted);
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var pointer: ?[*]u8 = null;
    pointer = null;
    if (!forward(null, pointer, ?[*]u8)) return 1;
    if (!forward(pointer, null, ?[*]u8)) return 2;
    const P = @TypeOf(null, pointer);
    if (@typeInfo(@typeInfo(P).optional.child).pointer.size != .many) return 3;
    if (!forward(null, array(), ?[2]u16) or calls != 1) return 4;
    const Unevaluated = @TypeOf(null, array());
    if (Unevaluated != ?[2]u16 or calls != 1) return 5;
    if (@TypeOf(null, @as(?u16, null)) != ?u16) return 6;
    if (@TypeOf(@as(?u16, null), null) != ?u16) return 7;
    return 42;
}
