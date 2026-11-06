const Auto = union(enum) { plain: []const u8, marked: [:7]const u8, none };
const Tag = enum(u8) { plain = 3, marked = 8, none = 10 };
const Explicit = union(Tag) { plain: []const u8, marked: [:7]const u8, none };
fn inspect(value: anytype) u8 {
    const pointer = @typeInfo(@TypeOf(value)).pointer;
    const sentinel = pointer.sentinel_ptr orelse return 10;
    const scalar: *const u8 = @ptrCast(@alignCast(sentinel));
    return scalar.*;
}
fn automatic(value: Auto) u8 {
    return switch (value) {
        .plain => |bytes| inspect(bytes),
        .marked => |bytes| inspect(bytes),
        .none => 0,
    };
}
fn explicit(value: Explicit) u8 {
    switch (value) {
        .plain => |bytes| return inspect(bytes),
        .marked => |bytes| return inspect(bytes),
        .none => return 0,
    }
}
pub fn main() u8 {
    const buffer: [2:7]u8 = .{ 11, 13 };
    const plain: []const u8 = &buffer;
    const marked: [:7]const u8 = &buffer;
    const a: Auto = .{ .plain = plain };
    const b: Auto = .{ .marked = marked };
    const c: Explicit = .{ .plain = plain };
    const d: Explicit = .{ .marked = marked };
    if (automatic(a) != 10 or automatic(b) != 7) return 1;
    if (explicit(c) != 10 or explicit(d) != 7) return 2;
    if (inspect(a.plain) != 10 or inspect(b.marked) != 7) return 3;
    if (inspect(c.plain) != 10 or inspect(d.marked) != 7) return 4;
    if (automatic(.none) != 0 or explicit(.none) != 0) return 5;
    return 42;
}
