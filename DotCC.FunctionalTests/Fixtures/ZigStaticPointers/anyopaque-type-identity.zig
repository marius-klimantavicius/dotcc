const Erased = anyopaque;
const Nothing = void;

fn same(text: []const u8, expected: []const u8) bool {
    if (text.len != expected.len) return false;
    for (text, expected) |a, b| if (a != b) return false;
    return true;
}

fn pointeeName(comptime T: type) bool {
    const Pointer = *T;
    const Child = @typeInfo(Pointer).pointer.child;
    return same(@typeName(Child), "anyopaque");
}

fn isOpaque(comptime T: type) bool {
    return @typeInfo(T) == .@"opaque";
}

comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (!same(@typeName(@typeInfo(*anyopaque).pointer.child), "anyopaque")) return 1;
    if (!same(@typeName(Erased), "anyopaque")) return 2;
    if (!same(@typeName(Nothing), "void")) return 3;
    if (!pointeeName(Erased)) return 4;
    if (!isOpaque(Erased) or isOpaque(Nothing) or !isOpaque(Erased)) return 5;
    if (@typeInfo(@typeInfo(*void).pointer.child) != .void) return 6;
    const Qualified = *align(8) const Erased;
    if (!same(@typeName(@typeInfo(Qualified).pointer.child), "anyopaque")) return 7;
    if (!@typeInfo(Qualified).pointer.attrs.@"const" or @typeInfo(Qualified).pointer.attrs.@"align".? != 8) return 8;
    return 42;
}
