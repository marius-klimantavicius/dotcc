const left = @import("TypeNames/left/types.zig");
const right = @import("TypeNames/right/types.zig");
const Renamed = left.Alias;
const Local = struct { value: u8 };

fn equal(actual: []const u8, expected: []const u8) bool {
    if (actual.len != expected.len) return false;
    for (actual, expected) |a, b| if (a != b) return false;
    return true;
}
fn describe(value: anytype) []const u8 {
    switch (@typeInfo(@TypeOf(value))) {
        .pointer => |info| return @typeName(info.child) ++ "@",
        else => unreachable,
    }
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var a: Renamed = .none;
    var b: right.Request = .none;
    if (!equal(describe(&a), "TypeNames.left.types.Request@")) return 1;
    if (!equal(describe(&b), "TypeNames.right.types.Request@")) return 2;
    if (!equal(@typeName(left.Outer.Inner), "TypeNames.left.types.Outer.Inner")) return 3;
    if (!equal(@typeName(Local), "type-name-declared.Local")) return 4;
    if (!equal(@typeName(*const Renamed), "*const TypeNames.left.types.Request")) return 5;
    return 42;
}
