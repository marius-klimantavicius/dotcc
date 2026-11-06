const Number = struct { pub const bits = 3; };
const Flags = struct { pub const bits = 2; };
const Tag = enum { number, flags };
const U = @Union(.auto, Tag, &.{ "number", "flags" }, &.{ type, type },
    &.{ .{ .@"align" = 1 }, .{ .@"align" = 1 } });
fn choose(comptime tag: Tag) ?U {
    return switch (tag) {
        .number => .{ .number = Number },
        .flags => .{ .flags = Flags },
    };
}
const Runtime = union(enum) { yes: u8, no: u8 };
var calls: u8 = 0;
fn runtimeValue() Runtime { calls += 1; return .{ .yes = 3 }; }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    switch (choose(.number).?) {
        .number => |Layout| { if (Layout != Number or Layout.bits != 3) return 1; },
        else => @compileError("unselected type arm was analyzed"),
    }
    switch (choose(.flags).?) {
        .flags => |Layout| { if (Layout != Flags or Layout.bits != 2) return 2; },
        else => @compileError("unselected type arm was analyzed"),
    }
    switch (runtimeValue()) {
        .yes => |value| { if (value != 3) return 3; },
        .no => return 4,
    }
    if (calls != 1) return 5;
    return 42;
}
